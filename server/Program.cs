using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Services;
using Server.Services.Scheduling;

namespace Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowClient", policy =>
            {
                policy.WithOrigins("http://localhost:5173", "https://*.vercel.app")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Configure DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=app.db"));

        // Register services
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<ISchedulerOrderService, SchedulerOrderService>();

        // Configure JWT Authentication
        var jwtSecret = builder.Configuration["Jwt:Secret"] ?? 
            Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
            "YourSuperSecretKeyForJWTTokenGenerationMinimum256BitsLong";

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Server",
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Client",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Important: UseCors must be called before UseAuthentication and UseAuthorization
        app.UseCors("AllowClient");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Health check endpoint
        app.MapGet("/healthz", () => "OK");

        // Ensure database is created
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }

        app.Run();
    }
}
