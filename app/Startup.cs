using apiBase.Extensions;

namespace apiBase
{
    public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        public IConfiguration Configuration { get; } = configuration;
        public IWebHostEnvironment Environment { get; } = environment;

        public void ConfigureServices(IServiceCollection services)
        {
            // Modular service registration
            services.AddControllers();
            services
                .AddCustomCors(Configuration)
                .AddCustomSwagger()
                .AddCustomLogging(Configuration, Environment)
                .AddMemoryCache()
                .AddCustomResponseCompression()
                .AddCustomDependencyInjection();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Middleware pipeline configuration

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "apiBase API V1");
            });

            app
            // .UseHttpsRedirection()
            .UseResponseCompression()
                .UseRouting()
                .UseCors("AllowAll")
                .UseAuthentication() // Make sure this is before UseAuthorization
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
