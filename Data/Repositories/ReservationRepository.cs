using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;

namespace Data.Repositories
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(StayHubContext db) : base(db)
        {
        }
    }
}
