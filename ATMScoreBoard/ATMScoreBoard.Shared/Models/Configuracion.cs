using System.ComponentModel.DataAnnotations;

namespace ATMScoreBoard.Shared.Models
{
    public class Configuracion
    {
        [Key]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        public string Valor { get; set; } = string.Empty;
    }
}
