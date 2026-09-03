import { Component, computed, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RequisitoMaestro } from '../../../../core/models/titulacion.models';

@Component({
  selector: 'app-requisitos-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './requisitos-tab.component.html',
  styleUrls: ['./requisitos-tab.component.css'],
})
export class RequisitosTabComponent {
  requisitos = input<RequisitoMaestro[]>([]);
  loading = input<boolean>(false);

  nuevoRequisito = output<void>();
  toggleEstado = output<RequisitoMaestro>();
  gestionarResponsables = output<RequisitoMaestro>();

  // Filtros
  busqueda = signal<string>('');
  filtroEstado = signal<'TODOS' | 'ACTIVOS' | 'INACTIVOS'>('TODOS');
  filtroTipo = signal<'TODOS' | 'PDF' | 'BOOL'>('TODOS');

  // Paginación
  paginaActual = signal<number>(1);
  tamanoPagina = signal<number>(10);

  // Conteo
  totalActivos = computed(() => this.requisitos().filter((r) => r.esActivo).length);
  totalInactivos = computed(() => this.requisitos().filter((r) => !r.esActivo).length);

  requisitosFiltrados = computed(() => {
    const raw = this.requisitos();
    const query = this.busqueda().toLowerCase().trim();
    const est = this.filtroEstado();
    const tipo = this.filtroTipo();

    return raw.filter((r) => {
      if (est === 'ACTIVOS' && !r.esActivo) return false;
      if (est === 'INACTIVOS' && r.esActivo) return false;

      if (tipo === 'PDF' && !r.esAdjunto) return false;
      if (tipo === 'BOOL' && r.esAdjunto) return false;

      if (query) {
        const matchName = (r.requisito || '').toLowerCase().includes(query);
        const matchId = r.idRequisitos.toString().includes(query);
        if (!matchName && !matchId) return false;
      }
      return true;
    });
  });

  totalFiltrados = computed(() => this.requisitosFiltrados().length);

  totalPaginas = computed(() => {
    const t = this.totalFiltrados();
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
    if (this.totalFiltrados() === 0) return 0;
    return (this.paginaActual() - 1) * this.tamanoPagina() + 1;
  });

  rangoFin = computed(() => {
    return Math.min(this.paginaActual() * this.tamanoPagina(), this.totalFiltrados());
  });

  requisitosPaginados = computed(() => {
    const start = (this.paginaActual() - 1) * this.tamanoPagina();
    return this.requisitosFiltrados().slice(start, start + this.tamanoPagina());
  });

  onSearchChange(val: string): void {
    this.busqueda.set(val);
    this.paginaActual.set(1);
  }

  setFiltroEstado(estado: 'TODOS' | 'ACTIVOS' | 'INACTIVOS'): void {
    this.filtroEstado.set(estado);
    this.paginaActual.set(1);
  }

  irAPagina(p: number): void {
    if (p >= 1 && p <= this.totalPaginas()) {
      this.paginaActual.set(p);
    }
  }

  onTamanoPaginaChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.tamanoPagina.set(+select.value || 10);
    this.paginaActual.set(1);
  }
}
