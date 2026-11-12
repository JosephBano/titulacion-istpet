import { Component, input, output, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { environment } from '../../../../../environments/environment';
import {
  PortalEstudiante,
  PostulacionDetalle,
  PostulacionRequisitoDetalle,
  ModalidadOfertada,
  RequisitoModalidadOfertada,
} from '../../../../core/models/titulacion.models';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConfirmService } from '../../../../core/services/confirm.service';
import {
  ConvocatoriaCardComponent,
  EstadoPuntoTipo,
} from '../../../../shared/components/convocatoria-card/convocatoria-card.component';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';

export interface PuntoProcesoInfo {
  titulo: string;
  descripcion: string;
  tipo: EstadoPuntoTipo;
}

export interface DetalleAcademicoModalidad {
  tag: string;
  resumen: string;
  ventajas: string[];
  consideraciones: string[];
  perfil: string;
  evaluacion: string;
  duracion: string;
  entregables: string;
}

@Component({
  selector: 'app-estudiante-proceso',
  standalone: true,
  imports: [CommonModule, ConvocatoriaCardComponent, DrawerComponent],
  templateUrl: './estudiante-proceso.component.html',
  styleUrls: ['./estudiante-proceso.component.css'],
})
export class EstudianteProcesoComponent {
  private readonly notification = inject(NotificationService);
  private readonly confirmService = inject(ConfirmService);
  readonly MAX_FILE_SIZE_BYTES = 2 * 1024 * 1024; // 2 MB

  portal = input<PortalEstudiante | null>(null);
  modalidadSeleccionada = input<number | null>(null);
  postulando = input<boolean>(false);
  cambiandoModalidad = input<boolean>(false);
  subiendoRequisitoId = input<number | null>(null);
  periodoNombreAmigable = input<string>('');

  modalidadSelect = output<number>();
  enviarPostulacion = output<void>();
  cambiarModalidad = output<number>();
  subirDocumento = output<{ req: PostulacionRequisitoDetalle; file: File }>();

  // Estado del Drawer de Guía y Detalles de Modalidad
  drawerOpen = signal<boolean>(false);
  modalidadParaDrawer = signal<ModalidadOfertada | null>(null);

  // Estado del Modal de Requisitos y Explicación de la Modalidad
  modalDetalleOpen = signal<boolean>(false);
  modalidadParaModal = signal<ModalidadOfertada | null>(null);
  esModoCambioModalidad = signal<boolean>(false);

  // Estado del Drawer de Cambio de Modalidad
  modalCambioOpen = signal<boolean>(false);
  modalidadParaCambio = signal<number | null>(null);

  // Modalidad seleccionada actualmente (objeto completo)
  modalidadSeleccionadaInfo = computed<ModalidadOfertada | null>(() => {
    const id = this.modalidadSeleccionada();
    if (!id) return null;
    const lista = this.portal()?.modalidadesDisponibles || [];
    return lista.find((m) => m.idModalidadTitulacionCarrera === id) || null;
  });

  // Requisitos iniciales vs finales de la modalidad seleccionada
  requisitosInicialesModalidad = computed<RequisitoModalidadOfertada[]>(() => {
    const mod = this.modalidadSeleccionadaInfo();
    if (!mod?.requisitos) return [];
    return mod.requisitos.filter((r) => !r.esRequisitoFinal);
  });

  constructor() {
    // Si la postulación activa pasa a existir en el portal del estudiante,
    // autocerrar el modal de requisitos/postulación si continuaba abierto.
    effect(() => {
      const p = this.portal();
      if (p?.postulacionActiva && this.modalDetalleOpen() && !this.esModoCambioModalidad()) {
        this.forzarCierreModal();
      }
    });
  }

  requisitosFinalesModalidad = computed<RequisitoModalidadOfertada[]>(() => {
    const mod = this.modalidadSeleccionadaInfo();
    if (!mod?.requisitos) return [];
    return mod.requisitos.filter((r) => r.esRequisitoFinal);
  });

