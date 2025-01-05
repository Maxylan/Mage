using Microsoft.Extensions.Logging.Configuration;
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

    public static string Environment => (
        System.Environment.GetEnvironmentVariable("RECEPTION_ENVIRONMENT") ?? 
        System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
        "Development"
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

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(conf => {
            conf.SwaggerDoc(VERSION, new() {
                Title = $"{AppName} '{ApiName}' ({ApiVersion}) {VERSION}",
                Version = VERSION
            });

            if (IsDevelopment) {
                conf.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "SwaggerAnnotations.xml"), true);
            }
        });
        builder.Services.AddDbContext<MageDbContext>(opts => {
            opts.EnableDetailedErrors();
            if (Environment == "Development") {

            }
            opts.EnableSensitiveDataLogging();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
