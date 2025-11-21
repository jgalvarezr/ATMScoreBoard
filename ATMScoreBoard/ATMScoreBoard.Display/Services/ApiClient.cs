using ATMScoreBoard.Shared.Configuration;
using ATMScoreBoard.Shared.DTOs;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ATMScoreBoard.Display.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(IOptions<StationSettings> settings)
        {
            _httpClient = new HttpClient();
            // Configuramos la dirección base del cliente HTTP con la URL de la API
            _httpClient.BaseAddress = new System.Uri(settings.Value.ApiBaseUrl);
        }

        public async Task<List<EstadisticaEquipoColRanking>?> GetRankingEquiposAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<EstadisticaEquipoColRanking>>("api/ranking/equipos");
            }
            catch (HttpRequestException) // Maneja errores de red
            {
                return null;
            }
        }

        public async Task<List<EstadisticaJugadorRanking>?> GetRankingJugadoresAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<EstadisticaJugadorRanking>>("api/ranking/jugadores");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<RankingParamsDto?> GetRankingParamsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<RankingParamsDto>("api/ranking/PARAMS");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}