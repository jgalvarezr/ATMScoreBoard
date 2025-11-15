using System.ComponentModel.DataAnnotations;

namespace ATMScoreBoard.Shared.Models
{
    public class Mesa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}