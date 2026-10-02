import { Component, input, output, signal, computed, inject, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  AperturarPeriodoRequest,
  ModalidadMaestra,
  ModalidadCarreraDto,
} from '../../../core/models/titulacion.models';
import { TitulacionService } from '../../../core/services/titulacion.service';
import { DrawerComponent } from '../drawer/drawer.component';

export interface StepperStep {
  readonly id: number;
  readonly label: string;
  readonly subtitle: string;
}

export interface CalendarDayItem {
  readonly dateStr: string;
  readonly dayNumber: number;
  readonly isCurrentMonth: boolean;
  readonly isToday: boolean;
  // Fase 1: Postulación (Inscripción)
  readonly isPostulacion: boolean;
  readonly isPostulacionStart: boolean;
  readonly isPostulacionEnd: boolean;
  // Fase 2: Desarrollo de Titulación (Estudiante)
  readonly isTitulacion: boolean;
  readonly isTitulacionStart: boolean;
  readonly isTitulacionEnd: boolean;
  // Fase 3: Prórroga / Gracia
  readonly isProrroga: boolean;
  readonly isProrrogaStart: boolean;
  readonly isProrrogaEnd: boolean;
}

interface ConvocatoriaDraft {
  pasoActual: number;
  form: {
    idPeriodo: string;
    detalleConvocatoria: string;
    fechaInicioStr: string;
    fechaFinStr: string;
    diasPermitidos: number;
    diasExtension: number;
    habilitarTodasLasCarreras: boolean;
  };
  carrerasSeleccionadas: number[];
  modalidadesSeleccionadas: number[];
}

const DRAFT_STORAGE_KEY = 'istpet_convocatoria_apertura_draft';

@Component({
  selector: 'app-apertura-periodo-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, DrawerComponent],
  templateUrl: './apertura-periodo-modal.component.html',
  styleUrls: ['./apertura-periodo-modal.component.css'],
})
export class AperturaPeriodoModalComponent implements OnInit {
  private readonly titulacionService = inject(TitulacionService);

  // Inputs y Outputs
  visible = input<boolean>(false);
  modalClose = output<void>();
  confirm = output<AperturarPeriodoRequest>();
  nuevaModalidad = output<void>();

  // Navegación Stepper (5 pasos)
  pasoActual = signal<number>(1);
  readonly totalPasos = 5;
  readonly steps: readonly StepperStep[] = [
    { id: 1, label: 'Período', subtitle: 'Académico' },
    { id: 2, label: 'Convocatoria', subtitle: 'Denominación' },
    { id: 3, label: 'Cronograma', subtitle: 'Fechas y Plazos' },
    { id: 4, label: 'Carreras', subtitle: 'Alcance' },
    { id: 5, label: 'Modalidades', subtitle: 'Confirmación' },
  ];

  // Datos de Backend
  periodos = signal<{ idPeriodo: string; nombre: string; esActivo?: boolean }[]>([]);
  modalidadesCarreras = signal<ModalidadCarreraDto[]>([]);
  modalidades = signal<ModalidadMaestra[]>([]);

  // Estados de recarga reactiva
  cargandoModalidades = signal<boolean>(false);
  cargandoCarreras = signal<boolean>(false);

  // Estado del Formulario
  form = signal({
    idPeriodo: 'ABR2026',
    detalleConvocatoria: 'Convocatoria Ordinaria ABR2026',
    fechaInicioStr: this.toYMD(new Date()),
    fechaFinStr: this.toYMD(new Date(Date.now() + 14 * 24 * 60 * 60 * 1000)),
    diasPermitidos: 90,
    diasExtension: 30,
    habilitarTodasLasCarreras: true,
  });

  // Filtros y Selección de Carreras (Paso 4)
  filtroTextoCarrera = signal<string>('');
  filtroModalidadEstudio = signal<string>('TODAS');
  carrerasSeleccionadas = signal<Set<number>>(new Set());

  // Modalidades Maestras Seleccionadas (Paso 5)
  modalidadesSeleccionadas = signal<Set<number>>(new Set());

  // ----------------------------------------------------
  // Paso 3: Cronograma Visual y Calendario Multi-Fase
  // ----------------------------------------------------
  currentCalendarMonth = signal<Date>(new Date(new Date().getFullYear(), new Date().getMonth(), 1));
  seleccionModo = signal<'inicio' | 'fin'>('inicio');
  tabFaseActiva = signal<'postulacion' | 'titulacion' | 'prorroga'>('postulacion');

