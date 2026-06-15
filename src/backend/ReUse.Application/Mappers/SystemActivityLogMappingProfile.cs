using AutoMapper;

using ReUse.Application.DTOs.SystemActivityLog;
using ReUse.Domain.Entities;

namespace ReUse.Application.Mappers;

public class SystemActivityLogMappingProfile : Profile
{
    public SystemActivityLogMappingProfile()
    {
        CreateMap<CreateSystemActivityLogRequest, SystemActivityLog>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.ActorUser, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());


        CreateMap<SystemActivityLog, SystemActivityLogResponse>()
            .ForMember(dest => dest.ActorName,
                opt => opt.MapFrom(src => src.ActorName))
            .ForMember(dest => dest.ActorEmail,
                opt => opt.MapFrom(src => src.ActorEmail));
    }
}