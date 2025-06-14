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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
