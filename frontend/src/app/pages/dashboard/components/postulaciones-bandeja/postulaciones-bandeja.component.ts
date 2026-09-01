import { Component, computed, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostulacionResumen, EstadoPostulacion } from '../../../../core/models/titulacion.models';
import { CarreraUsuarioItem } from '../../../../core/services/carreras.service';

export interface DictamenEvento {
  idPostulacion: number;
  decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR';
}

@Component({
  selector: 'app-postulaciones-bandeja',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './postulaciones-bandeja.component.html',
  styleUrls: ['./postulaciones-bandeja.component.css'],
})
export class PostulacionesBandejaComponent {
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

  seleccionarEstado(idEstado: number): void {
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

  emitirDictamen(idPostulacion: number, decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR'): void {
    this.dictamen.emit({ idPostulacion, decision });
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
}
