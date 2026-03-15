using System.ComponentModel.DataAnnotations;

namespace COLManager.Web.DTOs
{
    public class ProtocolCreateDto
    {
        [Required]
        [MaxLength(150)]
        public string ProtocolName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal MaxAllowedPressureBar { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxAllowedUsageHours { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxAllowedInjections { get; set; }

        public decimal? OperatingTemperatureC { get; set; }
    }

    public class ProtocolReadDto : ProtocolCreateDto
    {
        public int ProtocolID { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
    }
}
