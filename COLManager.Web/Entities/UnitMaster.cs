using System.ComponentModel.DataAnnotations;

namespace COLManager.Web.Entities
{
    public class UnitMaster
    {
        [Key]
        public int UnitID { get; set; }

        public int MeasurementTypeID { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitName { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string UnitSymbol { get; set; } = null!;

        public bool IsBaseUnit { get; set; }
    }
}
