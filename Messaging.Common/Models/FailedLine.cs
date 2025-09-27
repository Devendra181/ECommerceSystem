namespace Messaging.Common.Models
{
    public sealed class FailedLine
    {
        public Guid ProductId { get; set; } // The product that failed
        public int Requested { get; set; } // How many units were requested
        public int Available { get; set; }  // How many units were actually available
        public string Reason { get; set; } = "Insufficient stock";
    }
}
