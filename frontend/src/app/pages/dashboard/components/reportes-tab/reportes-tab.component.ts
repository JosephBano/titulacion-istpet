import {
  Component,
  OnInit,
  signal,
  computed,
  inject,
  input,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportesService } from '../../../../core/services/reportes.service';
import { EgresadosService } from '../../../../core/services/egresados.service';
import { NotificationService } from '../../../../core/services/notification.service';
import {
  ReporteCatalogoItem,
  FiltrosGeneracionReporte,
  MetadatosReporteInstitucional,
} from '../../../../core/models/reportes.models';
import {
  PeriodoItem,
  CarreraItem,
  EstudiantePendienteEgreso,
} from '../../../../core/models/egresados.models';
import { UserPermissions } from '../../../../core/models/auth.models';

@Component({
  selector: 'app-reportes-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reportes-tab.component.html',
  styleUrl: './reportes-tab.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportesTabComponent implements OnInit {
  private readonly reportesService = inject(ReportesService);
  private readonly egresadosService = inject(EgresadosService);
  private readonly notificationService = inject(NotificationService);

  currentUser = input<UserPermissions | null>(null);

  // Catálogo de reportes
  catalogo = signal<ReporteCatalogoItem[]>(this.reportesService.catalogoReportes);
  reporteSeleccionado = signal<ReporteCatalogoItem>(this.catalogo()[0]);

  // Listas de catálogos para filtros
  periodos = signal<PeriodoItem[]>([]);
  carreras = signal<CarreraItem[]>([]);

  // Filtros seleccionados
  periodoSeleccionado = signal<string>('');
  carreraSeleccionada = signal<number | null>(null);
  busquedaEstudiante = signal<string>('');
  filtroPostulacion = signal<'TODOS' | 'CON_POSTULACION' | 'SIN_POSTULACION'>('TODOS');

  // Estado de carga y datos
  cargandoDatos = signal<boolean>(false);
  generandoPdf = signal<boolean>(false);
  estudiantes = signal<EstudiantePendienteEgreso[]>([]);

  // Filtro de búsqueda en la nómina obtenida
  busquedaEnTabla = signal<string>('');

  // Estudiantes filtrados en la vista previa
  estudiantesFiltrados = computed(() => {
    let lista = this.estudiantes();
    const query = this.busquedaEnTabla().toLowerCase().trim();
    const estadoPost = this.filtroPostulacion();

    if (estadoPost === 'CON_POSTULACION') {
      lista = lista.filter((e) => e.tienePostulacionTitulacion);
    } else if (estadoPost === 'SIN_POSTULACION') {
      lista = lista.filter((e) => !e.tienePostulacionTitulacion);
    }

    if (!query) return lista;

    return lista.filter(
      (e) =>
        e.idAlumno.toLowerCase().includes(query) ||
        e.nombreCompleto.toLowerCase().includes(query) ||
        (e.carrera || '').toLowerCase().includes(query),
    );
  });

  // Métricas para el resumen previo
  totalPostulados = computed(() => {
    return this.estudiantes().filter((e) => e.tienePostulacionTitulacion).length;
  });

  totalSinPostular = computed(() => {
    return this.estudiantes().filter((e) => !e.tienePostulacionTitulacion).length;
  });

  ngOnInit(): void {
    this.cargarCatalogos();
  }

  private cargarCatalogos(): void {
    this.cargandoDatos.set(true);

    // Cargar períodos y carreras en paralelo
    this.egresadosService.getPeriodos().subscribe({
      next: (data) => {
        this.periodos.set(data || []);
        const periodoActivo = data.find((p) => p.activo);
        if (periodoActivo) {
          this.periodoSeleccionado.set(periodoActivo.idPeriodo);
        } else if (data.length > 0) {
          this.periodoSeleccionado.set(data[0].idPeriodo);
        }
        this.cargarDatosVistaPrevia();
      },
      error: () => {
        this.cargandoDatos.set(false);
      },
    });

    this.egresadosService.getCarreras().subscribe({
      next: (data) => this.carreras.set(data || []),
      error: () => this.carreras.set([]),
    });
  }

  seleccionarReporte(rep: ReporteCatalogoItem): void {
    if (!rep.disponible) {
      this.notificationService.info(
        `El reporte "${rep.titulo}" estará disponible en la próxima actualización.`,
      );
      return;
    }
    this.reporteSeleccionado.set(rep);
  }

  onPeriodoChange(nuevoPeriodo: string): void {
    this.periodoSeleccionado.set(nuevoPeriodo);
    this.cargarDatosVistaPrevia();
  }

  onCarreraChange(nuevaCarrera: number | null): void {
    this.carreraSeleccionada.set(nuevaCarrera);
    this.cargarDatosVistaPrevia();
  }

  cargarDatosVistaPrevia(): void {
    this.cargandoDatos.set(true);

    this.egresadosService
      .getPendientesEgreso({
        idPeriodo: this.periodoSeleccionado() || undefined,
        idCarrera: this.carreraSeleccionada() ?? undefined,
        cedulaOrNombre: this.busquedaEstudiante().trim() || undefined,
        pagina: 1,
        tamanoPagina: 500, // Cargar nómina completa para reporte
      })
      .subscribe({
        next: (res) => {
          this.estudiantes.set(res.items || []);
          this.cargandoDatos.set(false);
        },
        error: () => {
          this.estudiantes.set([]);
          this.cargandoDatos.set(false);
        },
      });
  }

  descargarPdf(): void {
    const lista = this.estudiantesFiltrados();
    if (lista.length === 0) {
      this.notificationService.info(
        'No existen registros para exportar con los filtros seleccionados.',
      );
      return;
    }

    this.generandoPdf.set(true);

    try {
      const periodoObj = this.periodos().find((p) => p.idPeriodo === this.periodoSeleccionado());
      const carreraObj = this.carreras().find((c) => c.idCarrera === this.carreraSeleccionada());
      const user = this.currentUser();

      const filtros: FiltrosGeneracionReporte = {
        idPeriodo: this.periodoSeleccionado(),
        nombrePeriodo: periodoObj?.nombre || this.periodoSeleccionado(),
        idCarrera: this.carreraSeleccionada() ?? undefined,
        nombreCarrera: carreraObj?.nombre || 'Todas las carreras',
      };

      const metadatos: MetadatosReporteInstitucional = {
        institucion: 'Instituto Superior Tecnológico Traversari (ISTPET)',
        sistema: 'Sistema de Titulación Académica',
        tituloReporte: this.reporteSeleccionado().titulo,
        periodoAcademico: periodoObj
          ? `${periodoObj.nombre} (${periodoObj.idPeriodo})`
          : this.periodoSeleccionado() || 'Ciclo Actual',
        carreraFiltro: carreraObj ? carreraObj.nombre : 'Todas las Carreras Habilitadas',
        fechaEmision: new Date(),
        usuarioEmisor: user?.nombre || 'Administrador Institucional',
        rolUsuario: user?.roles?.join(', ') || 'Administrador',
        totalRegistros: lista.length,
      };

      this.reportesService.generarReportePosiblesPostulantesPdf(lista, filtros, metadatos);
      this.notificationService.success(
        'Reporte oficial en PDF generado y descargado exitosamente.',
      );
    } catch (err) {
      console.error('Error generando PDF:', err);
      this.notificationService.error('Ocurrió un inconveniente al generar el documento PDF.');
    } finally {
      this.generandoPdf.set(false);
    }
  }
}
