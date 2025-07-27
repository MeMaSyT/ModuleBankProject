using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Transactions;

namespace ModulebankProject.Infrastructure.Data;

public interface IMyDataContext
{
    List<Account> Accounts { get; set; }
    List<Transaction> Transactions { get; set; }
}