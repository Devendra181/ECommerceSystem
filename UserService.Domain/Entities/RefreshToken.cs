namespace UserService.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;
        public string ClientId { get; set; } = null!;    // Web/Android/iOS etc.
        public Client Client { get; set; } = null!;
        public string UserAgent { get; set; } = null!;   // Chrome, Firefox, Safari, etc.
        public DateTime CreatedAt { get; set; }
        public string CreatedByIp { get; set; } = null!;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive => RevokedAt == null && !IsExpired;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
