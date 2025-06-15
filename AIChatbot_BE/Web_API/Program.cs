using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.Implement;
using Services.Interface;
using System;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<RegisteredUserService>();
builder.Services.AddDbContext<Repositories.DBContext.AichatbotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AIChatbotDB")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercelFrontend",
        policy =>
        {
            policy.WithOrigins("https://ai-chat-bot-law.vercel.app")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var environment = app.Environment;

// Kích hoạt Swagger trong Development hoặc Production
if (environment.IsDevelopment() || environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger";
    });

    // Redirect từ root đến Swagger
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}
var port = Environment.GetEnvironmentVariable("PORT") ?? "5171";
app.Urls.Add($"http://0.0.0.0:{port}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = services.GetRequiredService<IConfiguration>();
    var dbContext = services.GetRequiredService<AichatbotDbContext>();

    // Bind từ appsettings.json
    var adminSection = config.GetSection("AdminAccount");
    var adminConfig = adminSection.Get<AdminAccountConfig>();

    // Kiểm tra xem admin đã tồn tại chưa
    if (!dbContext.RegisteredUsers.Any(u => u.UserEmail == adminConfig.Email))
    {
        var admin = new RegisteredUser
        {
            UserId = Guid.NewGuid().ToString(),
            UserEmail = adminConfig.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(adminConfig.Password),
            UserName = adminConfig.Name,
            Role = adminConfig.Role,
            UserStatus = adminConfig.Status,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.RegisteredUsers.Add(admin);
        dbContext.SaveChanges();
        Console.WriteLine("✅ Admin account created.");
    }
    else
    {
        Console.WriteLine("ℹ️ Admin account already exists.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowVercelFrontend");
app.UseAuthorization();

app.MapControllers();

app.Run();
