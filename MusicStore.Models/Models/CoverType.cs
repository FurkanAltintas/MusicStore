using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.Models
{
    public class CoverType
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Cover Type alanını boş bırakamazsınız")]
        [MaxLength(50, ErrorMessage = "50 Karakterden uzun olamaz")]
        public string Name { get; set; }
    }
}
