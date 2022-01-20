using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.Models
{
    public class Product
    {
        [Key] // Primary Key olduğunu belirtir
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int CoverTypeId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        [Range(1, 1000)]
        public double ListPrice { get; set; }
        [Required]
        [Range(1, 1000)]
        public double Price { get; set; }


        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [ForeignKey("CoverTypeId")]
        public CoverType CoverType { get; set; }
    }
}
