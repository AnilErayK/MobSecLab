using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobSecLab.Models
{
    public class FileEntity
    {
        [Key]
        public int FileNum { get; set; }

        [ForeignKey("User")]
        public int UserNo { get; set; }
        public User User { get; set; }

        [Required]
        public int FileSeq { get; set; }

        [Required]
        public string File_Name { get; set; }

        [Required]
        public string File_md5 { get; set; }
    }
}
