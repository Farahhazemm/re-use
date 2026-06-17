using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Broadcast;

public class BroadcastFilterParams : PaginationParams
{
    public BroadcastStatus? Status { get; set; }
}