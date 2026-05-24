using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Comments;
using ReUse.Application.Enums;
using ReUse.Domain.Entities;

namespace ReUse.Application.Interfaces.Repository;

public interface ICommentRepository : IBaseRepository<ProductComment>
{
    Task<PagedResult<CommentResponse>> GetByProductIdAsync(Guid productId, PaginationParams pagination, SortDirection sortDirection = SortDirection.Desc);

    Task<CommentResponse?> GetByIdWithAuthorAsync(Guid commentId);

    Task<PagedResult<CommentResponse>> GetRepliesAsync(Guid parentCommentId, PaginationParams pagination, SortDirection sortDirection = SortDirection.Asc);

    Task<ProductComment?> GetCommentWithAuthorAsync(Guid commentId);

    Task<int> CountRecentCommentsByUserOnProductAsync(Guid userId, Guid productId, DateTime since);
}