using MusicStore.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.ViewModels
{
    public class OrderDetailsVM
    {
        public Order Order { get; set; }
        public IEnumerable<OrderDetails> OrderDetails { get; set; }
    }
}
