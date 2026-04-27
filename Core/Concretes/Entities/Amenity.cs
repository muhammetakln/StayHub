using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class Amenity : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int HotelId { get; set; }
        public virtual Hotel? Hotel { get; set; }

    }
}