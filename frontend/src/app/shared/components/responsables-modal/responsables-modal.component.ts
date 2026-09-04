import {
  Component,
  input,
  output,
  signal,
  computed,
  effect,
  inject,
  OnDestroy,
  HostListener,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription, of } from 'rxjs';
import {
  debounceTime,
  distinctUntilChanged,
  switchMap,
  catchError,
  finalize,
} from 'rxjs/operators';
import {
  RequisitoMaestro,
  ResponsableRequisito,
  ProfesorCandidato,
} from '../../../core/models/titulacion.models';
import { TitulacionService } from '../../../core/services/titulacion.service';
import { NotificationService } from '../../../core/services/notification.service';
import { DrawerComponent } from '../drawer/drawer.component';

@Component({
  selector: 'app-responsables-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, DrawerComponent],
  templateUrl: './responsables-modal.component.html',
  styleUrls: ['./responsables-modal.component.css'],
})
export class ResponsablesModalComponent implements OnDestroy {
  private readonly titulacionService = inject(TitulacionService);
  private readonly notificationService = inject(NotificationService);

  visible = input<boolean>(false);
  requisito = input<RequisitoMaestro | null>(null);

  cerrar = output<void>();
  cambioAsignaciones = output<void>();

  responsables = signal<ResponsableRequisito[]>([]);
  profesoresCandidatos = signal<ProfesorCandidato[]>([]);
  cargando = signal<boolean>(false);
  cargandoCandidatos = signal<boolean>(false);
  busquedaProfesor = signal<string>('');
  filtroAsignados = signal<string>('');
  mostrarDropdown = signal<boolean>(false);
  asignandoId = signal<string | null>(null);

  // Flujo reactivo para búsqueda remota en backend con debounce de 1 segundo
  private readonly busquedaSubject = new Subject<string>();
  private readonly searchSub: Subscription;

  /**
   * Docentes devueltos por la base de datos que aún NO están asignados a este requisito.
   * Si ya están asignados, se excluyen automáticamente para que no vuelvan a aparecer.
   */
  candidatosDisponibles = computed(() => {
    const asignadosIds = new Set(this.responsables().map((r) => r.idProfesor.trim().toLowerCase()));
    return this.profesoresCandidatos().filter(
      (p) => !asignadosIds.has(p.idProfesor.trim().toLowerCase()),
    );
  });

  // Docentes ya asignados filtrados en la tabla inferior
  responsablesFiltrados = computed(() => {
    const q = this.filtroAsignados().toLowerCase().trim();
    const lista = this.responsables();
    if (!q) return lista;
    return lista.filter(
      (r) =>
        r.idProfesor.toLowerCase().includes(q) ||
        r.nombreProfesor.toLowerCase().includes(q) ||
        (r.emailProfesor && r.emailProfesor.toLowerCase().includes(q)),
    );
  });

  constructor() {
    this.searchSub = this.busquedaSubject
      .pipe(
        debounceTime(1000),
        distinctUntilChanged(),
        switchMap((termino) => {
          this.cargandoCandidatos.set(true);
          return this.titulacionService.getProfesoresCandidatos(termino).pipe(
            catchError(() => of([])),
            finalize(() => this.cargandoCandidatos.set(false)),
          );
        }),
      )
      .subscribe((data) => {
        this.profesoresCandidatos.set(data);
        if (this.busquedaProfesor().trim()) {
          this.mostrarDropdown.set(true);
        }
      });

    effect(() => {
      const r = this.requisito();
      const isVisible = this.visible();
      if (isVisible && r) {
        this.busquedaProfesor.set('');
        this.filtroAsignados.set('');
        this.mostrarDropdown.set(false);
        this.asignandoId.set(null);
        this.profesoresCandidatos.set([]);
        this.cargarResponsables(r.idRequisitos);
      }
    });
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    this.mostrarDropdown.set(false);
  }

  ngOnDestroy(): void {
    this.searchSub.unsubscribe();
  }

  cargarResponsables(idRequisito: number): void {
    this.cargando.set(true);
    this.titulacionService.getResponsablesPorRequisito(idRequisito).subscribe({
      next: (data) => {
        this.responsables.set(data);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }

  cargarCandidatos(termino = ''): void {
    this.cargandoCandidatos.set(true);
    this.titulacionService
      .getProfesoresCandidatos(termino)
      .pipe(
        catchError(() => of([])),
        finalize(() => this.cargandoCandidatos.set(false)),
      )
      .subscribe((data) => {
        this.profesoresCandidatos.set(data);
        if (termino.trim()) {
          this.mostrarDropdown.set(true);
        }
      });
  }

  onBusquedaInput(valor: string): void {
    this.busquedaProfesor.set(valor);
    if (!valor.trim()) {
      this.mostrarDropdown.set(false);
      this.profesoresCandidatos.set([]);
      return;
    }
    this.busquedaSubject.next(valor);
  }

  buscarInmediato(): void {
    if (this.busquedaProfesor().trim()) {
      this.cargarCandidatos(this.busquedaProfesor());
    }
  }

  limpiarBusqueda(): void {
    this.busquedaProfesor.set('');
    this.mostrarDropdown.set(false);
    this.profesoresCandidatos.set([]);
  }

  onInputFocus(): void {
    if (this.busquedaProfesor().trim() && this.profesoresCandidatos().length > 0) {
      this.mostrarDropdown.set(true);
    }
  }

  getIniciales(nombre: string): string {
    if (!nombre) return 'D';
    const partes = nombre.trim().split(/\s+/);
    if (partes.length === 1) return partes[0].substring(0, 2).toUpperCase();
    return (partes[0][0] + partes[1][0]).toUpperCase();
  }

  seleccionarYAsignar(profesor: ProfesorCandidato): void {
    const r = this.requisito();
    if (!r || !profesor || this.asignandoId()) return;

    this.asignandoId.set(profesor.idProfesor);
    this.titulacionService.asignarProfesorRequisito(r.idRequisitos, profesor.idProfesor).subscribe({
      next: () => {
        this.asignandoId.set(null);
        this.mostrarDropdown.set(false);
        this.busquedaProfesor.set('');
        this.profesoresCandidatos.set([]);
        this.notificationService.success(
          `Docente ${profesor.nombresCompletos} asignado exitosamente.`,
        );
        this.cargarResponsables(r.idRequisitos);
        this.cambioAsignaciones.emit();
      },
      error: (err) => {
        this.asignandoId.set(null);
        this.notificationService.error(err.error?.message || 'Error al asignar docente.');
      },
    });
  }

  desasignar(idResponsableEvidencias: number): void {
    const r = this.requisito();
    if (!r) return;

    this.titulacionService.desasignarProfesorRequisito(idResponsableEvidencias).subscribe({
      next: () => {
        this.notificationService.success(
          'Asignación de docente removida. Vuelve a estar disponible.',
        );
        this.cargarResponsables(r.idRequisitos);
        this.cambioAsignaciones.emit();
      },
      error: (err) =>
        this.notificationService.error(err.error?.message || 'Error al remover asignación.'),
    });
  }
}
