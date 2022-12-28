namespace TransactionProcessor.Models
{

    public enum AccountStatus
    {
        Active,
        Suspended,
        Deleted,
    }

    public class AccountDto
    {
        public int Id { get; set; }

        public string Number { get; set; }

        public decimal Balance { get; set; }

        public string Currency { get; set; }

        public string StartTime { get; set; }

        public AccountStatus Status { get; set; }

        public int CustomerId { get; set; }
    }
}
