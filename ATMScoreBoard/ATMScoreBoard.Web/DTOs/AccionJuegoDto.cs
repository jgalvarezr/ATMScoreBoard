using System.Text.Json;

namespace ATMScoreBoard.Web.DTOs
{
    public class AccionJuegoDto
    {
        public int MesaId { get; set; }
        public string TipoAccion { get; set; } = string.Empty;
        public JsonElement Payload { get; set; }
    }

    public class EmbolsarBolaPayload
    {
        public int? EquipoId { get; set; }
        public int NumeroBola { get; set; }
    }

    public class AsignarGruposPayload
    {
        public int EquipoLisasId { get; set; }
    }
}
