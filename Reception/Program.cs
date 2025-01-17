using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.OpenApi.Models;
using Reception.Authentication;
using Reception.Interfaces;
using Reception.Services;

namespace Reception;

public sealed class Program
{
    public const string DEVELOPMENT_FLAG = "Development";
    public const string VERSION = "v1";

    public static string? AppName => System.Environment.GetEnvironmentVariable("APP_NAME");
    public static string? AppVersion => System.Environment.GetEnvironmentVariable("APP_VERSION");
    public static string? ApiName => System.Environment.GetEnvironmentVariable("RECEPTION_NAME");
    public static string? ApiVersion => System.Environment.GetEnvironmentVariable("RECEPTION_VERSION");
    public static string? ApiPathBase => System.Environment.GetEnvironmentVariable("RECEPTION_BASE_PATH");
    public static string? ApiInternalUrl => System.Environment.GetEnvironmentVariable("RECEPTION_URL");

    public static string Environment => (
        System.Environment.GetEnvironmentVariable("RECEPTION_ENVIRONMENT") ?? 
        System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
        DEVELOPMENT_FLAG
    );

    public static bool IsProduction => !IsDevelopment;
    public static bool IsDevelopment => (
        Environment == DEVELOPMENT_FLAG
    );

    private Program() { }

    public static void Main(string[] args)
    {
        // Swagger/OpenAPI reference & tutorial, if ever needed:
        // https://aka.ms/aspnetcore/swashbuckle
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Add services to the container.

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllers();

        builder.Services.AddAuthentication(MageAuthentication.SESSION_TOKEN_HEADER)
            .AddScheme<AuthenticationSchemeOptions, MageAuthentication>(MageAuthentication.SESSION_TOKEN_HEADER, opts => { });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(conf => {
            conf.SwaggerDoc(VERSION, new() {
                Title = $"{AppName} '{ApiName}' ({ApiVersion}) {VERSION}",
                Description = $"{AppName} Backend Server (ASP.NET 8.0, '{ApiPathBase}'). {VERSION}",
                Version = VERSION
            });

            conf.AddServer(new () {
                Url = ApiPathBase
            });

            conf.AddSecurityDefinition(VERSION, new() { // OpenApiSecurityScheme
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "x-mage-token",
            });

            /* if (IsDevelopment) {
                conf.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "SwaggerAnnotations.xml"), true);
            } */
        });
        builder.Services.AddDbContext<MageDbContext>(opts => {
            if (IsDevelopment) {
                opts.EnableSensitiveDataLogging();
            }

            opts.EnableDetailedErrors();
        });

        builder.Services.AddScoped<ILoggingService, LoggingService>();
        builder.Services.AddScoped<ISessionService, SessionService>();
        builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || IsDevelopment)
        {
            if (string.IsNullOrWhiteSpace(ApiPathBase)) {
                Console.WriteLine($"Won't initialize with Swagger; {nameof(ApiPathBase)} is null/empty.");
            }
            else {
                app.UseSwagger();
                app.UseSwaggerUI(opts => {
                    opts.EnableFilter();
                    opts.EnablePersistAuthorization();
                    opts.EnableTryItOutByDefault();
                    opts.DisplayRequestDuration();

                    opts.SwaggerEndpoint(ApiPathBase + "/swagger/v1/swagger.json", ApiName);
                    // opts.RoutePrefix = ApiPathBase[1..];
                    Console.WriteLine("Swagger Path: " + opts.RoutePrefix);
                });
            }
        }

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
