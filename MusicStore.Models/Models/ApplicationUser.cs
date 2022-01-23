using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Id vermiyoruz. Id, IdentityUser tarafından geliyor


        public int? CompanyId { get; set; } // Her kaydın olmak zorunda değil
        [Required]
        public string Name { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostaCode { get; set; }

        [NotMapped] // oluşturulan propertyi yok sayar yani tabloda oluşmasını engeller
        public string Role { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }
}
