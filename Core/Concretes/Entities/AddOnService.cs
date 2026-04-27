using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class AddOnService : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }="Per Service";
        public int HotelId { get; set; }
        public bool IsActive { get; set; }=true;
        public virtual ICollection<ReservationAddOnService> ReservationAddOnServices { get; set; } = [];
    }
}