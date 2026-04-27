using Core.Abstracts.Bases;
using Core.Concretes.Enum;

namespace Core.Concretes.Entities
{
    public class Payment : BaseEntity
    {
        public string PaymentReference { get; set; } = null!;
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public RoomStatus TransactionStatus { get; set; } = RoomStatus.Pending;
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public RoomStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public virtual Reservation? Reservation { get; set; }

    }
}