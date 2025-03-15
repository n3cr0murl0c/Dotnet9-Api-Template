using Microsoft.EntityFrameworkCore;

namespace apiBase.Database
{
    public class apiBaseDBContext : DbContext
    {
        public apiBaseDBContext(DbContextOptions<apiBaseDBContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("apiBase-schema"); //Nombre del esquema donde se hospedará la app.
        }
    }
}
