using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Application.Enums;

namespace ReUse.Application.DTOs.Dashboard;

public record DashboardQuery
{
    public DashboardPeriod Period { get; init; } = DashboardPeriod.Today;
}