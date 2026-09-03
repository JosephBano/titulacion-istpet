import { Component, computed, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ConvocatoriaDetalle,
  ConvocatoriaResumen,
} from '../../../../core/models/titulacion.models';
import { ConvocatoriaCardComponent } from '../../../../shared/components/convocatoria-card/convocatoria-card.component';

@Component({
  selector: 'app-cohortes-tab',
  standalone: true,
  imports: [CommonModule, FormsModule, ConvocatoriaCardComponent],
  templateUrl: './cohortes-tab.component.html',
  styleUrls: ['./cohortes-tab.component.css'],
})
export class CohortesTabComponent {
  convocatoriaActiva = input<ConvocatoriaDetalle | null>(null);
  convocatoriasLista = input<ConvocatoriaResumen[]>([]);
  loading = input<boolean>(false);
  periodoNombreAmigable = input<string>('');

  aperturarNuevo = output<void>();

  // Filtros
  busqueda = signal<string>('');
  filtroEstado = signal<'TODAS' | 'VIGENTES' | 'CERRADAS'>('TODAS');

  // Paginación
  paginaActual = signal<number>(1);
  tamanoPagina = signal<number>(10);

  totalVigentes = computed(
    () => this.convocatoriasLista().filter((c) => c.estaVigenteCorte).length,
  );
  totalCerradas = computed(
    () => this.convocatoriasLista().filter((c) => !c.estaVigenteCorte).length,
  );

  convocatoriasFiltradas = computed(() => {
    const raw = this.convocatoriasLista();
    const query = this.busqueda().toLowerCase().trim();
    const est = this.filtroEstado();

    return raw.filter((c) => {
      if (est === 'VIGENTES' && !c.estaVigenteCorte) return false;
      if (est === 'CERRADAS' && c.estaVigenteCorte) return false;

      if (query) {
        const matchPeriodo = (c.idPeriodo || '').toLowerCase().includes(query);
        const matchDetalle = (c.detalle || '').toLowerCase().includes(query);
        const matchId = c.idCohorte.toString().includes(query);
        if (!matchPeriodo && !matchDetalle && !matchId) return false;
      }
      return true;
    });
  });

  totalFiltrados = computed(() => this.convocatoriasFiltradas().length);

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

  convocatoriasPaginadas = computed(() => {
    const start = (this.paginaActual() - 1) * this.tamanoPagina();
    return this.convocatoriasFiltradas().slice(start, start + this.tamanoPagina());
  });

  onSearchChange(val: string): void {
    this.busqueda.set(val);
    this.paginaActual.set(1);
  }

  setFiltroEstado(estado: 'TODAS' | 'VIGENTES' | 'CERRADAS'): void {
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
