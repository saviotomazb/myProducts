namespace myProducts.Models
{
    public class PasswordResetCode
    {
        public int PasswordId { get; set; }

        public required int UserId { get; set; }

        public required string Code { get; set; }

        public required DateTime Expiration { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}