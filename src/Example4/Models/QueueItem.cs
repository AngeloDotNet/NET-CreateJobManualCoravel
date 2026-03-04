namespace Example4.Models;

public class QueueItem
{
    public EmailRequest Request { get; set; } = null!;
    public int Attempts { get; set; }
}