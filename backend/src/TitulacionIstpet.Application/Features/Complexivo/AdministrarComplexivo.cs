using FluentValidation;
using TitulacionIstpet.Application.Common.Models;

namespace TitulacionIstpet.Application.Features.Complexivo;

public interface IRepositorioComplexivo
{
    Task<IReadOnlyList<CohorteCarreraComplexivoDto>> ListarCohortesCarrerasAsync(CancellationToken ct);
    Task<IReadOnlyList<MallaComplexivoDto>> ListarMallaAsync(int idCohorteCarrera, CancellationToken ct);
    Task<MallaComplexivoDto?> ObtenerAsync(int id, CancellationToken ct);
    Task<int> GuardarAsync(int? id, GuardarMallaComplexivoDto dto, CancellationToken ct);
}

public sealed class AdministrarComplexivo(IRepositorioComplexivo repo, IValidator<GuardarMallaComplexivoDto> validator)
{
    public Task<IReadOnlyList<CohorteCarreraComplexivoDto>> ListarCohortesCarrerasAsync(CancellationToken ct)
        => repo.ListarCohortesCarrerasAsync(ct);

    public Task<IReadOnlyList<MallaComplexivoDto>> ListarMallaAsync(int idCohorteCarrera, CancellationToken ct)
        => repo.ListarMallaAsync(idCohorteCarrera, ct);

    public Task<MallaComplexivoDto?> ObtenerAsync(int id, CancellationToken ct) => repo.ObtenerAsync(id, ct);

    public async Task<int> GuardarAsync(int? id, GuardarMallaComplexivoDto dto, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
        {
            throw new ValidacionException(result.Errors);
        }
        return await repo.GuardarAsync(id, dto, ct);
    }
}

public sealed class GuardarMallaComplexivoValidator : AbstractValidator<GuardarMallaComplexivoDto>
{
    public GuardarMallaComplexivoValidator()
    {
        RuleFor(x => x.IdCohorteCarrera).GreaterThan(0);
        RuleFor(x => x.Detalle).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DetalleModulo).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Modalidad).NotEmpty().MaximumLength(500);
        RuleFor(x => x.FechaInicio).GreaterThanOrEqualTo(new DateOnly(1000, 1, 1));
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(new DateOnly(1000, 1, 1)).GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha final debe ser igual o posterior a la fecha de inicio.");
        RuleFor(x => x.TotalHoras).GreaterThan(0);
        RuleFor(x => x.Temas).Cascade(CascadeMode.Stop).NotEmpty().Must(x => x.Count <= 200 && x.All(t => t != null));
        RuleFor(x => x.Docentes).Cascade(CascadeMode.Stop).NotNull().Must(x => x.Count <= 50 && x.All(d => d != null));
        When(x => x.Temas != null && x.Temas.All(t => t != null), () =>
        {
            RuleFor(x => x).Must(x => x.Temas.Sum(t => (long)t.HorasPorTema) == x.TotalHoras)
                .WithMessage("La suma de horas de los temas debe coincidir con el total de horas del módulo.");
            RuleFor(x => x.Temas).Must(x => SinIdsRepetidos(x.Select(t => t.IdTemas)))
                .WithMessage("Los identificadores de los temas no deben repetirse.");
            RuleForEach(x => x.Temas).ChildRules(t =>
            {
                t.RuleFor(x => x.IdTemas).GreaterThanOrEqualTo(0);
                t.RuleFor(x => x.IdAsignatura).GreaterThan(0);
                t.RuleFor(x => x.Detalle).NotEmpty().MaximumLength(500);
                t.RuleFor(x => x.HorasPorTema).GreaterThan(0);
            });
        });
        When(x => x.Docentes != null && x.Docentes.All(d => d != null), () =>
        {
            RuleFor(x => x.Docentes).Must(x => SinIdsRepetidos(x.Select(d => d.IdMallaDocenteComplexivo)))
                .WithMessage("Los identificadores de asignación no deben repetirse.");
            RuleForEach(x => x.Docentes).ChildRules(d =>
            {
                d.RuleFor(x => x.IdMallaDocenteComplexivo).GreaterThanOrEqualTo(0);
                d.RuleFor(x => x.IdProfesor).MaximumLength(14);
                d.RuleFor(x => x.CodigoAsignacion).MaximumLength(10);
                d.RuleFor(x => x.FechaAsignacion).GreaterThanOrEqualTo(new DateOnly(1000, 1, 1)).When(x => x.FechaAsignacion.HasValue);
                d.RuleFor(x => x.Horarios).Cascade(CascadeMode.Stop).NotNull().Must(x => x.Count <= 100 && x.All(h => h != null));
                d.When(x => x.Horarios != null && x.Horarios.All(h => h != null), () =>
                {
                    d.RuleFor(x => x.Horarios).Must(x => SinIdsRepetidos(x.Select(h => h.IdModulosHorarios)));
                    d.RuleForEach(x => x.Horarios).ChildRules(h =>
                    {
                        h.RuleFor(x => x.IdModulosHorarios).GreaterThanOrEqualTo(0);
                        h.RuleFor(x => x.Dias).NotEmpty().MaximumLength(250);
                        h.RuleFor(x => x.FranjaHoraria).NotEmpty().MaximumLength(150);
                    });
                });
            });
        });
    }

    private static bool SinIdsRepetidos(IEnumerable<int> ids)
    {
        var existentes = ids.Where(x => x > 0).ToArray();
        return existentes.Distinct().Count() == existentes.Length;
    }
}
