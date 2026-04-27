using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class Review : BaseEntity
    {
        public int GuestId { get; set; }
        public int HotelId { get; set; }
        public int? RoomId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public int? CleanlinessRating { get; set; }
        public int? ComfortRating { get; set; }
        public int? ServiceRating { get; set; }
        public int? ValueRating { get; set; }
        public int HelpfulCount { get; set; } = 0;
        public int UnhelpfulCount { get; set; } = 0;
        public bool IsReplied { get; set; } = true;
        public string? OwnerReply { get; set; }
        public DateTime? OwnerReplyDate { get; set; }
        public bool IsPublished { get; set; }
        public virtual Guest? Guest { get; set; }
        public virtual Hotel? Hotel { get; set; }
        public virtual Room? Room { get; set; }
    }
}