using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Products;
using ReUse.Application.DTOs.Products.Responses;

namespace ReUse.Application.Interfaces.Services;

public interface IFavoriteService
{
    Task AddToFavoritesAsync(Guid userId, Guid productId);
    Task RemoveFromFavoritesAsync(Guid userId, Guid productId);
    Task<PagedResult<ProductResponse>> GetUserFavoritesAsync(Guid userId, ProductFilterParams filterParams);
}