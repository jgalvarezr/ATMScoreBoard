using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMScoreBoard.Shared.Models
{
    public class EstadisticaEquipoColRanking
    {
        public int Id { get; set; }
        public string? JugadorA { get; set; }
        public string? JugadorB { get; set; }
        public int PuntosRanking { get; set; }
    }
}
