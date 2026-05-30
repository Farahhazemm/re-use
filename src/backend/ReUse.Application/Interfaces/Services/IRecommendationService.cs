using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Products.Responses;
using ReUse.Application.DTOs.Recommendations;

namespace ReUse.Application.Interfaces.Services;

public interface IRecommendationService
{
    Task<PagedResult<ProductResponse>> GetPersonalisedFeedAsync(Guid? userId, PaginationParams @params);

    Task<IReadOnlyList<ProductResponse>> GetSimilarProductsAsync(Guid productId, Guid? userId, int count = 8);
}