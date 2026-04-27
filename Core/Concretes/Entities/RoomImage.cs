using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class RoomImage : BaseEntity
    {
        public string ImageUrl { get; set; } = null!;
        public string? ImageName { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
        public int RoomId    { get; set; }
        public bool UploadedAt { get; set; }
        public virtual Room? Room { get; set; }
    }
}