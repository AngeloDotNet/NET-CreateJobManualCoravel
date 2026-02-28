using Example1.Models;

namespace Example1.Services.Interfaces;

public interface IEmailQueue
{
    ValueTask EnqueueAsync(EmailRequest request);
}