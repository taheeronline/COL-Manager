using System.ComponentModel.DataAnnotations;

namespace COLManager.Web.DTOs
{
    // =========================
    // CHECKOUT (CREATE)
    // =========================
    public class ColumnCheckoutCreateDto
    {
        [Required]
        public int ColumnID { get; set; }

        // ❌ Removed ProtocolID & ProtocolName (derived from Column)

        public DateTime CheckoutDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    // =========================
    // CHECKOUT (READ)
    // =========================
    public class ColumnCheckoutReadDto
    {
        public int CheckoutID { get; set; }

        public int ColumnID { get; set; }

        // ❌ Removed ProtocolID (not needed anymore)

        public string ProtocolName { get; set; } = string.Empty; // ✅ still needed for UI

        public string Status { get; set; } = "Checked Out";

        public DateTime CheckoutDate { get; set; }

        public DateTime? CheckinDate { get; set; }

        public string ColumnName { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        // 🔥 Protocol limits (derived from Column → Protocol)
        public decimal MaxAllowedPressureBar { get; set; }
        public decimal MaxAllowedUsageHours { get; set; }
        public int MaxAllowedInjections { get; set; }

        // 🔥 Cumulative usage
        public decimal TotalRuntimeHours { get; set; }
        public int TotalInjections { get; set; }
        public decimal TotalPressureUsed { get; set; }
    }

    // =========================
    // CHECK-IN (UNCHANGED)
    // =========================
    public class ColumnCheckinDto
    {
        [Required]
        public int CheckoutID { get; set; }

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

        public DateTime CheckinDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    // =========================
    // DASHBOARD (UNCHANGED)
    // =========================
    public class ColumnUsageLifecycleReadDto
    {
        public int CheckoutID { get; set; }

        public int ColumnID { get; set; }

        public string ColumnName { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public string ProtocolName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CheckoutDate { get; set; }

        public DateTime? CheckinDate { get; set; }

        public decimal? RuntimeHours { get; set; }

        public int? NumberOfInjections { get; set; }

        public decimal? PreUseBackPressureBar { get; set; }

        public decimal? PostUseBackPressureBar { get; set; }

        public decimal? MaxPressureObservedBar { get; set; }

        public decimal? FlowRateMLPerMin { get; set; }

        public decimal? InjectionVolumeML { get; set; }

        public string? Remarks { get; set; }
    }
}