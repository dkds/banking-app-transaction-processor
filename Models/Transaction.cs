using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Models
{
    public enum TransactionType
    {
        Credit,
        Debit,
        Undefined,
    }

    public class Transaction
    {
        public int Id { get; set; }

        [StringLength(16, MinimumLength = 4)]
        public string ReferenceNumber { get; set; } = "0000";

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; } = TransactionType.Undefined;

        [StringLength(50)]
        public string Notes { get; set; } = "";

        [DataType(DataType.DateTime)]
        public string Time { get; set; } = DateTime.Now.ToUniversalTime().ToString("u").Replace(" ", "T");

        [StringLength(16, MinimumLength = 4)]
        public string? AccountNumberFrom { get; set; }

        [StringLength(16, MinimumLength = 4)]
        public string? AccountNumberTo { get; set; }
    }
}
