using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using ReUse.Domain.Enums;

namespace ReUse.Application.Mappers;

public class ConditionToStringConverter
    : ITypeConverter<ProductCondition?, string>
{
    public string Convert(
        ProductCondition? source,
        string destination,
        ResolutionContext context)
    {
        return source switch
        {
            ProductCondition.New => "New",
            ProductCondition.LikeNew => "Like New",
            ProductCondition.Used => "Used",
            ProductCondition.Broken => "Broken",
            _ => source?.ToString() ?? "Unknown"
        };
    }
}