  readonly diasSemana = ['Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sá', 'Do'];

  // Duración en días de la postulación
  duracionConvocatoriaDias = computed(() => {
    const f = this.form();
    if (!f.fechaInicioStr || !f.fechaFinStr) return 0;
    const d1 = this.fromYMD(f.fechaInicioStr).getTime();
    const d2 = this.fromYMD(f.fechaFinStr).getTime();
    if (isNaN(d1) || isNaN(d2) || d2 < d1) return 0;
    return Math.max(1, Math.round((d2 - d1) / (1000 * 60 * 60 * 24)) + 1);
  });

  // Cálculo de todas las fases del proceso de titulación
  cronogramaFases = computed(() => {
    const f = this.form();
    const inicioPost = f.fechaInicioStr;
    const finPost = f.fechaFinStr;

    const dFinPost = this.fromYMD(finPost);
    const dInicioTit = new Date(dFinPost.getTime() + 24 * 60 * 60 * 1000);
    const diasTit = Math.max(1, Number(f.diasPermitidos) || 90);
    const dFinTit = new Date(dInicioTit.getTime() + (diasTit - 1) * 24 * 60 * 60 * 1000);

    const dInicioPro = new Date(dFinTit.getTime() + 24 * 60 * 60 * 1000);
    const diasPro = Math.max(0, Number(f.diasExtension) || 0);
    const dFinPro =
      diasPro > 0 ? new Date(dInicioPro.getTime() + (diasPro - 1) * 24 * 60 * 60 * 1000) : dFinTit;

    const inicioTitStr = this.toYMD(dInicioTit);
    const finTitStr = this.toYMD(dFinTit);
    const inicioProStr = this.toYMD(dInicioPro);
    const finProStr = this.toYMD(dFinPro);

    return {
      postulacion: {
        inicioStr: inicioPost,
        finStr: finPost,
        dias: this.duracionConvocatoriaDias(),
      },
      titulacion: {
        inicioStr: inicioTitStr,
        finStr: finTitStr,
        dias: diasTit,
      },
      prorroga: {
        inicioStr: inicioProStr,
        finStr: finProStr,
        dias: diasPro,
      },
    };
  });

  calendarDays = computed<CalendarDayItem[]>(() => {
    const current = this.currentCalendarMonth();
    const year = current.getFullYear();
    const month = current.getMonth();

    const fases = this.cronogramaFases();
    const postIni = fases.postulacion.inicioStr;
    const postFin = fases.postulacion.finStr;
    const titIni = fases.titulacion.inicioStr;
    const titFin = fases.titulacion.finStr;
    const proIni = fases.prorroga.inicioStr;
    const proFin = fases.prorroga.finStr;
    const tienePro = fases.prorroga.dias > 0;

    const todayStr = this.toYMD(new Date());

    const firstDayIndex = (new Date(year, month, 1).getDay() + 6) % 7;
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const daysInPrevMonth = new Date(year, month, 0).getDate();

    const days: CalendarDayItem[] = [];

    for (let i = firstDayIndex - 1; i >= 0; i--) {
      const dNum = daysInPrevMonth - i;
      const prevDate = new Date(year, month - 1, dNum);
      const dStr = this.toYMD(prevDate);
      days.push(
        this.buildDayItem(dStr, dNum, false, todayStr, postIni, postFin, titIni, titFin, proIni, proFin, tienePro),
      );
    }

    for (let d = 1; d <= daysInMonth; d++) {
      const curDate = new Date(year, month, d);
      const dStr = this.toYMD(curDate);
      days.push(
        this.buildDayItem(dStr, d, true, todayStr, postIni, postFin, titIni, titFin, proIni, proFin, tienePro),
      );
    }

    const remaining = 42 - days.length;
    for (let d = 1; d <= remaining; d++) {
      const nextDate = new Date(year, month + 1, d);
      const dStr = this.toYMD(nextDate);
      days.push(
        this.buildDayItem(dStr, d, false, todayStr, postIni, postFin, titIni, titFin, proIni, proFin, tienePro),
      );
    }

    return days;
  });

