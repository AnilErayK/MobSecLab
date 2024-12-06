using System.ComponentModel.DataAnnotations;

namespace MobSecLab.Models
{
    public class User
    {
        [Key]
        public int UserNo { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
