using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Mappings
{
    public class RoomProfile : Profile
    {
        public RoomProfile()
        {
            CreateMap<RoomImage, RoomImageDto>().ReverseMap();

            CreateMap<Room, RoomDto>()
                .ForMember(dest => dest.RoomImage, opt => opt.MapFrom(src => src.RoomImage.Where(i => !i.IsDeleted)))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.RoomImage.FirstOrDefault(x => x.IsPrimary && !x.IsDeleted).ImageUrl))
                .ReverseMap();

            // Create Mapping
            CreateMap<CreateRoomDto, Room>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Update Mapping
            CreateMap<UpdateRoomDto, Room>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }
    }
}