using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMScoreBoard.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddNewStatisticsSPs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LadosIntercambiados",
                table: "PartidasActuales");

            migrationBuilder.Sql(@" create or alter function [dbo].[Puntos]
                                    (
	                                    @Equipo int,
	                                    @EquipoGanador int,
	                                    @Impecable bit
                                    )
                                    returns int
                                    as
                                    begin
	                                    declare @ret int = 0
	                                    if @Equipo = @EquipoGanador
	                                    begin
		                                    set @ret = 1
		                                    if @Impecable = 1
		                                    begin
			                                    set @ret = 2
		                                    end
	                                    end
	                                    return @ret
                                    end
                                ");

            migrationBuilder.Sql(@" create or alter view [dbo].[HistoricoEquipo]
                                    as
                                    with puntos			as (	
					                                    select			p.id, p.EquipoAId as EquipoId, 
									                                    p.Fecha,
									                                    datediff(day, p.Fecha, getdate()) as dias,
									                                    dbo.Puntos(p.EquipoAId, p.EquipoGanadorId, p.FueVictoriaImpecable) as Puntos,
									                                    p.FueVictoriaImpecable
						                                    from		Partidas p
					                                    union all
					                                    select			p.id, p.EquipoBId as EquipoId, 
									                                    p.Fecha,
									                                    datediff(day, p.Fecha, getdate()) as dias,
									                                    dbo.Puntos(p.EquipoBId, p.EquipoGanadorId, p.FueVictoriaImpecable) as Puntos,
									                                    p.FueVictoriaImpecable
						                                    from		Partidas p),
                                    puntosSort			as (
                                                        select			*,
									                                    row_number() over (partition by id order by fecha desc) as orden
						                                    from		puntos),
                                    JugadoresEquipoNum as (
                                                        select      ej.EquipoId,
                                                                    j.Nombre as NombreJugador,
                                                                    row_number() over(partition by ej.EquipoId order by j.Nombre) as JugadorNum
                                                        from        EquipoJugadores ej
                                                        join        Jugadores j on ej.JugadorId = j.Id),
                                    JugadoresEnColumnas as (
                                                        select      jen.EquipoId,
                                                                    max(case when jen.JugadorNum = 1 then jen.NombreJugador end) as JugadorA,
                                                                    max(case when jen.JugadorNum = 2 then jen.NombreJugador end) as JugadorB
                                                        from        JugadoresEquipoNum jen
                                                        group by    jen.EquipoId
                                                    )
                                    select			p.Id,
				                                    p.EquipoId, 
				                                    jc.JugadorA,
				                                    jc.JugadorB,
				                                    p.dias,
				                                    p.Puntos,
				                                    p.orden
	                                    from		puntosSort p
	                                    inner join	JugadoresEnColumnas jc on jc.EquipoId = p.EquipoId
	                                    where		jc.JugadorA is not null
		                                    and		jc.JugadorB is not null
                                ");

            migrationBuilder.Sql(@" create or alter view [dbo].[HistoricoJugador]
									as
									with puntos as (
														select			j.Id, 
																		j.Nombre, 
																		p.Fecha,
																		datediff(day, p.Fecha, getdate()) as dias,
																		dbo.Puntos(p.EquipoAId, p.EquipoGanadorId, p.FueVictoriaImpecable) as Puntos
															from		Partidas p
															inner join	EquipoJugadores e on e.EquipoId = p.EquipoAId 
															inner join	Jugadores j on	e.JugadorId = j.Id
														union all 
														select			j.Id, 
																		j.Nombre, 
																		p.fecha,
																		datediff(day, p.Fecha, getdate()) as dias,
																		dbo.Puntos(p.EquipoBId, p.EquipoGanadorId, p.FueVictoriaImpecable) as Puntos
															from		Partidas p
															inner join	EquipoJugadores e on e.EquipoId = p.EquipoBId 
															inner join	Jugadores j on	e.JugadorId = j.Id),


										puntosSort as (
														select			*,
																		row_number() over (partition by id order by fecha desc) as orden
															from		puntos
															)
										select			*
											from		puntosSort
                                ");

            migrationBuilder.Sql(@" create or alter procedure [dbo].[RankingEquipo]
										@partidos int,
										@dias int
									as
									begin
										select			h.JugadorA,
														h.jugadorB,
														sum(h.puntos) as puntos
											from		HistoricoEquipo h
											where		h.orden <= @partidos and h.dias < @dias
											group by	h.JugadorA,
														h.jugadorB
											order by	puntos desc
									end
                                ");

            migrationBuilder.Sql(@" create or alter procedure [dbo].[RankingJugadores]
										@partidos int,
										@dias int

									as
									begin
										select			h.Id, 
														h.Nombre,
														sum(h.puntos) as puntos,
														max(h.orden) as partidas,
														round(cast(sum(h.puntos) as float) / cast(max(h.orden) as float), 3) * 1000 as average
											from		HistoricoJugador h
											where		h.orden <= @partidos and h.dias < @dias
											group by	h.Id, h.Nombre
											order by	puntos desc
									end
                                ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LadosIntercambiados",
                table: "PartidasActuales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("drop procedure [dbo].[RankingJugadores]");
            migrationBuilder.Sql("drop procedure [dbo].[RankingEquipo]");
            migrationBuilder.Sql("drop view [dbo].[HistoricoEquipo]");
            migrationBuilder.Sql("drop view [dbo].[HistoricoJugador]");
            migrationBuilder.Sql("drop function [dbo].[Puntos]");
        }
    }
}
