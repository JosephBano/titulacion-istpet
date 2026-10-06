import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { documentHtml, printDocument } from '../../../../shared/utils/print-document';
import { DateFieldComponent } from '../../../../shared/components/date-field/date-field.component';
import { RequisicionPersonalInicial } from '../../../../domain/models/requisicion-personal.model';
import { PlantillaRequisicionStore } from '../../../../application/complexivo/plantilla-requisicion.store';
import { AuthService } from '../../../../core/services/auth.service';
import { EditorPlantillaComponent } from './editor-plantilla/editor-plantilla.component';
import {
  DatosFormularioRequisicion,
  resolverValoresIniciales,
} from '../../../../domain/models/plantilla-requisicion.model';
import { crearRequisicionPdf } from '../../../../shared/utils/requisicion-personal-pdf';

type CampoRequisicion =
  | 'fechaSolicitud'
  | 'area'
  | 'solicitante'
  | 'cargo'
  | 'horario'
  | 'lugar'
  | 'fechaIngreso'
  | 'detalleMotivo'
  | 'formacion'
  | 'experiencia'
  | 'conocimientos'
  | 'observaciones'
  | 'institucion'
  | 'contratacion'
  | 'jornada'
  | 'remuneracion'
  | 'motivo'
  | 'otroMotivo'
  | 'otraCompetencia';

interface CampoFormulario {
  clave: CampoRequisicion;
  etiqueta: string;
  requerido: boolean;
  tipo?: 'date' | 'text';
  multilinea?: boolean;
}