  modalidadesAlternativas = computed<ModalidadOfertada[]>(() => {
    const p = this.portal();
    if (!p || !p.modalidadesDisponibles) return [];
    const idActual = p.postulacionActiva?.idModalidadTitulacionCarrera;
    return p.modalidadesDisponibles.filter((m) => m.idModalidadTitulacionCarrera !== idActual);
  });

  modalidadParaCambioInfo = computed<ModalidadOfertada | null>(() => {
    const id = this.modalidadParaCambio();
    if (!id) return null;
    return (
      this.portal()?.modalidadesDisponibles.find((m) => m.idModalidadTitulacionCarrera === id) ||
      null
    );
  });

  puedeCambiarModalidad = computed<boolean>(() => {
    const p = this.portal();
    if (!p || !p.postulacionActiva) return false;
    const est = (p.postulacionActiva.nombreEstado || '').toUpperCase();
    const esFinalizada = est.includes('TITULAD') || est.includes('GRADUAD') || est.includes('ACTA');
    if (esFinalizada) return false;
    const esRechazada = est.includes('RECHAZ') || est.includes('NEGAD');
    return esRechazada || p.convocatoria.estaAbierta;
  });

  // Punto actual del estudiante en el proceso de titulación
  puntoProcesoInfo = computed<PuntoProcesoInfo>(() => {
    const p = this.portal();
    if (!p) {
      return {
        titulo: 'Cargando información',
        descripcion: 'Obteniendo el estado del proceso...',
        tipo: 'info',
      };
    }

    // 1. Caso sin postulación activa aún
    if (p.postulacionActiva === null) {
      if (!p.estudiante.esElegible) {
        return {
          titulo: 'Inhabilitado para postulación',
          descripcion:
            p.estudiante.mensajeElegibilidad ||
            'Usted no cumple con los requisitos previos para participar en el proceso de titulación.',
          tipo: 'warning',
        };
      }
      if (p.modalidadesDisponibles.length === 0) {
        return {
          titulo: 'Sin modalidades habilitadas',
          descripcion:
            'No hay modalidades habilitadas para su carrera en este momento. Comuníquese con la Coordinación de Carrera.',
          tipo: 'warning',
        };
      }
      return {
        titulo: 'Selección de modalidad',
        descripcion:
          'Seleccione una de las modalidades habilitadas para su carrera y confirme su postulación para iniciar formalmente su proceso de titulación.',
        tipo: 'info',
      };
    }

    const post = p.postulacionActiva;
    const est = (post.nombreEstado || '').toUpperCase();

    // 2. Si la postulación fue rechazada
    if (this.esPostulacionRechazada(est)) {
      return {
        titulo: 'Modalidad rechazada',
        descripcion:
          'El departamento de titulación ha rechazado su tipo de titulación. Tendrá que cambiar de modalidad para poder continuar con su proceso.',
        tipo: 'danger',
      };
    }

    // 3. Si la postulación fue aprobada
    if (this.esPostulacionAprobada(est)) {
      const requisitos = post.requisitos || [];
      const todosAprobados =
        requisitos.length > 0 && this.totalAprobados(requisitos) === requisitos.length;

      if (todosAprobados) {
        return {
          titulo: 'Requisitos cumplidos — En fase final',
          descripcion:
            'Ha completado la totalidad de requisitos académicos de su modalidad. Se encuentra en etapa de tutoría, revisión final y sustentación.',
          tipo: 'success',
        };
      }

      return {
        titulo: 'Modalidad aprobada — En curso',
        descripcion: `Su tipo de titulación (${post.modalidadTitulacion}) ha sido aprobado. Usted ha empezado este método; continúe cumpliendo y adjuntando los requisitos académicos requeridos.`,
        tipo: 'success',
      };
    }

    // 4. Si la postulación presenta observaciones
    if (est.includes('OBSERV')) {
      const obs = this.obtenerObservacionPostulacion(post);
      return {
        titulo: 'Postulación con observaciones',
        descripcion: obs
          ? `Su postulación presenta observaciones: "${obs}". Por favor subsane los requisitos observados para continuar con la validación.`
          : 'El departamento de titulación registró observaciones en sus requisitos. Revise los ítems señalados para subsanarlos.',
        tipo: 'warning',
      };
    }

    // 5. Si está finalizada / graduado
    if (est.includes('TITULAD') || est.includes('GRADUAD') || est.includes('ACTA')) {
      return {
        titulo: 'Proceso de titulación concluido',
        descripcion:
          '¡Felicitaciones! Ha culminado satisfactoriamente todos los requisitos y fases de su proceso de titulación.',
        tipo: 'success',
      };
    }

    // 6. Por defecto: En Revisión / Registrada (el punto donde el encargado valida)
    return {
      titulo: 'Validando tipo de titulación',
      descripcion:
        'El encargado del departamento de titulación se encuentra validando su tipo de titulación, en caso de ser aprobado usted empezará este método, caso contrario tendrá que cambiarlo.',
      tipo: 'info',
    };
  });

