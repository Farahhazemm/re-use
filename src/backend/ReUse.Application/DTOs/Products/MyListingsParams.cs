using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Products;

public class MyListingsParams : ProductFilterParams
{
    // active | sold | Closed | deleted
    public ProductStatus? Status { get; set; }

}