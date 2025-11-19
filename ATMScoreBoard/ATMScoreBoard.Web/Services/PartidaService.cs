using ATMScoreBoard.Shared;
using ATMScoreBoard.Shared.DTOs;
using ATMScoreBoard.Shared.Models;
using ATMScoreBoard.Web.DTOs;
using ATMScoreBoard.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Text.Json;

namespace ATMScoreBoard.Web.Services
{
    public class PartidaService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IHubContext<MarcadorHub> _hubContext;
        private readonly RankingService _rankingService;

        public PartidaService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IHubContext<MarcadorHub> hubContext, RankingService rankingService)
        {
            _dbContextFactory = dbContextFactory;
            _hubContext = hubContext;
            _rankingService = rankingService;
        }

        public async Task<PartidaActual> IniciarPartidaAsync(IniciarPartidaDto dto)
        {
            using var context = _dbContextFactory.CreateDbContext();

            if (await context.PartidasActuales.AnyAsync(p => p.MesaId == dto.MesaId))
                throw new Exception("Ya hay una partida en curso en esta mesa.");

            if (dto.JugadorIds.Count != 2 && dto.JugadorIds.Count != 4)
                throw new Exception("Se requieren 2 o 4 jugadores.");

            // ... (Lógica para determinar jugadores de cada equipo) ...
            List<int> jugadoresEquipoA_Ids;
            List<int> jugadoresEquipoB_Ids;
            if (dto.JugadorIds.Count == 2)
            {
                jugadoresEquipoA_Ids = new List<int> { dto.JugadorIds[0] };
                jugadoresEquipoB_Ids = new List<int> { dto.JugadorIds[1] };
            }
            else
            {
                jugadoresEquipoA_Ids = new List<int> { dto.JugadorIds[0], dto.JugadorIds[2] };
                jugadoresEquipoB_Ids = new List<int> { dto.JugadorIds[1], dto.JugadorIds[3] };
            }

            int equipoAId = await ObtenerOcrearEquipoIdAsync(jugadoresEquipoA_Ids);
            int equipoBId = await ObtenerOcrearEquipoIdAsync(jugadoresEquipoB_Ids);

            var partidaActual = new PartidaActual
            {
                MesaId = dto.MesaId,
                TipoJuegoId = dto.TipoJuegoId,
                EquipoAId = equipoAId,
                EquipoBId = equipoBId,
                FechaInicio = DateTime.UtcNow
            };

            await context.PartidasActuales.AddAsync(partidaActual);
            await context.SaveChangesAsync();

            await NotificarCambioDeEstado(dto.MesaId);
            return partidaActual;
        }

        private async Task<int> ObtenerOcrearEquipoIdAsync(List<int> jugadorIds)
        {

            using var context = _dbContextFactory.CreateDbContext();

            jugadorIds.Sort();
            int count = jugadorIds.Count;

            var equipoExistenteId = await context.Equipos
                .Where(e => e.EquipoJugadores.Count == count && e.EquipoJugadores.All(ej => jugadorIds.Contains(ej.JugadorId)))
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (equipoExistenteId != 0) return equipoExistenteId;

            var nuevoEquipo = new Equipo { IsIndividual = (count == 1) };
            context.Equipos.Add(nuevoEquipo);
            await context.SaveChangesAsync();

            var nuevosEquipoJugadores = jugadorIds.Select(jugadorId => new EquipoJugador
            {
                EquipoId = nuevoEquipo.Id,
                JugadorId = jugadorId
            }).ToList();

            context.EquipoJugadores.AddRange(nuevosEquipoJugadores);
            await context.SaveChangesAsync();

            return nuevoEquipo.Id;
        }

        public async Task RealizarAccionAsync(AccionJuegoDto dto)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var partidaActual = await context.PartidasActuales.FindAsync(dto.MesaId);
            if (partidaActual == null)
                throw new Exception("La partida no está en curso.");

            switch (dto.TipoAccion)
            {
                case "EmbolsarBola":
                    var embolsarPayload = dto.Payload.Deserialize<EmbolsarBolaPayload>();
                    if (embolsarPayload == null) throw new Exception("Payload inválido.");

                    bool bolaYaEmbolsada = await context.PartidasActualesBolas
                        .AnyAsync(b => b.MesaId == dto.MesaId && b.NumeroBola == embolsarPayload.NumeroBola);

                    if (bolaYaEmbolsada)
                    {
                        throw new Exception($"La bola {embolsarPayload.NumeroBola} ya ha sido embolsada en esta partida.");
                    }

                    var nuevaBola = new PartidaActualBolas
                    {
                        MesaId = dto.MesaId,
                        EquipoId = embolsarPayload.EquipoId,
                        NumeroBola = embolsarPayload.NumeroBola,
                        Timestamp = DateTime.UtcNow
                    };

                    await context.PartidasActualesBolas.AddAsync(nuevaBola);
                    Console.WriteLine("API: Preparando para enviar notificación SignalR...");
                    await context.SaveChangesAsync();
                    break;

                case "CorregirBola":
                    var corregirPayload = dto.Payload.Deserialize<EmbolsarBolaPayload>();
                    if (corregirPayload == null) throw new Exception("Payload inválido.");

                    // Buscamos la bola específica que se quiere corregir
                    var bolaACorregir = await context.PartidasActualesBolas
                        .FirstOrDefaultAsync(b => b.MesaId == dto.MesaId &&
                                                  b.NumeroBola == corregirPayload.NumeroBola);

                    if (bolaACorregir != null)
                    {
                        context.PartidasActualesBolas.Remove(bolaACorregir);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        // Opcional: Lanzar una excepción si se intenta corregir una bola que no existe
                        // throw new Exception($"La bola {dto.Payload.NumeroBola} no está en el marcador para ser corregida.");
                        // Por ahora, simplemente no hacemos nada si no la encontramos.
                    }
                    break;
                
                case "AsignarGruposBola8":
                    var asignarPayload = dto.Payload.Deserialize<AsignarGruposPayload>();
                    if (asignarPayload == null) throw new Exception("Payload inválido.");

                    // Asignamos el equipo de lisas en la partida actual
                    partidaActual.EquipoLisasId = asignarPayload.EquipoLisasId;
                    context.PartidasActuales.Update(partidaActual);

                    // Actualizamos retroactivamente las bolas "huérfanas"
                    var bolasHuerfanas = await context.PartidasActualesBolas
                        .Where(b => b.MesaId == dto.MesaId && b.EquipoId == null)
                        .ToListAsync();

                    if (bolasHuerfanas.Any())
                    {
                        var bolasLisasDef = new[] { 1, 2, 3, 4, 5, 6, 7 };
                        var equipoRayadasId = partidaActual.EquipoAId == asignarPayload.EquipoLisasId
                            ? partidaActual.EquipoBId
                            : partidaActual.EquipoAId;

                        foreach (var bola in bolasHuerfanas)
                        {
                            bola.EquipoId = bolasLisasDef.Contains(bola.NumeroBola)
                                ? asignarPayload.EquipoLisasId
                                : equipoRayadasId;

                            context.PartidasActualesBolas.Update(bola);
                        }
                    }
                    await context.SaveChangesAsync();
                    break;
                default:
                    throw new Exception("Tipo de acción no reconocido.");
            }

            await NotificarCambioDeEstado(dto.MesaId);

        }

        public async Task FinalizarPartidaAsync(FinalizarPartidaDto dto)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var partidaActual = await context.PartidasActuales.FindAsync(dto.MesaId);
            if (partidaActual == null)
                throw new Exception("No se encontró la partida en curso para finalizar.");


            // 1. Crear el registro histórico
            var partidaHistorica = new Partida
            {
                EquipoAId = partidaActual.EquipoAId,
                EquipoBId = partidaActual.EquipoBId,
                EquipoGanadorId = dto.EquipoGanadorId,
                TipoJuegoId = partidaActual.TipoJuegoId,
                Fecha = DateTime.UtcNow,
                // En el futuro, podríamos guardar las puntuaciones aquí también
                FueVictoriaImpecable = dto.FueVictoriaImpecable
            };
            await context.Partidas.AddAsync(partidaHistorica);

            // 2. Limpiar las tablas de tiempo real
            var bolasAEliminar = context.PartidasActualesBolas.Where(b => b.MesaId == dto.MesaId);
            context.PartidasActualesBolas.RemoveRange(bolasAEliminar);
            context.PartidasActuales.Remove(partidaActual);

            await context.SaveChangesAsync();

            var equiposIds = new List<int> { partidaActual.EquipoAId, partidaActual.EquipoBId };
            await LimpiezaEquipo(equiposIds);

            await _hubContext.Clients.Group($"Mesa_{dto.MesaId}").SendAsync("PartidaFinalizada");

        }

        public async Task CancelarPartidaAsync(int mesaId)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var partidaActual = await context.PartidasActuales.FindAsync(mesaId);
            if (partidaActual == null)
            {
                // No hay nada que cancelar, así que salimos sin error.
                return;
            }

            // 1. Limpiar bolas asociadas (si las hubiera)
            var bolasAEliminar = context.PartidasActualesBolas.Where(b => b.MesaId == mesaId);
            if (bolasAEliminar.Any())
            {
                context.PartidasActualesBolas.RemoveRange(bolasAEliminar);
            }

            // 2. Eliminar la partida actual
            context.PartidasActuales.Remove(partidaActual);

            await context.SaveChangesAsync();

            // 3. Lógica de limpieza de equipos huérfanos
            var equiposIds = new List<int> { partidaActual.EquipoAId, partidaActual.EquipoBId };
            await LimpiezaEquipo(equiposIds);

            await _hubContext.Clients.Group($"Mesa_{mesaId}").SendAsync("PartidaFinalizada");
        }

        private async Task LimpiezaEquipo(List<int> equiposIds)
        {
            using var context = _dbContextFactory.CreateDbContext();

            foreach (var equipoId in equiposIds)
            {
                // Comprobamos si el equipo ha jugado alguna partida histórica O está en otra partida actual
                bool tieneOtrasPartidas = await context.Partidas.AnyAsync(p => p.EquipoAId == equipoId || p.EquipoBId == equipoId) ||
                                          await context.PartidasActuales.AnyAsync(p => (p.EquipoAId == equipoId || p.EquipoBId == equipoId));

                if (!tieneOtrasPartidas)
                {
                    var equipoABorrar = await context.Equipos.FindAsync(equipoId);
                    if (equipoABorrar != null)
                    {
                        // La eliminación en cascada se encargará de borrar en EquipoJugadores
                        context.Equipos.Remove(equipoABorrar);
                    }
                }

            }
            await context.SaveChangesAsync();
        }

        public async Task<ResultadoChequeo> ChequearEstadoPartidaAsync(int mesaId)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var partidaActual = await context.PartidasActuales.FindAsync(mesaId);
            if (partidaActual == null)
                throw new Exception("No hay partida en curso.");

            var bolas = await context.PartidasActualesBolas
                .Where(b => b.MesaId == mesaId)
                .ToListAsync();

            var bolasA = bolas.Where(b => b.EquipoId == partidaActual.EquipoAId).Select(b => b.NumeroBola).ToList();
            var bolasB = bolas.Where(b => b.EquipoId == partidaActual.EquipoBId).Select(b => b.NumeroBola).ToList();

            switch (partidaActual.TipoJuegoId)
            {
                case 1:
                    return ChequeoBola8(bolasA, bolasB);
                case 2:
                    return ChequeoBola9(bolasA, bolasB);
                case 3:
                    return ChequeoBola10(bolasA, bolasB);
                case 4:
                    return ChequeoChapolin(bolasA, bolasB);
                default:
                    throw new Exception();
            }
        }

        public ResultadoChequeo ChequeoBola8(List<int> bolasA, List<int> bolasB)
        {
            EquipoIdentifier ChequearEquipo(EquipoIdentifier equipoActual, List<int> bolasPropias, List<int> bolasRivales)
            {
                var bolasLisas = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 };
                var bolasRayadas = new HashSet<int> { 9, 10, 11, 12, 13, 14, 15 };

                var equipoRival = equipoActual == EquipoIdentifier.EquipoA ? EquipoIdentifier.EquipoB : EquipoIdentifier.EquipoA;

                bool metioTodasLisas = bolasLisas.IsSubsetOf(bolasPropias);
                bool metioTodasRayadas = bolasRayadas.IsSubsetOf(bolasPropias);
                bool metioTodas = metioTodasLisas || metioTodasRayadas;

                if (metioTodas)
                {
                    if (bolasPropias.Contains(0))
                        return equipoRival;
                    else if (bolasPropias.Contains(8))
                        return equipoActual;

                }
                else
                {
                    if (bolasPropias.Contains(8))
                        return equipoRival;
                }
                return EquipoIdentifier.Ninguno;
            }


            var resultado = new ResultadoChequeo();

            var ChequearEquipoA = ChequearEquipo(EquipoIdentifier.EquipoA, bolasA, bolasB);

            if (ChequearEquipoA != EquipoIdentifier.Ninguno)
            {

                resultado.Ganador = ChequearEquipoA;

                if (resultado.Ganador == EquipoIdentifier.EquipoA)
                    resultado.Estado = bolasB.Count == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;
                else
                    resultado.Estado = bolasA.Count == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;

                return resultado;
            }

            var ChequearEquipoB = ChequearEquipo(EquipoIdentifier.EquipoB, bolasB, bolasA);

            if (ChequearEquipoB != EquipoIdentifier.Ninguno)
            {

                resultado.Ganador = ChequearEquipoB;

                if (resultado.Ganador == EquipoIdentifier.EquipoB)
                    resultado.Estado = bolasA.Count == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;
                else
                    resultado.Estado = bolasB.Count == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;

                return resultado;
            }
            return resultado;
        }

        public ResultadoChequeo ChequeoBola9(List<int> bolasA, List<int> bolasB)
        {
            var resultado = new ResultadoChequeo()
            {
                Estado = EstadoPartida.EnCurso,
                Ganador = EquipoIdentifier.Ninguno
            };

            if (bolasA.Contains(9))
            {
                resultado.Estado = bolasB.Count() == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;
                resultado.Ganador = EquipoIdentifier.EquipoA;
            }
            else if (bolasB.Contains(9))
            {
                resultado.Estado = bolasA.Count() == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;
                resultado.Ganador = EquipoIdentifier.EquipoB;
            }

            return resultado;
        }

        public ResultadoChequeo ChequeoBola10(List<int> bolasA, List<int> bolasB)
        {
            var resultado = new ResultadoChequeo()
            {
                Estado = EstadoPartida.EnCurso,
                Ganador = EquipoIdentifier.Ninguno
            };

            if (bolasA.Contains(10))
            {
                resultado.Estado = bolasB.Count() == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;
                resultado.Ganador = EquipoIdentifier.EquipoA;
            }
            else if (bolasB.Contains(10))
            {
                resultado.Estado = bolasA.Count() == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;
                resultado.Ganador = EquipoIdentifier.EquipoB;
            }

            return resultado;
        }

        public ResultadoChequeo ChequeoChapolin(List<int> bolasA, List<int> bolasB)
        {
            const int PUNTOS_PARA_GANAR = 66;
            const int PUNTOS_EMPATE_65 = 65;

            var puntuacionA = bolasA.Sum(b => b == 0 ? 10 : b);
            var puntuacionB = bolasB.Sum(b => b == 0 ? 10 : b);

            bool bolaBlancaEquipoA = bolasA.Contains(0);
            bool bolaBlancaEquipoB = bolasB.Contains(0);

            var resultado = new ResultadoChequeo()
            {
                Estado = EstadoPartida.EnCurso,
                Ganador = EquipoIdentifier.Ninguno
            };

            if (puntuacionA >= PUNTOS_PARA_GANAR || (puntuacionA == PUNTOS_EMPATE_65 && puntuacionB == PUNTOS_EMPATE_65 && bolaBlancaEquipoA))
            {
                resultado.Ganador = EquipoIdentifier.EquipoA;
                resultado.Estado = puntuacionB == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;
                return resultado;
            }

            if (puntuacionB >= PUNTOS_PARA_GANAR || (puntuacionA == PUNTOS_EMPATE_65 && puntuacionB == PUNTOS_EMPATE_65 && bolaBlancaEquipoB))
            {
                resultado.Ganador = EquipoIdentifier.EquipoB;
                resultado.Estado = puntuacionA == 0 ? EstadoPartida.Zapatero : EstadoPartida.Ganada;
                return resultado;
            }

            return resultado;
        }


        public async Task NotificarCambioDeEstado(int mesaId)
        {
            // Construimos el DTO con el estado más reciente
            var estadoDto = await ConstruirEstadoPartidaDtoAsync(mesaId);
            await _hubContext.Clients.Group($"Mesa_{mesaId}").SendAsync("PartidaActualizada", estadoDto);
        }

        // Método que construye el DTO (reemplaza al que teníamos en el controlador)
        // En Services/PartidaService.cs

        public async Task<EstadoPartidaDto?> ConstruirEstadoPartidaDtoAsync(int mesaId)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var partida = await context.PartidasActuales
                .Include(p => p.EquipoA)
                    .ThenInclude(e => e.EquipoJugadores)
                    .ThenInclude(ej => ej.Jugador)
                .Include(p => p.EquipoB)
                    .ThenInclude(e => e.EquipoJugadores)
                    .ThenInclude(ej => ej.Jugador)
                .AsNoTracking() 
                .FirstOrDefaultAsync(p => p.MesaId == mesaId);


            if (partida == null) return null;

            // --- 1. Cargar Datos Base (Rankings, Victorias, etc.) en paralelo ---
            var rankingJugadores = await _rankingService.ObtenerRankingJugadoresAsync(20, 90);
            var rankingEquipos = await _rankingService.ObtenerRankingEquiposAsync(20, 90);
            var partidasHistoricas = await context.Partidas.AsNoTracking().ToListAsync();
            var bolas = await context.PartidasActualesBolas.Where(b => b.MesaId == mesaId).AsNoTracking().ToListAsync();

            // --- 2. Función Auxiliar para construir las estadísticas de UN equipo ---
            

            // --- 3. Calcular Estado del Juego Actual ---
            var bolasA = bolas.Where(b => b.EquipoId == partida.EquipoAId).Select(b => b.NumeroBola).ToList();
            var bolasB = bolas.Where(b => b.EquipoId == partida.EquipoBId).Select(b => b.NumeroBola).ToList();


            var puntuacionA = 0;
            var puntuacionB = 0;

            var tipoJuego = (TipoJuego)partida.TipoJuegoId;

            switch (tipoJuego)
            {
                case TipoJuego.Chapolin:
                    puntuacionA = bolasA.Sum(b => b == 0 ? 10 : b);
                    puntuacionB = bolasB.Sum(b => b == 0 ? 10 : b);
                    break;

                case TipoJuego.Bola8:

                    var bolasDeGrupoA = partida.EquipoLisasId.HasValue
            ? (partida.EquipoLisasId == partida.EquipoAId
                ? bolasA.Where(b => b >= 1 && b <= 7)
                : bolasA.Where(b => b >= 9 && b <= 15))
            : bolasA.Where(b => b != 0 && b != 8); // Mesa abierta, contamos todas menos 8 y blanca

                    var bolasDeGrupoB = partida.EquipoLisasId.HasValue
                        ? (partida.EquipoLisasId == partida.EquipoBId
                            ? bolasB.Where(b => b >= 1 && b <= 7)
                            : bolasB.Where(b => b >= 9 && b <= 15))
                        : bolasB.Where(b => b != 0 && b != 8);

                    puntuacionA = bolasDeGrupoA.Count();
                    puntuacionB = bolasDeGrupoB.Count();
                    break;

                case TipoJuego.Bola9:
                case TipoJuego.Bola10:
                    // Para Bola 9 y 10, la puntuación es simplemente el total de bolas embolsadas.
                    puntuacionA = bolasA.Count;
                    puntuacionB = bolasB.Count;
                    break;

                default:
                    puntuacionA = 0;
                    puntuacionB = 0;
                    break;
            }


            var resultadoChequeo = await ChequearEstadoPartidaAsync(mesaId); // Este método ahora es más simple

            // --- 4. Construir y Devolver el DTO Final ---
            return new EstadoPartidaDto
            {
                MesaId = partida.MesaId,
                TipoJuegoId = partida.TipoJuegoId,
                EquipoA = ConstruirStatsEquipo(partida.EquipoAId, partida, rankingJugadores, rankingEquipos,partidasHistoricas),
                EquipoB = ConstruirStatsEquipo(partida.EquipoBId, partida, rankingJugadores, rankingEquipos, partidasHistoricas),
                Puntuaciones = new Dictionary<string, int> { { "a", puntuacionA }, { "b", puntuacionB } },
                BolasEntroneradas = new Dictionary<string, List<int>> { { "a", bolasA }, { "b", bolasB } },
                Estado = resultadoChequeo.Estado,
                Ganador = resultadoChequeo.Ganador,
                EquipoLisasId = partida.EquipoLisasId,
                BandaEquipoA = partida.BandaEquipoA
            };
        }

        private EquipoEstadisticasDto ConstruirStatsEquipo(int equipoId,
            PartidaActual partida,
            List<EstadisticaJugadorRanking> rankingJugadores,
            List<EstadisticaEquipoColRanking> rankingEquipos,
            List<Partida> partidasHistoricas)
        {
            var equipo = equipoId == partida.EquipoAId ? partida.EquipoA : partida.EquipoB;

            if (equipo == null) return new EquipoEstadisticasDto();

            var rankingDict = rankingJugadores.ToDictionary(r => r.Id, r => r.PuntosRanking);
            var jugadoresOrdenados = equipo.EquipoJugadores
                .OrderBy(ej => rankingDict.GetValueOrDefault(ej.JugadorId, 0))
                .ToList();

            var rankingEquipo = rankingEquipos.Select((r, i) => new { Rank = r, Index = i + 1 })
                                            .FirstOrDefault(x => x.Rank.Id == equipoId);

            var otroEquipoId = equipoId == partida.EquipoAId ? partida.EquipoBId : partida.EquipoAId;

            var res = new EquipoEstadisticasDto
            {
                Id = equipoId,
                Nombre = string.Join(" y ", jugadoresOrdenados.Select(ej => ej.Jugador.Nombre)),
                Jugadores = jugadoresOrdenados.Select(ej => {
                    var rankingJugador = rankingJugadores.Select((r, i) => new { Rank = r, Index = i + 1 })
                                                         .FirstOrDefault(x => x.Rank.Id == ej.JugadorId);
                    return new JugadorSimpleDto
                    {
                        Id = ej.JugadorId,
                        Nombre = ej?.Jugador?.Nombre ?? "",
                        PosicionRanking = rankingJugador?.Index ?? 0,
                        PuntosRanking = rankingJugador?.Rank.PuntosRanking ?? 0
                    };
                }).ToList(),
                PosicionRanking = rankingEquipo?.Index ?? 0,
                PuntosRanking = rankingEquipo?.Rank.PuntosRanking ?? 0,
                VictoriasGlobales = partidasHistoricas.Count(p => p.EquipoGanadorId == equipoId),
                VictoriasH2H = partidasHistoricas.Count(p => p.EquipoGanadorId == equipoId && (p.EquipoAId == otroEquipoId || p.EquipoBId == otroEquipoId))
            };

            return res;
        }

    }
        

    
    public class ResultadoChequeo
    {
        public EstadoPartida Estado { get; set; } = EstadoPartida.EnCurso;
        public EquipoIdentifier Ganador { get; set; } = EquipoIdentifier.Ninguno;
    }

}