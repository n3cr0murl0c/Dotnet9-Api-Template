using Microsoft.OpenApi.Models;

namespace apiBase.Extensions
{
    public static class SwaggerMiddleware
    {
        /// <summary>
        /// Adds Swagger services to the service collection.
        /// </summary>
        /// <remarks>
        /// This extension method adds Swagger services to the service collection.
        /// It adds the SwaggerGen service with a custom schema ID generator
        /// that uses the full name of the type as the schema ID. It also
        /// adds the SwaggerUI service with a custom security definition
        /// that uses the Bearer scheme for authorization. Finally, it adds
        /// the EndpointsApiExplorer service as a dependency of the Swagger
        /// services.
        /// </remarks>
        /// <param name="services">The service collection to add the Swagger
        /// services to.</param>
        /// <returns>The service collection with the Swagger services added.</returns>
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
