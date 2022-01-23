using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        /* 
         * Microsoftun tablolarını kullandığımız için bunun tipi string olarak tutuyor ve int ı açıyor. Id kısmı guid olarak oluşturuluyor
         */

        [ForeignKey("ApplicationUserId")] // Hangi kolon üzerinden bağlantı kuracağını ayarladık
        public ApplicationUser ApplicationUser { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        [Range(1, 1000, ErrorMessage = "Please Enter Value between 1 and 1000")]
        public int Count { get; set; } = 1; // 1 değerini atadık (default hali 0)
        [NotMapped] // View tarafında daha çok kullanıcağımız için database ile eşleştirmiyoruz
        public double Price { get; set; }
    }
}
