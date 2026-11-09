import { Component, computed, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  PostulacionResumen,
  EstadoPostulacion,
  PostulacionDetalle,
} from '../../../../core/models/titulacion.models';
import { ExpedienteAcademico } from '../../../../core/models/egresados.models';
import { CarreraUsuarioItem } from '../../../../core/services/carreras.service';
import { EgresadosService } from '../../../../core/services/egresados.service';
import { TitulacionService } from '../../../../core/services/titulacion.service';
import { environment } from '../../../../../environments/environment';

export interface DictamenEvento {
  idPostulacion: number;
  decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR';
  nombreAlumno?: string;
  cedulaAlumno?: string;
}

@Component({
  selector: 'app-postulaciones-bandeja',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './postulaciones-bandeja.component.html',
  styleUrls: ['./postulaciones-bandeja.component.css'],
})
export class PostulacionesBandejaComponent {
  private readonly egresadosService = inject(EgresadosService);
  private readonly titulacionService = inject(TitulacionService);

  readonly skeletonItems = Array.from({ length: 6 }, (_, i) => i);

  postulaciones = input<PostulacionResumen[]>([]);
  total = input<number>(0);
  loading = input<boolean>(false);
  estados = input<EstadoPostulacion[]>([]);
  carreras = input<CarreraUsuarioItem[]>([]);

  filtroBusqueda = input<string>('');
  filtroEstado = input<number | null>(null);
  filtroCarrera = input<number | null>(null);
  paginaActual = input<number>(1);
  tamanoPagina = input<number>(10);

  busquedaChange = output<string>();
  estadoChange = output<number | null>();
  carreraChange = output<number | null>();
  paginaChange = output<number>();
  tamanoPaginaChange = output<number>();
  refrescar = output<void>();
  dictamen = output<DictamenEvento>();

  // Estado del Modal / Drawer de Examen Integral
  mostrarExamenModal = signal<boolean>(false);
  cargandoExamen = signal<boolean>(false);
  cargandoExpedienteAcademico = signal<boolean>(false);
  errorExamen = signal<string | null>(null);
  postulacionResumenSeleccionada = signal<PostulacionResumen | null>(null);
  postulacionDetalleSeleccionada = signal<PostulacionDetalle | null>(null);
  expedienteSeleccionado = signal<ExpedienteAcademico | null>(null);
  seccionExamen = signal<'requisitos' | 'academico' | 'pagos'>('requisitos');
  semestreExpandido = signal<number | null>(null);

  totalPaginas = computed(() => {
    const t = this.total();
    const size = this.tamanoPagina() || 10;
    return Math.max(1, Math.ceil(t / size));
  });

  paginasDisponibles = computed(() => {
    const total = this.totalPaginas();
    const actual = this.paginaActual();
    const pages: number[] = [];
    const maxVisible = 5;

    let start = Math.max(1, actual - Math.floor(maxVisible / 2));
    const end = Math.min(total, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  });

  rangoInicio = computed(() => {
    if (this.total() === 0) return 0;
    return (this.paginaActual() - 1) * this.tamanoPagina() + 1;
  });

  rangoFin = computed(() => {
    return Math.min(this.paginaActual() * this.tamanoPagina(), this.total());
  });

  tieneFiltrosActivos = computed(() => {
    return (
      (this.filtroBusqueda() || '').trim().length > 0 ||
      this.filtroEstado() !== null ||
      this.filtroCarrera() !== null
    );
  });

  onSearchChange(value: string): void {
    this.busquedaChange.emit(value);
  }

  limpiarBusqueda(): void {
    this.busquedaChange.emit('');
  }

  seleccionarEstado(idEstado: number | null): void {
    const nuevo = this.filtroEstado() === idEstado ? null : idEstado;
    this.estadoChange.emit(nuevo);
  }

  onCarreraSelect(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const value = select.value ? +select.value : null;
    this.carreraChange.emit(value);
  }

  limpiarTodosFiltros(): void {
    this.busquedaChange.emit('');
    this.estadoChange.emit(null);
    this.carreraChange.emit(null);
  }

  irAPagina(pagina: number): void {
    if (pagina >= 1 && pagina <= this.totalPaginas() && pagina !== this.paginaActual()) {
      this.paginaChange.emit(pagina);
    }
  }

  onTamanoPaginaChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const size = +select.value || 10;
    this.tamanoPaginaChange.emit(size);
  }

