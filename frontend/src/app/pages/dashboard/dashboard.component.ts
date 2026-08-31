import { Component, effect, inject, signal, viewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AuthService } from '../../core/services/auth.service';
import {
  CarrerasService,
  CarreraUsuarioItem,
  UsuarioCarrerasResponseDto,
} from '../../core/services/carreras.service';
import { TitulacionService } from '../../core/services/titulacion.service';
import { NetworkStatusService } from '../../core/services/network-status.service';
import { NotificationService } from '../../core/services/notification.service';
import {
  PortalEstudiante,
  ConvocatoriaDetalle,
  ConvocatoriaResumen,
  ModalidadMaestra,
  RequisitoMaestro,
  RequisitoModalidadMatriz,
  PostularRequest,
  DictamenPostulacionRequest,
  AperturarPeriodoRequest,
  PostulacionResumen,
  EstadoPostulacion,
  ResumenGeneralSistema,
} from '../../core/models/titulacion.models';

import {
  TopbarComponent,
  NetworkBannerComponent,
  ConvocatoriaCardComponent,
  StepperComponent,
  KpiCardComponent,
  DictamenModalComponent,
  AperturaPeriodoModalComponent,
} from '../../shared/components';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    TopbarComponent,
    NetworkBannerComponent,
    ConvocatoriaCardComponent,
    StepperComponent,
    KpiCardComponent,
    DictamenModalComponent,
    AperturaPeriodoModalComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly carrerasService = inject(CarrerasService);
  private readonly titulacionService = inject(TitulacionService);
  private readonly networkService = inject(NetworkStatusService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  currentUser = this.authService.currentUser;
  currentYear = new Date().getFullYear();
  isDarkMode = signal(false);
  // En escritorio arranca expandida; en pantallas angostas arranca oculta (drawer).
  isSidebarCollapsed = signal(typeof window !== 'undefined' && window.innerWidth < 900);
  activeTab = signal('resumen');

  // Estado de Red y Resiliencia Offline
  isOnline = this.networkService.isOnline;
  isLowBandwidth = this.networkService.isLowBandwidth;
  connectionType = this.networkService.connectionType;

  // Estado Multicarrera
  carrerasDisponibles = signal<CarreraUsuarioItem[]>([]);
  carreraSeleccionada = signal<CarreraUsuarioItem | null>(null);
  carrerasCargando = signal<boolean>(true);

  // Estado del Portal del Alumno
  portalEstudiante = signal<PortalEstudiante | null>(null);
  portalCargando = signal<boolean>(false);
  modalidadSeleccionada = signal<number | null>(null);
  postulando = signal<boolean>(false);

  // Estado del Gestor de Titulación (Convocatorias & Configuración)
  convocatoriaActiva = signal<ConvocatoriaDetalle | null>(null);
  convocatoriaCargando = signal<boolean>(false);
  convocatoriasLista = signal<ConvocatoriaResumen[]>([]);
  convocatoriasHistoricoCargando = signal<boolean>(false);
  resumenGeneral = signal<ResumenGeneralSistema | null>(null);

  // Postulaciones Generales (Gestor)
  postulacionesLista = signal<PostulacionResumen[]>([]);
  postulacionesTotal = signal<number>(0);
  postulacionesCargando = signal<boolean>(false);
  estadosPostulacion = signal<EstadoPostulacion[]>([]);
  filtroEstado = signal<number | null>(null);
  filtroCarrera = signal<number | null>(null);
  filtroBusqueda = signal<string>('');
  paginaActual = signal<number>(1);
  tamanoPagina = signal<number>(20);

  // Configuración Maestra
  modalidadesMaestras = signal<ModalidadMaestra[]>([]);
  requisitosMaestros = signal<RequisitoMaestro[]>([]);
  matrizRequisitos = signal<RequisitoModalidadMatriz[]>([]);
  modalidadSeleccionadaMatriz = signal<ModalidadMaestra | null>(null);
  configCargando = signal<boolean>(false);

  // --------------------------------------------------------
  // Mat-table: fuentes de datos, orden y paginación
  // --------------------------------------------------------
  readonly displayedColumnsPostulaciones = [
    'id',
    'estudiante',
    'cedula',
    'carrera',
    'modalidad',
    'estado',
    'requisitos',
    'acciones',
  ];
  dataSourcePostulaciones = new MatTableDataSource<PostulacionResumen>([]);

  readonly displayedColumnsConvocatorias = [
    'id',
    'periodo',
    'detalle',
    'fechaInicio',
    'fechaFin',
    'diasCorte',
    'carreras',
    'postulaciones',
    'estado',
  ];
  dataSourceConvocatorias = new MatTableDataSource<ConvocatoriaResumen>([]);
  sortConvocatorias = viewChild<MatSort>('sortConvocatorias');

  readonly displayedColumnsRequisitos = ['id', 'requisito', 'tipo', 'subidoPor', 'estado', 'acciones'];
  dataSourceRequisitos = new MatTableDataSource<RequisitoMaestro>([]);
  sortRequisitos = viewChild<MatSort>('sortRequisitos');

  readonly displayedColumnsModalidades = [
    'id',
    'modalidad',
    'cantidadMinima',
    'caracteristicas',
    'requisitosAsociados',
    'estado',
    'acciones',
  ];
  dataSourceModalidades = new MatTableDataSource<ModalidadMaestra>([]);
  sortModalidades = viewChild<MatSort>('sortModalidades');

  private readonly _syncDataSourcePostulaciones = effect(() => {
    this.dataSourcePostulaciones.data = this.postulacionesLista();
  });

  private readonly _syncDataSourceConvocatorias = effect(() => {
    this.dataSourceConvocatorias.data = this.convocatoriasLista();
    const sort = this.sortConvocatorias();
    if (sort) this.dataSourceConvocatorias.sort = sort;
  });

  private readonly _syncDataSourceRequisitos = effect(() => {
    this.dataSourceRequisitos.data = this.requisitosMaestros();
    const sort = this.sortRequisitos();
    if (sort) this.dataSourceRequisitos.sort = sort;
  });

  private readonly _syncDataSourceModalidades = effect(() => {
    this.dataSourceModalidades.data = this.modalidadesMaestras();
    const sort = this.sortModalidades();
    if (sort) this.dataSourceModalidades.sort = sort;
  });

  onPagePostulaciones(event: PageEvent): void {
    this.paginaActual.set(event.pageIndex + 1);
    this.tamanoPagina.set(event.pageSize);
    this.cargarPostulaciones();
  }

  // Modales
  aperturaModalVisible = signal<boolean>(false);
  nuevoRequisitoModalVisible = signal<boolean>(false);
  nuevaModalidadModalVisible = signal<boolean>(false);
  matrizModalVisible = signal<boolean>(false);

  // Formulario de Apertura de Convocatoria
  aperturaForm = signal({
    idPeriodo: '2026-I',
    detalleConvocatoria: 'Convocatoria Ordinaria de Titulación 2026-I',
    fechaInicioCorte: new Date().toISOString().substring(0, 10),
    fechaFinCorte: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().substring(0, 10),
    diasPermitidos: 90,
    diasExtension: 30,
    habilitarTodasLasCarreras: true,
  });

  // Formulario de Creación de Modalidad
  nuevaModalidadForm = signal({
    modalidadTitulacion: '',
    esComplexivo: 'NO',
    esArticuloCientifico: 'NO',
    generaTesis: 'NO',
    cantidadMinima: 1,
  });

  // Formulario de Creación de Requisito
  nuevoRequisitoForm = signal({
    requisito: '',
    esAdjunto: true,
    esBool: false,
    subeAlumno: true,
    subeColaborador: false,
  });

  // Modal de Dictamen (Gestor)
  dictamenModal = signal<{
    visible: boolean;
    idPostulacion: number;
    decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR';
    observaciones: string;
  } | null>(null);

  userRolesFormatted(): string {
    if (this.isAdmin()) return 'Administrador';
    if (this.isDocente()) return 'Docente';
    return 'Estudiante';
  }

  isAdmin(): boolean {
    return (
      this.hasRole('ADMIN') ||
      this.hasRole('TITULACION_ADMIN') ||
      this.hasRole('ADMINISTRADOR') ||
      this.hasRole('ADMIN_SIST') ||
      this.hasRole('GESTOR')
    );
  }

  isDocente(): boolean {
    const user = this.currentUser();
    const esProfesorSigafi = user?.tablaSigafi?.toLowerCase() === 'profesor';
    return (this.hasRole('DOCENTE') || this.hasRole('PROFESOR') || esProfesorSigafi) && !this.isAdmin();
  }

  isEstudiante(): boolean {
    const user = this.currentUser();
    const esAlumnoSigafi = user?.tablaSigafi?.toLowerCase() === 'alumno' || user?.tablaSigafi?.toLowerCase() === 'alumnos';
    return (
      (this.hasRole('ESTUDIANTE') || this.hasRole('ALUMNO') || esAlumnoSigafi) &&
      !this.isAdmin() &&
      !this.isDocente()
    );
  }

  hasRole(roleCode: string): boolean {
    return this.authService.hasRole(roleCode);
  }

  hasAnyRole(roles: string[]): boolean {
    return this.authService.hasAnyRole(roles);
  }

  hasPermission(moduleName: string, operationName: string): boolean {
    return this.authService.hasPermission(moduleName, operationName);
  }

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('titulacion_theme') || 'light';
    const isDark = savedTheme === 'dark';
    this.isDarkMode.set(isDark);
    document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');

    if (this.isEstudiante()) {
      this.activeTab.set('postulacion');
      this.cargarPortalEstudiante();
    } else if (this.isDocente()) {
      this.activeTab.set('evaluacion');
    } else {
      this.activeTab.set('resumen');
      this.cargarResumenGeneral();
      this.cargarConvocatoriaActiva();
      this.cargarPostulaciones();
      this.cargarConfiguracionesMaestras();
      this.cargarEstadosPostulacion();
    }

    this.cargarCarrerasUsuario();
  }

  cargarCarrerasUsuario(): void {
    this.carrerasCargando.set(true);

    if (this.isAdmin()) {
      // Para Administrador Institucional: Listar TODAS las carreras del Instituto (EsInstituto == true)
      this.carrerasService.getCarrerasTodas().subscribe({
        next: (carreras) => {
          const lista: CarreraUsuarioItem[] = (carreras || []).map((c) => ({
            idCarrera: c.idCarrera,
            nombreCarrera: c.nombreCarrera,
            aliasCarrera: c.aliasCarrera,
            asignadoEnTodasLasCarreras: true,
          }));

          this.carrerasDisponibles.set(lista);
          if (lista.length > 0) {
            const actual = this.carreraSeleccionada();
            const existe = actual ? lista.find((item) => item.idCarrera === actual.idCarrera) : null;
            this.carreraSeleccionada.set(existe || lista[0]);
          }
          this.carrerasCargando.set(false);
        },
        error: (err) => {
          console.error('Error al cargar catálogo general de carreras:', err);
          this.carrerasCargando.set(false);
        },
      });
      return;
    }

    // Para Docentes y Estudiantes: Listar sus carreras asignadas / matriculadas
    this.carrerasService.getMisCarreras().subscribe({
      next: (data: UsuarioCarrerasResponseDto) => {
        let lista: CarreraUsuarioItem[];
        if (this.isEstudiante()) {
          lista = data.carrerasEstudiante || [];
        } else if (this.isDocente()) {
          lista = data.carrerasDocente || [];
        } else {
          lista = [...(data.carrerasEstudiante || []), ...(data.carrerasDocente || [])];
        }

        this.carrerasDisponibles.set(lista);
        if (lista.length > 0) {
          const actual = this.carreraSeleccionada();
          const existe = actual ? lista.find((item) => item.idCarrera === actual.idCarrera) : null;
          this.carreraSeleccionada.set(existe || lista[0]);
        }
        this.carrerasCargando.set(false);
      },
      error: (err) => {
        console.error('Error al cargar carreras del usuario:', err);
        this.carrerasCargando.set(false);
      },
    });
  }

  // ----------------------------------------------------
  // Flujo Alumno / Egresado
  // ----------------------------------------------------
  cargarPortalEstudiante(): void {
    this.portalCargando.set(true);
    this.titulacionService.getMiPortal().subscribe({
      next: (data) => {
        this.portalEstudiante.set(data);
        if (data.modalidadesDisponibles && data.modalidadesDisponibles.length > 0) {
          this.modalidadSeleccionada.set(data.modalidadesDisponibles[0].idModalidadTitulacionCarrera);
        }
        this.portalCargando.set(false);
      },
      error: (err) => {
        console.warn('Error al obtener portal del estudiante:', err);
        this.portalCargando.set(false);
      },
    });
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

  enviarPostulacion(): void {
    const portal = this.portalEstudiante();
    const idMod = this.modalidadSeleccionada();
    if (!portal || !idMod || !portal.estudiante.idMatricula) {
      this.mostrarMensaje('info', 'Seleccione una modalidad de titulación válida.');
      return;
    }

    this.postulando.set(true);
    const req: PostularRequest = {
      idMatricula: portal.estudiante.idMatricula,
      idModalidadTitulacionCarrera: idMod,
      requisitos: [],
    };

    this.titulacionService.postular(req).subscribe({
      next: () => {
        this.postulando.set(false);
        this.mostrarMensaje('exito', '¡Postulación registrada exitosamente en el sistema!');
        this.cargarPortalEstudiante();
      },
      error: (err) => {
        this.postulando.set(false);
        this.mostrarMensaje('error', err.error?.message || 'Error al enviar la postulación.');
      },
    });
  }

  // ----------------------------------------------------
  // Flujo Gestor de Titulación (Postulaciones & KPIs)
  // ----------------------------------------------------
  cargarPostulaciones(): void {
    this.postulacionesCargando.set(true);
    const idCarrera = this.filtroCarrera() || undefined;
    const idEstado = this.filtroEstado() || undefined;
    const busqueda = this.filtroBusqueda() || undefined;

    this.titulacionService
      .getPostulaciones(this.paginaActual(), this.tamanoPagina(), idCarrera, undefined, idEstado, busqueda)
      .subscribe({
      next: (data) => {
        this.postulacionesLista.set(data.items || []);
        this.postulacionesTotal.set(data.total || 0);
        this.postulacionesCargando.set(false);
      },
      error: (err) => {
        console.warn('Error al cargar postulaciones:', err);
        this.postulacionesLista.set([]);
        this.postulacionesCargando.set(false);
      },
    });
  }

  cargarEstadosPostulacion(): void {
    this.titulacionService.getEstadosPostulacion().subscribe({
      next: (data) => this.estadosPostulacion.set(data),
      error: (err) => console.warn('Error al cargar estados:', err),
    });
  }

  totalAprobadas(): number {
    return this.postulacionesLista().filter((p) =>
      p.nombreEstado.toUpperCase().includes('APROB'),
    ).length;
  }

  totalEnRevision(): number {
    return this.postulacionesLista().filter((p) =>
      p.nombreEstado.toUpperCase().includes('REVIS') ||
      p.nombreEstado.toUpperCase().includes('OBSERV') ||
      p.nombreEstado.toUpperCase().includes('REGISTR'),
    ).length;
  }

  // ----------------------------------------------------
  // Convocatorias & Cortes (Gestor)
  // ----------------------------------------------------
  cargarResumenGeneral(): void {
    this.titulacionService.getResumenGeneral().subscribe({
      next: (data) => this.resumenGeneral.set(data),
      error: (err) => console.warn('Error al cargar resumen general:', err),
    });
  }

  periodoNombreAmigable(): string {
    const res = this.resumenGeneral();
    if (res?.periodoNombreHumano) return res.periodoNombreHumano;
    const activa = this.convocatoriaActiva();
    if (activa?.idPeriodo === 'ABR2026') return 'Abril – Septiembre 2026';
    if (activa?.idPeriodo === 'MAC2026') return 'Marzo – Septiembre 2026';
    if (activa?.idPeriodo === 'OCT2025') return 'Octubre 2025 – Marzo 2026';
    return activa?.idPeriodo || 'Período Vigente';
  }

  cargarConvocatoriaActiva(): void {
    this.convocatoriaCargando.set(true);
    this.titulacionService.getConvocatoriaActiva().subscribe({
      next: (data) => {
        this.convocatoriaActiva.set(data);
        this.convocatoriaCargando.set(false);
      },
      error: () => {
        this.convocatoriaActiva.set(null);
        this.convocatoriaCargando.set(false);
      },
    });
  }

  cargarHistoricoConvocatorias(): void {
    this.convocatoriasHistoricoCargando.set(true);
    this.titulacionService.getConvocatorias().subscribe({
      next: (data) => {
        this.convocatoriasLista.set(data);
        this.convocatoriasHistoricoCargando.set(false);
      },
      error: () => {
        this.convocatoriasLista.set([]);
        this.convocatoriasHistoricoCargando.set(false);
      },
    });
  }

  aperturarPeriodo(solicitud?: AperturarPeriodoRequest): void {
    const form = this.aperturaForm();
    const req: AperturarPeriodoRequest = solicitud || {
      idPeriodo: form.idPeriodo,
      detalleConvocatoria: form.detalleConvocatoria,
      fechaInicioCorte: new Date(form.fechaInicioCorte).toISOString(),
      fechaFinCorte: new Date(form.fechaFinCorte + 'T23:59:59').toISOString(),
      diasPermitidos: form.diasPermitidos,
      diasExtension: form.diasExtension,
      habilitarTodasLasCarreras: form.habilitarTodasLasCarreras,
    };

    if (!req.idPeriodo || !req.fechaInicioCorte || !req.fechaFinCorte) {
      this.mostrarMensaje('error', 'Por favor complete todos los campos de la convocatoria.');
      return;
    }

    this.titulacionService.aperturarPeriodo(req).subscribe({
      next: (cohorte) => {
        this.convocatoriaActiva.set(cohorte);
        this.aperturaModalVisible.set(false);
        this.mostrarMensaje('exito', `¡Período ${req.idPeriodo} aperturado masivamente con éxito!`);
        this.cargarHistoricoConvocatorias();
      },
      error: (err) => {
        this.mostrarMensaje('error', err.error?.message || 'Error al aperturar el período.');
      },
    });
  }

  // ----------------------------------------------------
  // Configuración Maestra de Requisitos y Modalidades
  // ----------------------------------------------------
  cargarConfiguracionesMaestras(): void {
    this.configCargando.set(true);
    this.titulacionService.getModalidadesMaestras().subscribe({
      next: (data) => this.modalidadesMaestras.set(data),
      error: (err) => console.warn('Error al cargar modalidades maestras:', err),
    });

    this.titulacionService.getRequisitosMaestros().subscribe({
      next: (data) => {
        this.requisitosMaestros.set(data);
        this.configCargando.set(false);
      },
      error: (err) => {
        console.warn('Error al cargar requisitos maestros:', err);
        this.configCargando.set(false);
      },
    });
  }

  toggleModalidadEstado(m: ModalidadMaestra): void {
    const nuevoEstado = !m.esActivo;
    this.titulacionService.cambiarEstadoModalidad(m.idModalidadTitulacion, nuevoEstado).subscribe({
      next: () => {
        this.mostrarMensaje('exito', `Modalidad "${m.modalidadTitulacion}" actualizada.`);
        this.cargarConfiguracionesMaestras();
      },
      error: (err) => this.mostrarMensaje('error', err.error?.message || 'Error al actualizar estado.'),
    });
  }

  toggleRequisitoEstado(r: RequisitoMaestro): void {
    const nuevoEstado = !r.esActivo;
    this.titulacionService.cambiarEstadoRequisito(r.idRequisitos, nuevoEstado).subscribe({
      next: () => {
        this.mostrarMensaje('exito', `Requisito "${r.requisito}" actualizado.`);
        this.cargarConfiguracionesMaestras();
      },
      error: (err) => this.mostrarMensaje('error', err.error?.message || 'Error al actualizar estado.'),
    });
  }

  crearModalidad(): void {
    const f = this.nuevaModalidadForm();
    if (!f.modalidadTitulacion.trim()) {
      this.mostrarMensaje('error', 'El nombre de la modalidad es obligatorio.');
      return;
    }

    this.titulacionService.crearModalidadMaestra(f).subscribe({
      next: () => {
        this.mostrarMensaje('exito', 'Modalidad maestra creada exitosamente.');
        this.nuevaModalidadModalVisible.set(false);
        this.cargarConfiguracionesMaestras();
      },
      error: (err) => this.mostrarMensaje('error', err.error?.message || 'Error al crear la modalidad.'),
    });
  }

  crearRequisito(): void {
    const f = this.nuevoRequisitoForm();
    if (!f.requisito.trim()) {
      this.mostrarMensaje('error', 'El nombre del requisito es obligatorio.');
      return;
    }

    this.titulacionService.crearRequisitoMaestro(f).subscribe({
      next: () => {
        this.mostrarMensaje('exito', 'Requisito maestro creado exitosamente.');
        this.nuevoRequisitoModalVisible.set(false);
        this.cargarConfiguracionesMaestras();
      },
      error: (err) => this.mostrarMensaje('error', err.error?.message || 'Error al crear requisito.'),
    });
  }

  abrirMatriz(m: ModalidadMaestra): void {
    this.modalidadSeleccionadaMatriz.set(m);
    this.matrizModalVisible.set(true);
    this.titulacionService.getRequisitosPorModalidad(m.idModalidadTitulacion).subscribe({
      next: (data) => this.matrizRequisitos.set(data),
      error: () => this.matrizRequisitos.set([]),
    });
  }

  asignarRequisito(idRequisito: number): void {
    const m = this.modalidadSeleccionadaMatriz();
    if (!m) return;

    this.titulacionService.asignarRequisitoAModalidad(m.idModalidadTitulacion, idRequisito).subscribe({
      next: () => {
        this.mostrarMensaje('exito', 'Requisito asignado a la modalidad.');
        this.abrirMatriz(m);
      },
      error: (err) => this.mostrarMensaje('error', err.error?.message || 'Error al asociar requisito.'),
    });
  }

  desasignarRequisito(idRequisitoModalidad: number): void {
    const m = this.modalidadSeleccionadaMatriz();
    if (!m) return;

    this.titulacionService.desasignarRequisitoDeModalidad(idRequisitoModalidad).subscribe({
      next: () => {
        this.mostrarMensaje('exito', 'Requisito desvinculado.');
        this.abrirMatriz(m);
      },
      error: (err) => this.mostrarMensaje('error', err.error?.message || 'Error al desvincular.'),
    });
  }

  // ----------------------------------------------------
  // Dictámenes y Calificación
  // ----------------------------------------------------
  abrirModalDictamen(idPostulacion: number, decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR'): void {
    this.dictamenModal.set({
      visible: true,
      idPostulacion,
      decision,
      observaciones: '',
    });
  }

  cerrarModalDictamen(): void {
    this.dictamenModal.set(null);
  }

  enviarDictamen(solicitud?: DictamenPostulacionRequest): void {
    const modal = this.dictamenModal();
    const req: DictamenPostulacionRequest | null = solicitud || (modal ? {
      idPostulacionAlumnos: modal.idPostulacion,
      decision: modal.decision,
      observaciones: modal.observaciones,
    } : null);

    if (!req) return;

    this.titulacionService.dictaminarPostulacion(req).subscribe({
      next: () => {
        this.mostrarMensaje('exito', `Dictamen "${req.decision}" registrado correctamente.`);
        this.cerrarModalDictamen();
        this.cargarPostulaciones();
      },
      error: (err) => {
        this.mostrarMensaje('error', err.error?.message || 'Error al registrar el dictamen.');
      },
    });
  }

  mostrarMensaje(tipo: 'exito' | 'error' | 'info', texto: string): void {
    if (tipo === 'exito') this.notificationService.success(texto);
    else if (tipo === 'error') this.notificationService.error(texto);
    else this.notificationService.info(texto);
  }

  onCarreraChange(idCarrera: number): void {
    const seleccionada = this.carrerasDisponibles().find((c) => c.idCarrera === idCarrera);
    if (seleccionada) {
      this.carreraSeleccionada.set(seleccionada);
      if (this.isAdmin()) {
        this.filtroCarrera.set(idCarrera);
        this.cargarPostulaciones();
      }
    }
  }

  toggleTheme(): void {
    const nextTheme = !this.isDarkMode();
    this.isDarkMode.set(nextTheme);
    const themeStr = nextTheme ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', themeStr);
    localStorage.setItem('titulacion_theme', themeStr);
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed.set(!this.isSidebarCollapsed());
  }

  setActiveTab(tab: string): void {
    this.activeTab.set(tab);
    if (tab === 'resumen') {
      this.cargarResumenGeneral();
      this.cargarConvocatoriaActiva();
    } else if (tab === 'cohortes') {
      this.cargarHistoricoConvocatorias();
    } else if (tab === 'postulaciones') {
      this.cargarPostulaciones();
    } else if (tab === 'requisitos' || tab === 'modalidades') {
      this.cargarConfiguracionesMaestras();
    } else if (tab === 'postulacion') {
      this.cargarPortalEstudiante();
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
