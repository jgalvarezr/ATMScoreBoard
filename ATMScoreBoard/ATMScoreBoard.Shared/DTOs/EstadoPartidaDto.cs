
using System.Collections.Generic;

namespace ATMScoreBoard.Shared.DTOs
{
    // Este DTO (Data Transfer Object) representa el estado completo de una partida
    // en un momento dado. Se usa para comunicar el estado desde el servidor (API)
    // a los clientes (Blazor y WPF).
    public class EstadoPartidaDto
    {
        public int MesaId { get; set; }
        public int TipoJuegoId { get; set; }

        // Información de los equipos
        public EquipoEstadisticasDto EquipoA { get; set; } = new();
        public EquipoEstadisticasDto EquipoB { get; set; } = new();

        // Estado del juego (ej. puntuaciones de Chapolín)
        public Dictionary<string, int> Puntuaciones { get; set; } = new();
        public Dictionary<string, List<int>> BolasEntroneradas { get; set; } = new();

        // Resultado del chequeo de estado
        public EstadoPartida Estado { get; set; }
        public EquipoIdentifier Ganador { get; set; }

        // Datos específicos de Bola 8
        public int? EquipoLisasId { get; set; }
        public string? BandaEquipoA { get; set; }
    }
}