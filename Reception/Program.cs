using Microsoft.Extensions.Logging.Configuration;
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
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(conf => {
            conf.AddServer(new Microsoft.OpenApi.Models.OpenApiServer() {
                Description = $"{AppName} Backend Server (ASP.NET 8.0, '{ApiPathBase}'). {VERSION}",
                Url = ApiPathBase + "/swagger"
            });

            conf.SwaggerDoc(VERSION, new() {
                Title = $"{AppName} '{ApiName}' ({ApiVersion}) {VERSION}",
                Version = VERSION
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

        app.UsePathBase(ApiPathBase);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || IsDevelopment)
        {
            app.UseSwagger(/* opts => {
                opts.RouteTemplate = "{documentName}" + ApiPathBase + "/swagger/v1/swagger.json";
            } */);
            
            app.UseSwaggerUI(opts => {
                opts.EnableFilter();
                opts.EnablePersistAuthorization();
                opts.EnableTryItOutByDefault();
                opts.DisplayRequestDuration();

                // opts.SwaggerEndpoint(ApiPathBase + "/swagger/v1/swagger.json", ApiName);
            });
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
