namespace Example4.Entities;

public class DeadLetter
{
    public int Id { get; set; }
    public string Payload { get; set; } = ""; // serialized EmailRequest
    public int Attempts { get; set; }
    public string LastError { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}