  private buildDayItem(
    dStr: string,
    dNum: number,
    isCurrentMonth: boolean,
    todayStr: string,
    postIni: string,
    postFin: string,
    titIni: string,
    titFin: string,
    proIni: string,
    proFin: string,
    tienePro: boolean,
  ): CalendarDayItem {
    const isPost = dStr >= postIni && dStr <= postFin;
    const isTit = dStr >= titIni && dStr <= titFin;
    const isPro = tienePro && dStr >= proIni && dStr <= proFin;

    return {
      dateStr: dStr,
      dayNumber: dNum,
      isCurrentMonth,
      isToday: dStr === todayStr,
      isPostulacion: isPost,
      isPostulacionStart: dStr === postIni,
      isPostulacionEnd: dStr === postFin,
      isTitulacion: isTit,
      isTitulacionStart: dStr === titIni,
      isTitulacionEnd: dStr === titFin,
      isProrroga: isPro,
      isProrrogaStart: tienePro && dStr === proIni,
      isProrrogaEnd: tienePro && dStr === proFin,
    };
  }

  calendarMonthLabel = computed(() => {
    const cur = this.currentCalendarMonth();
    const meses = [
      'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
      'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre',
    ];
    return `${meses[cur.getMonth()]} ${cur.getFullYear()}`;
  });

  nombreMesVisual = computed(() => this.calendarMonthLabel());

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