  abrirModalCambioModalidad(): void {
    const alt = this.modalidadesAlternativas();
    if (alt.length > 0) {
      this.modalidadParaCambio.set(alt[0].idModalidadTitulacionCarrera);
    }
    this.modalCambioOpen.set(true);
  }

  cerrarModalCambioModalidad(): void {
    this.modalCambioOpen.set(false);
    this.modalidadParaCambio.set(null);
  }

  seleccionarModalidadParaCambio(mod: ModalidadOfertada): void {
    this.modalCambioOpen.set(false);
    this.modalidadParaCambio.set(mod.idModalidadTitulacionCarrera);
    this.modalidadParaModal.set(mod);
    this.esModoCambioModalidad.set(true);
    this.modalDetalleOpen.set(true);
  }

  confirmarCambioDesdeModal(mod: ModalidadOfertada): void {
    this.cambiarModalidad.emit(mod.idModalidadTitulacionCarrera);
    this.forzarCierreModal();
  }

  confirmarCambioModalidad(): void {
    const id = this.modalidadParaCambio();
    if (id) {
      this.cambiarModalidad.emit(id);
      this.modalCambioOpen.set(false);
    }
  }

  esPostulacionAprobada(estado?: string): boolean {
    if (!estado) return false;
    const est = estado.toUpperCase();
    return est.includes('APROB') || est.includes('ACEPT');
  }

  esPostulacionEnRevision(estado?: string): boolean {
    if (!estado) return false;
    return !this.esPostulacionRechazada(estado) && !this.esPostulacionAprobada(estado);
  }

  elegirModalidad(id: number): void {
    this.modalidadSelect.emit(id);
  }

  abrirModalModalidad(mod: ModalidadOfertada): void {
    this.esModoCambioModalidad.set(false);
    this.modalidadParaModal.set(mod);
    this.modalDetalleOpen.set(true);
    this.modalidadSelect.emit(mod.idModalidadTitulacionCarrera);
  }

  cerrarModalModalidad(): void {
    if (this.postulando() || this.cambiandoModalidad()) return;
    this.forzarCierreModal();
  }

  forzarCierreModal(): void {
    this.modalDetalleOpen.set(false);
    this.modalidadParaModal.set(null);
    this.esModoCambioModalidad.set(false);
  }

  confirmarPostulacionDesdeModal(mod: ModalidadOfertada): void {
    this.modalidadSelect.emit(mod.idModalidadTitulacionCarrera);
    this.enviarPostulacion.emit();
  }

  getIconoTipoModalidad(
    mod: ModalidadOfertada,
  ): 'investigacion' | 'complexivo' | 'articulo' | 'general' {
    const nombre = (mod.modalidadTitulacion || '').toLowerCase();
    if (
      mod.esComplexivo === 'S' ||
      mod.esComplexivo === '1' ||
      mod.esComplexivo === 'true' ||
      nombre.includes('complexivo') ||
      nombre.includes('examen')
    ) {
      return 'complexivo';
    }
    if (
      mod.esArticuloCientifico === 'S' ||
      mod.esArticuloCientifico === '1' ||
      mod.esArticuloCientifico === 'true' ||
      nombre.includes('artículo') ||
      nombre.includes('articulo') ||
      nombre.includes('científico')
    ) {
      return 'articulo';
    }
    if (
      nombre.includes('investig') ||
      nombre.includes('proyecto') ||
      nombre.includes('propuesta') ||
      nombre.includes('técnico') ||
      nombre.includes('tecnico')
    ) {
      return 'investigacion';
    }
    return 'general';
  }

