using System.ComponentModel.DataAnnotations;

namespace COLManager.Web.Entities
{
    public class StatusMaster
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        [MaxLength(50)]
        public string StatusName { get; set; } = null!;
    }
}
