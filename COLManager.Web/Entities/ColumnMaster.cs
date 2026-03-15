using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COLManager.Web.Entities
{
    public class ColumnMaster
    {
        [Key]
        public int ColumnID { get; set; }

        [Required]
        [MaxLength(150)]
        public string ColumnName { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Manufacturer { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal LengthMM { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal InternalDiameterMM { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal ParticleSizeMicron { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MaxPressureBar { get; set; }

        public int? ProtocolID { get; set; }

        public int StatusID { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalRuntimeHours { get; set; }

        public int TotalInjections { get; set; }

        public DateTime? InstalledOn { get; set; }

        public DateTime? RetiredOn { get; set; }

        public DateTime CreatedOn { get; set; }

        // Navigation properties can be added if needed
    }
}
