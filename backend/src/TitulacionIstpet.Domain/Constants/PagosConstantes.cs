namespace TitulacionIstpet.Domain.Constants;

public static class PagosConstantes
{
    public const decimal TotalTitulacionOficial = 700.00m;
    public const decimal TopeMaximoInversion = 200.00m;

    public static class Estados
    {
        public const string Registrado = "REGISTRADO";
        public const string Aprobado = "APROBADO";
        public const string Rechazado = "RECHAZADO";

        public static readonly string[] Todos = [Registrado, Aprobado, Rechazado];

        public static bool CuentaComoPagado(string estado) =>
            string.Equals(estado, Registrado, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(estado, Aprobado, StringComparison.OrdinalIgnoreCase);
    }

    public static class Rubros
    {
        public const string Base = "BASE";
        public const string Inversion = "INVERSION";

        public static readonly string[] Todos = [Base, Inversion];

        public static bool EsValido(string rubro) =>
            string.Equals(rubro, Base, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rubro, Inversion, StringComparison.OrdinalIgnoreCase);
    }

    public static class Roles
    {
        public const string TitulacionAdmin = "TITULACION_ADMIN";
        public const string Secretaria = "SECRETARIA";
        public const string Contabilidad = "CONTABILIDAD";
    }
}
