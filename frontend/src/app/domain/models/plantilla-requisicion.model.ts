import { RequisicionPersonalInicial } from './requisicion-personal.model';

export interface MargenesMm {
  superior: number;
  derecho: number;
  inferior: number;
  izquierdo: number;
}

export interface PaginaPlantilla {
  formato: 'A4';
  orientacion: 'vertical';
  margenesMm: MargenesMm;
}

export interface EstiloPlantilla {
  fuente: 'sans' | 'serif' | 'mono';
  tamanoFuentePt: number;
  colorTexto: string;
  colorEncabezado: string;
  colorDestacado: string;
  colorFondo: string;
  colorBorde: string;
  grosorBordeMm: number;
}

export interface LogoPlantilla {
  usarLogoInstitucional: boolean;
  idAdjuntosImagenes: number | null;
  anchoMm: number;
}

export interface FondoPlantilla {
  idAdjuntosImagenes: number | null;
  opacidad: number;
  ajuste: 'contain' | 'cover';
}

export interface DocumentoPlantilla {
  titulo: string;
  subtitulo: string;
  pagina: PaginaPlantilla;
  estilo: EstiloPlantilla;
  logo: LogoPlantilla;
  fondo: FondoPlantilla;
  notaFinal: string;
}

export interface SeccionPlantilla {
  id: string;
  titulo: string;
}

export interface CampoPlantilla {
  etiqueta: string;
  ayuda: string;
}

export interface OpcionItem {
  id: string;
  etiqueta: string;
}

export interface OpcionesPlantilla {
  institucion: OpcionItem[];
  contratacion: OpcionItem[];
  jornada: OpcionItem[];
  remuneracion: OpcionItem[];
  motivo: OpcionItem[];
  competencias: OpcionItem[];
}

export interface ValorInicialPlantilla {
  fuente: 'literal' | 'contexto' | 'texto';
  valor?: string | null;
  clave?: string | null;
}

export interface FirmaPlantilla {
  id: 'requerido' | 'aprobado' | 'rrhh';
  titulo: string;
  etiquetaFirma: string;
  etiquetaNombre: string;
  etiquetaCargo: string;
}

export interface PlantillaRequisicionDefinicion {
  schemaVersion: number;
  codigo: string;
  documento: DocumentoPlantilla;
  secciones: SeccionPlantilla[];
  campos: Record<string, CampoPlantilla>;
  opciones: OpcionesPlantilla;
  valoresIniciales: Record<string, ValorInicialPlantilla>;
  firmas: FirmaPlantilla[];
}

export interface PlantillaRequisicionEnvelope {
  codigo: string;
  revision: number;
  esPredeterminada: boolean;
  fechaActualizacion: string;
  definicion: PlantillaRequisicionDefinicion;
}

export interface ActualizarPlantillaRequisicionRequest {
  revisionEsperada: number;
  definicion: PlantillaRequisicionDefinicion;
}

export interface AdjuntoImagenPlantilla {
  idAdjuntosImagenes: number;
  nombreArchivo: string;
  mimeType: string;
  tamanoBytes: number;
  url: string;
}

export interface DatosFormularioRequisicion {
  fechaSolicitud: string;
  area: string;
  solicitante: string;
  cargo: string;
  numeroVacantes: number | null;
  institucion: string;
  contratacion: string;
  jornada: string;
  remuneracion: string;
  valorRemuneracion: number | null;
  valorTotal: number | null;
  horario: string;
  lugar: string;
  fechaIngreso: string;
  motivo: string;
  otroMotivo: string;
  detalleMotivo: string;
  formacion: string;
  experiencia: string;
  conocimientos: string;
  competencias: string[];
  otraCompetencia: string;
  observaciones: string;
}

