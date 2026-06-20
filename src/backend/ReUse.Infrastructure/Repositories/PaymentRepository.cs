
using Microsoft.EntityFrameworkCore;

using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces.Repository;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;
using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    private readonly ApplicationDbContext _context;
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByTransactionId(string transactionId)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        return payment;
    }

    public async Task<decimal> SumByStatusAsync(PaymentStatus status, DateTime? from, DateTime? to)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Where(p => p.Status == status);

        if (from.HasValue)
            query = query.Where(p => p.PaymentDate >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.PaymentDate <= to.Value);

        return await query.SumAsync(p => p.Amount);
    }
}