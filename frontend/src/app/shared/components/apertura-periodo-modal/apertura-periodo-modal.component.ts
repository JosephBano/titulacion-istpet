import { Component, input, output, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  AperturarPeriodoRequest,
  ModalidadMaestra,
  ModalidadCarreraDto,
} from '../../../core/models/titulacion.models';
import { TitulacionService } from '../../../core/services/titulacion.service';

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

@Component({
  selector: 'app-apertura-periodo-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './apertura-periodo-modal.component.html',
  styleUrls: ['./apertura-periodo-modal.component.css'],
})
export class AperturaPeriodoModalComponent implements OnInit {
  private readonly titulacionService = inject(TitulacionService);

  // Inputs y Outputs
  visible = input<boolean>(false);
  modalClose = output<void>();
  confirm = output<AperturarPeriodoRequest>();

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

  // ----------------------------------------------------
  // Paso 3: Cronograma Visual y Calendario Multi-Fase
  // ----------------------------------------------------
  currentCalendarMonth = signal<Date>(
    new Date(new Date().getFullYear(), new Date().getMonth(), 1),
  );
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
    // Inicio de titulación = día posterior al cierre de postulación
    const dInicioTit = new Date(dFinPost.getTime() + 24 * 60 * 60 * 1000);
    const diasTit = Math.max(1, Number(f.diasPermitidos) || 90);
    const dFinTit = new Date(dInicioTit.getTime() + (diasTit - 1) * 24 * 60 * 60 * 1000);

    // Inicio de prórroga = día posterior al fin de titulación reglamentaria
    const dInicioPro = new Date(dFinTit.getTime() + 24 * 60 * 60 * 1000);
    const diasPro = Math.max(0, Number(f.diasExtension) || 0);
    const dFinPro =
      diasPro > 0
        ? new Date(dInicioPro.getTime() + (diasPro - 1) * 24 * 60 * 60 * 1000)
        : dFinTit;

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

  // Generador de la cuadrícula del calendario
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

    const firstDayOfWeek = new Date(year, month, 1).getDay();
    const startOffset = firstDayOfWeek === 0 ? 6 : firstDayOfWeek - 1;

    const prevMonthDaysCount = new Date(year, month, 0).getDate();
    const days: CalendarDayItem[] = [];

    const buildDay = (d: Date, isCurMonth: boolean): CalendarDayItem => {
      const s = this.toYMD(d);
      const isPost = s >= postIni && s <= postFin;
      const isTit = s >= titIni && s <= titFin;
      const isPro = tienePro && s >= proIni && s <= proFin;

      return {
        dateStr: s,
        dayNumber: d.getDate(),
        isCurrentMonth: isCurMonth,
        isToday: s === todayStr,
        isPostulacion: isPost,
        isPostulacionStart: s === postIni,
        isPostulacionEnd: s === postFin,
        isTitulacion: isTit,
        isTitulacionStart: s === titIni,
        isTitulacionEnd: s === titFin,
        isProrroga: isPro,
        isProrrogaStart: tienePro && s === proIni,
        isProrrogaEnd: tienePro && s === proFin,
      };
    };

    for (let i = startOffset - 1; i >= 0; i--) {
      days.push(buildDay(new Date(year, month - 1, prevMonthDaysCount - i), false));
    }

    const daysInMonth = new Date(year, month + 1, 0).getDate();
    for (let day = 1; day <= daysInMonth; day++) {
      days.push(buildDay(new Date(year, month, day), true));
    }

    const remaining = (7 - (days.length % 7)) % 7;
    for (let day = 1; day <= remaining; day++) {
      days.push(buildDay(new Date(year, month + 1, day), false));
    }

