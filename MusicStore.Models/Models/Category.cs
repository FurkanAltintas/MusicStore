using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(250, MinimumLength = 3, ErrorMessage ="Şartlara uygun bir değer giriniz")]
        public string Name { get; set; }
    }
}
