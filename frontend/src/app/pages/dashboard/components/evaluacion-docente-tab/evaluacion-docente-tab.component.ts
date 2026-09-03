import { Component, computed, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RequisitoEvaluacionDocente } from '../../../../core/models/titulacion.models';

export interface GuardarEvaluacionEvento {
  item: RequisitoEvaluacionDocente;
  aprobado: boolean;
  observaciones: string;
  archivo?: File;
}

@Component({
  selector: 'app-evaluacion-docente-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './evaluacion-docente-tab.component.html',
  styleUrls: ['./evaluacion-docente-tab.component.css']
})
export class EvaluacionDocenteTabComponent {
  items = input<RequisitoEvaluacionDocente[]>([]);
  cargando = input<boolean>(false);
  guardandoId = input<number | null>(null);

  guardarEvaluacion = output<GuardarEvaluacionEvento>();

  // Filtros de UI
  busqueda = signal<string>('');
  filtroCumplimiento = signal<'TODOS' | 'PENDIENTES' | 'APROBADOS'>('TODOS');

  // Paginación
  paginaActual = signal<number>(1);
  tamanoPagina = signal<number>(10);

  // Estado local para edición de formulario por fila
  archivosPorFila = signal<Record<number, File>>({});
  observacionesPorFila = signal<Record<number, string>>({});
  aprobadoPorFila = signal<Record<number, boolean>>({});

  // Conteo de items
  totalPendientes = computed(() => {
    return this.items().filter(item => !this.getAprobado(item)).length;
  });

  totalAprobados = computed(() => {
    return this.items().filter(item => this.getAprobado(item)).length;
  });

  // Items filtrados y ordenados (priorizando no aprobados)
  itemsFiltrados = computed(() => {
    const rawItems = this.items();
    const query = this.busqueda().toLowerCase().trim();
    const filtro = this.filtroCumplimiento();

    return rawItems
      .filter(item => {
        const esAprob = this.getAprobado(item);
        if (filtro === 'PENDIENTES' && esAprob) return false;
        if (filtro === 'APROBADOS' && !esAprob) return false;

        if (query) {
          const matchAlumno = (item.nombreAlumno || '').toLowerCase().includes(query);
          const matchCedula = (item.cedulaAlumno || '').toLowerCase().includes(query);
          const matchReq = (item.nombreRequisito || '').toLowerCase().includes(query);
          const matchCarrera = (item.carrera || '').toLowerCase().includes(query);
          const matchExp = item.idPostulacionAlumnos.toString().includes(query);
          if (!matchAlumno && !matchCedula && !matchReq && !matchCarrera && !matchExp) {
            return false;
          }
        }
        return true;
      })
      .sort((a, b) => {
        const aAprob = this.getAprobado(a) ? 1 : 0;
        const bAprob = this.getAprobado(b) ? 1 : 0;
        if (aAprob !== bAprob) return aAprob - bAprob;
        return b.idPostulacionAlumnos - a.idPostulacionAlumnos;
      });
  });

  totalFiltrados = computed(() => this.itemsFiltrados().length);

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

  itemsPaginados = computed(() => {
    const start = (this.paginaActual() - 1) * this.tamanoPagina();
    return this.itemsFiltrados().slice(start, start + this.tamanoPagina());
  });

  setFiltro(tipo: 'PENDIENTES' | 'APROBADOS' | 'TODOS'): void {
    this.filtroCumplimiento.set(tipo);
    this.paginaActual.set(1);
  }

  onSearchChange(val: string): void {
    this.busqueda.set(val);
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

  onFileChange(idPostulacionAlumnoRequisitoModalidad: number, event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    if (inputEl.files && inputEl.files.length > 0) {
      const file = inputEl.files[0];
      this.archivosPorFila.update(map => ({
        ...map,
        [idPostulacionAlumnoRequisitoModalidad]: file
      }));
    }
  }

  getAprobado(item: RequisitoEvaluacionDocente): boolean {
    const custom = this.aprobadoPorFila()[item.idPostulacionAlumnoRequisitoModalidad];
    if (custom !== undefined) return custom;
    return item.aprobado;
  }

  setAprobado(item: RequisitoEvaluacionDocente, valor: boolean): void {
    this.aprobadoPorFila.update(map => ({
      ...map,
      [item.idPostulacionAlumnoRequisitoModalidad]: valor
    }));
  }

  getObservaciones(item: RequisitoEvaluacionDocente): string {
    const custom = this.observacionesPorFila()[item.idPostulacionAlumnoRequisitoModalidad];
    if (custom !== undefined) return custom;
    return item.observaciones || '';
  }

  setObservaciones(item: RequisitoEvaluacionDocente, valor: string): void {
    this.observacionesPorFila.update(map => ({
      ...map,
      [item.idPostulacionAlumnoRequisitoModalidad]: valor
    }));
  }

  enviarGuardado(item: RequisitoEvaluacionDocente): void {
    const aprobado = this.getAprobado(item);
    const observaciones = this.getObservaciones(item);
    const archivo = this.archivosPorFila()[item.idPostulacionAlumnoRequisitoModalidad];

    this.guardarEvaluacion.emit({
      item,
      aprobado,
      observaciones,
      archivo
    });
  }
}
