using Example1.HostedServices;
using Example1.Services;
using Example1.Services.Interfaces;

namespace Example1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<EmailQueue>(); // o AddSingleton<IEmailQueue, EmailQueue>()
        builder.Services.AddSingleton<IEmailQueue>(sp => sp.GetRequiredService<EmailQueue>());
        builder.Services.AddHostedService<EmailSenderHostedService>();

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
