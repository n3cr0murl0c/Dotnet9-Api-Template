namespace apiBase.Extensions
{
    public static class CacheMiddleware
    {
        public static IServiceCollection AddCustomCaching(this IServiceCollection services)
        {
            services.AddResponseCaching(options =>
            {
                // Custom caching configuration
                options.MaximumBodySize = 1024 * 1024; // 1MB
                options.UseCaseSensitivePaths = true;
            });

            return services;
        }
    }
}
