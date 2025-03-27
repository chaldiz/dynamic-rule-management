using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DynamicValidation.Domain.Repositories;
using DynamicValidation.Domain.Services;
using DynamicValidation.Infrastructure.Data;
using MediatR;
using System.Reflection;
using DynamicValidation.Application.Commands;

namespace DynamicValidation.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamicValidationServices(this IServiceCollection services, IConfiguration configuration)
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
            services.AddMediatR(config => config.RegisterServicesFromAssemblies(
                typeof(CreateModelCommand).GetTypeInfo().Assembly));
            
            return services;
        }
    }
}