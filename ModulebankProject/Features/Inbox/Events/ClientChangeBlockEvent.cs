namespace ModulebankProject.Features.Inbox.Events;

public class ClientChangeBlockEvent : EventBase
{
    public Guid ClientId { get; set; }
}