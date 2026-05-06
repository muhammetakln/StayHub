using System.ComponentModel.DataAnnotations.Schema;
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

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public string? TransactionId { get; set; }

        public string? Notes { get; set; }

        // EF Core'a ReservationId kolonunu kullanmasını açıkça söylüyoruz
        [ForeignKey(nameof(ReservationId))]
        public virtual Reservation? Reservation { get; set; }
    }
}