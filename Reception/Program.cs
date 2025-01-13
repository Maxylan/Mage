using Microsoft.Extensions.Logging.Configuration;
using Microsoft.OpenApi.Models;
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
            conf.SwaggerDoc(VERSION, new() {
                Title = $"{AppName} '{ApiName}' ({ApiVersion}) {VERSION}",
                Description = $"{AppName} Backend Server (ASP.NET 8.0, '{ApiPathBase}'). {VERSION}",
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

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || IsDevelopment)
        {
            if (string.IsNullOrWhiteSpace(ApiPathBase)) {
                Console.WriteLine($"Won't initialize with Swagger; {nameof(ApiPathBase)} is null/empty.");
            }
            else {
                app.UseSwagger(/* opts =>{
                    opts.RouteTemplate = ApiPathBase + "/swagger/{documentName}/swagger.json";
                    Console.WriteLine("Swagger Route Template: " + opts.RouteTemplate);
                } */);
                app.MapSwagger(ApiPathBase + "/swagger/{documentName}/swagger.json", opts => {
                    opts.PreSerializeFilters.Add((swaggerDoc, request) => {
                        var serverUrl = $"{request.Scheme}://{request.Host}{ApiPathBase}";
                        swaggerDoc.Servers.Add(new OpenApiServer() { Url = serverUrl });

                        swaggerDoc.Paths = new OpenApiPaths(swaggerDoc.Paths.Select(p => new KeyValuePair<string, OpenApiPathItem>(p.Key, p.Value)).ToDictionary(), swaggerDoc.Paths.Extensions);
                        /* OpenApiPaths newPaths = new(,);

                        for(int i = 0; i < swaggerDoc.Paths.Keys.Count; i++) {
                            swaggerDoc.Paths.Key[i] = swaggerDoc.Paths.Keys[i];
                        }

                        foreach(var path in swaggerDoc.Paths) {
                            path.Key = ApiPathBase + path.Key;
                        } */
                    });
                });
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
