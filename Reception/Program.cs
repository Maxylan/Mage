using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Reception.Authentication;
using Reception.Interfaces;
using Reception.Services;
using Reception.Caching;
using SixLabors.ImageSharp;
using Reception.Utilities;

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
        builder.Services
            .AddControllers()
            .AddJsonOptions(opts =>
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
            );

        builder.Services
            .AddAuthentication(conf => {
                conf.DefaultAuthenticateScheme = Parameters.SCHEME;
                // conf.DefaultScheme = Parameters.SCHEME;
            })
            .AddScheme<AuthenticationSchemeOptions, MageAuthentication>(
                Parameters.SCHEME,
                opts => { opts.Validate(); }
            );

        builder.Services.AddAuthorizationBuilder()
            .AddDefaultPolicy(Parameters.AUTHENTICATED_POLICY, policy => policy.RequireAuthenticatedUser());

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(conf => {
            conf.SwaggerDoc(VERSION, new() {
                Title = $"{AppName} '{ApiName}' ({ApiVersion}) {VERSION}",
                Description = $"{AppName} Backend Server (ASP.NET 8.0, '{ApiPathBase}'). {VERSION}",
                Version = VERSION
            });

            OpenApiSecurityScheme scheme =  new() {
                Description = "Custom Bearer Authentication Header.",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Scheme = "Authorization",
                Name = "x-mage-token",
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = Parameters.SCHEME
                }
            };

            OpenApiSecurityRequirement requirement = new() {
                [scheme] = []
            };

            conf.AddSecurityDefinition(Parameters.SCHEME, scheme);
            conf.AddSecurityRequirement(requirement);

            conf.AddServer(new OpenApiServer() {
                Url = ApiPathBase
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

        builder.Services.AddSingleton<LoginTracker>();

        builder.Services.AddScoped<ILoggingService, LoggingService>();
        builder.Services.AddScoped<ISessionService, SessionService>();
        builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IPhotoService, PhotoService>();

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

        app.UseForwardedHeaders();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
