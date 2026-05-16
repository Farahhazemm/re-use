using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Products.Responses;

public record SellerDashboardResponse
{
    public SellerSummaryResponse Summary { get; init; } = default!;
    public List<ProductResponse> Products { get; init; } = [];
}