import { Component, input, output, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  PortalEstudiante,
  PostulacionDetalle,
  PostulacionRequisitoDetalle,
  ModalidadOfertada,
  RequisitoModalidadOfertada,
} from '../../../../core/models/titulacion.models';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConvocatoriaCardComponent } from '../../../../shared/components/convocatoria-card/convocatoria-card.component';
import { StepperComponent } from '../../../../shared/components/stepper/stepper.component';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';

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
  imports: [CommonModule, ConvocatoriaCardComponent, StepperComponent, DrawerComponent],
  templateUrl: './estudiante-proceso.component.html',
  styleUrls: ['./estudiante-proceso.component.css'],
})
export class EstudianteProcesoComponent {
  private readonly notification = inject(NotificationService);
  readonly MAX_FILE_SIZE_BYTES = 2 * 1024 * 1024; // 2 MB

  portal = input<PortalEstudiante | null>(null);
  modalidadSeleccionada = input<number | null>(null);
  postulando = input<boolean>(false);
  subiendoRequisitoId = input<number | null>(null);
  periodoNombreAmigable = input<string>('');

  modalidadSelect = output<number>();
  enviarPostulacion = output<void>();
  subirDocumento = output<{ req: PostulacionRequisitoDetalle; file: File }>();

  // Estado del Drawer de Guía y Detalles de Modalidad
  drawerOpen = signal<boolean>(false);
  modalidadParaDrawer = signal<ModalidadOfertada | null>(null);

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

  requisitosFinalesModalidad = computed<RequisitoModalidadOfertada[]>(() => {
    const mod = this.modalidadSeleccionadaInfo();
    if (!mod?.requisitos) return [];
    return mod.requisitos.filter((r) => r.esRequisitoFinal);
  });

  elegirModalidad(id: number): void {
    this.modalidadSelect.emit(id);
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

  onFileChange(req: PostulacionRequisitoDetalle, event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    if (inputEl && inputEl.files && inputEl.files.length > 0) {
      const file = inputEl.files[0];

      // Validación 1: Formato estrictamente PDF
      const esPdf = file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf');
      if (!esPdf) {
        this.notification.error(
          `Formato inválido: El documento "${file.name}" debe ser obligatoriamente un archivo PDF (.pdf).`,
        );
        inputEl.value = '';
        return;
      }

      // Validación 2: Límite estricto de tamaño 2 MB
      if (file.size > this.MAX_FILE_SIZE_BYTES) {
        const tamanoMb = (file.size / (1024 * 1024)).toFixed(2);
        this.notification.error(
          `El archivo supera el límite: "${file.name}" pesa ${tamanoMb} MB. El tamaño máximo permitido es de 2 MB.`,
        );
        inputEl.value = '';
        return;
      }

      this.subirDocumento.emit({ req, file });
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
    return requisitos.filter((r) => r.valorBool === true || r.estadoValidacion === 'APROBADO')
      .length;
  }

  esRequisitoAprobado(req: PostulacionRequisitoDetalle): boolean {
    const est = (req.estadoValidacion || '').toUpperCase();
    return (
      est === 'APROBADO' || (req.valorBool === true && est !== 'RECHAZADO' && est !== 'OBSERVADO')
    );
  }

  esRequisitoRechazado(req: PostulacionRequisitoDetalle): boolean {
    if (this.esRequisitoAprobado(req)) return false;
    const est = (req.estadoValidacion || '').toUpperCase();
    return est === 'RECHAZADO' || est === 'OBSERVADO' || est.includes('RECHAZ');
  }

  esRequisitoPendiente(req: PostulacionRequisitoDetalle): boolean {
    return !this.esRequisitoAprobado(req) && !this.esRequisitoRechazado(req);
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
