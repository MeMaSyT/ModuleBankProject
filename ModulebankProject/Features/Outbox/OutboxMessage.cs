using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace ModulebankProject.Features.Outbox;

public class    OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "Base";
    public string Content { get; set; } = string.Empty;
    [Column(TypeName = "jsonb")]
    public Dictionary<string, object>? Properties { get; set; }
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string Error { get; set; } = string.Empty;
}