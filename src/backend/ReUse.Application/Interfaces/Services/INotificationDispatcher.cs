using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Entities;

namespace ReUse.Application.Interfaces.Services;

public interface INotificationDispatcher
{
    Task DispatchAsync(Notification notification);
}