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
            Category = new CategoryRepository(_context);
            CoverType = new CoverTypeRepository(_context);
            Sp_Call = new SPCallRepository(_context);
        }

        public ICategoryRepository Category { get; private set; }

        public ISPCallRepository Sp_Call { get; private set; }

        public ICoverTypeRepository CoverType { get; private set; }

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
