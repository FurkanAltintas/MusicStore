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
    internal class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public CoverTypeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(CoverType coverType)
        {
            var data = _context.CoverTypes.FirstOrDefault(x => x.Id == coverType.Id);
            if (data != null)
            {
                data.Name = coverType.Name;
            }

        }
    }
}
