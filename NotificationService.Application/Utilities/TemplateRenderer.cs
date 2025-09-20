using NotificationService.Application.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NotificationService.Application.Utilities
{
    // TemplateRenderer replaces placeholder tokens in a string template (e.g. {CustomerName})
    // with actual values provided in a dictionary.
    public class TemplateRenderer : ITemplateRenderer
    {
        // Precompile a regex pattern that matches placeholders in the format {KeyName}.
        // Example: "Hello {CustomerName}" -> key = "CustomerName"
        private static readonly Regex _rx = new Regex(
            "{(?<key>[A-Za-z0-9_.]+)}",
            RegexOptions.Compiled);

        // Replaces placeholders inside the template with values from the given dictionary.
        // template: The string template that may contain placeholders like {OrderId}
        // data: Key-value pairs where key = placeholder name, value = replacement
        // returns: A new string with placeholders replaced by their values
        public string Render(string template, IReadOnlyDictionary<string, object>? data)
        {
            // If template is null or empty, OR if no data is provided,
            // return the template unchanged (or empty if null).
            if (string.IsNullOrEmpty(template) || data == null || data.Count == 0)
                return template ?? string.Empty;

            // Use regex Replace: find every placeholder and substitute with dictionary values
            return _rx.Replace(template, m =>
            {
                // Extract the placeholder key (e.g., "CustomerName" from "{CustomerName}")
                var key = m.Groups["key"].Value;

                // If the key exists in the dictionary AND the value is not null
                if (!data.TryGetValue(key, out var val) || val == null)
                    return m.Value;

                // Special handling for Items
                if (key.Equals("Items", StringComparison.OrdinalIgnoreCase) && val is JsonElement json && json.ValueKind == JsonValueKind.Array)
                {
                    var sb = new StringBuilder();
                    sb.Append("<table style='border-collapse: collapse; width:100%;'>");
                    sb.Append("<tr style='background-color:#f2f2f2; text-align:left;'>");
                    sb.Append("<th style='border:1px solid #ddd; padding:8px;'>Name</th>");
                    sb.Append("<th style='border:1px solid #ddd; padding:8px;'>Quantity</th>");
                    sb.Append("<th style='border:1px solid #ddd; padding:8px;'>Price</th>");
                    sb.Append("</tr>");

                    foreach (var item in json.EnumerateArray())
                    {
                        var name = item.GetProperty("Name").GetString();
                        var qty = item.GetProperty("Quantity").GetInt32();
                        var price = item.GetProperty("Price").GetDecimal();

                        sb.Append("<tr>");
                        sb.Append($"<td style='border:1px solid #ddd; padding:8px;'>{name}</td>");
                        sb.Append($"<td style='border:1px solid #ddd; padding:8px;'>{qty}</td>");
                        sb.Append($"<td style='border:1px solid #ddd; padding:8px;'>₹{price}</td>");
                        sb.Append("</tr>");
                    }

                    sb.Append("</table>");
                    return sb.ToString();
                }

                // If no matching key in dictionary, keep the original token unchanged
                // Example: if {TrackingLink} not found in data, keep "{TrackingLink}" in output

                return val.ToString() ?? string.Empty;
            });
        }
    }
}
