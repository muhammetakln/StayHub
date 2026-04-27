using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class ReservationAddOnService:BaseEntity
    {
        public int ReservationId { get; set; }
        public int AddOnServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
       
        public virtual Reservation?Reservation { get; set; }
        public virtual AddOnService? AddOnService { get; set; }


    }
}