  getExplicacionProcesoModalidad(mod: ModalidadOfertada): {
    resumenFlujo: string;
    siguienteFase: string;
  } {
    const tipo = this.getIconoTipoModalidad(mod);
    switch (tipo) {
      case 'complexivo':
        return {
          resumenFlujo:
            'Al elegir Examen Complexivo, la Unidad de Titulación verificará si cumple los requerimientos académicos previos. Una vez verificado y aprobado que cumple con los requisitos, se le dejará pasar al siguiente proceso: asignación de temarios teóricos, preparación y fecha de evaluación.',
          siguienteFase:
            'Preparación teórica, resolución de caso práctico y prueba ante tribunal evaluador.',
        };
      case 'articulo':
        return {
          resumenFlujo:
            'Al elegir Artículo Científico, la Unidad de Titulación verificará si cumple los requerimientos académicos previos. Una vez verificado y aprobado su cumplimiento, se le dejará pasar al siguiente proceso: asignación de tutor investigador, redacción científica y envío a arbitraje/publicación.',
          siguienteFase:
            'Tutoría metodológica, redacción del manuscrito y validación de publicación indexada.',
        };
      case 'investigacion':
      default:
        return {
          resumenFlujo:
            'Al elegir Proyecto de Investigación, la Unidad de Titulación verifica si cumple los requerimientos académicos. Una vez que la Unidad de Titulación comprueba y aprueba el cumplimiento de los requisitos, le dejará pasar al siguiente proceso: asignación de docente tutor, desarrollo del proyecto y sustentación final.',
          siguienteFase:
            'Asignación de tutor institucional, desarrollo de la propuesta técnica y defensa de grado.',
        };
    }
  }

  abrirGuiaModalidad(mod: ModalidadOfertada, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.modalidadParaDrawer.set(mod);
    this.drawerOpen.set(true);
  }

  cerrarGuiaModalidad(): void {
    this.drawerOpen.set(false);
  }

  seleccionarDesdeDrawer(): void {
    const mod = this.modalidadParaDrawer();
    if (mod) {
      this.modalidadSelect.emit(mod.idModalidadTitulacionCarrera);
      this.drawerOpen.set(false);
    }
  }

