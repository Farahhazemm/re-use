using AutoMapper;

using ReUse.Application.DTOs.Reports;
using ReUse.Domain.Entities;

namespace ReUse.Application.Mappers;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<User, ReportUserResponse>();

        CreateMap<Report, ReportResponse>()
            .ForMember(dest => dest.Reporter, opt => opt.MapFrom(src => src.Reporter));

        CreateMap<Report, ReportDetailsResponse>()
            .ForMember(dest => dest.Reporter, opt => opt.MapFrom(src => src.Reporter))
            .ForMember(dest => dest.ReviewedBy, opt => opt.MapFrom(src => src.ReviewedBy));

        CreateMap<Report, AdminReportListResponse>()
            .ForMember(dest => dest.Reporter, opt => opt.MapFrom(src => src.Reporter))
            .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.ReporterName))
            .ForMember(dest => dest.ReporterEmail, opt => opt.MapFrom(src => src.ReporterEmail))
            .ForMember(dest => dest.ReviewedBy, opt => opt.MapFrom(src => src.ReviewedBy));
    }
}