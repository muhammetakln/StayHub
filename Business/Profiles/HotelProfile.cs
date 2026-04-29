using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class HotelProfile : Profile
    {
        public HotelProfile()
        {
            CreateMap<Hotel, HotelDto>();

            CreateMap<Hotel, HotelDetailDto>()
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.AverageRating.ToString("F1")));

            CreateMap<CreateHotelDto, Hotel>();
            CreateMap<UpdateHotelDto, Hotel>();

            CreateMap<Review, ReviewDto>();
            CreateMap<Amenity, AmenityDto>();
            CreateMap<Room, RoomDto>();
        }
    }
}