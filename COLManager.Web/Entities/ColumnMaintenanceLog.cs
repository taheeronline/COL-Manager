using System.ComponentModel.DataAnnotations;

namespace COLManager.Web.Entities
{
    public class ColumnMaintenanceLog
    {
        [Key]
        public int MaintenanceID { get; set; }

        public int ColumnID { get; set; }

        [Required]
        [MaxLength(100)]
        public string MaintenanceType { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime PerformedOn { get; set; }

        [Required]
        [MaxLength(150)]
        public string PerformedBy { get; set; } = null!;
    }
}
