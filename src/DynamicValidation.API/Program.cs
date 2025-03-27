using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DynamicValidation.Domain.Repositories;
using DynamicValidation.Domain.Services;
using DynamicValidation.Infrastructure.Data;
using DynamicValidation.Application.Commands;
using System.Reflection;
using System.Text.Json;

namespace DynamicValidation.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Kestrel configuration to fix timeout issues
        builder.WebHost.ConfigureKestrel(options =>
        {
            // Disable minimum data rate to prevent timeout issues
            options.Limits.MinRequestBodyDataRate = null;
            
            // Optionally increase other timeout limits
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
        });

        // Add services to the container.
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        ConfigureMiddleware(app, app.Environment);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        services.AddScoped<IDynamicModelRepository, DynamicModelRepository>();

        // Register domain services
        services.AddScoped<IValidationService, ValidationService>();

        // Add MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateModelCommand).Assembly);
        });

        // Add controllers
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.DictionaryKeyPolicy = null;
                
                // Add options to improve JSON deserialization
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        // Add problem details for better error responses
        services.AddProblemDetails();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DynamicValidationPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()  // Geliştirme aşamasında tüm origin'lere izin ver
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Add API explorer and Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Dinamik Model Validasyon API",
                Version = "v1",
                Description = "Dinamik olarak tanımlanan modeller ve validasyon kuralları için API"
            });
        });
    }

    private static void ConfigureMiddleware(WebApplication app, IHostEnvironment env)
    {
        // Development specific middleware
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dinamik Model Validasyon API v1"));
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // Production specific middleware
            app.UseExceptionHandler();
            app.UseHsts();
        }

        app.UseStaticFiles();

        app.UseRouting();

        // CORS middleware'i routing ve authorization arasında
        app.UseCors("DynamicValidationPolicy");


        app.MapControllers();

        // Ensure the database is created (optional)
        // This can alternatively be done with a migration in a production scenario
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }
    }
}