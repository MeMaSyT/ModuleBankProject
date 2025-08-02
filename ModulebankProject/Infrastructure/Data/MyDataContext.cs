using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Transactions;

namespace ModulebankProject.Infrastructure.Data
{
    public class MyDataContext : IMyDataContext
    {
        public List<Account> Accounts { get; set; } = [];
        public List<Transaction> Transactions { get; set; } = [];
    }
}
