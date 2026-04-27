using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Mappings
{
    public class HotelProfile : Profile
    {
        public HotelProfile()
        {
            CreateMap<Hotel, HotelDto>();

            CreateMap<Hotel, HotelDetailDto>();

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