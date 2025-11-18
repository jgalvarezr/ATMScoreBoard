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
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<EquipoJugador> EquipoJugadores { get; set; }
        public DbSet<Partida> Partidas { get; set; }
        public DbSet<PartidaActual> PartidasActuales { get; set; }
        public DbSet<PartidaActualBolas> PartidasActualesBolas { get; set; }

        public DbSet<PuntosHistoricoView> PuntosHistorico { get; set; }
        public DbSet<EstadisticaEquipoColRanking> RankingEquipos { get; set; }
        public DbSet<EstadisticaJugadorRanking> RankingJugadores { get; set; }



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

            // Relación para EquipoA
            modelBuilder.Entity<PartidaActual>()
                .HasOne(pa => pa.EquipoA)
                .WithMany() // Un equipo puede estar en muchas PartidasActuales como EquipoA
                .HasForeignKey(pa => pa.EquipoAId)
                .OnDelete(DeleteBehavior.Restrict); // ¡LA CLAVE! Impide el borrado en cascada.

            // Relación para EquipoB
            modelBuilder.Entity<PartidaActual>()
                .HasOne(pa => pa.EquipoB)
                .WithMany() // Un equipo puede estar en muchas PartidasActuales como EquipoB
                .HasForeignKey(pa => pa.EquipoBId)
                .OnDelete(DeleteBehavior.Restrict); // ¡LA CLAVE! Impide el borrado en cascada.

            modelBuilder.Entity<PuntosHistoricoView>(eb =>
            {
                eb.HasNoKey(); // ¡Crucial! Le dice a EF que no hay clave primaria.
                eb.ToView("PuntosHistorico"); // Le dice a EF el nombre de la vista en la BD.
            });

            modelBuilder.Entity<EstadisticaEquipoColRanking>().HasNoKey();
            modelBuilder.Entity<EstadisticaJugadorRanking>().HasNoKey();
        }

    }
}
