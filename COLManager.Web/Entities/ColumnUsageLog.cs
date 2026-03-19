using COLManager.Web.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ColumnUsageLog
{
    [Key]
    public int UsageID { get; set; }

    public int ColumnID { get; set; }

    // =========================
    // CHECKOUT FIELDS
    // =========================

    public DateTime? CheckoutDate { get; set; }

    public DateTime? CheckinDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Checked Out";

    // =========================
    // USAGE FIELDS
    // =========================

    [Column(TypeName = "decimal(10,2)")]
    public decimal? RuntimeHours { get; set; }

    public int? NumberOfInjections { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PreUseBackPressureBar { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PostUseBackPressureBar { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? MaxPressureObservedBar { get; set; }

    [Column(TypeName = "decimal(10,3)")]
    public decimal? FlowRateMLPerMin { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? InjectionVolumeML { get; set; }

    public DateTime? RunDate { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    // =========================
    // NAVIGATION
    // =========================

    public ColumnMaster? Column { get; set; }
}