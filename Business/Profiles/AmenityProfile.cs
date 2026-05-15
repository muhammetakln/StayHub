using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Mappings
{
    public class AmenityProfile : Profile
    {
        public AmenityProfile()
        {
            CreateMap<Amenity,AmenityDto>();
            CreateMap<CreateAmenityDto,Amenity>();
            CreateMap<UpdateAmenityDto,Amenity>();
        }
    }
}