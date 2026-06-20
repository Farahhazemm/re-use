using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Application.Interfaces.Repository
{

    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<Payment?> GetByTransactionId(string transactionId);
        Task<decimal> SumByStatusAsync(PaymentStatus status, DateTime? from, DateTime? to);

    }
}