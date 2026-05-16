using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Products;

public class MyListingsParams
{
    public PaginationParams Pagination { get; set; } = new();

    // active | sold | inactive | deleted
    public string? Status { get; set; }

    public ProductStatus? MappedStatus => Status?.ToLower() switch
    {
        "active" => ProductStatus.Active,
        "sold" => ProductStatus.Sold,
        "inactive" => ProductStatus.Closed,   // frontend label
        "deleted" => ProductStatus.Deleted,
        _ => null
    };
}