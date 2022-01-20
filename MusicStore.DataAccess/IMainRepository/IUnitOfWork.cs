using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.IMainRepository
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Category { get; } // değer alabilir
        ICoverTypeRepository CoverType { get; }
        IProductRepository Product { get; }
        ISPCallRepository Sp_Call { get; }

        void Save();
    }
}