    return days;
  });

  nombreMesVisual = computed(() => {
    const d = this.currentCalendarMonth();
    const meses = [
      'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
      'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
    ];
    return `${meses[d.getMonth()]} ${d.getFullYear()}`;
  });

  // ----------------------------------------------------
  // Paso 4: Selección Personalizada de Carreras
  // ----------------------------------------------------
  filtroTextoCarrera = signal<string>('');
  filtroModalidadEstudio = signal<string>('TODAS');
  carrerasSeleccionadas = signal<Set<number>>(new Set());
  modalidadesSeleccionadas = signal<Set<number>>(new Set());

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

  metricasSeleccion = computed(() => {
    const total = this.modalidadesCarreras().length;
    const seleccionadas = this.carrerasSeleccionadas().size;
    const porcentaje = total > 0 ? Math.round((seleccionadas / total) * 100) : 0;
    return {
      total,
      seleccionadas,
      porcentaje,
      todasSeleccionadas: total > 0 && seleccionadas === total,
      ningunaSeleccionada: seleccionadas === 0,
    };
  });

  // ----------------------------------------------------
  // Ciclo de Vida e Inicialización
  // ----------------------------------------------------
  ngOnInit(): void {
    this.cargarDatosIniciales();
  }

  cargarDatosIniciales(): void {
    // 1. Períodos académicos ISTPET (Solo vigentes o futuros)
    this.titulacionService.getPeriodosAcademicos(true).subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          const currentYear = new Date().getFullYear();
          const vigentesOFuturos = data.filter((p) => {
            if (p.esActivo) return true;
            if (p.fechaFinal) {
              const dFin = new Date(p.fechaFinal);
              if (dFin >= new Date(currentYear, new Date().getMonth() - 2, 1)) {
                return true;
              }
            }
            const match = p.idPeriodo.match(/\d{4}/);
            if (match) {
              const y = parseInt(match[0], 10);
              return y >= currentYear;
            }
            return false;
          });

          const listaFinal = vigentesOFuturos.length > 0 ? vigentesOFuturos : data;
          this.periodos.set(listaFinal);

          const activo = listaFinal.find((p) => p.esActivo);
          const defaultPeriodo = activo ? activo.idPeriodo : listaFinal[0].idPeriodo;

          this.form.update((f) => ({
            ...f,
            idPeriodo: defaultPeriodo,
            detalleConvocatoria: `Convocatoria Ordinaria ${defaultPeriodo}`.substring(0, 45),
          }));
        }
      },
      error: () => this.periodos.set([]),
    });

    // 2. Modalidades carreras
    this.titulacionService.getModalidadesCarreras(true).subscribe({
      next: (data: ModalidadCarreraDto[]) => {
        this.modalidadesCarreras.set(data || []);
        const set = new Set<number>((data || []).map((c) => c.idModalidadCarrera));
        this.carrerasSeleccionadas.set(set);
      },
      error: () => this.modalidadesCarreras.set([]),
    });

    // 3. Modalidades maestras de titulación
    this.titulacionService.getModalidadesMaestras(true).subscribe({
      next: (data: ModalidadMaestra[]) => {
        const activas = (data || []).filter((m) => m.esActivo);
        this.modalidades.set(activas);
        const set = new Set<number>(activas.map((m) => m.idModalidadTitulacion));
        this.modalidadesSeleccionadas.set(set);
      },
      error: () => this.modalidades.set([]),
    });
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
  }

  siguientePaso(): void {
    const actual = this.pasoActual();
    if (this.esPasoValido(actual) && actual < this.totalPasos) {
      this.pasoActual.set(actual + 1);
    }
  }

  anteriorPaso(): void {
    const actual = this.pasoActual();
    if (actual > 1) {
      this.pasoActual.set(actual - 1);
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
  onPeriodoSelect(idPeriodo: string): void {
    const detalleCorto = `Convocatoria Ordinaria ${idPeriodo}`.substring(0, 45);
    this.form.update((f) => ({
      ...f,
      idPeriodo,
      detalleConvocatoria: detalleCorto,
    }));
  }

  // ----------------------------------------------------
  // Métodos Paso 2: Convocatoria
  // ----------------------------------------------------
  aplicarSugerenciaDetalle(tipo: 'Ordinaria' | 'Extraordinaria' | 'Especial'): void {
    const id = this.form().idPeriodo || 'ACTUAL';
    const nuevo = `Convocatoria ${tipo} ${id}`.substring(0, 45);
    this.form.update((f) => ({ ...f, detalleConvocatoria: nuevo }));
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
    const clicked = day.dateStr;
    const f = this.form();

    this.tabFaseActiva.set('postulacion');

    if (this.seleccionModo() === 'inicio') {
      this.form.update((prev) => {
        const fin = clicked > prev.fechaFinStr ? clicked : prev.fechaFinStr;
        return { ...prev, fechaInicioStr: clicked, fechaFinStr: fin };
      });
      this.seleccionModo.set('fin');
    } else {
      if (clicked < f.fechaInicioStr) {
        this.form.update((prev) => ({
          ...prev,
          fechaInicioStr: clicked,
        }));
      } else {
        this.form.update((prev) => ({
          ...prev,
          fechaFinStr: clicked,
        }));
        this.seleccionModo.set('inicio');
      }
    }
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
  }

  aplicarPresetDias(dias: number): void {
    this.onDiasPostulacionChange(dias);
    this.seleccionModo.set('inicio');
  }

  aplicarDiasTitPreset(dias: number): void {
    this.form.update((f) => ({ ...f, diasPermitidos: dias }));
  }

  aplicarDiasExtPreset(dias: number): void {
    this.form.update((f) => ({ ...f, diasExtension: dias }));
  }

  // ----------------------------------------------------
  // Métodos Paso 4: Carreras
  // ----------------------------------------------------
  toggleHabilitarTodasLasCarreras(todas: boolean): void {
    this.form.update((f) => ({ ...f, habilitarTodasLasCarreras: todas }));
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
    this.carrerasSeleccionadas.set(new Set());
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
  }

  seleccionarTodasModalidades(): void {
    const set = new Set<number>(this.modalidades().map((m) => m.idModalidadTitulacion));
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

    const dInicio = this.fromYMD(f.fechaInicioStr);
    dInicio.setHours(0, 0, 0, 0);

    const dFin = this.fromYMD(f.fechaFinStr);
    dFin.setHours(23, 59, 59, 999);

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

  onClose(): void {
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
