using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using CrudApi.Application.Interfaces;
using CrudApi.Application.Services;
using CrudApi.Application.Mappings;

namespace CrudApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddValidatorsFromAssemblyContaining<MappingProfile>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
