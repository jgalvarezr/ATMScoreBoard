using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMScoreBoard.Shared.Models
{
    // Enums para las opciones
    public enum Modalidad { Individual, Parejas }
    public enum TipoJuego { Bola8 = 1, Bola9 = 2, Bola10 = 3, Chapolin = 4 }

    public class SetupPartidaModel
    {
        public int MesaId { get; set; } = 1; // Asumimos MVP con una mesa
        public Modalidad Modalidad { get; set; } = Modalidad.Individual;
        public TipoJuego TipoJuego { get; set; } = TipoJuego.Chapolin;

        [Required(ErrorMessage = "Debe seleccionar un jugador.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un jugador.")]
        public int JugadorA1Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un jugador.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un jugador.")]
        public int JugadorB1Id { get; set; }

        // Estos solo son requeridos si el modo es Parejas
        public int? JugadorA2Id { get; set; }
        public int? JugadorB2Id { get; set; }
    }
}
