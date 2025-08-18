using ModulebankProject.Features.Transactions;

namespace ModulebankProject.Features.Inbox.Events;
// ReSharper disable UnusedMember.Global это чисто описание для API
public class MoneyCreditedEvent : EventBase
{
    public Guid AccountId { get; set; }
    public TransactionType EventType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
    public Guid OperationId { get; set; }
}