  emitirDictamen(
    idPostulacion: number,
    decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR',
    nombreAlumno?: string,
    cedulaAlumno?: string
  ): void {
    this.dictamen.emit({ idPostulacion, decision, nombreAlumno, cedulaAlumno });
  }

  getEstadoClass(nombreEstado: string): string {
    const est = (nombreEstado || '').toUpperCase();
    if (est.includes('APROB')) return 'estado-aprobado';
    if (est.includes('REVIS') || est.includes('OBSERV')) return 'estado-observado';
    if (est.includes('RECHAZ')) return 'estado-rechazado';
    return 'estado-registrado';
  }

  getEstadoDotClass(nombreEstado: string): string {
    const est = (nombreEstado || '').toUpperCase();
    if (est.includes('APROB')) return 'status-dot--success';
    if (est.includes('REVIS') || est.includes('OBSERV')) return 'status-dot--warning';
    if (est.includes('RECHAZ')) return 'status-dot--danger';
    return 'status-dot--info';
  }

  getPorcentajeRequisitos(completados: number, total: number): number {
    if (!total || total === 0) return 0;
    return Math.min(100, Math.round((completados / total) * 100));
  }

  // ----------------------------------------------------
  // Examen Integral de Postulación y Expediente Académico
  // ----------------------------------------------------
  examinarPostulacion(p: PostulacionResumen): void {
    this.postulacionResumenSeleccionada.set(p);
    this.postulacionDetalleSeleccionada.set(null);
    this.expedienteSeleccionado.set(null);
    this.errorExamen.set(null);
    this.cargandoExamen.set(true);
    this.mostrarExamenModal.set(true);
    this.seccionExamen.set('requisitos'); // Abre directamente en requisitos y evidencias
    this.semestreExpandido.set(null);

    // Carga inmediata del detalle de la postulación (requisitos y evidencias)
    this.titulacionService.getPostulacionPorId(p.idPostulacionAlumnos).pipe(
      catchError((err) => {
        console.warn('No se pudo cargar el detalle de postulación:', err);
        return of(null);
      }),
    ).subscribe({
      next: (postulacion) => {
        this.postulacionDetalleSeleccionada.set(postulacion);
        this.cargandoExamen.set(false);
      },
      error: () => {
        this.errorExamen.set('No se pudo obtener el expediente de la postulación.');
        this.cargandoExamen.set(false);
      },
    });
  }

  cerrarExamenModal(): void {
    this.mostrarExamenModal.set(false);
    this.postulacionResumenSeleccionada.set(null);
    this.postulacionDetalleSeleccionada.set(null);
    this.expedienteSeleccionado.set(null);
    this.seccionExamen.set('requisitos');
    this.semestreExpandido.set(null);
    this.errorExamen.set(null);
  }

  cambiarSeccionExamen(seccion: 'requisitos' | 'academico' | 'pagos'): void {
    this.seccionExamen.set(seccion);

    // Carga perezosa (lazy-load) del expediente académico sólo si el usuario hace clic en Malla o Pagos
    if ((seccion === 'academico' || seccion === 'pagos') && !this.expedienteSeleccionado() && !this.cargandoExpedienteAcademico()) {
      const p = this.postulacionResumenSeleccionada();
      if (p) {
        this.cargandoExpedienteAcademico.set(true);
        this.egresadosService.getExpedienteAcademico(p.idAlumno, p.idCarrera).pipe(
          catchError((err) => {
            console.warn('No se pudo cargar el expediente académico curricular:', err);
            return of(null);
          }),
        ).subscribe((expediente) => {
          this.expedienteSeleccionado.set(expediente);
          this.cargandoExpedienteAcademico.set(false);
        });
      }
    }
  }

  toggleSemestre(idMatricula: number): void {
    this.semestreExpandido.update((curr) => (curr === idMatricula ? null : idMatricula));
  }

  imprimirExpediente(): void {
    window.print();
  }

  dictaminarDesdeExamen(decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR'): void {
    const resumen = this.postulacionResumenSeleccionada();
    if (!resumen) return;
    this.emitirDictamen(
      resumen.idPostulacionAlumnos,
      decision,
      resumen.nombreAlumno,
      resumen.cedulaAlumno
    );
  }

  getArchivoUrl(ruta?: string | null): string {
    if (!ruta) return '';
    if (ruta.startsWith('http://') || ruta.startsWith('https://') || ruta.startsWith('blob:')) {
      return ruta;
    }
    const cleanPath = ruta.startsWith('/') ? ruta : '/' + ruta;
    return `${environment.apiBaseUrl}${cleanPath}`;
  }
}
