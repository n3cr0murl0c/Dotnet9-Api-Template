using apiBase.Database;
using Microsoft.EntityFrameworkCore;

namespace apiBase.Extensions
{
    public static class EntityFrameworkMiddleware
    {
        public static IServiceCollection AddEntityFramework(
            this IServiceCollection services,
            IWebHostEnvironment environment,
            IConfiguration configuration
        )
        {
            var dbPath = "";
            if (environment.IsDevelopment())
            {
                var authSettings = configuration.GetSection("Authentication");
                dbPath = Path.Join(environment.ContentRootPath, "Database", "apiBase_Database.db");
            }

            services.AddDbContext<apiBaseDBContext>(options =>
            {
                // if (environment.IsDevelopment())
                // {
                //     options.UseSqlite($"Data Source={dbPath}");
                // }
                // else
                // {
                options.UseSqlServer(
                    configuration.GetConnectionString("SQL_DEFAULT")
                        ?? throw new ArgumentNullException(
                            "SQL_DEFAULT connection strings are missing"
                        )
                );
                // }
                // Enable OpenIddict support
                // options.UseOpenIddict();
            });

            return services;
        }
    }
}
