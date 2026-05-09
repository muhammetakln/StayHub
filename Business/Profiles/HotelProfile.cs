using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

public class HotelProfile : Profile
{
    public HotelProfile()
    {
        // Hotel -> HotelDto (Liste sayfasında da hizmetler gerekiyorsa eklendi)
        CreateMap<Hotel, HotelDto>()
            .ForMember(dest => dest.AddOnServices, opt => opt.MapFrom(src => src.AddOnServices));

        // Hotel -> HotelDetailDto (Müşteri detay sayfası için KRİTİK KISIM)
        CreateMap<Hotel, HotelDetailDto>()
            .ForMember(dest => dest.AddOnServices, opt => opt.MapFrom(src => src.AddOnServices))
            .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms))
            .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews))
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities));

        // Create ve Update işlemleri
        CreateMap<CreateHotelDto, Hotel>()
            .ForMember(dest => dest.AddOnServices, opt => opt.MapFrom(src => src.AddOnServices));

        CreateMap<UpdateHotelDto, Hotel>()
            .ForMember(dest => dest.AddOnServices, opt => opt.MapFrom(src => src.AddOnServices));

        // Alt nesnelerin DTO eşleşmeleri
        CreateMap<AddOnService, AddOnServiceDto>().ReverseMap();
        CreateMap<Review, ReviewDto>().ReverseMap();
        CreateMap<Amenity, AmenityDto>().ReverseMap();
        CreateMap<Room, RoomDto>().ReverseMap();

        // Düzenleme sayfası için gerekli olabilecek alt nesne eşleşmesi
        CreateMap<AddOnService, UpdateAddOnServiceDto>().ReverseMap();
    }
}