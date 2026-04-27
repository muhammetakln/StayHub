using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.DTOs.Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Mappings
{
    public class HotelProfile : Profile
    {
        public HotelProfile()
        {
            CreateMap<Hotel, HotelDto>();

            // ✅ FIX: ForMember'lar eklenmiş
            CreateMap<Hotel, HotelDetailDto>()
                .ForMember(d => d.Rooms, o => o.MapFrom(s => s.Rooms ?? new List<Room>()))
                .ForMember(d => d.Amenities, o => o.MapFrom(s => s.Amenities ?? new List<Amenity>()))
                .ForMember(d => d.Reviews, o => o.MapFrom(s => s.Reviews ?? new List<Review>()))
                .ForMember(d => d.AverageRating, o => o.MapFrom(s =>
                    s.Reviews != null && s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0))
                .ForMember(d => d.ReviewCount, o => o.MapFrom(s =>
                    s.Reviews != null ? s.Reviews.Count : 0));
            CreateMap<Hotel, HotelDetailDto>()
    .ForMember(d => d.Rooms, o => o.MapFrom(s => s.Rooms ?? new List<Room>()))
    .ForMember(d => d.Amenities, o => o.MapFrom(s => s.Amenities ?? new List<Amenity>()))
    .ForMember(d => d.Reviews, o => o.MapFrom(s => s.Reviews ?? new List<Review>()))
    .ForMember(d => d.AverageRating, o => o.MapFrom(s =>
        s.Reviews != null && s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0))
    .ForMember(d => d.ReviewCount, o => o.MapFrom(s =>
        s.Reviews != null ? s.Reviews.Count : 0));
            CreateMap<Hotel, HotelFilterDto>()
                .ForMember(dest => dest.MinRating, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.Rating) ? 0 : decimal.Parse(src.Rating)))
                .ForMember(dest => dest.MaxRating, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.Rating) ? 5 : decimal.Parse(src.Rating)));

            CreateMap<CreateHotelDto, Hotel>()
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.StarRating.ToString()))
                .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region ?? ""))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website ?? ""));

            CreateMap<UpdateHotelDto, Hotel>()
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.StarRating.ToString()))
                .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region ?? ""))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website ?? ""));
        }
    }
}