using System.ComponentModel.DataAnnotations;

namespace ATMScoreBoard.Shared.Models
{
    public class TipoJuego
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;
    }
}