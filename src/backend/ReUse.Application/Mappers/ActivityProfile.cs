using AutoMapper;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Activity;
using ReUse.Domain.Entities;

namespace ReUse.Application.Mappers;

public class ActivityProfile : Profile
{
    public ActivityProfile()
    {
        CreateMap<ActivityEvent, ActivityEventDto>()
            .ForMember(dest => dest.Product,
                opt => opt.MapFrom(src => src.Product));

        CreateMap<Product, ProductBriefDto>()
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.ProductType.ToString()))
            .ForMember(dest => dest.Condition,
                opt => opt.MapFrom(src => src.Condition != null ? src.Condition.ToString() : null))
            .ForMember(dest => dest.CoverImageUrl,
                opt => opt.MapFrom(src =>
                    src.ProductImages
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => i.Url)
                        .FirstOrDefault() ?? string.Empty))
            .ForMember(dest => dest.SellerName,
                opt => opt.MapFrom(src => src.Owner != null ? src.Owner.FullName : string.Empty));

        CreateMap<CreateActivityRequest, ActivityEvent>()
            .ForMember(dest => dest.Timestamp,
                opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}