  carrerasFiltradas = computed(() => {
    let list = this.modalidadesCarreras();
    const texto = this.filtroTextoCarrera().toLowerCase().trim();
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

  metricasSeleccion = computed(() => {
    const total = this.modalidadesCarreras().length;
    const seleccionadas = this.carrerasSeleccionadas().size;
    const porcentaje = total > 0 ? Math.round((seleccionadas / total) * 100) : 0;
    return { total, seleccionadas, porcentaje };
  });

  constructor() {
    effect(() => {
      if (this.visible()) {
        this.recuperarBorrador();
      }
    });
  }

  ngOnInit(): void {
    this.cargarDatosBackend();
  }

  // ----------------------------------------------------
  // Persistencia de Borrador en SessionStorage
  // ----------------------------------------------------
  guardarBorrador(): void {
    try {
      const draft: ConvocatoriaDraft = {
        pasoActual: this.pasoActual(),
        form: this.form(),
        carrerasSeleccionadas: Array.from(this.carrerasSeleccionadas()),
        modalidadesSeleccionadas: Array.from(this.modalidadesSeleccionadas()),
      };
      sessionStorage.setItem(DRAFT_STORAGE_KEY, JSON.stringify(draft));
    } catch {
      // Ignorar errores de storage
    }
  }

  recuperarBorrador(): boolean {
    try {
      const raw = sessionStorage.getItem(DRAFT_STORAGE_KEY);
      if (!raw) return false;
      const draft: ConvocatoriaDraft = JSON.parse(raw);
      if (draft && draft.form) {
        this.form.set(draft.form);
        if (draft.carrerasSeleccionadas && Array.isArray(draft.carrerasSeleccionadas)) {
          this.carrerasSeleccionadas.set(new Set(draft.carrerasSeleccionadas));
        }
        if (draft.modalidadesSeleccionadas && Array.isArray(draft.modalidadesSeleccionadas)) {
          this.modalidadesSeleccionadas.set(new Set(draft.modalidadesSeleccionadas));
        }
        if (draft.pasoActual && draft.pasoActual >= 1 && draft.pasoActual <= this.totalPasos) {
          this.pasoActual.set(draft.pasoActual);
        }
        return true;
      }
    } catch {
      // Fallback si hay error de parsing
    }
    return false;
  }

  limpiarBorrador(): void {
    try {
      sessionStorage.removeItem(DRAFT_STORAGE_KEY);
    } catch {
      // Ignorar error si sessionStorage no está disponible
    }
  }

  private resetFormulario(): void {
    this.pasoActual.set(1);
    this.form.set({
      idPeriodo: 'ABR2026',
      detalleConvocatoria: 'Convocatoria Ordinaria ABR2026',
      fechaInicioStr: this.toYMD(new Date()),
      fechaFinStr: this.toYMD(new Date(Date.now() + 14 * 24 * 60 * 60 * 1000)),
      diasPermitidos: 90,
      diasExtension: 30,
      habilitarTodasLasCarreras: true,
    });
  }

  // ----------------------------------------------------
  // Carga de Datos desde Backend
  // ----------------------------------------------------
  cargarDatosBackend(): void {
    const tieneBorrador = this.recuperarBorrador();

    // 1. Períodos académicos registrados en SIGAFI
    this.titulacionService.getPeriodosAcademicos(false).subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.periodos.set(data);

          if (!tieneBorrador) {
            const activo = data.find((p) => p.esActivo) || data[0];
            const defaultPeriodo = activo ? activo.idPeriodo : data[0].idPeriodo;

            this.form.update((f) => ({
              ...f,
              idPeriodo: defaultPeriodo,
              detalleConvocatoria: this.generarTituloConvocatoria(defaultPeriodo),
            }));
          }
        }
      },
      error: () => this.periodos.set([]),
    });

    // 2. Modalidades carreras
    this.recargarCarreras(!tieneBorrador);

    // 3. Modalidades maestras de titulación
    this.recargarModalidades(!tieneBorrador);
  }

  recargarCarreras(autoSeleccionarTodas = false): void {
    this.cargandoCarreras.set(true);
    this.titulacionService.getModalidadesCarreras(true).subscribe({
      next: (data: ModalidadCarreraDto[]) => {
        this.modalidadesCarreras.set(data || []);
        if (autoSeleccionarTodas || this.carrerasSeleccionadas().size === 0) {
          const set = new Set<number>((data || []).map((c) => c.idModalidadCarrera));
          this.carrerasSeleccionadas.set(set);
        }
        this.cargandoCarreras.set(false);
        this.guardarBorrador();
      },
      error: () => this.cargandoCarreras.set(false),
    });
  }

  recargarModalidades(autoSeleccionarTodas = false): void {
    this.cargandoModalidades.set(true);
    this.titulacionService.getModalidadesMaestras(true).subscribe({
      next: (data: ModalidadMaestra[]) => {
        const activas = (data || []).filter((m) => m.esActivo);
        this.modalidades.set(activas);

        if (autoSeleccionarTodas || this.modalidadesSeleccionadas().size === 0) {
          const set = new Set<number>(activas.map((m) => m.idModalidadTitulacion));
          this.modalidadesSeleccionadas.set(set);
        } else {
          // Mantener selecciones previas agregando nuevas activas encontradas
          const set = new Set(this.modalidadesSeleccionadas());
          activas.forEach((m) => set.add(m.idModalidadTitulacion));
          this.modalidadesSeleccionadas.set(set);
        }

        this.cargandoModalidades.set(false);
        this.guardarBorrador();
      },
      error: () => this.cargandoModalidades.set(false),
    });
  }

  crearNuevaModalidad(): void {
    this.nuevaModalidad.emit();
  }

  abrirCrearModalidadEnPestana(): void {
    this.nuevaModalidad.emit();
  }

  // ----------------------------------------------------
  // Navegación del Stepper
  // ----------------------------------------------------
  irAPaso(paso: number): void {
    if (paso < 1 || paso > this.totalPasos) return;
    if (paso > this.pasoActual() && !this.esPasoValido(this.pasoActual())) {
      return;
    }
    this.pasoActual.set(paso);
    this.guardarBorrador();
  }

  siguientePaso(): void {
    const actual = this.pasoActual();
    if (this.esPasoValido(actual) && actual < this.totalPasos) {
      this.pasoActual.set(actual + 1);
      this.guardarBorrador();
    }
  }

  anteriorPaso(): void {
    const actual = this.pasoActual();
    if (actual > 1) {
      this.pasoActual.set(actual - 1);
      this.guardarBorrador();
    }
  }

  esPasoValido(paso: number): boolean {
    const f = this.form();
    switch (paso) {
      case 1:
        return !!f.idPeriodo && f.idPeriodo.trim().length > 0;
      case 2:
        return !!f.detalleConvocatoria && f.detalleConvocatoria.trim().length > 0;
      case 3:
        return (
          !!f.fechaInicioStr &&
          !!f.fechaFinStr &&
          f.fechaFinStr >= f.fechaInicioStr &&
          (f.diasPermitidos || 0) >= 15
        );
      case 4:
        return f.habilitarTodasLasCarreras || this.carrerasSeleccionadas().size > 0;
      case 5:
        return this.modalidadesSeleccionadas().size > 0;
      default:
        return true;
    }
  }

  // ----------------------------------------------------
  // Métodos Paso 1: Período
  // ----------------------------------------------------
  periodoSeleccionadoNombre = computed(() => {
    const id = this.form().idPeriodo;
    const p = this.periodos().find((x) => x.idPeriodo === id);
    return p?.nombre || id || '';
  });

  generarTituloConvocatoria(
    idPeriodo: string,
    tipo?: 'Ordinaria' | 'Extraordinaria' | 'Especial',
  ): string {
    const p = this.periodos().find((x) => x.idPeriodo === idPeriodo);
    const detallePeriodo = p?.nombre || idPeriodo;

    let titulo = tipo
      ? `Convocatoria ${tipo} ${detallePeriodo}`
      : `Convocatoria ${detallePeriodo}`;

    if (titulo.length > 50) {
      titulo = `Convocatoria ${detallePeriodo}`;
    }
    return titulo.substring(0, 50);
  }

  onPeriodoSelect(idPeriodo: string): void {
    const detalle = this.generarTituloConvocatoria(idPeriodo);
    this.form.update((f) => ({
      ...f,
      idPeriodo,
      detalleConvocatoria: detalle,
    }));
    this.guardarBorrador();
  }

  // ----------------------------------------------------
  // Métodos Paso 2: Convocatoria
  // ----------------------------------------------------
  aplicarSugerenciaDetalle(tipo: 'Ordinaria' | 'Extraordinaria' | 'Especial'): void {
    const id = this.form().idPeriodo || 'ACTUAL';
    const nuevo = this.generarTituloConvocatoria(id, tipo);
    this.form.update((f) => ({ ...f, detalleConvocatoria: nuevo }));
    this.guardarBorrador();
  }

  // ----------------------------------------------------
  // Métodos Paso 3: Cronograma, Fechas y Pestañas Excel
  // ----------------------------------------------------
  seleccionarTabFase(tab: 'postulacion' | 'titulacion' | 'prorroga'): void {
    this.tabFaseActiva.set(tab);
    this.irAFase(tab);
  }

  prevMes(): void {
    const cur = this.currentCalendarMonth();
    this.currentCalendarMonth.set(new Date(cur.getFullYear(), cur.getMonth() - 1, 1));
  }

  nextMes(): void {
    const cur = this.currentCalendarMonth();
    this.currentCalendarMonth.set(new Date(cur.getFullYear(), cur.getMonth() + 1, 1));
  }

  irMesActual(): void {
    const now = new Date();
    this.currentCalendarMonth.set(new Date(now.getFullYear(), now.getMonth(), 1));
  }

  irAFase(fase: 'postulacion' | 'titulacion' | 'prorroga'): void {
    const fases = this.cronogramaFases();
    let targetStr = fases.postulacion.inicioStr;
    if (fase === 'titulacion') {
      targetStr = fases.titulacion.inicioStr;
    } else if (fase === 'prorroga') {
      targetStr = fases.prorroga.inicioStr;
    }
    const d = this.fromYMD(targetStr);
    this.currentCalendarMonth.set(new Date(d.getFullYear(), d.getMonth(), 1));
  }

  setModoSeleccion(modo: 'inicio' | 'fin'): void {
    this.seleccionModo.set(modo);
    this.tabFaseActiva.set('postulacion');
  }

  onDayClick(day: CalendarDayItem): void {
    const f = this.form();
    const clickedDate = day.dateStr;

    if (this.seleccionModo() === 'inicio') {
      if (f.fechaFinStr && clickedDate > f.fechaFinStr) {
        const d1 = this.fromYMD(clickedDate);
        const d2 = new Date(d1.getTime() + 14 * 24 * 60 * 60 * 1000);
        this.form.update((prev) => ({
          ...prev,
          fechaInicioStr: clickedDate,
          fechaFinStr: this.toYMD(d2),
        }));
      } else {
        this.form.update((prev) => ({ ...prev, fechaInicioStr: clickedDate }));
      }
      this.seleccionModo.set('fin');
    } else {
      if (clickedDate < f.fechaInicioStr) {
        this.form.update((prev) => ({
          ...prev,
          fechaInicioStr: clickedDate,
          fechaFinStr: prev.fechaInicioStr,
        }));
      } else {
        this.form.update((prev) => ({ ...prev, fechaFinStr: clickedDate }));
      }
      this.seleccionModo.set('inicio');
    }
    this.guardarBorrador();
  }

  onDiasPostulacionChange(dias: number): void {
    const d = Math.max(1, Math.min(365, Number(dias) || 1));
    const f = this.form();
    const baseInicio = this.fromYMD(f.fechaInicioStr);
    const fin = new Date(baseInicio.getTime() + (d - 1) * 24 * 60 * 60 * 1000);
    this.form.update((prev) => ({
      ...prev,
      fechaFinStr: this.toYMD(fin),
    }));
    this.guardarBorrador();
  }

  aplicarPresetDias(dias: number): void {
    this.onDiasPostulacionChange(dias);
    this.seleccionModo.set('inicio');
  }

  aplicarDiasTitPreset(dias: number): void {
    this.form.update((f) => ({ ...f, diasPermitidos: dias }));
    this.guardarBorrador();
  }

  aplicarDiasExtPreset(dias: number): void {
    this.form.update((f) => ({ ...f, diasExtension: dias }));
    this.guardarBorrador();
  }

  // ----------------------------------------------------
  // Métodos Paso 4: Carreras
  // ----------------------------------------------------
  toggleHabilitarTodasLasCarreras(todas: boolean): void {
    this.form.update((f) => ({ ...f, habilitarTodasLasCarreras: todas }));
    this.guardarBorrador();
  }

  toggleCarrera(idModalidadCarrera: number): void {
    const set = new Set(this.carrerasSeleccionadas());
    if (set.has(idModalidadCarrera)) {
      set.delete(idModalidadCarrera);
    } else {
      set.add(idModalidadCarrera);
    }
    this.carrerasSeleccionadas.set(set);
    this.guardarBorrador();
  }

  seleccionarTodasCarreras(): void {
    const set = new Set<number>(this.modalidadesCarreras().map((c) => c.idModalidadCarrera));
    this.carrerasSeleccionadas.set(set);
    this.guardarBorrador();
  }

  deseleccionarTodasCarreras(): void {
    this.carrerasSeleccionadas.set(new Set());
    this.guardarBorrador();
  }

  invertirSeleccionCarreras(): void {
    const current = this.carrerasSeleccionadas();
    const invertido = new Set<number>();
    for (const c of this.modalidadesCarreras()) {
      if (!current.has(c.idModalidadCarrera)) {
        invertido.add(c.idModalidadCarrera);
      }
    }
    this.carrerasSeleccionadas.set(invertido);
    this.guardarBorrador();
  }

  seleccionarPorModalidadEstudio(modalidad: string): void {
    const set = new Set(this.carrerasSeleccionadas());
    const carrerasDeMod = this.modalidadesCarreras().filter(
      (c) => c.nombreModalidadEstudio.toUpperCase() === modalidad.toUpperCase(),
    );
    for (const c of carrerasDeMod) {
      set.add(c.idModalidadCarrera);
    }
    this.carrerasSeleccionadas.set(set);
    this.guardarBorrador();
  }

  // ----------------------------------------------------
  // Métodos Paso 5: Modalidades y Confirmación
  // ----------------------------------------------------
  toggleModalidad(idModalidad: number): void {
    const set = new Set(this.modalidadesSeleccionadas());
    if (set.has(idModalidad)) {
      set.delete(idModalidad);
    } else {
      set.add(idModalidad);
    }
    this.modalidadesSeleccionadas.set(set);
    this.guardarBorrador();
  }

  seleccionarTodasModalidades(): void {
    const set = new Set<number>(this.modalidades().map((m) => m.idModalidadTitulacion));
    this.modalidadesSeleccionadas.set(set);
    this.guardarBorrador();
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

    const dInicio = this.fromYMD(f.fechaInicioStr);
    dInicio.setHours(0, 0, 0, 0);

    const dFin = this.fromYMD(f.fechaFinStr);
    dFin.setHours(23, 59, 59, 999);

    this.limpiarBorrador();
    this.resetFormulario();

    this.confirm.emit({
      idPeriodo: f.idPeriodo,
      detalleConvocatoria: f.detalleConvocatoria,
      fechaInicioCorte: dInicio.toISOString(),
      fechaFinCorte: dFin.toISOString(),
      diasPermitidos: Number(f.diasPermitidos) || 90,
      diasExtension: Number(f.diasExtension) || 30,
      habilitarTodasLasCarreras: f.habilitarTodasLasCarreras,
      idsModalidadesCarrerasHabilitadas: idsModalidadesCarreras,
      idsModalidadesHabilitadas: idsModalidades,
    });
  }

  cancelarApertura(): void {
    this.limpiarBorrador();
    this.resetFormulario();
    this.modalClose.emit();
  }

  onClose(): void {
    this.guardarBorrador();
    this.modalClose.emit();
  }

  // ----------------------------------------------------
  // Helpers de Formato de Fechas
  // ----------------------------------------------------
  toYMD(d: Date): string {
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  fromYMD(s: string): Date {
    if (!s) return new Date();
    const parts = s.split('-').map((p) => parseInt(p, 10));
    if (parts.length === 3 && !isNaN(parts[0]) && !isNaN(parts[1]) && !isNaN(parts[2])) {
      return new Date(parts[0], parts[1] - 1, parts[2]);
    }
    return new Date();
  }
}
