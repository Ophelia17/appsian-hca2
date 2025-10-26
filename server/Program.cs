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
                logger.LogInformation("Initializing database...");
                context.Database.EnsureCreated();
                logger.LogInformation("Database.EnsureCreated() completed successfully");
                
                // Check if UserFeedbacks table exists, if not create it
                var connection = context.Database.GetDbConnection();
                connection.Open();
                logger.LogInformation("Database connection opened");
                
                using var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='UserFeedbacks';";
                var result = checkCommand.ExecuteScalar();
                
                if (result == null)
                {
                    logger.LogWarning("UserFeedbacks table not found, creating it...");
                    using var createCommand = connection.CreateCommand();
                    createCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS UserFeedbacks (
                            Id TEXT NOT NULL PRIMARY KEY,
                            UserId TEXT NOT NULL,
                            Rating INTEGER NOT NULL,
                            Comment TEXT,
                            CreatedAt TEXT NOT NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        );";
                    createCommand.ExecuteNonQuery();
                    logger.LogInformation("UserFeedbacks table created successfully");
                }
                else
                {
                    logger.LogInformation("UserFeedbacks table already exists");
                }
                
                connection.Close();
                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CRITICAL: Error initializing database - {Message}", ex.Message);
                logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                // Don't throw - allow app to start even if migration fails
                // The table might already exist and the check failed for some reason
            }
        }

        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        app.Run($"http://0.0.0.0:{port}");
    }
}
