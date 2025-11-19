using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMScoreBoard.Shared.DTOs
{
    public class JugadorSimpleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Estadísticas individuales del jugador
        public int PosicionRanking { get; set; }
        public int PuntosRanking { get; set; }
    }
}
