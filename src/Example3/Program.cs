
using Example3.Data;
using Example3.HostedServices;
using Example3.Services;
using Example3.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Example3;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Queue & HostedService
        builder.Services.AddSingleton<EmailQueue>();
        builder.Services.AddSingleton<IEmailQueue>(sp => sp.GetRequiredService<EmailQueue>());
        builder.Services.AddHostedService<EmailSenderHostedService>();

        // MailKit sender
        builder.Services.AddScoped<IMailKitSender, MailKitSender>();

        var app = builder.Build();
        app.UseHttpsRedirection();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}
