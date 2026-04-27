using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;

namespace Data.Repositories
{
    public class RoomImageRepository : Repository<RoomImage>, IRoomImageRepository
    {
        public RoomImageRepository(StayHubContext db) : base(db)
        {
        }
    }
}
