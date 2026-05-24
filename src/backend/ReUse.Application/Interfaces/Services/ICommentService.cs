using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Comments;
using ReUse.Application.Enums;

namespace ReUse.Application.Interfaces.Services;

public interface ICommentService
{
    Task<PagedResult<CommentResponse>> GetProductCommentsAsync(Guid productId, PaginationParams pagination, SortDirection sortDirection = SortDirection.Desc);

    Task<CommentResponse> GetCommentByIdAsync(Guid productId, Guid commentId);

    Task<PagedResult<CommentResponse>> GetRepliesAsync(Guid productId, Guid commentId, PaginationParams pagination, SortDirection sortDirection = SortDirection.Asc);

    Task<CommentResponse> AddCommentAsync(Guid productId, Guid userId, CreateCommentRequest request);

    Task UpdateCommentAsync(Guid commentId, Guid requestingUserId, UpdateCommentRequest request);

    Task DeleteCommentAsync(Guid commentId, Guid requestingUserId);
}