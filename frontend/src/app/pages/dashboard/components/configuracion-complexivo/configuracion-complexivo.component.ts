import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ComplexivoStore } from '../../../../application/complexivo/complexivo.store';
import {
  DocenteComplexivo,
  GuardarMallaComplexivo,
  MallaComplexivo,
  ProfesorComplexivo,
  TemaComplexivo,
} from '../../../../domain/models/complexivo.model';
import { imprimirPlanificacion } from '../../../../shared/utils/complexivo-planning-pdf';
import { RequisicionPersonalInicial } from '../../../../domain/models/requisicion-personal.model';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { DateFieldComponent } from '../../../../shared/components/date-field/date-field.component';

@Component({
  selector: 'app-configuracion-complexivo',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DrawerComponent,
    DateFieldComponent,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  providers: [ComplexivoStore],
  templateUrl: './configuracion-complexivo.component.html',
  styleUrls: ['../complexivo-ui.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfiguracionComplexivoComponent implements OnInit {
  readonly store = inject(ComplexivoStore);
  readonly puedeCrear = input(false);
  readonly puedeEditar = input(false);
  readonly idCarreraPredeterminada = input<number | null>(null);
  readonly requisicionSolicitada = output<RequisicionPersonalInicial>();
  readonly idCohorte = signal<number | null>(null);
  readonly mensaje = signal<string | null>(null);
  readonly intentoGuardar = signal(false);
  readonly busquedasDocentes = new Map<DocenteComplexivo, string>();
  readonly mostrarProfesor = (id: string | null): string => (id ? this.profesor(id) : '');
  readonly cohortes = computed(() => [
    ...new Map(this.store.cohortesCarreras().map((c) => [c.idCohorte, c])).values(),
  ]);
  readonly carreras = computed(() =>
    this.store.cohortesCarreras().filter((c) => c.idCohorte === this.idCohorte()),
  );
  readonly modulosActivos = computed(() => this.store.malla().filter((m) => m.esActivo));
  readonly modulosSinDocentes = computed(() =>
    this.modulosActivos().filter((modulo) => !this.tieneDocentes(modulo)),
  );
  readonly vacantesRequeridas = computed(() =>
    this.modulosSinDocentes().reduce(
      (total, modulo) =>
        total + Math.max(1, modulo.docentes.filter((d) => d.esActivo && !d.idProfesor).length),
      0,
    ),
  );
  readonly planificacion = computed(() =>
    this.modulosActivos().map((modulo, indice) => {
      const asignaturas = new Map<number, TemaComplexivo[]>();
      for (const tema of modulo.temas) {
        const temas = asignaturas.get(tema.idAsignatura) ?? [];
        temas.push(tema);
        asignaturas.set(tema.idAsignatura, temas);
      }
      return {
        modulo,
        color: indice % 3,
        asignaturas: [...asignaturas].map(([idAsignatura, temas]) => ({ idAsignatura, temas })),
        docentes: modulo.docentes.filter((docente) => docente.esActivo),
      };
    }),
  );
  readonly periodoCurso = computed(() => {
    const fechas = this.modulosActivos()
      .flatMap((m) => [m.fechaInicio, m.fechaFin])
      .sort();
    return { inicio: fechas[0], fin: fechas.at(-1) };
  });
  readonly modalidadesCurso = computed(() =>
    [...new Set(this.modulosActivos().map((m) => m.modalidad))].join(', '),
  );
  editor: GuardarMallaComplexivo | null = null;
  idEdicion: number | null = null;
  private original = '';

  async ngOnInit(): Promise<void> {
    await this.store.iniciar();
    const lista = this.store.cohortesCarreras();
    const inicial =
      lista.find((c) => c.esActivo && c.idCarrera === this.idCarreraPredeterminada()) ??
      lista.find((c) => c.esActivo) ??
      lista[0];
    if (inicial) {
      this.idCohorte.set(inicial.idCohorte);
      await this.store.seleccionar(inicial);
    }
  }

  async cambiarCohorte(id: number): Promise<void> {
    if (this.store.guardando()) return;
    this.idCohorte.set(id);
    const contexto = this.carreras()[0];
    if (contexto) await this.seleccionarCarrera(contexto.idCohorteCarrera);
  }

  async seleccionarCarrera(id: number): Promise<void> {
    if (this.store.guardando()) return;
    const contexto = this.store.cohortesCarreras().find((c) => c.idCohorteCarrera === id);
    this.mensaje.set(null);
    if (contexto) await this.store.seleccionar(contexto);
  }

  solicitarPersonal(): void {
    const contexto = this.store.contexto();
    if (
      !contexto?.esActivo ||
      !this.vacantesRequeridas() ||
      this.editor ||
      this.store.guardando() ||
      this.store.cargando()
    )
      return;
    this.requisicionSolicitada.emit({
      cargo: `Docente para curso complexivo de la carrera de ${contexto.carrera}`,
      numeroVacantes: this.vacantesRequeridas(),
      origen: {
        idCohorteCarrera: contexto.idCohorteCarrera,
        carrera: contexto.carrera,
        cohorte: contexto.cohorte,
        idsMallaComplexivo: this.modulosSinDocentes().map((modulo) => modulo.idMallaComplexivo),
      },
    });
  }

  nuevo(): void {
    const contexto = this.store.contexto();
    if (!contexto || !this.puedeCrear() || this.store.guardando()) return;
    this.idEdicion = null;
    this.editor = {
      idCohorteCarrera: contexto.idCohorteCarrera,
      detalle: '',
      detalleModulo: '',
      fechaInicio: '',
      fechaFin: '',
      modalidad: '',
      totalHoras: 0,
      esActivo: true,
      temas: [],
      docentes: [],
    };
    this.original = JSON.stringify(this.editor);
    this.intentoGuardar.set(false);
    this.busquedasDocentes.clear();
    this.store.error.set(null);
    this.mensaje.set(null);
  }

  editar(malla: MallaComplexivo): void {
    if (!this.puedeEditar() || this.store.guardando()) return;
    this.idEdicion = malla.idMallaComplexivo;
    this.editor = structuredClone(malla);
    this.original = JSON.stringify(this.editor);
    this.intentoGuardar.set(false);
    this.busquedasDocentes.clear();
    this.store.error.set(null);
    this.mensaje.set(null);
  }

  cerrarEditor(): void {
    if (this.store.guardando()) return;
    this.editor = null;
    this.store.error.set(null);
  }

  hayCambios(): boolean {
    return this.editor !== null && JSON.stringify(this.editor) !== this.original;
  }

  sumarHoras(): number {
    return (
      this.editor?.temas.reduce((total, tema) => total + (Number(tema.horasPorTema) || 0), 0) ?? 0
    );
  }

  temasCompletos(): boolean {
    return !!this.editor?.temas.length && this.editor.temas.every((tema) => tema.idAsignatura > 0);
  }

  agregarTema(): void {
    this.editor?.temas.push({ idTemas: 0, idAsignatura: 0, detalle: '', horasPorTema: 0 });
  }

  agregarDocente(): DocenteComplexivo | null {
    if (!this.editor) return null;
    const docente: DocenteComplexivo = {
      idMallaDocenteComplexivo: 0,
      idProfesor: null,
      fechaAsignacion: null,
      codigoAsignacion: null,
      esActivo: true,
      horarios: [],
    };
    this.editor.docentes.push(docente);
    return docente;
  }

  agregarHorario(docente?: DocenteComplexivo): void {
    const asignacion =
      docente ??
      this.editor?.docentes.find((d) => d.esActivo && !d.idProfesor) ??
      this.agregarDocente();
    asignacion?.horarios.push({ idModulosHorarios: 0, dias: '', franjaHoraria: '' });
  }

  tieneDocentes(modulo: MallaComplexivo): boolean {
    return modulo.docentes.some((d) => d.esActivo && !!d.idProfesor);
  }

  tieneHorarios(modulo: MallaComplexivo): boolean {
    return modulo.docentes.some((d) => d.esActivo && d.horarios.length > 0);
  }

  diferenciaHoras(): string | null {
    if (!this.editor || this.editor.totalHoras <= 0) return null;
    const diferencia = this.editor.totalHoras - this.sumarHoras();
    if (diferencia === 0) return null;
    return diferencia > 0
      ? `Faltan ${diferencia} horas por distribuir en los temas para completar las ${this.editor.totalHoras} horas del módulo.`
      : `Los temas exceden el total del módulo en ${-diferencia} horas.`;
  }

  usarHorasTemas(): void {
    if (this.editor && this.sumarHoras() > 0) this.editor.totalHoras = this.sumarHoras();
  }

  erroresValidacion(form: NgForm): string[] {
    if (!this.editor) return [];
    const etiquetas: Record<string, string> = {
      detalle: 'Nombre / número del módulo',
      modalidad: 'Modalidad del curso',
      detalleModulo: 'Descripción del módulo',
      fechaInicio: 'Fecha de inicio',
      fechaFin: 'Fecha final',
      totalHoras: 'Total de horas del módulo',
    };
    const errores = Object.entries(form.controls)
      .filter(([, control]) => control.invalid)
      .map(([nombre, control]) => {
        const etiqueta =
          etiquetas[nombre] ??
          nombre
            .replace(/^asignatura-(\d+)$/, (_, i: string) => `Asignatura del tema ${Number(i) + 1}`)
            .replace(/^tema-horas-(\d+)$/, (_, i: string) => `Horas del tema ${Number(i) + 1}`)
            .replace(
              /^tema-detalle-(\d+)$/,
              (_, i: string) => `Contenidos del tema ${Number(i) + 1}`,
            )
            .replace(
              /^asignacion-fecha-(\d+)$/,
              (_, i: string) => `Fecha de la asignación ${Number(i) + 1}`,
            )
            .replace(
              /^dias-(\d+)-(\d+)$/,
              (_, d: string, h: string) =>
                `Días del horario ${Number(h) + 1}, asignación ${Number(d) + 1}`,
            )
            .replace(
              /^franja-(\d+)-(\d+)$/,
              (_, d: string, h: string) =>
                `Franja del horario ${Number(h) + 1}, asignación ${Number(d) + 1}`,
            );
        if (control.hasError('required')) return `${etiqueta}: completa este campo.`;
        if (control.hasError('matDatepickerParse'))
          return `${etiqueta}: escribe una fecha válida (dd/mm/aaaa).`;
        if (control.hasError('matDatepickerMin'))
          return 'La fecha final debe ser igual o posterior a la fecha de inicio.';
        if (control.hasError('min') || control.hasError('pattern'))
          return `${etiqueta}: usa un número entero mayor que cero.`;
        return `${etiqueta}: revisa el valor ingresado.`;
      });
    if (!this.temasCompletos()) errores.push('Agrega al menos un tema y selecciona su asignatura.');
    const horas = this.diferenciaHoras();
    if (horas) errores.push(horas);
    if (
      this.editor.fechaFin < this.editor.fechaInicio &&
      !errores.includes('La fecha final debe ser igual o posterior a la fecha de inicio.')
    )
      errores.push('La fecha final debe ser igual o posterior a la fecha de inicio.');
    return errores;
  }

  async guardar(form: NgForm, element?: HTMLFormElement): Promise<void> {
    if (!this.editor || this.store.guardando()) return;
    this.intentoGuardar.set(true);
    form.form.markAllAsTouched();
    if (this.erroresValidacion(form).length) {
      (
        element?.querySelector<HTMLElement>(
          'input.ng-invalid, select.ng-invalid, textarea.ng-invalid',
        ) ?? element?.querySelector<HTMLElement>('#modulo-horas')
      )?.focus();
      return;
    }
    if (this.idEdicion === null ? !this.puedeCrear() : !this.puedeEditar()) return;
    const datos = structuredClone(this.editor);
    datos.docentes = datos.docentes.map((d) => ({
      ...d,
      fechaAsignacion: d.fechaAsignacion || null,
      codigoAsignacion: d.codigoAsignacion || null,
    }));
    if (await this.store.guardar(this.idEdicion, datos)) {
      this.editor = null;
      this.mensaje.set('La configuración se guardó correctamente.');
    }
  }

  asignatura(id: number): string {
    return (
      this.store.asignaturas().find((a) => a.idAsignatura === id)?.nombre ?? `Asignatura ${id}`
    );
  }

  profesor(id: string | null): string {
    if (!id) return 'Docente pendiente de asignación';
    return this.store.profesores().find((p) => p.idProfesor === id)?.nombresCompletos ?? id;
  }

  profesorEnCatalogo(id: string): boolean {
    return this.store.profesores().some((profesor) => profesor.idProfesor === id);
  }

  profesoresFiltrados(docente: DocenteComplexivo): ProfesorComplexivo[] {
    const normalizar = (texto: string): string =>
      texto
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .toLowerCase()
        .trim();
    const busqueda = normalizar(this.busquedasDocentes.get(docente) ?? '');
    return this.store
      .profesores()
      .filter(
        (profesor) =>
          (profesor.activo || profesor.idProfesor === docente.idProfesor) &&
          normalizar(`${profesor.nombresCompletos} ${profesor.idProfesor}`).includes(busqueda),
      );
  }

  imprimir(documento: HTMLElement): void {
    imprimirPlanificacion(documento);
  }
}
