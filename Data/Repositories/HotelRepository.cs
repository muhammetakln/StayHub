using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class HotelRepository:Repository<Hotel>,IHotelRepository
    {
        public HotelRepository(StayHubContext db): base(db)
        {
            
        }
    }
}
