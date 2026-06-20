using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.Extensions;

public static class PercentageChangeExtensions
{
    public static double? CalculatePercentageChange(this decimal previous, decimal current)
    {
        if (previous == 0)
            return current == 0 ? 0 : null;

        return (double)((current - previous) / previous * 100);
    }
}