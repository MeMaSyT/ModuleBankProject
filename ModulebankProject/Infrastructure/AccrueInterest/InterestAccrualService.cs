using Microsoft.EntityFrameworkCore;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Infrastructure.Data;

namespace ModulebankProject.Infrastructure.AccrueInterest;

public class InterestAccrualService
{
    private readonly AccrueInterestHandler _handler;
    private readonly ModulebankDataContext _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
    public InterestAccrualService(AccrueInterestHandler handler, ModulebankDataContext dbContext)
    {
        _handler = handler;
        _dbContext = dbContext;
    }
    public async Task InvokeHandler()
    {
        Console.WriteLine("Interest Accrual Now");
        var depositAccounts = await _dbContext.Accounts
            .Where(a => a.AccountType == AccountType.Deposit)
            .Select(a => a.Id)
            .ToListAsync();

        foreach (var accountId in depositAccounts)
        {
            await _handler.Handle(accountId);
        }
    }
}