namespace myProducts.Models
{
    public partial class UserSession
    {
        public int SessionId { get; set; }
        
        public required int UserId { get; set; }

        public required byte[] RefreshTokenHash { get; set; }

        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public required DateTime ExpiresAt { get; set;}

        public DateTime? RevokedAt { get; set; }

        public int? ReplacedBySessionId { get; set; }

        public String? DeviceInfo { get; set; }

        public string? IpAddress { get; set; }

        public virtual User User { get; set; } = null!;
    }
}