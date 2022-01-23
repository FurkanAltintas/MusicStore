using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.IMainRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IApplicationUserRepository ApplicationUser { get; }
        ICategoryRepository Category { get; } // değer alabilir
        ICompanyRepository Company { get; }
        ICoverTypeRepository CoverType { get; }
        IOrderRepository Order { get; }
        IOrderDetailRepository OrderDetails { get; }
        IProductRepository Product { get; }
        IShoppingCartRepository ShoppingCart { get; }
        ISPCallRepository Sp_Call { get; }

        void Save();
    }
}
