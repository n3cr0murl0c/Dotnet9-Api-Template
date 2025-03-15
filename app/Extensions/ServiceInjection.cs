using apiBase.Interfaces;
using apiBase.Services;

namespace apiBase.Extensions
{
    public static class ServiceInjectionMiddleware
    {
        public static IServiceCollection AddCustomDependencyInjection(
            this IServiceCollection services
        )
        {
            // Scoped Services
            services.AddScoped<IOdbcService, OdbcService>();
            services.AddScoped<ISQLService, SQLService>();
            // Email service chain - register in order of dependency
            // services.AddSingleton<IEmailServiceClient, EmailServiceClient>();
            // services.AddSingleton<ICustomEmailSender<ArcaUser>, EmailServiceSender>();
            // services.AddSingleton<IEmailSender<ArcaUser>, EmailSenderAdapter>();
            return services;
        }
    }
}
