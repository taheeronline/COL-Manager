using System.ComponentModel.DataAnnotations;

namespace COLManager.Web.Entities
{
    public class AuditTrail
    {
        [Key]
        public int AuditID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; } = null!;

        public int RecordID { get; set; }

        [Required]
        [MaxLength(10)]
        public string OperationType { get; set; } = null!;

        public string? OldData { get; set; }
        public string? NewData { get; set; }

        [Required]
        [MaxLength(150)]
        public string ChangedBy { get; set; } = null!;

        public DateTime ChangedOn { get; set; }
    }
}