  getDetalleModalidad(mod: ModalidadOfertada): DetalleAcademicoModalidad {
    const nombre = (mod.modalidadTitulacion || '').toLowerCase();
    const esComplexivo =
      mod.esComplexivo === 'S' ||
      mod.esComplexivo === '1' ||
      mod.esComplexivo === 'true' ||
      nombre.includes('complexivo') ||
      nombre.includes('examen');
    const esArticulo =
      mod.esArticuloCientifico === 'S' ||
      mod.esArticuloCientifico === '1' ||
      mod.esArticuloCientifico === 'true' ||
      nombre.includes('artículo') ||
      nombre.includes('articulo') ||
      nombre.includes('científico');

    if (esComplexivo) {
      return {
        tag: 'Evaluación Integral Teórico-Práctica',
        resumen:
          'Evaluación estandarizada que valida los conocimientos teóricos del pensum y la resolución de un caso práctico o reactivos aplicados a la profesión.',
        ventajas: [
          'Cronograma estructurado con fechas y fases precisas.',
          'No requiere redacción extensa de tesis ni desarrollo de prototipos a largo plazo.',
          'Temarios delimitados y sesiones de acompañamiento/repaso institucional.',
        ],
        consideraciones: [
          'Exige dominio integral de las asignaturas troncales de la carrera.',
          'Evaluación puntual y concentrada frente al tribunal examinador.',
        ],
        perfil:
          'Ideal para estudiantes con buen dominio conceptual y práctico que buscan titularse ágilmente en el menor tiempo.',
        evaluacion:
          'Consta de dos componentes: 1) Examen Teórico (banco de reactivos) y 2) Examen Práctico / Resolución de caso práctico ante tribunal.',
        duracion: 'Conforme al cronograma de la cohorte ordinaria (aprox. 4 a 8 semanas).',
        entregables:
          'Rendición de pruebas teóricas y defensa del caso práctico asignado en acta oficial.',
      };
    }

    if (esArticulo) {
      return {
        tag: 'Investigación y Publicación Científica',
        resumen:
          'Elaboración de un manuscrito de rigor científico o tecnológico orientado a su difusión en revistas indexadas o memorias de congresos avalados por ISTPET.',
        ventajas: [
          'Alto valor curricular para postulaciones a maestrías, doctorados o docencia universitaria.',
          'Enfoque analítico y metodológico con mentoría especializada.',
          'Reconocimiento institucional por producción investigativa.',
        ],
        consideraciones: [
          'Requiere dominio de metodología de investigación y redacción académica.',
          'Depende de tiempos de arbitraje y dictamen del comité editorial.',
        ],
        perfil:
          'Recomendado para estudiantes con vocación investigativa, analítica o interés en estudios de posgrado futuros.',
        evaluacion:
          'Revisión metodológica por pares evaluadores, carta de aceptación/publicación y sustentación pública.',
        duracion: 'Desarrollo en cohorte académica con entregas de avances según cronograma.',
        entregables:
          'Artículo completo en formato normado (IEEE, APA o formato institucional) y carta de arbitraje.',
      };
    }

    // Modalidad estándar: Proyecto de Titulación / Desarrollo Tecnológico / TIC
    return {
      tag: 'Desarrollo Tecnológico Aplicado',
      resumen:
        'Diseño y desarrollo de una propuesta técnica, sistema informático, dispositivo o proyecto aplicado que solventa un problema real en una organización o comunidad.',
      ventajas: [
        'Acompañamiento personalizado continuo con un docente tutor asignado.',
        'Posibilidad de aplicar el proyecto directamente a su entorno laboral o empresa.',
        'Generación de un producto o software tangible para portafolio profesional.',
      ],
      consideraciones: [
        'Requiere constancia en entregas periódicas de informe técnico y desarrollo.',
        'Mayor inversión de tiempo en documentación, pruebas y validaciones.',
      ],
      perfil:
        'Recomendado para estudiantes orientados a la implementación práctica, innovación técnica o que cuenten con un proyecto en su centro de trabajo.',
      evaluacion:
        'Aprobación de informe final por el docente tutor, dictamen de revisor y defensa oral con demostración práctica ante tribunal.',
      duracion: 'A lo largo del período académico de la cohorte con cronograma de tutorías.',
      entregables:
        'Documento técnico institucional (memoria técnica), prototipo/software funcional y manuales.',
    };
  }

