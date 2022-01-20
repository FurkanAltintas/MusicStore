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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        // Bütün işlemlerimiz yapılmış oldu
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            // base olarak contextten türetiyoruz. Base yapının ana classtan geleceğini söylüyor.
            // Ana class Repository olduğu için işlemi halletmiş olduk
            _context = context;
        }

        public void Update(Category category)
        {
            var data = _context.Categories.FirstOrDefault(x => x.Id == category.Id);
            if (data != null)
            {
                data.Name = category.Name;
            }
        }
    }
}
