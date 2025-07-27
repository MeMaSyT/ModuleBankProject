namespace ModulebankProject.Features.Accounts
{
    public record AccountDto(
        Guid Id,
        Guid OwnerId,
        AccountType AccountType,
        string Currency,
        decimal Balance,
        decimal InterestRate,
        DateTime OpenDate,
        DateTime CloseDate
    );
}
