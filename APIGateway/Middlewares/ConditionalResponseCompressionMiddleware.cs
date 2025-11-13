using System.IO.Compression;
using Microsoft.Extensions.Options;
using APIGateway.Models;

namespace APIGateway.Middlewares
{
    // Middleware for conditional response compression at the API Gateway level.
    // - Uses a memory buffer to inspect the generated response.
    // - Applies Brotli or Gzip based on:
    //     * Client capabilities (Accept-Encoding)
    //     * Response size threshold
    //     * Allowed content types
    // - Respects dynamic settings via IOptionsMonitor (no restart needed for config changes).
    public class ConditionalResponseCompressionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<CompressionSettings> _settingsMonitor;

        public ConditionalResponseCompressionMiddleware(RequestDelegate next, IOptionsMonitor<CompressionSettings> settings)
        {
            _next = next;
            _settingsMonitor = settings;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Always read the latest config so changes in appsettings (with reloadOnChange) are honored.
            var settings = _settingsMonitor.CurrentValue;

            // 1. Global toggle:
            // If compression is disabled via configuration, short-circuit and let the pipeline continue as-is.
            if (!settings.Enabled)
            {
                await _next(context);
                return;
            }

            // 2. Swap the response body with an in-memory buffer:
            // We let downstream components write to this buffer so we can:
            //   - Inspect size
            //   - Inspect content type
            //   - Decide whether and how to compress
            var originalBody = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            // 3. Execute the rest of the pipeline:
            // Controllers / endpoints run here and write their response into "buffer".
            await _next(context);

            // 4. Capture response metadata after execution.
            // At this point, the response body is fully available in the buffer.
            var contentType = context.Response.ContentType;
            var acceptEncoding = context.Request.Headers["Accept-Encoding"].ToString();

            // 5. Skip compression if:
            //  - Client did not send "Accept-Encoding" header (meaning it can’t handle compressed data)
            //  - Or the response content type is not suitable for compression (e.g., images, PDFs, videos)
            if (string.IsNullOrEmpty(acceptEncoding) ||
                !IsCompressibleContentType(contentType))
            {
                // Restore original stream and flush the uncompressed response.
                buffer.Position = 0;
                context.Response.Body = originalBody;
                await buffer.CopyToAsync(context.Response.Body);
                return;
            }

            // 6. Compress only if the response size exceeds the configured threshold
            // (e.g., 1 KB or 2 KB as specified in appsettings.json)
            if (buffer.Length > settings.CompressionThresholdBytes)
            {
                // Resolve which encoding to use based on:
                //   - Client's Accept-Encoding header.
                //   - SupportedEncodings + DefaultEncoding from configuration.
                var encoding = SelectEncoding(acceptEncoding, settings);

                // Ensure we read from the beginning.
                buffer.Position = 0;
                using var compressed = new MemoryStream();

                // 7. Perform the actual compression into the "compressed" stream.
                // Brotli: more efficient, preferred by modern clients.
                if (encoding.Equals("br", StringComparison.OrdinalIgnoreCase))
                {
                    using (var brotli = new BrotliStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
                    {
                        await buffer.CopyToAsync(brotli);
                    }
                }
                // Gzip: widely supported, safe fallback.
                else if (encoding.Equals("gzip", StringComparison.OrdinalIgnoreCase))
                {
                    using (var gzip = new GZipStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
                    {
                        await buffer.CopyToAsync(gzip);
                    }
                }
                else
                {
                    // If we end up with an encoding we don't handle,
                    // fall back to sending the original uncompressed response.
                    buffer.Position = 0;
                    context.Response.Body = originalBody;
                    await buffer.CopyToAsync(context.Response.Body);
                    return;
                }

                // 8. Update response headers:
                // Inform client which encoding is used and set Content-Length accordingly.
                context.Response.Headers["Content-Encoding"] = encoding;
                context.Response.ContentLength = compressed.Length;

                // 9. Write compressed payload to the real response stream.
                compressed.Position = 0;
                context.Response.Body = originalBody;
                await compressed.CopyToAsync(context.Response.Body);
            }
            else
            {
                // 10. Below threshold: send as-is without compression.
                // Restores original stream and copies buffered content.
                buffer.Position = 0;
                context.Response.Body = originalBody;
                await buffer.CopyToAsync(context.Response.Body);
            }
        }

        // Determines the preferred compression encoding based on the client’s Accept-Encoding header
        // and supported encodings defined in appsettings.json.
        private string SelectEncoding(string acceptEncoding, CompressionSettings settings)
        {
            // Prefer Brotli when:
            // - Client accepts "br"
            // - And "br" is enabled in configuration.
            if (acceptEncoding.Contains("br", StringComparison.OrdinalIgnoreCase) &&
                settings.SupportedEncodings.Contains("br", StringComparer.OrdinalIgnoreCase))
            {
                return "br";
            }

            // Otherwise, try Gzip under the same conditions.
            if (acceptEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase) &&
                settings.SupportedEncodings.Contains("gzip", StringComparer.OrdinalIgnoreCase))
            {
                return "gzip";
            }

            // As a safety net, use whatever DefaultEncoding is configured.
            return settings.DefaultEncoding;
        }

        // Determines whether a given content type is suitable for compression.
        // We compress only textual formats (JSON, XML, HTML, JavaScript, etc.)
        // and skip binary formats (images, PDFs, audio, video, etc.).
        private static bool IsCompressibleContentType(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            // Allow only compressible MIME types
            return contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ||
                   contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
                   contentType.StartsWith("application/xml", StringComparison.OrdinalIgnoreCase) ||
                   contentType.StartsWith("application/javascript", StringComparison.OrdinalIgnoreCase) ||
                   contentType.StartsWith("application/xhtml+xml", StringComparison.OrdinalIgnoreCase);
        }
    }
}