@Component({
  selector: 'app-requisicion-personal',
  standalone: true,
  imports: [CommonModule, FormsModule, DateFieldComponent, EditorPlantillaComponent],
  templateUrl: './requisicion-personal.component.html',
  styleUrls: ['../complexivo-ui.css', './requisicion-personal.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RequisicionPersonalComponent implements OnInit {
  readonly plantillaStore = inject(PlantillaRequisicionStore);
  readonly authService = inject(AuthService);

  readonly nombreSolicitante = input('');
  readonly datosIniciales = input<RequisicionPersonalInicial | null>(null);

  readonly modalEditorAbierto = signal<boolean>(false);
  readonly generandoPdfDescarga = signal<boolean>(false);

  readonly puedeEditarPlantilla = computed(() => {
    return (
      this.authService.hasRole('TITULACION_ADMIN') &&
      (this.authService.hasPermission('Examen Complexivo', 'editar') ||
        this.authService.currentUser()?.roles.some((r) => r.toUpperCase().includes('ADMIN')) ||
        false)
    );
  });

  readonly datos: Record<CampoRequisicion, string> = {
    fechaSolicitud: '',
    area: '',
    solicitante: '',
    cargo: '',
    horario: '',
    lugar: '',
    fechaIngreso: '',
    detalleMotivo: '',
    formacion: '',
    experiencia: '',
    conocimientos: '',
    observaciones: '',
    institucion: '',
    contratacion: '',
    jornada: '',
    remuneracion: '',
    motivo: '',
    otroMotivo: '',
    otraCompetencia: '',
  };

  numeroVacantes: number | null = null;
  valorRemuneracion: number | null = null;
  valorTotal: number | null = null;
  readonly competenciasSeleccionadas = new Set<string>();

  get instituciones(): string[] {
    return (
      this.plantillaStore.plantillaVigente().opciones.institucion?.map((i) => i.etiqueta) ?? [
        'Escuela de Conducción del Instituto Superior Tecnológico Mayor Pedro Traversari',
        'Instituto Superior Tecnológico Mayor Pedro Traversari',
        'Unidad Educativa Particular Bilingüe Academia Militar General Miguel Iturralde',
        'Unidad Educativa Bilingüe Particular Academia Militar General Miguel Iturralde 2',
        'Escuela de Conducción No Profesionales ECMI',
        'Club Deportivo Miguel Iturralde',
      ]
    );
  }

  get contrataciones(): string[] {
    return (
      this.plantillaStore.plantillaVigente().opciones.contratacion?.map((c) => c.etiqueta) ?? [
        'Contrato',
        'Nombramiento',
        'Servicios Profesionales',
      ]
    );
  }

  get jornadas(): string[] {
    return (
      this.plantillaStore.plantillaVigente().opciones.jornada?.map((j) => j.etiqueta) ?? [
        'Tiempo completo',
        'Medio tiempo',
        'Horas',
      ]
    );
  }

  get motivos(): string[] {
    return (
      this.plantillaStore.plantillaVigente().opciones.motivo?.map((m) => m.etiqueta) ?? [
        'Reemplazo',
        'Renuncia',
        'Desvinculación',
        'Proyecto temporal',
        'Incremento de personal',
        'Licencia / Maternidad',
        'Nueva creación de cargo',
        'Otro',
      ]
    );
  }

  get competencias(): string[] {
    return (
      this.plantillaStore.plantillaVigente().opciones.competencias?.map((c) => c.etiqueta) ?? [
        'Liderazgo',
        'Orientación a resultados',
        'Resolución de conflicto',
        'Trabajo en equipo',
        'Comunicación efectiva',
        'Adaptabilidad',
        'Organización',
        'Otro',
      ]
    );
  }

  get etiquetaCargo(): string {
    return this.plantillaStore.plantillaVigente().campos['cargo']?.etiqueta ?? 'Cargo solicitado';
  }

  get etiquetaVacantes(): string {
    return (
      this.plantillaStore.plantillaVigente().campos['numeroVacantes']?.etiqueta ??
      'Número de vacantes'
    );
  }

  readonly generales: CampoFormulario[] = [
    { clave: 'fechaSolicitud', etiqueta: 'Fecha de solicitud', requerido: true, tipo: 'date' },
    { clave: 'area', etiqueta: 'Área / Departamento', requerido: true },
    { clave: 'solicitante', etiqueta: 'Nombre del solicitante', requerido: true },
  ];

  readonly vacante: CampoFormulario[] = [
    { clave: 'cargo', etiqueta: 'Cargo solicitado', requerido: true },
    { clave: 'horario', etiqueta: 'Horario requerido', requerido: true, multilinea: true },
    { clave: 'lugar', etiqueta: 'Lugar de trabajo', requerido: true },
    {
      clave: 'fechaIngreso',
      etiqueta: 'Fecha requerida de ingreso',
      requerido: true,
      multilinea: true,
    },
  ];

  readonly perfil: CampoFormulario[] = [
    { clave: 'formacion', etiqueta: 'Formación académica', requerido: true, multilinea: true },
    { clave: 'experiencia', etiqueta: 'Experiencia requerida', requerido: true, multilinea: true },
    {
      clave: 'conocimientos',
      etiqueta: 'Conocimientos específicos',
      requerido: true,
      multilinea: true,
    },
  ];

  ngOnInit(): void {
    const fecha = new Date();
    this.datos.fechaSolicitud = `${fecha.getFullYear()}-${String(fecha.getMonth() + 1).padStart(2, '0')}-${String(fecha.getDate()).padStart(2, '0')}`;
    this.datos.solicitante = this.nombreSolicitante();

    const iniciales = this.datosIniciales();
    if (iniciales) {
      this.datos.cargo = iniciales.cargo;
      this.numeroVacantes = iniciales.numeroVacantes;
    }

    void this.cargarYResolverPlantilla();
  }

  private async cargarYResolverPlantilla(): Promise<void> {
    try {
      await this.plantillaStore.cargarPlantilla();
      const p = this.plantillaStore.plantillaVigente();
      const iniciales = this.datosIniciales();
      const resueltos = resolverValoresIniciales(p, iniciales, this.nombreSolicitante());

      if (iniciales && p.valoresIniciales['cargo']?.fuente === 'texto' && resueltos.cargo) {
        this.datos.cargo = resueltos.cargo;
      }
      if (
        this.numeroVacantes === null &&
        resueltos.numeroVacantes !== null &&
        resueltos.numeroVacantes !== undefined
      ) {
        this.numeroVacantes = resueltos.numeroVacantes;
      }
      if (!this.datos.area && resueltos.area) {
        this.datos.area = resueltos.area;
      }
    } catch {
      // Si la carga falla se conservan los valores por defecto
    }
  }

  abrirEditorPlantilla(): void {
    this.modalEditorAbierto.set(true);
  }

  cerrarEditorPlantilla(): void {
    this.modalEditorAbierto.set(false);
  }

  onPlantillaGuardada(): void {
    // Al guardar la plantilla no se sobreescriben respuestas ya editadas por el usuario
  }

  seleccionarCompetencia(competencia: string, seleccionada: boolean): void {
    if (seleccionada) this.competenciasSeleccionadas.add(competencia);
    else this.competenciasSeleccionadas.delete(competencia);
  }

  cargarEjemplo(): void {
    const inst =
      this.instituciones[1] || 'Instituto Superior Tecnológico Mayor Pedro Traversari';
    const ejemplo: Record<CampoRequisicion, string> = {
      fechaSolicitud: '2026-10-05',
      area: 'Unidad de Titulación',
      solicitante: this.nombreSolicitante() || 'Responsable de la Unidad de Titulación (ejemplo)',
      cargo: '[EJEMPLO] Docentes del curso de examen complexivo de Diseño Gráfico',
      horario:
        'Lunes a jueves: 19:00 a 21:00 (virtual).\nSábados: 08:00 a 11:00; cierre de módulo: 08:00 a 10:00 (presencial).',
      lugar: 'ISTPET — EVA y aulas de Diseño Gráfico',
      fechaIngreso: '05/10/2026 — Módulo 1\n26/10/2026 — Módulo 2\n18/11/2026 — Módulo 3',
      detalleMotivo:
        'Docentes para impartir tres módulos de preparación del examen complexivo de Diseño Gráfico, de 32 horas cada uno (96 horas en total).',
      formacion:
        'Título de tercer nivel en Diseño Gráfico, Publicidad, Comunicación Visual o áreas afines.',
      experiencia:
        'Mínimo dos años de experiencia profesional y docente en diseño gráfico, herramientas digitales o animación 2D/3D.',
      conocimientos:
        'Comunicación visual, identidad corporativa y packaging; Illustrator, Photoshop, InDesign, fotografía y diseño web; animación, After Effects y modelado 3D.',
      observaciones:
        'DATOS DE EJEMPLO. Los docentes deben acoplarse al horario del módulo asignado. Tarifa ilustrativa: USD 20 por hora; 96 horas = USD 1.920.',
      institucion: inst,
      contratacion: 'Servicios Profesionales',
      jornada: 'Horas',
      remuneracion: 'Factura',
      motivo: 'Proyecto temporal',
      otroMotivo: '',
      otraCompetencia: '',
    };
    Object.assign(this.datos, ejemplo);
    this.numeroVacantes = 3;
    this.valorRemuneracion = 20;
    this.valorTotal = 1920;
    this.competenciasSeleccionadas.clear();
    for (const competencia of [
      'Orientación a resultados',
      'Trabajo en equipo',
      'Comunicación efectiva',
      'Adaptabilidad',
    ]) {
      this.competenciasSeleccionadas.add(competencia);
    }
  }

  imprimir(documento: HTMLElement): void {
    printDocument(documento, 'Requisición de personal — ISTPET');
  }

  async descargarPdfOficial(): Promise<void> {
    this.generandoPdfDescarga.set(true);
    try {
      const pdf = await crearRequisicionPdf(
        this.plantillaStore.plantillaVigente(),
        this.obtenerDatosFormulario(),
      );
      pdf.save('requisicion-personal-istpet.pdf');
    } finally {
      this.generandoPdfDescarga.set(false);
    }
  }

  abrirVistaPrevia(
    dialog: HTMLDialogElement,
    frame: HTMLIFrameElement,
    documento: HTMLElement,
  ): void {
    frame.srcdoc = documentHtml(documento, 'Requisición de personal — ISTPET', true);
    dialog.showModal();
  }

  obtenerDatosFormulario(): DatosFormularioRequisicion {
    return {
      fechaSolicitud: this.datos.fechaSolicitud,
      area: this.datos.area,
      solicitante: this.datos.solicitante,
      cargo: this.datos.cargo,
      numeroVacantes: this.numeroVacantes,
      institucion: this.datos.institucion,
      contratacion: this.datos.contratacion,
      jornada: this.datos.jornada,
      remuneracion: this.datos.remuneracion,
      valorRemuneracion: this.valorRemuneracion,
      valorTotal: this.valorTotal,
      horario: this.datos.horario,
      lugar: this.datos.lugar,
      fechaIngreso: this.datos.fechaIngreso,
      motivo: this.datos.motivo,
      otroMotivo: this.datos.otroMotivo,
      detalleMotivo: this.datos.detalleMotivo,
      formacion: this.datos.formacion,
      experiencia: this.datos.experiencia,
      conocimientos: this.datos.conocimientos,
      competencias: Array.from(this.competenciasSeleccionadas),
      otraCompetencia: this.datos.otraCompetencia,
      observaciones: this.datos.observaciones,
    };
  }

  esSeccionCompleta(seccion: number): boolean {
    switch (seccion) {
      case 1:
        return Boolean(this.datos.fechaSolicitud && this.datos.area && this.datos.solicitante);
      case 2:
        return Boolean(this.datos.institucion);
      case 3:
        return Boolean(
          this.numeroVacantes &&
            this.numeroVacantes > 0 &&
            this.datos.contratacion &&
            this.datos.jornada &&
            this.datos.cargo &&
            this.datos.horario &&
            this.datos.lugar &&
            this.datos.fechaIngreso,
        );
      case 4:
        return Boolean(
          this.datos.motivo &&
            (this.datos.motivo !== 'Otro' || this.datos.otroMotivo) &&
            this.datos.detalleMotivo,
        );
      case 5:
        return Boolean(
          this.datos.formacion &&
            this.datos.experiencia &&
            this.datos.conocimientos &&
            (!this.competenciasSeleccionadas.has('Otro') || this.datos.otraCompetencia) &&
            this.competenciasSeleccionadas.size > 0,
        );
      case 6:
        return Boolean(this.datos.observaciones);
      default:
        return false;
    }
  }

  progreso(): { completadas: number; total: number; porcentaje: number } {
    let completadas = 0;
    for (let i = 1; i <= 5; i++) {
      if (this.esSeccionCompleta(i)) completadas++;
    }
    return {
      completadas,
      total: 5,
      porcentaje: Math.round((completadas / 5) * 100),
    };
  }

  aplicarTotalSugerido(): void {
    if (this.valorRemuneracion && this.valorRemuneracion > 0) {
      this.valorTotal = Number((this.valorRemuneracion * 96).toFixed(2));
    }
  }

  desplazarASeccion(id: string): void {
    if (typeof document !== 'undefined') {
      const el = document.getElementById(id);
      el?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }
}
