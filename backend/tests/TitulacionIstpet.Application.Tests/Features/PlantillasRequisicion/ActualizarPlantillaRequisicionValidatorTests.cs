using TitulacionIstpet.Application.Features.PlantillasRequisicion;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.PlantillasRequisicion;

public class ActualizarPlantillaRequisicionValidatorTests
{
    private readonly ActualizarPlantillaRequisicionValidator _validator = new();

    [Fact]
    public async Task Comando_valido_pasa_validacion()
    {
        var comando = new ActualizarPlantillaRequisicionComandoDto(
            RevisionEsperada: 1,
            Definicion: PlantillaRequisicionBase.CrearDefinicionPredeterminada()
        );

        var resultado = await _validator.ValidateAsync(comando);

        Assert.True(resultado.IsValid, string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public async Task Revision_negativa_falla()
    {
        var comando = new ActualizarPlantillaRequisicionComandoDto(
            RevisionEsperada: -1,
            Definicion: PlantillaRequisicionBase.CrearDefinicionPredeterminada()
        );

        var resultado = await _validator.ValidateAsync(comando);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName.Contains("RevisionEsperada"));
    }

    [Fact]
    public async Task SchemaVersion_invalida_falla()
    {
        var def = PlantillaRequisicionBase.CrearDefinicionPredeterminada() with { SchemaVersion = 99 };
        var comando = new ActualizarPlantillaRequisicionComandoDto(1, def);

        var resultado = await _validator.ValidateAsync(comando);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName.Contains("SchemaVersion"));
    }

    [Fact]
    public async Task Color_invalido_falla()
    {
        var defOriginal = PlantillaRequisicionBase.CrearDefinicionPredeterminada();
        var estiloInvalido = defOriginal.Documento.Estilo with { ColorDestacado = "blue" };
        var docInvalido = defOriginal.Documento with { Estilo = estiloInvalido };
        var def = defOriginal with { Documento = docInvalido };

        var comando = new ActualizarPlantillaRequisicionComandoDto(1, def);
        var resultado = await _validator.ValidateAsync(comando);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName.Contains("ColorDestacado"));
    }

    [Fact]
    public async Task Opciones_con_ids_duplicados_falla()
    {
        var defOriginal = PlantillaRequisicionBase.CrearDefinicionPredeterminada();
        var opcionesDuplicadas = defOriginal.Opciones with
        {
            Contratacion =
            [
                new("contrato", "Contrato 1"),
                new("contrato", "Contrato 2")
            ]
        };
        var def = defOriginal with { Opciones = opcionesDuplicadas };

        var comando = new ActualizarPlantillaRequisicionComandoDto(1, def);
        var resultado = await _validator.ValidateAsync(comando);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName.Contains("Contratacion"));
    }

    [Fact]
    public async Task Variable_contexto_no_permitida_en_texto_falla()
    {
        var defOriginal = PlantillaRequisicionBase.CrearDefinicionPredeterminada();
        var valores = new Dictionary<string, ValorInicialPlantillaDto>(defOriginal.ValoresIniciales)
        {
            ["cargo"] = new("texto", "Cargo para {{variableInvalida}}", null)
        };
        var def = defOriginal with { ValoresIniciales = valores };

        var comando = new ActualizarPlantillaRequisicionComandoDto(1, def);
        var resultado = await _validator.ValidateAsync(comando);

        Assert.False(resultado.IsValid);
    }
}
