namespace apiBase.Extensions
{
    public static class CacheMiddleware
    {
        /// <summary>
        /// Adds response caching services to the service collection with custom configuration.
        /// </summary>
        /// <param name="services">The service collection to add the response caching services to.</param>
        /// <returns>The service collection with response caching services added.</returns>
        /// <remarks>
        /// Configures the response caching to limit the maximum body size to 1MB and
        /// to use case-sensitive paths.
        /// </remarks>

        public static IServiceCollection AddCustomCaching(this IServiceCollection services)
        {
            services.AddResponseCaching(options =>
            {
                // Custom caching configuration
                options.MaximumBodySize = 5 * 1024 * 1024; // 5MB
                options.UseCaseSensitivePaths = true;
            });

            return services;
        }
    }
}
