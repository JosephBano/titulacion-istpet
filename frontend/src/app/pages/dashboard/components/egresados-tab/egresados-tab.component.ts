import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EgresadosService } from '../../../../core/services/egresados.service';
import {
  EstudiantePendienteEgreso,
  FiltroPendientesEgreso,
  ExpedienteAcademico,
  PeriodoItem,
  CarreraItem,
  ModalidadItem,
} from '../../../../core/models/egresados.models';

@Component({
  selector: 'app-egresados-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './egresados-tab.component.html',
  styleUrls: ['./egresados-tab.component.css'],
})
export class EgresadosTabComponent implements OnInit {
  private readonly egresadosService = inject(EgresadosService);

  // Estados reactivos principales
  estudiantes = signal<EstudiantePendienteEgreso[]>([]);
  totalRegistros = signal<number>(0);
  cargando = signal<boolean>(false);
  error = signal<string | null>(null);

  // Filtros
  periodos = signal<PeriodoItem[]>([]);
  carreras = signal<CarreraItem[]>([]);
  modalidades = signal<ModalidadItem[]>([]);

  periodoSeleccionado = signal<string>('');
  carreraSeleccionada = signal<number | null>(null);
  modalidadSeleccionada = signal<number | null>(null);
  busquedaCedula = signal<string>('');

  paginaActual = signal<number>(1);
  tamanoPagina = signal<number>(10);

  // Expediente Detalle Drawer/Modal
  expedienteSeleccionado = signal<ExpedienteAcademico | null>(null);
  cargandoExpediente = signal<boolean>(false);
  mostrarExpedienteModal = signal<boolean>(false);

  // Paginación computada
  totalPaginas = computed(() => {
    const total = this.totalRegistros();
    const size = this.tamanoPagina();
    return Math.max(1, Math.ceil(total / size));
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

  ngOnInit(): void {
    this.cargarFiltrosIniciales();
  }

  private cargarFiltrosIniciales(): void {
    // 1. Cargar Periodos
    this.egresadosService.getPeriodos().subscribe({
      next: (res: PeriodoItem[]) => {
        this.periodos.set(res || []);
        // Seleccionar periodo activo por defecto
        const activo = res.find((p) => p.activo);
        if (activo) {
          this.periodoSeleccionado.set(activo.idPeriodo);
        } else if (res.length > 0) {
          this.periodoSeleccionado.set(res[0].idPeriodo);
        }
        this.cargarEstudiantes();
      },
      error: () => {
        this.cargarEstudiantes();
      },
    });

    // 2. Cargar Carreras
    this.egresadosService.getCarreras().subscribe({
      next: (res: CarreraItem[]) => {
        this.carreras.set(res || []);
      },
    });

    // 3. Cargar Modalidades
    this.egresadosService.getModalidades().subscribe({
      next: (res: ModalidadItem[]) => {
        this.modalidades.set(res || []);
      },
    });
  }

  cargarEstudiantes(): void {
    this.cargando.set(true);
    this.error.set(null);

    const filtro: FiltroPendientesEgreso = {
      idPeriodo: this.periodoSeleccionado() || undefined,
      idCarrera: this.carreraSeleccionada() || undefined,
      idModalidad: this.modalidadSeleccionada() || undefined,
      cedulaOrNombre: this.busquedaCedula().trim() || undefined,
      pagina: this.paginaActual(),
      tamanoPagina: this.tamanoPagina(),
    };

    this.egresadosService.getPendientesEgreso(filtro).subscribe({
      next: (res) => {
        this.estudiantes.set(res.items || []);
        this.totalRegistros.set(res.totalRegistros || 0);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el listado de estudiantes pendientes.');
        this.cargando.set(false);
      },
    });
  }

  onPeriodoChange(periodo: string): void {
    this.periodoSeleccionado.set(periodo);
    this.paginaActual.set(1);
    this.cargarEstudiantes();
  }

  onCarreraChange(idCarrera: number | null): void {
    this.carreraSeleccionada.set(idCarrera);
    this.paginaActual.set(1);
    this.cargarEstudiantes();
  }

  onBusquedaSubmit(): void {
    this.paginaActual.set(1);
    this.cargarEstudiantes();
  }

  limpiarFiltros(): void {
    const activo = this.periodos().find((p) => p.activo);
    this.periodoSeleccionado.set(activo ? activo.idPeriodo : (this.periodos()[0]?.idPeriodo || ''));
    this.carreraSeleccionada.set(null);
    this.modalidadSeleccionada.set(null);
    this.busquedaCedula.set('');
    this.paginaActual.set(1);
    this.cargarEstudiantes();
  }

  irAPagina(p: number): void {
    if (p >= 1 && p <= this.totalPaginas() && p !== this.paginaActual()) {
      this.paginaActual.set(p);
      this.cargarEstudiantes();
    }
  }

  cambiarTamanoPagina(size: number): void {
    this.tamanoPagina.set(size);
    this.paginaActual.set(1);
    this.cargarEstudiantes();
  }

  verExpediente(estudiante: EstudiantePendienteEgreso): void {
    this.cargandoExpediente.set(true);
    this.mostrarExpedienteModal.set(true);
    this.expedienteSeleccionado.set(null);

    this.egresadosService
      .getExpedienteAcademico(estudiante.idAlumno, estudiante.idCarrera)
      .subscribe({
        next: (exp) => {
          this.expedienteSeleccionado.set(exp);
          this.cargandoExpediente.set(false);
        },
        error: () => {
          this.cargandoExpediente.set(false);
        },
      });
  }

  cerrarExpedienteModal(): void {
    this.mostrarExpedienteModal.set(false);
    this.expedienteSeleccionado.set(null);
  }
}
