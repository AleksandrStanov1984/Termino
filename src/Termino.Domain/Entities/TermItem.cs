using Termino.Domain.Enums;
namespace Termino.Domain.Entities;
public class TermItem{
    public Guid Id {get;set;} = Guid.NewGuid();
    public string Title {get;set;} = string.Empty;
    public string? Description {get;set;}
    public DateTimeOffset DueAt {get;set;}
    public TermStatus Status {get;set;} = TermStatus.Pending;
    public DateTimeOffset CreatedAt {get;set;} = DateTimeOffset.Now;
    public DateTimeOffset? UpdatedAt {get;set;}
    public DateTimeOffset? CompletedAt {get;set;}
    public DateTimeOffset? LastReminderSentAt {get;set;}
    public DateTimeOffset? Reminder24hSentAt {get;set;}
    public DateTimeOffset? Reminder12hSentAt {get;set;}
    public DateTimeOffset? Reminder1hSentAt {get;set;}
}