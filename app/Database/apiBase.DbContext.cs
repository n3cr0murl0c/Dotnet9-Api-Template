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
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            // builder.Entity<ArcaUser>().Property((u) => u.UsuarioBasis).HasMaxLength(10); //GEnera VARCHAR(10)
            builder.HasDefaultSchema("apiBase-schema"); //Nombre de la base de datos.
        }
    }
}
