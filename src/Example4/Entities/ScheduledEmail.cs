namespace Example4.Entities;

// Scheduled email example for the IInvocable
public class ScheduledEmail
{
    public int Id { get; set; }
    public string To { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime NextRunUtc { get; set; }
    public TimeSpan RepeatInterval { get; set; } // e.g. TimeSpan.FromHours(24)
    public bool Enabled { get; set; } = true;
}