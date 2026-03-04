
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Example4.Data;
using Example4.HostedServices;
using Example4.Services;
using Example4.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Example4;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // EF Core
        builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // MailKit sender
        builder.Services.AddScoped<IMailKitSender, MailKitSender>();

        // Coravel: queue + scheduler
        builder.Services.AddQueue();
        builder.Services.AddScheduler();

        // Register any invocables you schedule
        builder.Services.AddScoped<SendScheduledEmailsInvocable>();

        // Channel-based queue + HostedService (for the Channel example)
        builder.Services.AddSingleton<EmailQueue>();
        builder.Services.AddSingleton<IEmailQueue>(sp => sp.GetRequiredService<EmailQueue>());
        builder.Services.AddHostedService<EmailSenderHostedService>();

        var app = builder.Build();
        app.UseHttpsRedirection();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();

        // Get IScheduler and schedule the recurring invocation.
        // Note: The Schedule<T>().EveryMinute() syntax refers to Coravel's fluent API.
        // If your version differs, adapt this code to your Coravel package.
        using (var scope = app.Services.CreateScope())
        {
            var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();

            // Esegue SendScheduledEmailsInvocable ogni minuto
            scheduler.Schedule<SendScheduledEmailsInvocable>().EveryMinute();
        }

        app.MapControllers();
        app.Run();
    }
}