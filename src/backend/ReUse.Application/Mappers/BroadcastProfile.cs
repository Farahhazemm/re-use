using AutoMapper;

using ReUse.Application.DTOs.Broadcast;
using ReUse.Domain.Entities;

namespace ReUse.Application.Mappers;

public class BroadcastProfile : Profile
{
    public BroadcastProfile()
    {
        CreateMap<BroadcastMessage, BroadcastResponse>()
            .ForMember(dest => dest.TargetAudience, opt => opt.MapFrom(src => src.TargetAudience.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.FullName));
    }
}