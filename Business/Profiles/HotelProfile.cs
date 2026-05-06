using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

public class HotelProfile : Profile
{
    public HotelProfile()
    {
        // Türler aynı (TimeOnly -> TimeOnly) olduğu için .ForMember yazmaya GEREK YOKTUR.
        // AutoMapper isimler aynıysa otomatik eşleme yapar.

        CreateMap<Hotel, HotelDto>();
        CreateMap<Hotel, HotelDetailDto>();

        CreateMap<CreateHotelDto, Hotel>();
        CreateMap<UpdateHotelDto, Hotel>();

        // Diğer eşlemeler aynı kalabilir
        CreateMap<AddOnService, AddOnServiceDto>();
        CreateMap<Review, ReviewDto>();
        CreateMap<Amenity, AmenityDto>();
        CreateMap<Room, RoomDto>();
    }
}