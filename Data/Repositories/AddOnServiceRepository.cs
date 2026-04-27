using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;

namespace Data.Repositories
{
    public class AddOnServiceRepository : Repository<AddOnService>, IAddOnServiceRepository
    {
        public AddOnServiceRepository(StayHubContext db ):base(db) 
        {
            
        }
    }
}
