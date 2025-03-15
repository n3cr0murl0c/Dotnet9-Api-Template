using Microsoft.AspNetCore.ResponseCompression;

namespace apiBase.Extensions
{
    public static class ResponseCompressionService
    {
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
