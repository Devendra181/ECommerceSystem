namespace APIGateway.Models
{
    public class CompressionSettings
    {
        public bool Enabled { get; set; } = true;
        public int CompressionThresholdBytes { get; set; } = 1024; // Default 1 KB
        public string[] SupportedEncodings { get; set; } = new[] { "br", "gzip" };
        public string DefaultEncoding { get; set; } = "gzip";
    }
}
