using AutoMapper;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;

namespace Calmative.Server.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<RegisterDto, User>();

            // Portfolio mappings
            CreateMap<Portfolio, PortfolioDto>()
                .ForMember(dest => dest.TotalValue, opt => opt.Ignore()); // Calculated in DTO
            CreateMap<CreatePortfolioDto, Portfolio>();

            // Asset mappings
            CreateMap<Asset, AssetDto>()
                .ForMember(dest => dest.TotalValue, opt => opt.Ignore()) // Calculated in DTO
                .ForMember(dest => dest.ProfitLoss, opt => opt.Ignore()) // Calculated in DTO
                .ForMember(dest => dest.ProfitLossPercentage, opt => opt.Ignore()); // Calculated in DTO
            CreateMap<CreateAssetDto, Asset>()
                .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.CurrentPrice));
            CreateMap<UpdateAssetDto, Asset>();

            // PriceHistory mappings
            CreateMap<PriceHistory, PriceHistoryDto>();
            
            // CustomAssetType mappings
            CreateMap<CustomAssetType, CustomAssetTypeDto>();
            CreateMap<CreateCustomAssetTypeDto, CustomAssetType>();
            CreateMap<UpdateCustomAssetTypeDto, CustomAssetType>();
        }
    }
} 