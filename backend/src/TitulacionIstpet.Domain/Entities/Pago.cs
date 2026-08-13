using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Pago
{
    public int IdPago { get; set; }

    public int? IdMatricula { get; set; }

    public DateTime? Fecha { get; set; }

    public int? IdCuenta { get; set; }

    public string? Factura { get; set; }

    public string? NumeroDeposito { get; set; }

    public DateOnly? FechaDeposito { get; set; }

    public decimal? Valor { get; set; }

    public decimal? Descuento { get; set; }

    public string? Observacion { get; set; }

    public string? TipoDocumento { get; set; }

    public bool? Anulado { get; set; }

    public DateOnly? FechaAnulacion { get; set; }

    public int? NumeroRegistro { get; set; }

    public bool? NumeroExcepcion { get; set; }

    public string? UserPago { get; set; }

    public bool? GeneraManual { get; set; }

    public string? DocumentoFactura { get; set; }

    public virtual ICollection<DetallePago> DetallePagos { get; set; } = new List<DetallePago>();

    public virtual ICollection<DetallesDocumentosPago> DetallesDocumentosPagos { get; set; } = new List<DetallesDocumentosPago>();
}
