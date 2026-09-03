import { Component, computed, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ModalidadMaestra } from '../../../../core/models/titulacion.models';

@Component({
  selector: 'app-modalidades-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './modalidades-tab.component.html',
  styleUrls: ['./modalidades-tab.component.css'],
})
export class ModalidadesTabComponent {
  modalidades = input<ModalidadMaestra[]>([]);
  loading = input<boolean>(false);

  nuevaModalidad = output<void>();
  abrirMatriz = output<ModalidadMaestra>();
  toggleEstado = output<ModalidadMaestra>();

  // Filtros
  busqueda = signal<string>('');
  filtroEstado = signal<'TODAS' | 'ACTIVAS' | 'INACTIVAS'>('TODAS');

  // Paginación
  paginaActual = signal<number>(1);
  tamanoPagina = signal<number>(10);

  totalActivas = computed(() => this.modalidades().filter((m) => m.esActivo).length);
  totalInactivas = computed(() => this.modalidades().filter((m) => !m.esActivo).length);

  modalidadesFiltradas = computed(() => {
    const raw = this.modalidades();
    const query = this.busqueda().toLowerCase().trim();
    const est = this.filtroEstado();

    return raw.filter((m) => {
      if (est === 'ACTIVAS' && !m.esActivo) return false;
      if (est === 'INACTIVAS' && m.esActivo) return false;

      if (query) {
        const matchName = (m.modalidadTitulacion || '').toLowerCase().includes(query);
        const matchId = m.idModalidadTitulacion.toString().includes(query);
        if (!matchName && !matchId) return false;
      }
      return true;
    });
  });

  totalFiltrados = computed(() => this.modalidadesFiltradas().length);

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

  modalidadesPaginadas = computed(() => {
    const start = (this.paginaActual() - 1) * this.tamanoPagina();
    return this.modalidadesFiltradas().slice(start, start + this.tamanoPagina());
  });

  onSearchChange(val: string): void {
    this.busqueda.set(val);
    this.paginaActual.set(1);
  }

  setFiltroEstado(estado: 'TODAS' | 'ACTIVAS' | 'INACTIVAS'): void {
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
