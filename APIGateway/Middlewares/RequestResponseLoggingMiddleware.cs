using Serilog;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace APIGateway.Middlewares
{
    // Middleware for logging detailed information about every incoming request
    // and outgoing response in the ASP.NET Core API Gateway.
    // Responsibilities:
    //      1. Logs HTTP Method, URL Path, QueryString, IP Address, Request Body.
    //      2. Measure and log total request-processing time.
    //      3. Logs response status code, response time, and response body.
    //      4. Masks sensitive fields like passwords, tokens, and secrets.
    public class RequestResponseLoggingMiddleware
    {
        // List of sensitive key names that should be masked in logs.
        // Add or remove entries as needed for your application security needs.
        private static readonly string[] DefaultSensitiveKeys = new[]
        {
            "password", "pwd", "token", "secret", "api_key", "apikey", "token",
            "accesstoken", "refreshtoken", "access_token", "refresh_token"
        };

        // Delegate reference to invoke the next middleware in the request pipeline.
        private readonly RequestDelegate _next;

        // Limit how much data can be logged from a request or response body (in bytes).
        // This protects the gateway from memory overuse if clients send very large payloads.
        private const int MaxBodySize = 64 * 1024; // 64 KB

        // Constructor injection of the pipeline delegate.
        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // ---------------------------------------------------------------------
        // Primary Entry Point for Every HTTP Request
        // ---------------------------------------------------------------------
        public async Task InvokeAsync(HttpContext context)
        {
            // Stopwatch to measure total request processing time.
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            // Variable to hold request body content (if captured).
            string requestBody = string.Empty;

            try
            {
                // ================================================================
                // CAPTURE REQUEST BODY (only for JSON POST/PUT/PATCH requests)
                // ================================================================

                if (ShouldCaptureRequest(context.Request))
                {
                    // EnableBuffering allows us to read the body stream multiple times
                    // (without consuming it permanently).
                    context.Request.EnableBuffering();

                    // Read the full request body as a string.
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    var raw = await reader.ReadToEndAsync();

                    // Reset the body stream position so downstream middleware can still read it.
                    context.Request.Body.Position = 0;

                    // If the request body is within allowed size, log it safely (after masking sensitive data)
                    requestBody = raw.Length <= MaxBodySize
                        ? MaskSensitiveData(raw)
                        : $"[Request body too large: {raw.Length / 1024} KB truncated]";
                }

                // ================================================================
                // PREPARE TO CAPTURE RESPONSE BODY
                // ================================================================

                // Original Response Stream: Keep a reference to the original stream
                var originalBody = context.Response.Body;

                //  Temporary Response Stream: Temporary buffer to hold response.
                await using var memoryStream = new MemoryStream();

                // Replace the response stream.
                context.Response.Body = memoryStream;

                // Gather request metadata for structured logging
                var requestPath = context.Request.Path.Value ?? "/";
                var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "-";
                var reqLenText = context.Request.ContentLength.HasValue ? $"{context.Request.ContentLength.Value} B" : "-";

                // Build a human-readable request log message with pipe-separated fields
                var reqSegments = new List<string>
                {
                    $"Incoming request Method: {context.Request.Method}",
                    $"Path: {requestPath}{queryString}",
                    $"IP={clientIp}",
                    $"ContentLength={reqLenText}"
                };

                // Add the request body if available
                if (!string.IsNullOrEmpty(requestBody))
                    reqSegments.Add($"Request Body: {requestBody}");

                // Log the final combined request details
                Log.Information(string.Join(" | ", reqSegments));

                // ================================================================
                // INVOKE NEXT MIDDLEWARE (Actual API Execution)
                // ================================================================
                await _next(context);

                // Stop the stopwatch as soon as response is generated
                stopWatch.Stop();

                // ================================================================
                // CAPTURE RESPONSE BODY CONTENT
                // ================================================================

                // Reset the memory stream’s position to the beginning
                // because downstream middleware has already written the full response
                // into this temporary stream. We now need to read it from the start.
                memoryStream.Position = 0;

                // Variable that will store the final response body text.
                string responseBody;

                // Create a StreamReader to read text from the memoryStream (the captured response).
                // leaveOpen: true keeps the memoryStream usable later when we copy it back to the original stream.
                using (var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true))
                {
                    // Read the entire response stream into a raw string.
                    // This gives us the exact JSON/text returned by the downstream service.
                    var raw = await reader.ReadToEndAsync();

                    // Reset the position back to zero again so that the stream can be
                    // copied to the original Response.Body later for client delivery.
                    memoryStream.Position = 0;

                    // Only process the body if it’s within the configured maximum capture size (64 KB)
                    if (raw.Length <= MaxBodySize)
                    {
                        // Attempt to format the body as indented JSON.
                        // This makes the logged response human-readable instead of a
                        // compact single-line JSON string. If the content is not JSON,
                        // parsing will throw and we’ll fall back to plain text.
                        try
                        {
                            // Parse the raw string into a structured JsonDocument.
                            using var jsonDoc = JsonDocument.Parse(raw);

                            // Serialize the JsonDocument back to text but with indentation.
                            // WriteIndented = true adds line breaks and spacing for readability.
                            var pretty = JsonSerializer.Serialize(
                                jsonDoc,
                                new JsonSerializerOptions { WriteIndented = true }
                            );

                            // Apply masking to hide sensitive fields (passwords, tokens, etc.)
                            // before including the content in logs.
                            responseBody = MaskSensitiveData(pretty);
                        }
                        catch
                        {
                            // Fallback path when parsing fails.
                            // If the response isn’t valid JSON (for example plain text,
                            // XML, or binary data), skip formatting and just mask sensitive
                            // values using Regex on the raw string.
                            responseBody = MaskSensitiveData(raw);
                        }
                    }
                    else
                    {
                        // Handle large payloads.
                        // When the response body exceeds MaxBodySize (64 KB),
                        // don’t log its contents. Instead, record a truncated note.
                        responseBody = $"[Response body too large: {raw.Length / 1024} KB truncated]";
                    }
                }

                // Copy the captured response back to the original stream
                await memoryStream.CopyToAsync(originalBody);

                // ================================================================
                // BUILD & LOG RESPONSE DETAILS
                // ================================================================

                // Response size in bytes
                var resLenText = $"{memoryStream.Length} B";

                // Build a human-readable response log
                var resSegments = new List<string>
                {
                    $"Outgoing response {context.Response.StatusCode} in {stopWatch.ElapsedMilliseconds} ms",
                    $"IP={clientIp}",
                    $"ContentLength={resLenText}"
                };

                // Include body if available.
                if (!string.IsNullOrEmpty(responseBody))
                    resSegments.Add($"Response Body: {responseBody}");

                // Log the final combined response details
                Log.Information(string.Join(" | ", resSegments));
            }
            catch (Exception ex)
            {
                // ================================================================
                // HANDLE UNEXPECTED ERRORS
                // ================================================================

                // Stop timer if exception occurs
                stopWatch.Stop();

                // Log error with full exception stack trace for debugging
                Log.Error(ex, "Error in RequestResponseLoggingMiddleware");

                // Re-throw to allow global exception handlers to act
                throw;
            }
        }

        // Determines whether the request body should be captured based on:
        //      1. Content-Type being JSON.
        //      2. HTTP Method being POST, PUT, or PATCH.
        // Avoids unnecessary logging for GET requests or binary content.
        private static bool ShouldCaptureRequest(HttpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ContentType))
                return false;

            return request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase)
                   && (request.Method == HttpMethods.Post
                       || request.Method == HttpMethods.Put
                       || request.Method == HttpMethods.Patch);
        }

        // Masks sensitive values (like passwords, tokens, or secrets) from JSON-formatted
        // strings before they are logged. This ensures that confidential information
        // never appears in log files.
        // Example transformation:
        // Input:  {"user":"admin","password":"mypwd","token":"abcd123"}
        // Output: {"user":"admin","password":"****","token":"****"}
        // -----------------------------------------------------------------------------
        private static string MaskSensitiveData(string body)
        {
            try
            {
                // ---------------------------------------------------------------------
                // STEP 1: Build a single regex pattern that includes all sensitive keys.
                // ---------------------------------------------------------------------
                // DefaultSensitiveKeys = ["password", "pwd", "token", "secret", ...]
                //
                // The `string.Join("|", ...)` joins all items using the '|' character,
                // which in regex means "OR".
                //
                // Example output pattern:
                //     password|pwd|token|secret|api_key|apikey|access_token|refresh_token
                //
                // `Regex.Escape()` ensures that special characters (like underscores)
                // in key names are treated literally, not as regex operators.
                var pattern = string.Join("|", DefaultSensitiveKeys.Select(Regex.Escape));

                // ---------------------------------------------------------------------
                // STEP 2: Construct the actual regex expression.
                // ---------------------------------------------------------------------
                // The constructed regex will look like this:
                //   (?i)("(?:(password|pwd|token|...))"\s*:\s*")[^"]*(")
                //
                // Explanation of each piece:
                //   (?i)                  → Enables case-insensitive matching (so "Password" or "TOKEN" also match).
                //   \"(?:{pattern})\"     → Matches any property name in double quotes whose name is in our key list.
                //   \s*:\s*               → Matches the colon separating the key from its value, with optional spaces.
                //   \"[^\"]*\"            → Matches the quoted string value (everything between the next pair of quotes).
                //
                // Combined, this finds JSON fragments like:
                //     "password": "myp@ss"
                //     "api_key" : "12345"
                // and captures them in groups for replacement.

                var regex = new Regex($"(?i)(\"(?:{pattern})\"\\s*:\\s*\")[^\"]*(\")");

                // ---------------------------------------------------------------------
                // STEP 3: Replace all captured sensitive values with "****"
                // ---------------------------------------------------------------------
                //
                // $1 and $2 are backreferences to the first and second capture groups:
                //   $1 → everything before the sensitive value (e.g., "password":")
                //   $2 → the closing double quote after the value
                //
                // The sensitive content between $1 and $2 gets replaced with "****".
                //
                // Example:
                //   Before:  "token":"abcd1234"
                //   After :  "token":"****"
                return regex.Replace(body, "$1****$2");
            }
            catch
            {
                // ---------------------------------------------------------------------
                // STEP 4: Fallback for unexpected situations.
                // ---------------------------------------------------------------------
                // If:
                //   - The input isn't valid JSON,
                //   - The regex construction fails, or
                //   - The body contains special encodings that break parsing,
                // ...then we simply return the original text without modification.
                //
                // This ensures logging continues safely instead of crashing the app.
                return body;
            }
        }

    }

    // Extension method for easily adding this middleware
    // to the ASP.NET Core pipeline via app.UseRequestResponseLogging().
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            // Enables a clean, self-explanatory registration in Program.cs:
            // app.UseRequestResponseLogging();
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
