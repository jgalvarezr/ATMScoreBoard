using ATMScoreBoard.Shared;
using ATMScoreBoard.Shared.Models;
using ATMScoreBoard.Web.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace ATMScoreBoard.Web.Services
{
    public class PartidaService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public PartidaService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
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
                    if (dto.Payload == null) throw new Exception("Payload inválido.");

                    bool bolaYaEmbolsada = await context.PartidasActualesBolas
                        .AnyAsync(b => b.MesaId == dto.MesaId && b.NumeroBola == dto.Payload.NumeroBola);

                    if (bolaYaEmbolsada)
                    {
                        throw new Exception($"La bola {dto.Payload.NumeroBola} ya ha sido embolsada en esta partida.");
                    }


                    var nuevaBola = new PartidaActualBolas
                    {
                        MesaId = dto.MesaId,
                        EquipoId = dto.Payload.EquipoId,
                        NumeroBola = dto.Payload.NumeroBola,
                        Timestamp = DateTime.UtcNow
                    };

                    await context.PartidasActualesBolas.AddAsync(nuevaBola);
                    await context.SaveChangesAsync();
                    break;

                case "CorregirBola":
                    if (dto.Payload == null)
                        throw new Exception("Payload inválido.");

                    // Buscamos la bola específica que se quiere corregir
                    var bolaACorregir = await context.PartidasActualesBolas
                        .FirstOrDefaultAsync(b => b.MesaId == dto.MesaId &&
                                                  b.NumeroBola == dto.Payload.NumeroBola);

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


                default:
                    throw new Exception("Tipo de acción no reconocido.");
            }
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
                    else if (bolasPropias.Contains(8) )
                        return equipoActual;

                } else
                {
                    if (bolasPropias.Contains(8))
                        return equipoRival;
                }
                return EquipoIdentifier.Ninguno;
            }


            var resultado = new ResultadoChequeo();

            var ChequearEquipoA = ChequearEquipo(EquipoIdentifier.EquipoA, bolasA, bolasB);
            
            if (ChequearEquipoA != EquipoIdentifier.Ninguno) {

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
            else if(bolasB.Contains(9)){
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


    }


    public enum EquipoIdentifier { EquipoA, EquipoB, Ninguno }
    public enum EstadoPartida { EnCurso, Ganada, Zapatero }
    public class ResultadoChequeo
    {
        public EstadoPartida Estado { get; set; } = EstadoPartida.EnCurso;
        public EquipoIdentifier Ganador { get; set; } = EquipoIdentifier.Ninguno;
    }

}