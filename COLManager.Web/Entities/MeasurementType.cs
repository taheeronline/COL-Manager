using System.ComponentModel.DataAnnotations;

namespace COLManager.Web.Entities
{
    public class MeasurementType
    {
        [Key]
        public int MeasurementTypeID { get; set; }

        [Required]
        [MaxLength(50)]
        public string MeasurementName { get; set; } = null!;
    }
}
