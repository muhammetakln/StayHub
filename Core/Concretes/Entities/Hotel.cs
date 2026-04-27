using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class Hotel : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int StarRating { get; set; }
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? HotelType { get; set; }
        public string? Region { get; set; }
        public string Address { get; set; } = null!;
        public string? Rating { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Website { get; set; } 
        public TimeOnly CheckInTime { get; set; } = new(14, 0);
        public TimeOnly CheckOutTime { get; set; } = new(11, 0);
        public bool IsActive { get; set; } = true;
        public decimal MinPrice { get; set; } = 250; // ✅ EKLENDI
        public double AverageRating { get; set; } = 0.0; // ✅ EKLENDI
        public int ReviewCount { get; set; } = 0; // ✅ EKLENDI
        public string? CoverImageUrl { get; set; } // ✅ EKLENDI
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}