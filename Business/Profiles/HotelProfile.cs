using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

public class HotelProfile : Profile
{
    public HotelProfile()
    {
        

        CreateMap<Hotel, HotelDto>();
        CreateMap<Hotel, HotelDetailDto>();

        CreateMap<CreateHotelDto, Hotel>();
        CreateMap<UpdateHotelDto, Hotel>();

        CreateMap<AddOnService, AddOnServiceDto>();
        CreateMap<Review, ReviewDto>();
        CreateMap<Amenity, AmenityDto>();
        CreateMap<Room, RoomDto>();
    }
}