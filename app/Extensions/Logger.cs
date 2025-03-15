using System.Collections.ObjectModel;
using System.Data;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace apiBase.Extensions
{
    public static class LoggerMiddleware
    {
        /// <summary>
        /// Configures Serilog for logging to a file and, in production, to a PostgreSQL database.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to read settings from.</param>
        /// <param name="environment">The <see cref="IWebHostEnvironment"/> to determine the environment.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddCustomLogging(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment
        )
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "logs", $"log-apiBase.txt");
            // Configure Serilog
            var loggerConfiguration = new LoggerConfiguration();
            if (environment.IsDevelopment())
            {
                loggerConfiguration
                    .MinimumLevel.Debug()
                    .WriteTo.Debug()
                    .Enrich.FromLogContext()
                    .WriteTo.Console() // Always write to console for container logging
                    .WriteTo.File(
                        path: logPath,
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                    );
            }
            else
            {
                loggerConfiguration
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .WriteTo.Console() // Always write to console for container logging
                    .WriteTo.File(
                        path: logPath,
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                    )
                    .WriteTo.PostgreSQL(
                        connectionString: configuration.GetConnectionString("Postgres")
                            ?? throw new ArgumentNullException(
                                "Postgres connection strings are missing"
                            ),
                        tableName: "apiBase-Logs",
                        columnOptions: new Dictionary<string, ColumnWriterBase>
                        {
                            { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
                            {
                                "message_template",
                                new MessageTemplateColumnWriter(NpgsqlDbType.Text)
                            },
                            { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                            { "raise_date", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
                            { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
                            {
                                "properties",
                                new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb)
                            },
                            { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
                            {
                                "machine_name",
                                new SinglePropertyColumnWriter(
                                    "MachineName",
                                    PropertyWriteMethod.ToString,
                                    NpgsqlDbType.Text,
                                    "l"
                                )
                            },
                        }
                    // columnOptions: columnOptions
                    )
                    .CreateLogger();
            }
            // Only add OpenTelemetry if configured
            if (configuration.GetValue<bool>("UseOpenTelemetry"))
            {
                try
                {
                    loggerConfiguration.WriteTo.OpenTelemetry(x =>
                    {
                        x.Endpoint =
                            configuration["OpenTelemetry:Endpoint"]
                            ?? "http://localhost:4317/ingest/";
                        x.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;
                        x.Headers = new Dictionary<string, string>
                        {
                            ["X-Seq-ApiKey"] = configuration["OpenTelemetry:ApiKey"] ?? "1234",
                        };
                        x.ResourceAttributes.Add("api.name", "apiBase");
                    });
                }
                catch (Exception ex)
                {
                    // Log the error but continue without OpenTelemetry
                    Console.WriteLine($"Failed to configure OpenTelemetry: {ex.Message}");
                }
            }

            // Create Serilog logger
            Log.Logger = loggerConfiguration.CreateLogger();

            // Add Serilog to .NET Core
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

                loggingBuilder.AddSerilog(dispose: true);
            });

            return services;
        }
    }
}
