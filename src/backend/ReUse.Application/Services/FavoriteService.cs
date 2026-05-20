using AutoMapper;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Products;
using ReUse.Application.DTOs.Products.Responses;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Services;
using ReUse.Domain.Entities;

namespace ReUse.Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FavoriteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task AddToFavoritesAsync(Guid userId, Guid productId)
    {
        var productExists = await _unitOfWork.Product.GetByIdAsync(productId);
        if (productExists is null)
            throw new NotFoundException("Product");

        var alreadyFavorited = await _unitOfWork.Favorites.IsFavoritedAsync(userId, productId);
        if (alreadyFavorited)
            throw new ConflictException("Product is already in favorites");

        var favorite = new Favorite
        {
            UserId = userId,
            ProductId = productId
        };

        _unitOfWork.Favorites.Add(favorite);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFromFavoritesAsync(Guid userId, Guid productId)
    {
        var favorite = await _unitOfWork.Favorites.GetFavoriteAsync(userId, productId);
        if (favorite is null)
            throw new NotFoundException("Favorite not found");

        _unitOfWork.Favorites.Remove(favorite);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResult<ProductResponse>> GetUserFavoritesAsync(
        Guid userId,
        ProductFilterParams filterParams)
    {
        var pagedProducts = await _unitOfWork.Favorites
            .GetUserFavoriteProductsAsync(userId, filterParams);

        var mappedItems = _mapper.Map<List<ProductResponse>>(pagedProducts.Data);

        return new PagedResult<ProductResponse>
        {
            Data = mappedItems,
            PageNumber = pagedProducts.PageNumber,
            PageSize = pagedProducts.PageSize,
            TotalRecords = pagedProducts.TotalRecords
        };
    }
}