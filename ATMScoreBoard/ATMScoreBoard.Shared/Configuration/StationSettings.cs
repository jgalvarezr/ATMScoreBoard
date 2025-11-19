namespace ATMScoreBoard.Shared.Configuration
{
    public class StationSettings
    {
        // Esta constante nos ayuda a evitar "strings mágicos" en el código.
        public const string SectionName = "StationSettings";

        public int MesaId { get; set; }
        public string ApiBaseUrl { get; set; } = string.Empty;
    }
}