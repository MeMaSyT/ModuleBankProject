using ModulebankProject.Features.Accounts;
using Swashbuckle.AspNetCore.Filters;

namespace ModulebankProject.Features.Inbox.Events.AccountOpened
{
    public class AccountOpenedExample : IExamplesProvider<AccountOpenedEvent>
    {
        public AccountOpenedEvent GetExamples() =>
            new AccountOpenedEvent
            {
                AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Currency = "USD",
                Type = AccountType.Checking
            };
    }
}