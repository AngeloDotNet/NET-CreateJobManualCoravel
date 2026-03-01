
using Coravel;
using Example2.Data;
using Example2.Services;
using Microsoft.EntityFrameworkCore;

namespace Example2;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Coravel queue (e scheduler se ti serve)
        builder.Services.AddQueue();       // queue processing
        builder.Services.AddScheduler();   // opzionale: scheduling cron-like

        // register MailKit sender (scoped is fine)
        builder.Services.AddScoped<IMailKitSender, MailKitSender>();

        // EF Core DbContext
        builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        var app = builder.Build();
        app.UseHttpsRedirection();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}