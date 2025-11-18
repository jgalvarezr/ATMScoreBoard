namespace ATMScoreBoard.Web.DTOs
{
    public class AccionJuegoDto
    {
        public int MesaId { get; set; }
        public string TipoAccion { get; set; } = string.Empty;
        public EmbolsarBolaPayload Payload { get; set; } = new(); // Lo hacemos específico por ahora
    }
}
