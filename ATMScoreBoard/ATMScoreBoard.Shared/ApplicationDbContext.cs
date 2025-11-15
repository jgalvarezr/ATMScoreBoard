using ATMScoreBoard.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ATMScoreBoard.Shared
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Jugador> Jugadores { get; set; }
        public DbSet<Mesa> Mesas { get; set; }

        public DbSet<TipoJuego> TiposJuego { get; set; }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<EquipoJugador> EquipoJugadores { get; set; }
        public DbSet<Partida> Partidas { get; set; }
        public DbSet<PartidaActual> PartidasActuales { get; set; }
        public DbSet<PartidaActualBolas> PartidasActualesBolas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Jugador>()
                .HasIndex(j => j.Nombre)
                .IsUnique();

            modelBuilder.Entity<Jugador>()
                .HasMany(j => j.EquipoJugadores)
                .WithOne(ej => ej.Jugador)
                .HasForeignKey(ej => ej.JugadorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EquipoJugador>()
                .HasKey(ej => new { ej.EquipoId, ej.JugadorId });


            modelBuilder.Entity<Equipo>()
                .HasMany(e => e.EquipoJugadores) // Un equipo tiene muchos EquipoJugadores
                .WithOne(ej => ej.Equipo)       // Cada EquipoJugador pertenece a un Equipo
                .HasForeignKey(ej => ej.EquipoId) // La clave foránea es EquipoId
                .OnDelete(DeleteBehavior.Cascade); // ¡LA MAGIA! Al borrar un Equipo, borra en cascada.

        }

    }
}
