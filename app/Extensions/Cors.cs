namespace apiBase.Extensions
{
    public static class CorsMiddleware
    {
        /// <summary>
        /// Adds custom CORS policies to the application.
        /// </summary>
        /// <param name="services">The collection of services to add to.</param>
        /// <param name="configuration">The application's configuration.</param>
        /// <returns>The collection of services with the added CORS policies.</returns>
        /// <remarks>
        /// Uses the configuration section "Cors:AllowedOrigins" to configure the allowed origins.
        /// </remarks>
        public static IServiceCollection AddCustomCors(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddCors(options =>
            {
                var allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>();

                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        // .WithOrigins(allowedOrigins ?? new[] { "http://localhost:3000" })
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
                options.AddPolicy(
                    "AllowAll",
                    policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
                // .AllowCredentials()
                );
            });
            return services;
        }
    }
}
