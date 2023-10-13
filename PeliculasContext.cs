using Microsoft.EntityFrameworkCore;
using ProyectoGrado.Models;

namespace ProyectoGrado
{
    public class PeliculasContext : DbContext
    {

        protected readonly IConfiguration Configuration;

        public PeliculasContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to postgres with connection string from app settings
            options.UseNpgsql(Environment.GetEnvironmentVariable("connectionString"));
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Lista> Listas { get; set; }
        public DbSet<Contiene> Contiene { get; set; }
        public DbSet<Fav> Favoritos { get; set; }
        public DbSet<Ver> Vistas { get; set; }
        public DbSet<Resena> Resenas { get; set; }


    }
}
