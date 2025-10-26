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
                policy.SetIsOriginAllowed(origin =>
                {
                    // Allow localhost for development
                    if (origin.StartsWith("http://localhost:5173"))
                        return true;
                    
                    // Allow production Vercel domain
                    if (origin == "https://appsian-hca2.vercel.app")
                        return true;
                    
                    // Allow Vercel preview deployments
                    if (origin.StartsWith("https://appsian-hca2-") && origin.EndsWith(".vercel.app"))
                        return true;
                    
                    return false;
                })
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

        // Ensure database is created and schema is up to date
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            try
            {
                context.Database.EnsureCreated();
                
                // Check if UserFeedbacks table exists, if not create it
                var connection = context.Database.GetDbConnection();
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='UserFeedbacks';";
                var result = command.ExecuteScalar();
                
                if (result == null)
                {
                    logger.LogInformation("UserFeedbacks table not found, creating it...");
                    command.CommandText = @"
                        CREATE TABLE UserFeedbacks (
                            Id TEXT NOT NULL PRIMARY KEY,
                            UserId TEXT NOT NULL,
                            Rating INTEGER NOT NULL,
                            Comment TEXT,
                            CreatedAt TEXT NOT NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        );";
                    command.ExecuteNonQuery();
                    logger.LogInformation("UserFeedbacks table created successfully");
                }
                
                connection.Close();
                logger.LogInformation("Database schema verified");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error ensuring database");
            }
        }

        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        app.Run($"http://0.0.0.0:{port}");
    }
}
