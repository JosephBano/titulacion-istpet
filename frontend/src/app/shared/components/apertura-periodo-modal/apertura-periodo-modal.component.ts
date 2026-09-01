import { Component, input, output, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import {
  AperturarPeriodoRequest,
  ModalidadMaestra,
  ModalidadCarreraDto,
} from '../../../core/models/titulacion.models';
import { TitulacionService } from '../../../core/services/titulacion.service';

@Component({
  selector: 'app-apertura-periodo-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  templateUrl: './apertura-periodo-modal.component.html',
  styleUrls: ['./apertura-periodo-modal.component.css'],
})
export class AperturaPeriodoModalComponent implements OnInit {
  private readonly titulacionService = inject(TitulacionService);

  visible = input<boolean>(false);

  modalClose = output<void>();
  confirm = output<AperturarPeriodoRequest>();

  periodos = signal<{ idPeriodo: string; nombre: string; esActivo?: boolean }[]>([]);
  modalidadesCarreras = signal<ModalidadCarreraDto[]>([]);
  modalidades = signal<ModalidadMaestra[]>([]);

  // Filtros de búsqueda para carreras
  filtroTextoCarrera = signal<string>('');
  filtroModalidadEstudio = signal<string>('TODAS');

  // Selección interactiva por idModalidadCarrera
  carrerasSeleccionadas = signal<Set<number>>(new Set());
  modalidadesSeleccionadas = signal<Set<number>>(new Set());

  // Carreras filtradas computadas
  carrerasFiltradas = computed(() => {
    let list = this.modalidadesCarreras();
    const texto = this.filtroTextoCarrera().trim().toLowerCase();
    const mod = this.filtroModalidadEstudio();

    if (texto) {
      list = list.filter(
        (c) =>
          c.nombreCarrera.toLowerCase().includes(texto) ||
          (c.aliasCarrera && c.aliasCarrera.toLowerCase().includes(texto)) ||
          c.nombreModalidadEstudio.toLowerCase().includes(texto),
      );
    }

    if (mod !== 'TODAS') {
      list = list.filter((c) => c.nombreModalidadEstudio.toUpperCase() === mod.toUpperCase());
    }

    return list;
  });

  // Modalidades de estudio únicas disponibles para el filtro
  modalidadesEstudioDisponibles = computed(() => {
    const list = this.modalidadesCarreras();
    const set = new Set<string>();
    for (const c of list) {
      if (c.nombreModalidadEstudio) {
        set.add(c.nombreModalidadEstudio.toUpperCase());
      }
    }
    return Array.from(set);
  });

  form = signal({
    idPeriodo: 'ABR2026',
    detalleConvocatoria: 'Convocatoria Ordinaria ABR2026',
    fechaInicioCorte: new Date(),
    fechaFinCorte: new Date(Date.now() + 45 * 24 * 60 * 60 * 1000),
    diasPermitidos: 90,
    diasExtension: 30,
    habilitarTodasLasCarreras: true,
  });

  ngOnInit(): void {
    this.cargarDatosIniciales();
  }

  cargarDatosIniciales(): void {
    // 1. Cargar períodos de ISTPET
    this.titulacionService.getPeriodosAcademicos().subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.periodos.set(data);
          const primer = data[0].idPeriodo;
          this.form.update((f) => ({
            ...f,
            idPeriodo: primer,
            detalleConvocatoria: `Convocatoria Ordinaria ${primer}`.substring(0, 45),
          }));
        }
      },
      error: () => this.periodos.set([]),
    });

    // 2. Cargar modalidades carreras desde modalidades_carreras
    this.titulacionService.getModalidadesCarreras(true).subscribe({
      next: (data: ModalidadCarreraDto[]) => {
        this.modalidadesCarreras.set(data || []);
        // Inicializar todas seleccionadas por defecto
        const set = new Set<number>((data || []).map((c) => c.idModalidadCarrera));
        this.carrerasSeleccionadas.set(set);
      },
      error: () => this.modalidadesCarreras.set([]),
    });

    // 3. Cargar modalidades maestras de titulación
    this.titulacionService.getModalidadesMaestras(true).subscribe({
      next: (data: ModalidadMaestra[]) => {
        const activas = (data || []).filter((m) => m.esActivo);
        this.modalidades.set(activas);
        // Inicializar todas seleccionadas
        const set = new Set<number>(activas.map((m) => m.idModalidadTitulacion));
        this.modalidadesSeleccionadas.set(set);
      },
      error: () => this.modalidades.set([]),
    });
  }

  onPeriodoSelect(idPeriodo: string): void {
    const detalleCorto = `Convocatoria Ordinaria ${idPeriodo}`.substring(0, 45);
    this.form.update((f) => ({
      ...f,
      idPeriodo,
      detalleConvocatoria: detalleCorto,
    }));
  }

  toggleHabilitarTodasLasCarreras(todas: boolean): void {
    this.form.update((f) => ({ ...f, habilitarTodasLasCarreras: todas }));
    if (todas) {
      const set = new Set<number>(this.modalidadesCarreras().map((c) => c.idModalidadCarrera));
      this.carrerasSeleccionadas.set(set);
    }
  }

  toggleCarrera(idModalidadCarrera: number): void {
    const set = new Set(this.carrerasSeleccionadas());
    if (set.has(idModalidadCarrera)) {
      set.delete(idModalidadCarrera);
    } else {
      set.add(idModalidadCarrera);
    }
    this.carrerasSeleccionadas.set(set);
  }

  seleccionarTodasCarreras(): void {
    const set = new Set<number>(this.modalidadesCarreras().map((c) => c.idModalidadCarrera));
    this.carrerasSeleccionadas.set(set);
  }

  deseleccionarTodasCarreras(): void {
    this.carrerasSeleccionadas.set(new Set<number>());
  }

  toggleModalidad(idModalidad: number): void {
    const set = new Set(this.modalidadesSeleccionadas());
    if (set.has(idModalidad)) {
      set.delete(idModalidad);
    } else {
      set.add(idModalidad);
    }
    this.modalidadesSeleccionadas.set(set);
  }

  onConfirm(): void {
    const f = this.form();
    const idsModalidadesCarreras = f.habilitarTodasLasCarreras
      ? undefined
      : Array.from(this.carrerasSeleccionadas());

    const idsModalidades =
      this.modalidadesSeleccionadas().size > 0
        ? Array.from(this.modalidadesSeleccionadas())
        : undefined;

    const finCorte = new Date(f.fechaFinCorte);
    finCorte.setHours(23, 59, 59, 0);

    this.confirm.emit({
      idPeriodo: f.idPeriodo,
      detalleConvocatoria: f.detalleConvocatoria,
      fechaInicioCorte: new Date(f.fechaInicioCorte).toISOString(),
      fechaFinCorte: finCorte.toISOString(),
      diasPermitidos: Number(f.diasPermitidos) || 90,
      diasExtension: Number(f.diasExtension) || 30,
      habilitarTodasLasCarreras: f.habilitarTodasLasCarreras,
      idsModalidadesCarrerasHabilitadas: idsModalidadesCarreras,
      idsModalidadesHabilitadas: idsModalidades,
    });
  }

  onClose(): void {
    this.modalClose.emit();
  }
}
