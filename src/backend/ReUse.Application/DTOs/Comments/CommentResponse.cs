using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Comments;

public record CommentResponse
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public Guid? ParentCommentId { get; init; }
    public string Body { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsEdited => UpdatedAt.HasValue;
    public int ReplyCount { get; init; }
    public CommentAuthorResponse Author { get; init; } = default!;
}