using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobSecLab.Models
{
    public class Results
    {
        [Key]
        public int ResultsNo { get; set; }

        public int FileSeq { get; set; }

        [Required]
        public string md5 { get; set; }

        [Required]
        public string File_Name { get; set; }

        public int UserNo { get; set; }

        public int TotalMalwarePermission { get; set; }
        public int TotalPermission { get; set; }
        public int SeverityHigh { get; set; }
        public int StatusDangerous { get; set; }
        public string minSdk { get; set; }
        public int SecurityScore { get; set; }
    }
}