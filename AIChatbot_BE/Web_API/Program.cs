using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.Implement;
using Services.Interface;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IRegisteredUser, RegisteredUserService>();
builder.Services.AddScoped<IQuestion, QuestionService>();
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
builder.Services.AddScoped<INotification, NotificationService>();

builder.Services.AddScoped<ILegalService, LegalService>();
builder.Services.AddScoped<IChatRoomService, ChatRoomService>();

builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));

builder.Services.AddDbContext<Repositories.DBContext.AichatbotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AIChatbotDB")));
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = new MongoClient(settings.ConnectionString);

    try
    {
        // Gọi thử để kiểm tra kết nối
        var databaseNames = client.ListDatabaseNames().ToList();
        Console.WriteLine("✅ Kết nối MongoDB thành công. Các database gồm: " + string.Join(", ", databaseNames));
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Kết nối MongoDB thất bại: " + ex.Message);
    }

    return client;
});
// Đăng ký như singleton để inject trực tiếp nếu cần
builder.Services.AddSingleton(sp =>
    builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()
);
builder.Services.AddSingleton<LawRepository>();

//add google login service
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
       options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
       options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
       options.CallbackPath = "/api/GoogleAuthenticate/GoogleResponse";
    });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("https://ai-chatbot-fe-web.vercel.app", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Optional, if credentials are needed
    });
});

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
        Console.WriteLine("Admin account created!");
    }
    else
    {
        Console.WriteLine("Admin account already exists!");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
