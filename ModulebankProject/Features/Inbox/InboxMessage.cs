namespace ModulebankProject.Features.Inbox
{
    public class InboxMessage
    {
        public Guid Id { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string Handler { get; set; }
        public string? Payload { get; set; }
    }
}
