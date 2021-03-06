using MusicStore.DataAccess.Data;
using MusicStore.DataAccess.IMainRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.MainRepository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            ApplicationUser = new ApplicationUserRepository(_context);
            Category = new CategoryRepository(_context);
            Company = new CompanyRepository(_context);
            CoverType = new CoverTypeRepository(_context);
            Order = new OrderRepository(_context);
            OrderDetails = new OrderDetailRepository(_context);
            Product = new ProductRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
            Sp_Call = new SPCallRepository(_context);
        }

        public ICategoryRepository Category { get; private set; }

        public ISPCallRepository Sp_Call { get; private set; }

        public ICoverTypeRepository CoverType { get; private set; }

        public IProductRepository Product { get; private set; }

        public ICompanyRepository Company { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IOrderRepository Order { get; private set; }

        public IOrderDetailRepository OrderDetails { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
