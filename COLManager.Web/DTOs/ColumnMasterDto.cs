using System.ComponentModel.DataAnnotations;

namespace COLManager.Web.DTOs
{
    public class ColumnMasterCreateDto
    {
        [Required]
        [MaxLength(150)]
        public string ColumnName { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Manufacturer { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = null!;

        [Range(0.01, double.MaxValue)]
        public decimal LengthMM { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal InternalDiameterMM { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal ParticleSizeMicron { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal MaxPressureBar { get; set; }

        public int? ProtocolID { get; set; }

        public int StatusID { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalRuntimeHours { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalInjections { get; set; }

        public DateTime? InstalledOn { get; set; }

        public DateTime? RetiredOn { get; set; }
    }

    public class ColumnMasterUpdateDto : ColumnMasterCreateDto
    {
        [Required]
        public int ColumnID { get; set; }
    }

    public class ColumnMasterReadDto
    {
        public int ColumnID { get; set; }
        public string ColumnName { get; set; } = null!;
        public string Manufacturer { get; set; } = null!;
        public string SerialNumber { get; set; } = null!;
        public decimal LengthMM { get; set; }
        public decimal InternalDiameterMM { get; set; }
        public decimal ParticleSizeMicron { get; set; }
        public decimal MaxPressureBar { get; set; }
        public int? ProtocolID { get; set; }
        public int StatusID { get; set; }
        public decimal TotalRuntimeHours { get; set; }
        public int TotalInjections { get; set; }
        public DateTime? InstalledOn { get; set; }
        public DateTime? RetiredOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
