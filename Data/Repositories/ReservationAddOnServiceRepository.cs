using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;

namespace Data.Repositories
{
    public class ReservationAddOnServiceRepository : Repository<ReservationAddOnService>, IReservationAddOnServiceRepository
    {
        public ReservationAddOnServiceRepository(StayHubContext db) : base(db)
        {
        }
    }
}