  sanitizarParaNombreArchivo(texto: string): string {
    return texto
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-zA-Z0-9_\-\s]/g, '')
      .trim()
      .replace(/\s+/g, '_');
  }

  generarNombreArchivoRequisito(nombreRequisito: string): string {
    const reqLimpio = this.sanitizarParaNombreArchivo(nombreRequisito);
    const now = new Date();
    const yyyy = now.getFullYear();
    const mm = String(now.getMonth() + 1).padStart(2, '0');
    const dd = String(now.getDate()).padStart(2, '0');
    const fecha = `${yyyy}-${mm}-${dd}`;

    const rawPeriodo =
      this.portal()?.convocatoria?.periodo || this.periodoNombreAmigable() || 'PERIODO';
    const periodoLimpio = this.sanitizarParaNombreArchivo(rawPeriodo);

    return `${reqLimpio}_${fecha}_${periodoLimpio}.pdf`;
  }

  async onFileChange(req: PostulacionRequisitoDetalle, event: Event): Promise<void> {
    const inputEl = event.target as HTMLInputElement;
    if (inputEl && inputEl.files && inputEl.files.length > 0) {
      const originalFile = inputEl.files[0];

      if (!this.puedeSubirOReemplazar(req, this.portal()?.postulacionActiva?.nombreEstado)) {
        this.notification.warning(
          'El documento ya fue enviado y se encuentra en revisión o ya fue aprobado. Solo se permite reemplazar si es observado o rechazado.',
        );
        inputEl.value = '';
        return;
      }

      // Validación 1: Formato estrictamente PDF
      const esPdf =
        originalFile.type === 'application/pdf' || originalFile.name.toLowerCase().endsWith('.pdf');
      if (!esPdf) {
        this.notification.error(
          `Formato inválido: El documento "${originalFile.name}" debe ser obligatoriamente un archivo PDF (.pdf).`,
        );
        inputEl.value = '';
        return;
      }

      // Validación 2: Límite estricto de tamaño 2 MB
      if (originalFile.size > this.MAX_FILE_SIZE_BYTES) {
        const tamanoMb = (originalFile.size / (1024 * 1024)).toFixed(2);
        this.notification.error(
          `El archivo supera el límite: "${originalFile.name}" pesa ${tamanoMb} MB. El tamaño máximo permitido es de 2 MB.`,
        );
        inputEl.value = '';
        return;
      }

      // Renombrar archivo según convención: [nombre_requisito]_[fecha]_[cod_periodo].pdf
      const nuevoNombre = this.generarNombreArchivoRequisito(req.nombreRequisito);
      const file = new File([originalFile], nuevoNombre, { type: 'application/pdf' });

      const tamanoKb = (file.size / 1024).toFixed(1);
      const confirmado = await this.confirmService.confirm({
        title: '¿Confirmar subida de documento?',
        message:
          'Por favor, verifique que el documento adjunto sea legible, oficial y corresponda al requisito seleccionado.',
        confirmText: 'Sí, subir documento',
        cancelText: 'Cancelar',
        variant: 'primary',
        icon: 'upload',
        documentName: nuevoNombre,
        details: [
          { label: 'Requisito', value: req.nombreRequisito },
          { label: 'Nombre institucional', value: nuevoNombre },
          { label: 'Archivo original', value: originalFile.name },
          { label: 'Tamaño del archivo', value: `${tamanoKb} KB` },
          { label: 'Formato', value: 'Documento PDF (.pdf)' },
        ],
      });

      if (confirmado) {
        this.subirDocumento.emit({ req, file });
      }
      inputEl.value = '';
    }
  }

  obtenerEtapaStepper(estado?: string): number {
    if (!estado) return 1;
    const est = estado.toUpperCase();
    if (est.includes('REGISTRAD') || est.includes('POSTULAD')) return 1;
    if (est.includes('REVIS') || est.includes('PENDIENT') || est.includes('OBSERV')) return 2;
    if (est.includes('APROBAD') || est.includes('MODALIDAD')) return 3;
    if (est.includes('EVALUA') || est.includes('COMPLEXIVO') || est.includes('TUTOR')) return 4;
    if (est.includes('TITULAD') || est.includes('GRADUAD') || est.includes('ACTA')) return 5;
    return 2;
  }

  totalAprobados(requisitos?: PostulacionRequisitoDetalle[]): number {
    if (!requisitos) return 0;
    return requisitos.filter((r) => this.esRequisitoAprobado(r)).length;
  }

  obtenerEstadoTexto(req: PostulacionRequisitoDetalle, postulacionEstado?: string): string {
    if (this.esRequisitoAprobado(req)) return 'Aprobado';
    if (this.esPostulacionRechazada(postulacionEstado)) return 'Rechazado';
    const est = (req.estadoValidacion || '').toUpperCase();
    if (est === 'OBSERVADO' || est.includes('OBSERV')) return 'Observado';
    if (this.esRequisitoRechazado(req)) return 'Rechazado';
    if (req.subeAlumno) {
      if (req.nombreArchivoAdjunto || req.rutaArchivoAdjunto) {
        return 'Subido (En Revisión)';
      }
      return 'Pendiente de Subida';
    }
    if (est === 'PENDIENTE') return 'Pendiente';
    return req.estadoValidacion || 'En Validación';
  }

  obtenerClaseEstado(req: PostulacionRequisitoDetalle, postulacionEstado?: string): string {
    if (this.esRequisitoAprobado(req)) return 'status-pill--success';
    if (this.esRequisitoRechazado(req) || this.esPostulacionRechazada(postulacionEstado)) {
      return 'status-pill--danger';
    }
    if (req.subeAlumno && (req.nombreArchivoAdjunto || req.rutaArchivoAdjunto)) {
      return 'status-pill--info';
    }
    return 'status-pill--warning';
  }

  getArchivoUrl(ruta?: string | null): string {
    if (!ruta) return '';
    if (ruta.startsWith('http://') || ruta.startsWith('https://') || ruta.startsWith('blob:')) {
      return ruta;
    }
    const cleanPath = ruta.startsWith('/') ? ruta : '/' + ruta;
    return `${environment.apiBaseUrl}${cleanPath}`;
  }

  obtenerTextoTooltip(req: PostulacionRequisitoDetalle): string {
    if (req.nombreEvaluador) {
      return `Este requisito debe ser validado por: ${req.nombreEvaluador}`;
    }
    return 'Este requisito debe ser validado por el docente asignado';
  }

  esRequisitoAprobado(req: PostulacionRequisitoDetalle): boolean {
    const est = (req.estadoValidacion || '').toUpperCase();
    if (
      est === 'RECHAZADO' ||
      est === 'OBSERVADO' ||
      est.includes('RECHAZ') ||
      est.includes('OBSERV')
    ) {
      return false;
    }
    return est === 'APROBADO' || req.valorBool === true;
  }

  esRequisitoRechazado(req: PostulacionRequisitoDetalle): boolean {
    if (this.esRequisitoAprobado(req)) return false;
    const est = (req.estadoValidacion || '').toUpperCase();
    return (
      est === 'RECHAZADO' || est === 'OBSERVADO' || est.includes('RECHAZ') || est.includes('OBSERV')
    );
  }

  esRequisitoPendiente(req: PostulacionRequisitoDetalle): boolean {
    return !this.esRequisitoAprobado(req) && !this.esRequisitoRechazado(req);
  }

  puedeSubirOReemplazar(req: PostulacionRequisitoDetalle, postulacionEstado?: string): boolean {
    // Si la postulación completa ya fue rechazada, no se permite subir nada
    if (this.esPostulacionRechazada(postulacionEstado)) return false;
    // Si el requisito ya fue aprobado, queda congelado y no se puede volver a subir
    if (this.esRequisitoAprobado(req)) return false;

    const tieneArchivo = !!(
      req.nombreArchivoAdjunto ||
      req.rutaArchivoAdjunto ||
      req.idAdjuntosImagenes
    );
    // Si ya tiene un archivo subido, solo puede reemplazar si fue RECHAZADO u OBSERVADO
    if (tieneArchivo) {
      return this.esRequisitoRechazado(req);
    }

    // Si aún no tiene archivo subido y no está aprobado, sí puede subir por primera vez
    return true;
  }

  esPostulacionRechazada(estado?: string): boolean {
    if (!estado) return false;
    const est = estado.toUpperCase();
    return est.includes('RECHAZ') || est.includes('NEGAD');
  }

  requisitosFaltantes(requisitos?: PostulacionRequisitoDetalle[]): PostulacionRequisitoDetalle[] {
    if (!requisitos) return [];
    return requisitos.filter((r) => !this.esRequisitoAprobado(r));
  }

  tieneRequisitosFinalesPendientes(requisitos?: PostulacionRequisitoDetalle[]): boolean {
    if (!requisitos) return false;
    return requisitos.some((r) => r.esRequisitoFinal && !this.esRequisitoAprobado(r));
  }

  obtenerObservacionPostulacion(postulacion: PostulacionDetalle | null): string | null {
    if (!postulacion) return null;
    if (
      postulacion.observacionDictamen &&
      typeof postulacion.observacionDictamen === 'string' &&
      postulacion.observacionDictamen.trim().length > 0
    ) {
      return postulacion.observacionDictamen.trim();
    }
    if (postulacion.requisitos && Array.isArray(postulacion.requisitos)) {
      const conObs = postulacion.requisitos.find(
        (r) =>
          r.observaciones &&
          typeof r.observaciones === 'string' &&
          r.observaciones.trim().length > 0,
      );
      if (conObs && conObs.observaciones) {
        return conObs.observaciones.trim();
      }
    }
    return null;
  }
}
