using Microsoft.AspNetCore.ResponseCompression;

namespace apiBase.Extensions
{
    public static class ResponseCompressionService
    {
        /// <summary>
        /// Configures the application to use custom response compression.
        /// </summary>
        /// <param name="services">The collection of services to add the response compression to.</param>
        /// <returns>The collection of services with response compression configured.</returns>
        /// <remarks>
        /// This method adds Brotli and Gzip compression providers, specifies additional MIME types
        /// for compression, and enables compression for HTTPS requests.
        /// </remarks>

        public static IServiceCollection AddCustomResponseCompression(
            this IServiceCollection services
        )
        {
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-json", "text/plain" }
                );
                options.EnableForHttps = true;
            });
            return services;
        }
    }
}
