using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace Business.Mappings
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            // Register
            CreateMap<RegisterDto, Guest>();

            // Guest → DTOs
            CreateMap<Guest, AuthDto>();
            CreateMap<Guest, RegisterResponseDto>();
            CreateMap<Guest, LoginResponseDto>();
            CreateMap<Guest, GuestDto>();
            CreateMap<Guest, GuestListDto>();
            CreateMap<Guest, GuestProfileDto>();

            // DTOs → Guest
            CreateMap<GuestDto, Guest>();
            CreateMap<CreateGuestDto, Guest>();
            CreateMap<UpdateGuestDto, Guest>();
        }
        // neden guset den auth dto aldık araştır.
    }
}