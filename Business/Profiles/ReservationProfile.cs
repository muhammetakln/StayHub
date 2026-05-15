using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Mappings
{
    public class ReservationProfile : Profile
    {
        public ReservationProfile()
        {
            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Room.Hotel.Name))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room.RoomNumber))
                .ForMember(dest => dest.SelectedServices, opt => opt.MapFrom(src => src.SelectedServices));

            CreateMap<CreateReservationDto, Reservation>();

           
            CreateMap<ReservationAddOnService, AddOnServiceDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AddOnServiceId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.AddOnService.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));
        }
    }
}