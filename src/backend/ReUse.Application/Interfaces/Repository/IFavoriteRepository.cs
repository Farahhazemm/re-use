using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Products;
using ReUse.Domain.Entities;

namespace ReUse.Application.Interfaces.Repository;

public interface IFavoriteRepository : IBaseRepository<Favorite>
{
    Task<bool> IsFavoritedAsync(Guid userId, Guid productId);
    Task<Favorite?> GetFavoriteAsync(Guid userId, Guid productId);
    Task<PagedResult<Product>> GetUserFavoriteProductsAsync(Guid userId, ProductFilterParams filterParams);
}