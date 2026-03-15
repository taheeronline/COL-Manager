using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLManager.Web.Entities
{
    public class Protocol
    {
        [Key]
        public int ProtocolID { get; set; }

        [Required]
        [MaxLength(150)]
        public string ProtocolName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MaxAllowedPressureBar { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MaxAllowedUsageHours { get; set; }

        public int MaxAllowedInjections { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? OperatingTemperatureC { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsActive { get; set; }
    }
}
