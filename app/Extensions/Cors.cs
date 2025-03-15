namespace apiBase.Extensions
{
    public static class CorsMiddleware
    {
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
