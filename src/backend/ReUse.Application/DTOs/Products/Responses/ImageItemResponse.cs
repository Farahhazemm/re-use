using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Products.Responses;

public record ImageItemResponse(
    Guid Id,
    string Url,
    ProductImageType Type,
    int DisplayOrder
);