using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Parroquia
{
    public int IdParroquias { get; set; }

    public int Idciudades { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Alumno> Alumnos { get; set; } = [];

    public virtual Ciudade IdciudadesNavigation { get; set; } = null!;

    public virtual ICollection<Profesore> ProfesoreIdParroquiaNacimientoNavigations { get; set; } = [];

    public virtual ICollection<Profesore> ProfesoreIdParroquiaResidenciaNavigations { get; set; } = [];
}
