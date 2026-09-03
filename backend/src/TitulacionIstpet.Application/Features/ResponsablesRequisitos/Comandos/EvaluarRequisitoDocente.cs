using TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;

namespace TitulacionIstpet.Application.Features.ResponsablesRequisitos.Comandos;

public sealed record EvaluarRequisitoDocenteComando(
    EvaluarRequisitoDocenteDto Datos,
    string IdEvaluador
);

public sealed class EvaluarRequisitoDocente(IRepositorioResponsablesRequisitos repositorio)
{
    private readonly IRepositorioResponsablesRequisitos _repositorio = repositorio;

    public Task EjecutarAsync(EvaluarRequisitoDocenteComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        ArgumentNullException.ThrowIfNull(comando.Datos);

        if (comando.Datos.IdPostulacionAlumnoRequisitoModalidad <= 0)
        {
            throw new ArgumentException("El requisito de postulación a evaluar es inválido.", nameof(comando));
        }

        if (comando.Datos.IdResponsableEvidencias <= 0)
        {
            throw new ArgumentException("La asignación de responsable es requerida.", nameof(comando));
        }

        if (string.IsNullOrWhiteSpace(comando.IdEvaluador))
        {
            throw new ArgumentException("El identificador del evaluador es requerido.", nameof(comando));
        }

        return _repositorio.EvaluarRequisitoAsync(comando.Datos, comando.IdEvaluador.Trim(), ct);
    }
}
