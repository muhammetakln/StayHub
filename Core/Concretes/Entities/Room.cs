using Core.Abstracts.Bases;
using Core.Concretes.Enum;

namespace Core.Concretes.Entities
{
    public class Room : BaseEntity
    {
        public string RoomNumber { get; set; } = null!;
        public string? Name { get; set; }
        public RoomType Type { get; set; }
        public int Size { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Available;
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public int FloorNumber { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int HotelId { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual Hotel? Hotel { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();

    }
}