using Core.Abstracts.Bases;
using Core.Concretes.Enum;

namespace Core.Concretes.Entities
{
    public class Reservation : BaseEntity
    {
        public string ReservationNumber { get; set; } = null!;
        public int GuestId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOf { get; set; }
        public int NumberOfNights { get; set; }
        public decimal PricePerNights { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public string? SpecialRequest { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public virtual Guest? Guest { get; set; }
        public virtual Room? Room { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<ReservationAddOnService> AddOnServices { get; set; } = new List<ReservationAddOnService>();

    }
}