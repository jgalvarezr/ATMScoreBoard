namespace ATMScoreBoard.Web.DTOs
{
    public class IniciarPartidaDto
    {
        public int MesaId { get; set; }
        public int TipoJuegoId { get; set; }
        public List<int> JugadorIds { get; set; } = [];
    }
}
