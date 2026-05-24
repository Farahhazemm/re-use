using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Comments;

public record CommentAuthorResponse
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
}