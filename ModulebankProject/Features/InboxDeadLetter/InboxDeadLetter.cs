// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
namespace ModulebankProject.Features.InboxDeadLetter;

public class InboxDeadLetter
{
    public Guid Id { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string Handler { get; set; } = "";
    public string Payload { get; set; } = "";
    public string Error { get; set; } = "";
}