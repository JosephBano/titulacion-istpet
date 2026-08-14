namespace TitulacionIstpet.Application.DTOs.Auth;

public class UserPermissionsDto
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string EmailInstitucional { get; set; } = string.Empty;
    public string IdSigafi { get; set; } = string.Empty;
    public string TablaSigafi { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<RbacModuloPermissionsDto> Modulos { get; set; } = new();
}

public class RbacModuloPermissionsDto
{
    public int IdModulo { get; set; }
    public string NombreModulo { get; set; } = string.Empty;
    public List<string> Operaciones { get; set; } = new();
}
