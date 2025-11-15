using System.ComponentModel.DataAnnotations;

namespace ATMScoreBoard.Shared.Models
{
    public class Partida
    {
        [Key]
        public int Id { get; set; }
        public int EquipoAId { get; set; }
        public int EquipoBId { get; set; }
        public int? EquipoGanadorId { get; set; }
        public int TipoJuegoId { get; set; }
        public DateTime Fecha { get; set; }
        public bool FueVictoriaImpecable { get; set; }
    }
}