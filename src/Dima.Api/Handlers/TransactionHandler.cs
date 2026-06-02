using Dima.Api.Data;
using Dima.Core.Common.Extensions;
using Dima.Core.Enums;
using Dima.Core.Handlers;
using Dima.Core.Models;
using Dima.Core.Requests.Transactions;
using Dima.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Dima.Api.Handlers;

public class TransactionHandler(AppDbContext context) : ITransactionHandler
{
    private const int RecurringMonths = 24;

    public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
    {
        try
        {
            var category = await context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == request.UserId);

            if (category is null)
                return new Response<Transaction?>(null, 404, "Categoria não encontrada");

            var type = category.Type;
            var amount = Math.Abs(request.Amount);
            if (type == ETransactionType.Withdraw)
                amount *= -1;

            if (!request.IsRecurring || request.PaidOrReceivedAt is null)
            {
                var transaction = new Transaction
                {
                    UserId = request.UserId,
                    CategoryId = request.CategoryId,
                    CreatedAt = DateTime.Now,
                    Amount = amount,
                    PaidOrReceivedAt = request.PaidOrReceivedAt,
                    Title = request.Title,
                    Type = type
                };

                await context.Transactions.AddAsync(transaction);
                await context.SaveChangesAsync();

                return new Response<Transaction?>(transaction, 201, "Transação criada com sucesso!");
            }

            var recurrenceId = Guid.NewGuid();
            var baseDate = request.PaidOrReceivedAt.Value;
            var firstTransaction = new Transaction
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.Now,
                Amount = amount,
                PaidOrReceivedAt = baseDate,
                Title = request.Title,
                Type = type,
                RecurrenceId = recurrenceId
            };
            await context.Transactions.AddAsync(firstTransaction);

            for (var i = 1; i < RecurringMonths; i++)
            {
                await context.Transactions.AddAsync(new Transaction
                {
                    UserId = request.UserId,
                    CategoryId = request.CategoryId,
                    CreatedAt = DateTime.Now,
                    Amount = amount,
                    PaidOrReceivedAt = AddMonthsClamped(baseDate, i),
                    Title = request.Title,
                    Type = type,
                    RecurrenceId = recurrenceId
                });
            }

            await context.SaveChangesAsync();

            return new Response<Transaction?>(firstTransaction, 201,
                $"Lançamento recorrente criado: {RecurringMonths} ocorrências.");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Não foi possível criar sua transação");
        }
    }

    public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
    {
        try
        {
            var transaction = await context
                .Transactions
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Transação não encontrada");

            var category = await context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == request.UserId);

            if (category is null)
                return new Response<Transaction?>(null, 404, "Categoria não encontrada");

            var type = category.Type;
            var amount = Math.Abs(request.Amount);
            if (type == ETransactionType.Withdraw)
                amount *= -1;

            ApplyChanges(transaction, request, type, amount);

            var isRecurring = transaction.RecurrenceId.HasValue
                              && request.Scope != ERecurrenceScope.OnlyThis;

            if (isRecurring)
            {
                var recurrenceId = transaction.RecurrenceId!.Value;
                var query = context.Transactions
                    .Where(x => x.RecurrenceId == recurrenceId
                                && x.UserId == request.UserId
                                && x.Id != transaction.Id);

                if (request.Scope == ERecurrenceScope.ThisAndFuture)
                {
                    var pivotDate = transaction.PaidOrReceivedAt;
                    query = query.Where(x => x.PaidOrReceivedAt > pivotDate);
                }

                var siblings = await query.ToListAsync();
                foreach (var sibling in siblings)
                    ApplyChanges(sibling, request, type, amount, preserveDate: true);
            }

            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction);
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Não foi possível atualizar sua transação");
        }
    }

    public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
    {
        try
        {
            var transaction = await context
                .Transactions
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Transação não encontrada");

            if (transaction.RecurrenceId.HasValue && request.Scope != ERecurrenceScope.OnlyThis)
            {
                var recurrenceId = transaction.RecurrenceId.Value;
                var query = context.Transactions
                    .Where(x => x.RecurrenceId == recurrenceId && x.UserId == request.UserId);

                if (request.Scope == ERecurrenceScope.ThisAndFuture)
                {
                    var pivotDate = transaction.PaidOrReceivedAt;
                    query = query.Where(x => x.PaidOrReceivedAt >= pivotDate);
                }

                var toDelete = await query.ToListAsync();
                context.Transactions.RemoveRange(toDelete);
            }
            else
            {
                context.Transactions.Remove(transaction);
            }

            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction);
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Não foi possível remover sua transação");
        }
    }

    public async Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request)
    {
        try
        {
            var transaction = await context
                .Transactions
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            return transaction is null
                ? new Response<Transaction?>(null, 404, "Transação não encontrada")
                : new Response<Transaction?>(transaction);
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Não foi possível recuperar sua transação");
        }
    }

    public async Task<PagedResponse<List<Transaction>?>> GetByPeriodAsync(GetTransactionsByPeriodRequest request)
    {
        try
        {
            request.StartDate ??= DateTime.Now.GetFirstDay();
            request.EndDate ??= DateTime.Now.GetLastDay();
        }
        catch
        {
            return new PagedResponse<List<Transaction>?>(null, 500,
                "Não foi possível determinar a data de início ou término");
        }

        try
        {
            var query = context
                .Transactions
                .AsNoTracking()
                .Where(x =>
                    x.PaidOrReceivedAt >= request.StartDate &&
                    x.PaidOrReceivedAt <= request.EndDate &&
                    x.UserId == request.UserId)
                .OrderBy(x => x.PaidOrReceivedAt);

            var transactions = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var count = await query.CountAsync();

            return new PagedResponse<List<Transaction>?>(
                transactions,
                count,
                request.PageNumber,
                request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Transaction>?>(null, 500, "Não foi possível obter as transações");
        }
    }

    private static void ApplyChanges(
        Transaction target,
        UpdateTransactionRequest source,
        ETransactionType type,
        decimal amount,
        bool preserveDate = false)
    {
        target.CategoryId = source.CategoryId;
        target.Amount = amount;
        target.Title = source.Title;
        target.Type = type;

        if (!preserveDate)
            target.PaidOrReceivedAt = source.PaidOrReceivedAt;
    }

    private static DateTime AddMonthsClamped(DateTime baseDate, int monthsToAdd)
    {
        var target = baseDate.AddMonths(monthsToAdd);
        var daysInTargetMonth = DateTime.DaysInMonth(target.Year, target.Month);
        var day = Math.Min(baseDate.Day, daysInTargetMonth);
        return new DateTime(target.Year, target.Month, day,
            baseDate.Hour, baseDate.Minute, baseDate.Second, baseDate.Kind);
    }
}
