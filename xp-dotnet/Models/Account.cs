namespace XpDotnetSqlServer.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public DateTime? DateOfBirth { get; set; }
        public string? AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
