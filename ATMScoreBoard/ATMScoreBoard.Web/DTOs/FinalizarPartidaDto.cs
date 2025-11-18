namespace ATMScoreBoard.Web.DTOs
{
    public class FinalizarPartidaDto
    {
        public int MesaId { get; set; }
        public int? EquipoGanadorId { get; set; }
        public int PuntuacionA { get; set; }
        public int PuntuacionB { get; set; }
        public bool FueVictoriaImpecable { get; set; }
    }
}
