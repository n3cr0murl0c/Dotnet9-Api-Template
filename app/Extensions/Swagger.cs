using Microsoft.OpenApi.Models;

namespace apiBase.Extensions
{
    public static class SwaggerMiddleware
    {
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            // Explicitly add Swagger services
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "apiBase", Version = "v1" });
                // Add this custom schema ID generator
                c.CustomSchemaIds(type => type.FullName);
                c.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                    }
                );
                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                            },
                            Array.Empty<string>()
                        },
                    }
                );
            });

            // Add required Swagger dependencies
            services.AddEndpointsApiExplorer();

            return services;
        }
    }
}
