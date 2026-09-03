import { Component, input, output, signal, computed, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  RequisitoMaestro,
  ResponsableRequisito,
  ProfesorCandidato,
} from '../../../core/models/titulacion.models';
import { TitulacionService } from '../../../core/services/titulacion.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-responsables-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './responsables-modal.component.html',
  styleUrls: ['./responsables-modal.component.css'],
})
export class ResponsablesModalComponent {
  private readonly titulacionService = inject(TitulacionService);
  private readonly notificationService = inject(NotificationService);

  visible = input<boolean>(false);
  requisito = input<RequisitoMaestro | null>(null);

  cerrar = output<void>();
  cambioAsignaciones = output<void>();

  responsables = signal<ResponsableRequisito[]>([]);
  profesoresCandidatos = signal<ProfesorCandidato[]>([]);
  cargando = signal<boolean>(false);
  busquedaProfesor = signal<string>('');
  filtroAsignados = signal<string>('');
  profesorSeleccionado = signal<string>('');
  asignando = signal<boolean>(false);

  // Docentes candidatos filtrados localmente o por búsqueda remota
  profesoresFiltrados = computed(() => {
    const q = this.busquedaProfesor().toLowerCase().trim();
    const lista = this.profesoresCandidatos();
    if (!q) return lista;
    return lista.filter(
      (p) =>
        p.idProfesor.toLowerCase().includes(q) ||
        p.nombresCompletos.toLowerCase().includes(q) ||
        (p.email && p.email.toLowerCase().includes(q)),
    );
  });

  // Docentes ya asignados filtrados
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
    effect(() => {
      const r = this.requisito();
      const isVisible = this.visible();
      if (isVisible && r) {
        this.busquedaProfesor.set('');
        this.filtroAsignados.set('');
        this.profesorSeleccionado.set('');
        this.cargarResponsables(r.idRequisitos);
        this.cargarCandidatos();
      }
    });
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

  cargarCandidatos(): void {
    this.titulacionService.getProfesoresCandidatos().subscribe({
      next: (data) => this.profesoresCandidatos.set(data),
      error: () => this.profesoresCandidatos.set([]),
    });
  }

  onBusquedaChange(): void {
    // El computed profesoresFiltrados se actualiza reactivamente
  }

  asignarProfesor(): void {
    const r = this.requisito();
    const idProf = this.profesorSeleccionado();
    if (!r || !idProf) return;

    this.asignando.set(true);
    this.titulacionService.asignarProfesorRequisito(r.idRequisitos, idProf).subscribe({
      next: () => {
        this.asignando.set(false);
        this.profesorSeleccionado.set('');
        this.notificationService.success('Docente asignado exitosamente como responsable.');
        this.cargarResponsables(r.idRequisitos);
        this.cambioAsignaciones.emit();
      },
      error: (err) => {
        this.asignando.set(false);
        this.notificationService.error(err.error?.message || 'Error al asignar docente.');
      },
    });
  }

  desasignar(idResponsableEvidencias: number): void {
    const r = this.requisito();
    if (!r) return;

    this.titulacionService.desasignarProfesorRequisito(idResponsableEvidencias).subscribe({
      next: () => {
        this.notificationService.success('Asignación de docente removida.');
        this.cargarResponsables(r.idRequisitos);
        this.cambioAsignaciones.emit();
      },
      error: (err) =>
        this.notificationService.error(err.error?.message || 'Error al remover asignación.'),
    });
  }
}
