using ModulebankProject.Features.Transactions;

namespace ModulebankProject.Features.Inbox.Events;

public class MoneyDebitedEvent : EventBase
{
    public Guid AccountId { get; set; }
    public TransactionType EventType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public Guid OperationId { get; set; }

}