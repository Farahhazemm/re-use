using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Entities;

namespace ReUse.Domain.Events;

public record NotificationCreatedDomainEvent(Notification Notification) : IDomainEvent;