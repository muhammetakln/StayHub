using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;

namespace Data.Repositories
{
    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
    {
        public AmenityRepository(StayHubContext db  ) : base(db) 
        {
            
        }
    }
}
