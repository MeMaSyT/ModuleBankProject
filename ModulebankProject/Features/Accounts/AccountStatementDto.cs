using ModulebankProject.Features.Transactions;

namespace ModulebankProject.Features.Accounts;

public record AccountStatementDto(
    Guid Id,
    Guid OwnerId,
    AccountType AccountType,
    string Currency,
    decimal Balance,
    decimal InterestRate,
    DateTime OpenDate,
    DateTime CloseDate,
    List<TransactionDto> Transactions);