using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using ReUse.Application.DTOs.Comments;
using ReUse.Domain.Entities;

namespace ReUse.Application.Mappers;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<ProductComment, CommentResponse>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.ReplyCount,
                opt => opt.MapFrom(src => src.Replies.Count(r => !r.IsDeleted)));

        CreateMap<User, CommentAuthorResponse>();
    }
}