using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMScoreBoard.Shared.DTOs
{
    public class EquipoEstadisticasDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Usamos JugadorSimpleDto para los miembros del equipo
        public List<JugadorSimpleDto> Jugadores { get; set; } = new();

        // Estadísticas consolidadas del equipo
        public int PosicionRanking { get; set; }
        public int PuntosRanking { get; set; }
        public int VictoriasGlobales { get; set; }
        public int VictoriasH2H { get; set; }
    }
}
