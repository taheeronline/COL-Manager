using System.ComponentModel.DataAnnotations;

namespace COLManager.Web.DTOs
{
    public class ColumnUsageCreateDto
    {
        [Required]
        public int ColumnID { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal RuntimeHours { get; set; }

        [Range(0, int.MaxValue)]
        public int NumberOfInjections { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PreUseBackPressureBar { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PostUseBackPressureBar { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxPressureObservedBar { get; set; }

        public decimal? FlowRateMLPerMin { get; set; }
        public decimal? InjectionVolumeML { get; set; }
        public DateTime RunDate { get; set; } = DateTime.UtcNow;
        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    public class ColumnUsageReadDto : ColumnUsageCreateDto
    {
        public int UsageID { get; set; }
    }
}
