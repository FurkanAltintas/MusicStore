using Microsoft.AspNetCore.Mvc.Rendering;
using MusicStore.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.ViewModels
{
    public class ProductVM
    {
        public Product  Product { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> CoverTypes { get; set; }
    }
}
