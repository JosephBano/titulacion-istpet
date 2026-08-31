using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TitulacionIstpet.Application.Common.Behaviors;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Consultas;

namespace TitulacionIstpet.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var ensamblado = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(ensamblado);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(ensamblado);

        // Feature: AdjuntosImagenes
        services.AddScoped<CrearAdjunto>();
        services.AddScoped<ActualizarAdjunto>();
        services.AddScoped<EliminarAdjunto>();
        services.AddScoped<ObtenerAdjuntoPorId>();
        services.AddScoped<ListarAdjuntos>();

        // Feature: Postulaciones (Consultas y Comandos)
        services.AddScoped<Features.Postulaciones.Consultas.ConsultarElegibilidadEstudiante>();
        services.AddScoped<Features.Postulaciones.Consultas.ListarModalidadesOfertadas>();
        services.AddScoped<Features.Postulaciones.Consultas.ObtenerMiPostulacion>();
        services.AddScoped<Features.Postulaciones.Consultas.ObtenerPostulacionPorId>();
        services.AddScoped<Features.Postulaciones.Consultas.ListarPostulaciones>();
        services.AddScoped<Features.Postulaciones.Consultas.ListarEstadosPostulacion>();

        services.AddScoped<Features.Postulaciones.Comandos.CrearPostulacion>();
        services.AddScoped<Features.Postulaciones.Comandos.ActualizarRequisitosPostulacion>();
        services.AddScoped<Features.Postulaciones.Comandos.CambiarEstadoPostulacion>();
        services.AddScoped<Features.Postulaciones.Comandos.SolicitarCambioModalidad>();
        services.AddScoped<Features.Postulaciones.Consultas.ObtenerPortalEstudiante>();
        services.AddScoped<Features.Postulaciones.Comandos.DictaminarPostulacion>();

        // Feature: ConfiguracionGeneral
        services.AddScoped<Features.ConfiguracionGeneral.CasosDeUso.ListarConfiguracionGeneral>();
        services.AddScoped<Features.ConfiguracionGeneral.CasosDeUso.AdministrarModalidades>();
        services.AddScoped<Features.ConfiguracionGeneral.CasosDeUso.AdministrarRequisitos>();
        services.AddScoped<Features.ConfiguracionGeneral.CasosDeUso.AdministrarMatrizRequisitosModalidad>();

        // Feature: Convocatorias
        services.AddScoped<Features.Convocatorias.CasosDeUso.AperturarPeriodoConvocatoria>();
        services.AddScoped<Features.Convocatorias.CasosDeUso.ConsultarConvocatorias>();
        services.AddScoped<Features.Convocatorias.CasosDeUso.AdministrarConvocatoria>();

        return services;
    }
}