export const PLANTILLA_REQUISICION_BASE: PlantillaRequisicionDefinicion = {
  schemaVersion: 1,
  codigo: 'requisicion-personal',
  documento: {
    titulo: 'FORMATO DE REQUISICIÓN DE PERSONAL',
    subtitulo: '',
    pagina: {
      formato: 'A4',
      orientacion: 'vertical',
      margenesMm: {
        superior: 14,
        derecho: 14,
        inferior: 14,
        izquierdo: 14,
      },
    },
    estilo: {
      fuente: 'sans',
      tamanoFuentePt: 9,
      colorTexto: '#171717',
      colorEncabezado: '#171717',
      colorDestacado: '#002855',
      colorFondo: '#FFFFFF',
      colorBorde: '#334155',
      grosorBordeMm: 0.35,
    },
    logo: {
      usarLogoInstitucional: false,
      idAdjuntosImagenes: null,
      anchoMm: 24,
    },
    fondo: {
      idAdjuntosImagenes: null,
      opacidad: 0.1,
      ajuste: 'contain',
    },
    notaFinal:
      'Toda requisición de personal deberá contar con la aprobación de Rectorado y disponibilidad presupuestaria previa al inicio del proceso de reclutamiento y selección.',
  },
  secciones: [
    { id: 'general', titulo: '1. INFORMACIÓN GENERAL' },
    { id: 'institucion', titulo: '2. INSTITUCIÓN SOLICITANTE' },
    { id: 'vacante', titulo: '3. INFORMACIÓN DE LA VACANTE' },
    { id: 'motivo', titulo: '4. MOTIVO DE LA VACANTE' },
    { id: 'perfil', titulo: '5. PERFIL REQUERIDO' },
    { id: 'observaciones', titulo: '6. OBSERVACIONES' },
  ],
  campos: {
    fechaSolicitud: { etiqueta: 'Fecha de solicitud', ayuda: '' },
    area: { etiqueta: 'Área / Departamento', ayuda: '' },
    solicitante: { etiqueta: 'Nombre del solicitante', ayuda: '' },
    cargo: { etiqueta: 'Cargo solicitado', ayuda: '' },
    numeroVacantes: { etiqueta: 'Número de vacantes', ayuda: '' },
    institucion: { etiqueta: 'Institución solicitante', ayuda: '' },
    contratacion: { etiqueta: 'Tipo de contratación', ayuda: '' },
    jornada: { etiqueta: 'Jornada laboral', ayuda: '' },
    remuneracion: { etiqueta: 'Remuneración', ayuda: '' },
    valorRemuneracion: { etiqueta: 'Sueldo / valor por hora', ayuda: '' },
    valorTotal: { etiqueta: 'Valor total', ayuda: '' },
    horario: { etiqueta: 'Horario requerido', ayuda: '' },
    lugar: { etiqueta: 'Lugar de trabajo', ayuda: '' },
    fechaIngreso: { etiqueta: 'Fecha requerida de ingreso', ayuda: '' },
    motivo: { etiqueta: 'Motivo de la vacante', ayuda: '' },
    otroMotivo: { etiqueta: 'Otro motivo', ayuda: '' },
    detalleMotivo: { etiqueta: 'Detalle del motivo', ayuda: '' },
    formacion: { etiqueta: 'Formación académica', ayuda: '' },
    experiencia: { etiqueta: 'Experiencia requerida', ayuda: '' },
    conocimientos: { etiqueta: 'Conocimientos específicos', ayuda: '' },
    competencias: { etiqueta: 'Competencias requeridas', ayuda: '' },
    otraCompetencia: { etiqueta: 'Otra competencia', ayuda: '' },
    observaciones: { etiqueta: 'Observaciones', ayuda: '' },
  },
  opciones: {
    institucion: [
      {
        id: 'escuela_conduccion_istpet',
        etiqueta:
          'Escuela de Conducción del Instituto Superior Tecnológico Mayor Pedro Traversari',
      },
      {
        id: 'istpet',
        etiqueta: 'Instituto Superior Tecnológico Mayor Pedro Traversari',
      },
      {
        id: 'academia_miguel_iturralde',
        etiqueta:
          'Unidad Educativa Particular Bilingüe Academia Militar General Miguel Iturralde',
      },
      {
        id: 'academia_miguel_iturralde_2',
        etiqueta:
          'Unidad Educativa Bilingüe Particular Academia Militar General Miguel Iturralde 2',
      },
      {
        id: 'ecmi',
        etiqueta: 'Escuela de Conducción No Profesionales ECMI',
      },
      {
        id: 'club_miguel_iturralde',
        etiqueta: 'Club Deportivo Miguel Iturralde',
      },
    ],
    contratacion: [
      { id: 'contrato', etiqueta: 'Contrato' },
      { id: 'nombramiento', etiqueta: 'Nombramiento' },
      { id: 'servicios_profesionales', etiqueta: 'Servicios Profesionales' },
    ],
    jornada: [
      { id: 'tiempo_completo', etiqueta: 'Tiempo completo' },
      { id: 'medio_tiempo', etiqueta: 'Medio tiempo' },
      { id: 'horas', etiqueta: 'Horas' },
    ],
    remuneracion: [
      { id: 'sueldo', etiqueta: 'Sueldo' },
      { id: 'factura', etiqueta: 'Factura' },
    ],
    motivo: [
      { id: 'reemplazo', etiqueta: 'Reemplazo' },
      { id: 'renuncia', etiqueta: 'Renuncia' },
      { id: 'desvinculacion', etiqueta: 'Desvinculación' },
      { id: 'proyecto_temporal', etiqueta: 'Proyecto temporal' },
      { id: 'incremento_personal', etiqueta: 'Incremento de personal' },
      { id: 'licencia_maternidad', etiqueta: 'Licencia / Maternidad' },
      { id: 'nueva_creacion', etiqueta: 'Nueva creación de cargo' },
      { id: 'otro', etiqueta: 'Otro' },
    ],
    competencias: [
      { id: 'liderazgo', etiqueta: 'Liderazgo' },
      { id: 'resultados', etiqueta: 'Orientación a resultados' },
      { id: 'conflictos', etiqueta: 'Resolución de conflicto' },
      { id: 'equipo', etiqueta: 'Trabajo en equipo' },
      { id: 'comunicacion', etiqueta: 'Comunicación efectiva' },
      { id: 'adaptabilidad', etiqueta: 'Adaptabilidad' },
      { id: 'organizacion', etiqueta: 'Organización' },
      { id: 'otro', etiqueta: 'Otro' },
    ],
  },
  valoresIniciales: {
    fechaSolicitud: { fuente: 'contexto', clave: 'fechaActual' },
    solicitante: { fuente: 'contexto', clave: 'solicitante.nombre' },
    area: { fuente: 'literal', valor: 'Unidad de Titulación' },
    institucion: { fuente: 'literal', valor: 'istpet' },
    cargo: {
      fuente: 'texto',
      valor: 'Docente para curso complexivo de la carrera de {{curso.carrera}}',
    },
    numeroVacantes: { fuente: 'contexto', clave: 'curso.numeroVacantes' },
    contratacion: { fuente: 'literal', valor: 'servicios_profesionales' },
    jornada: { fuente: 'literal', valor: 'horas' },
    remuneracion: { fuente: 'literal', valor: 'factura' },
    motivo: { fuente: 'literal', valor: 'proyecto_temporal' },
  },
  firmas: [
    {
      id: 'requerido',
      titulo: 'REQUERIDO POR:',
      etiquetaFirma: 'Firma',
      etiquetaNombre: 'Nombre',
      etiquetaCargo: 'Cargo',
    },
    {
      id: 'aprobado',
      titulo: 'APROBADO POR:',
      etiquetaFirma: 'Firma',
      etiquetaNombre: 'Nombre',
      etiquetaCargo: 'Cargo',
    },
    {
      id: 'rrhh',
      titulo: 'RECIBIDO POR: RRHH',
      etiquetaFirma: 'Firma',
      etiquetaNombre: 'Nombre',
      etiquetaCargo: 'Cargo',
    },
  ],
};

