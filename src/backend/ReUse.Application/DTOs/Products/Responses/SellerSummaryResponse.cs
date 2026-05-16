using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Products.Responses;

public record SellerSummaryResponse
{
    public int TotalProducts { get; init; }
    public int ActiveCount { get; init; }
    public int SoldCount { get; init; }
}