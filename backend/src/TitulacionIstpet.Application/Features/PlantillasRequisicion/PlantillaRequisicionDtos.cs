namespace TitulacionIstpet.Application.Features.PlantillasRequisicion;

public sealed record PlantillaRequisicionEnvelopeDto(
    string Codigo,
    int Revision,
    bool EsPredeterminada,
    DateTime FechaActualizacion,
    PlantillaRequisicionDefinicionDto Definicion
);

public sealed record ActualizarPlantillaRequisicionComandoDto(
    int RevisionEsperada,
    PlantillaRequisicionDefinicionDto Definicion
);

public sealed record PlantillaRequisicionDefinicionDto(
    int SchemaVersion,
    string Codigo,
    DocumentoPlantillaDto Documento,
    List<SeccionPlantillaDto> Secciones,
    Dictionary<string, CampoPlantillaDto> Campos,
    OpcionesPlantillaDto Opciones,
    Dictionary<string, ValorInicialPlantillaDto> ValoresIniciales,
    List<FirmaPlantillaDto> Firmas
);

public sealed record DocumentoPlantillaDto(
    string Titulo,
    string? Subtitulo,
    PaginaPlantillaDto Pagina,
    EstiloPlantillaDto Estilo,
    LogoPlantillaDto Logo,
    FondoPlantillaDto Fondo,
    string NotaFinal
);

public sealed record PaginaPlantillaDto(
    string Formato,
    string Orientacion,
    MargenesMmDto MargenesMm
);

public sealed record MargenesMmDto(
    double Superior,
    double Derecho,
    double Inferior,
    double Izquierdo
);

public sealed record EstiloPlantillaDto(
    string Fuente,
    double TamanoFuentePt,
    string ColorTexto,
    string ColorEncabezado,
    string ColorDestacado,
    string ColorFondo,
    string ColorBorde,
    double GrosorBordeMm
);

public sealed record LogoPlantillaDto(
    bool UsarLogoInstitucional,
    int? IdAdjuntosImagenes,
    double AnchoMm
);

public sealed record FondoPlantillaDto(
    int? IdAdjuntosImagenes,
    double Opacidad,
    string Ajuste
);

public sealed record SeccionPlantillaDto(
    string Id,
    string Titulo
);

public sealed record CampoPlantillaDto(
    string Etiqueta,
    string? Ayuda
);

public sealed record OpcionesPlantillaDto(
    List<OpcionItemDto> Institucion,
    List<OpcionItemDto> Contratacion,
    List<OpcionItemDto> Jornada,
    List<OpcionItemDto> Remuneracion,
    List<OpcionItemDto> Motivo,
    List<OpcionItemDto> Competencias
);

public sealed record OpcionItemDto(
    string Id,
    string Etiqueta
);

public sealed record ValorInicialPlantillaDto(
    string Fuente,
    string? Valor,
    string? Clave
);

public sealed record FirmaPlantillaDto(
    string Id,
    string Titulo,
    string EtiquetaFirma,
    string EtiquetaNombre,
    string EtiquetaCargo
);

public sealed record AdjuntoImagenPlantillaDto(
    int IdAdjuntosImagenes,
    string NombreArchivo,
    string MimeType,
    long TamanoBytes,
    string Url
);