/**
 * Resuelve los valores iniciales configurados en la plantilla utilizando
 * el contexto del curso y el usuario actual.
 */
export function resolverValoresIniciales(
  definicion: PlantillaRequisicionDefinicion,
  contexto?: RequisicionPersonalInicial | null,
  nombreSolicitante = '',
): Partial<DatosFormularioRequisicion> {
  const resultado: Partial<DatosFormularioRequisicion> = {};
  const hoy = new Date();
  const fechaHoy = `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}-${String(hoy.getDate()).padStart(2, '0')}`;

  const valores = definicion.valoresIniciales ?? {};

  for (const [campo, conf] of Object.entries(valores)) {
    if (!conf) continue;

    if (conf.fuente === 'literal') {
      if (campo === 'numeroVacantes') {
        const num = Number(conf.valor);
        resultado.numeroVacantes = Number.isFinite(num) ? num : null;
      } else {
        (resultado as Record<string, unknown>)[campo] = conf.valor ?? '';
      }
    } else if (conf.fuente === 'contexto') {
      switch (conf.clave) {
        case 'fechaActual':
          resultado.fechaSolicitud = fechaHoy;
          break;
        case 'solicitante.nombre':
          resultado.solicitante = nombreSolicitante;
          break;
        case 'curso.carrera':
          resultado.cargo = contexto?.origen.carrera ?? '';
          break;
        case 'curso.numeroVacantes':
          resultado.numeroVacantes = contexto?.numeroVacantes ?? null;
          break;
      }
    } else if (conf.fuente === 'texto') {
      let texto = conf.valor ?? '';
      const tieneVariableCurso = texto.includes('{{curso.');

      if (tieneVariableCurso && !contexto) {
        // Al abrir manualmente sin curso, los valores dependientes quedan vacíos
        // y no muestran marcadores {{...}} sin resolver
        (resultado as Record<string, unknown>)[campo] = '';
      } else {
        texto = texto
          .replace(/\{\{curso\.carrera\}\}/g, contexto?.origen?.carrera ?? '')
          .replace(/\{\{curso\.cohorte\}\}/g, contexto?.origen?.cohorte ?? '')
          .replace(
            /\{\{curso\.numeroVacantes\}\}/g,
            contexto?.numeroVacantes ? String(contexto.numeroVacantes) : '',
          )
          .replace(/\{\{solicitante\.nombre\}\}/g, nombreSolicitante)
          .replace(/\{\{fechaActual\}\}/g, fechaHoy)
          .replace(/\{\{[^}]+\}\}/g, '')
          .trim();

        (resultado as Record<string, unknown>)[campo] = texto;
      }
    }
  }

  return resultado;
}
