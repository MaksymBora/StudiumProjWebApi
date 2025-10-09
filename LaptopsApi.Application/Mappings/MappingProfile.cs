using AutoMapper;
using LopTopWebApi.Domain.Entities;
using LaptopsApi.Application.Common.DTOs;

namespace LaptopsApi.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>();
        }
    }
}