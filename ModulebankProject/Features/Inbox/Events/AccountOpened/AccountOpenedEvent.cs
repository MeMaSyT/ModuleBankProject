using ModulebankProject.Features.Accounts;

namespace ModulebankProject.Features.Inbox.Events.AccountOpened;

public class AccountOpenedEvent : EventBase
{
    /// <summary>
    /// Идентификатор счета
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Идентификатор владельца счета
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Валюта счета (ISO код)
    /// </summary>
    /// <example>USD</example>
    public string Currency { get; set; }

    /// <summary>
    /// Тип счета
    /// </summary>
    /// <example>Current</example>
    public AccountType Type { get; set; }
}