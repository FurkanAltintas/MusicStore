using MusicStore.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.IMainRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        // ICategory, IRepository olucak. Artık IRepository içerisindeki metotlar ICategoryRepository içerisinde de olucak.

        void Update(Category category);
    }
}
