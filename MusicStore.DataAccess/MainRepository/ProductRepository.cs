using MusicStore.DataAccess.Data;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.MainRepository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var data = _context.Products.FirstOrDefault(x => x.Id == product.Id);
            if (data != null)
            {
                if (product.ImageUrl != null)
                {
                    data.ImageUrl = product.ImageUrl;
                }
                data.CategoryId = product.CategoryId;
                data.CoverTypeId = product.CoverTypeId;
                data.Title = product.Title;
                data.Description = product.Description;
                data.Author = product.Author;
                data.ISBN = product.ISBN;
                data.ListPrice = product.ListPrice;
                data.Price = product.Price;
            }

        }
    }
}
