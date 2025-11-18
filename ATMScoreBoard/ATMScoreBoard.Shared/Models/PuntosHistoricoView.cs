using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMScoreBoard.Shared.Models
{
    public class PuntosHistoricoView
    {
        public int EquipoId { get; set; }
        public int PartidaId { get; set; }
        public DateTime Fecha { get; set; }
        public int Puntos { get; set; }
        public int RivalId { get; set; }
        public int Dias { get; set; }
        public long Orden { get; set; }
    }
}
