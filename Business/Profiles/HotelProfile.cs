using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

public class HotelProfile : Profile
{
    public HotelProfile()
    {
       
        CreateMap<Hotel, HotelDto>()
            .ForMember(dest => dest.AddOnServices, opt => opt.MapFrom(src => src.AddOnServices))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.AverageRating))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.ReviewCount));

        CreateMap<Hotel, HotelDetailDto>()
            .ForMember(dest => dest.AddOnServices, opt => opt.MapFrom(src => src.AddOnServices))
            .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms))
            .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews))
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities));

       
        CreateMap<CreateHotelDto, Hotel>()
             .ForMember(dest => dest.AddOnServices, opt => opt.MapFrom(src => src.AddOnServices))
             .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities))
             .ForMember(dest => dest.HotelPassword, opt => opt.MapFrom(src => src.HotelPassword)); // Yeni şifre alanı

        CreateMap<UpdateHotelDto, Hotel>()
            .ForMember(dest => dest.AddOnServices, opt => opt.MapFrom(src => src.AddOnServices))
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities))
            .ForMember(dest => dest.HotelPassword, opt => opt.MapFrom(src => src.HotelPassword));

       
        CreateMap<CreateAmenityDto, Amenity>().ReverseMap();

        CreateMap<AddOnService, AddOnServiceDto>().ReverseMap();
        CreateMap<Review, ReviewDto>().ReverseMap();
        CreateMap<Amenity, AmenityDto>().ReverseMap();
        CreateMap<Room, RoomDto>().ReverseMap();

        CreateMap<AddOnService, UpdateAddOnServiceDto>().ReverseMap();
    }
}