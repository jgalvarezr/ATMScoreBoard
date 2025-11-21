using ATMScoreBoard.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace ATMScoreBoard.Web.Services
{
    public class ConfiguracionService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        // Un diccionario para cachear los valores de configuración
        private readonly ConcurrentDictionary<string, string> _cache = new();

        public ConfiguracionService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<int> GetIntAsync(string clave, int valorPorDefecto = 0)
        {
            var valorString = await GetStringAsync(clave);
            return int.TryParse(valorString, out int valor) ? valor : valorPorDefecto;
        }

        public async Task<string> GetStringAsync(string clave)
        {
            // Intenta obtener el valor de la caché primero
            if (_cache.TryGetValue(clave, out var valorCache))
            {
                return valorCache;
            }

            // Si no está en caché, lo busca en la BD
            using var context = _dbContextFactory.CreateDbContext();
            var config = await context.Configuraciones.FindAsync(clave);
            var valorDb = config?.Valor ?? string.Empty;

            // Lo guarda en caché para futuras peticiones
            _cache.TryAdd(clave, valorDb);

            return valorDb;
        }
    }
}