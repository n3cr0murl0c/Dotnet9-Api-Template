using apiBase.Database;
using Microsoft.EntityFrameworkCore;

namespace apiBase.Extensions
{
    public static class EntityFrameworkMiddleware
    {
        /// <summary>
        /// Configures the Entity Framework database context for the application.
        /// </summary>
        /// <param name="services">The service collection to add the DbContext to.</param>
        /// <param name="environment">The web hosting environment used to determine the configuration settings.</param>
        /// <param name="configuration">The application configuration to retrieve connection strings and settings from.</param>
        /// <returns>The updated service collection with the DbContext configured.</returns>
        /// <remarks>
        /// In development environments, this method sets up the DbContext to use SQLite with a database file
        /// located at the root path of the application. In other environments, it configures the DbContext to
        /// use SQL Server with the connection string specified in the application configuration.
        /// </remarks>

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
