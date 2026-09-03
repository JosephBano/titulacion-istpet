import { Component, OnInit, effect, inject, signal, viewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { AlumnosTitulacionService } from '../../core/services/alumnos-titulacion.service';
import { AlumnoApto, GraduadoHistorico } from '../../core/models/alumno-filtro.model';

@Component({
  selector: 'app-alumnos-titulacion',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
  ],
  template: `
    <div class="alumnos-container">
      <header class="page-header">
        <div class="header-title">
          <h1>Gestión y Filtrado de Estudiantes</h1>
          <p class="subtitle">
            Identificación de estudiantes aptos para titulación y registro histórico de graduados
          </p>
        </div>
        <a routerLink="/dashboard" class="btn-back">
          <i class="bi bi-arrow-left"></i> Volver al Dashboard
        </a>
      </header>

      <!-- Banner Normativa Institucional: Restricción Semestres Iniciales -->
      <div class="restriction-notice-banner">
        <div class="notice-icon">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="20"
            height="20"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
          >
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="8" x2="12" y2="12"></line>
            <line x1="12" y1="16" x2="12.01" y2="16"></line>
          </svg>
        </div>
        <div class="notice-content">
          <strong>Restricción Académica Institucional de Postulación</strong>
          <p>
            Los estudiantes matriculados en 1ro, 2do y 3er nivel/semestre no están habilitados para
            participar en el proceso de titulación y se encuentran bloqueados automáticamente por el
            sistema. El proceso de admisión es exclusivo para estudiantes de 4to nivel en adelante o
            egresados.
          </p>
        </div>
      </div>

      <!-- Pestañas de Navegación (Tabs) -->
      <div class="tabs-nav">
        <button class="tab-btn" [class.active]="tabActiva() === 'aptos'" (click)="setTab('aptos')">
          <i class="bi bi-person-check-fill"></i>
          Alumnos Aptos (Disponibles)
        </button>
        <button
          class="tab-btn"
          [class.active]="tabActiva() === 'graduados'"
          (click)="setTab('graduados')"
        >
          <i class="bi bi-journal-bookmark-fill"></i>
          Histórico de Graduados (alumnos_titulos)
        </button>
      </div>

      <!-- Barra de Filtros y Búsqueda -->
      <div class="filter-card">
        <div class="search-box">
          <i class="bi bi-search search-icon"></i>
          <input
            type="text"
            [(ngModel)]="busquedaTexto"
            (input)="aplicarFiltros()"
            placeholder="Buscar por cédula, nombres o título..."
            class="input-search"
          />
        </div>
        <div class="filter-actions">
          <button class="btn-refresh" (click)="cargarDatos()">
            <i class="bi bi-arrow-clockwise"></i> Actualizar
          </button>
        </div>
      </div>

      <!-- Spinner / Carga -->
      @if (cargando()) {
        <div class="loading-state">
          <div class="spinner"></div>
          <p>Cargando registros desde la base de datos...</p>
        </div>
      }

      <!-- Contenido Tab 1: Alumnos Aptos -->
      @if (!cargando() && tabActiva() === 'aptos') {
        <div class="table-card">
          <div class="table-responsive">
            <table
              mat-table
              [dataSource]="dataSourceAptos"
              matSort
              #sortAptos="matSort"
              class="data-table"
            >
              <ng-container matColumnDef="id">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Identificación</th>
                <td mat-cell *matCellDef="let alumno" class="font-mono">{{ alumno.idAlumno }}</td>
              </ng-container>

              <ng-container matColumnDef="nombres">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Nombres Completos</th>
                <td mat-cell *matCellDef="let alumno" class="font-bold">
                  {{ alumno.nombresCompletos }}
                </td>
              </ng-container>

              <ng-container matColumnDef="contacto">
                <th mat-header-cell *matHeaderCellDef>Contacto</th>
                <td mat-cell *matCellDef="let alumno">
                  <div class="contact-info">
                    <span
                      ><i class="bi bi-envelope"></i> {{ alumno.emailInstitucional || 'N/A' }}</span
                    >
                    <span><i class="bi bi-phone"></i> {{ alumno.celular || 'N/A' }}</span>
                  </div>
                </td>
              </ng-container>

              <ng-container matColumnDef="modalidad">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Modalidad</th>
                <td mat-cell *matCellDef="let alumno">
                  <span class="badge-modalidad">{{ alumno.modalidad }}</span>
                </td>
              </ng-container>

              <ng-container matColumnDef="periodo">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Período</th>
                <td mat-cell *matCellDef="let alumno">{{ alumno.idPeriodo }}</td>
              </ng-container>

              <ng-container matColumnDef="estado">
                <th mat-header-cell *matHeaderCellDef>Estado Titulación</th>
                <td mat-cell *matCellDef="let alumno">
                  <span class="badge-status disponible">
                    <i class="bi bi-check-circle-fill"></i> {{ alumno.estadoTitulacion }}
                  </span>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumnsAptos"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumnsAptos"></tr>

              <tr class="mat-mdc-row" *matNoDataRow>
                <td [attr.colspan]="displayedColumnsAptos.length" class="empty-state">
                  <i class="bi bi-inbox"></i> No se encontraron alumnos aptos disponibles.
                </td>
              </tr>
            </table>
          </div>
          <mat-paginator
            #paginatorAptos
            [pageSizeOptions]="[10, 20, 50]"
            showFirstLastButtons
            aria-label="Paginación de alumnos aptos"
          ></mat-paginator>
        </div>
      }

      <!-- Contenido Tab 2: Graduados Históricos -->
      @if (!cargando() && tabActiva() === 'graduados') {
        <div class="table-card">
          <div class="table-responsive">
            <table
              mat-table
              [dataSource]="dataSourceGraduados"
              matSort
              #sortGraduados="matSort"
              class="data-table"
            >
              <ng-container matColumnDef="cedula">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Cédula</th>
                <td mat-cell *matCellDef="let g" class="font-mono">{{ g.idAlumno }}</td>
              </ng-container>

              <ng-container matColumnDef="acta">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>N° Acta</th>
                <td mat-cell *matCellDef="let g">
                  <span class="badge-acta">{{ g.numeroActa }}</span>
                </td>
              </ng-container>

              <ng-container matColumnDef="fechaActa">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Fecha Acta</th>
                <td mat-cell *matCellDef="let g">{{ g.fechaActa || 'N/A' }}</td>
              </ng-container>

              <ng-container matColumnDef="promedio">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Promedio</th>
                <td mat-cell *matCellDef="let g" class="font-bold text-accent">
                  {{ g.promedioEstudios || 'N/A' }}
                </td>
              </ng-container>

              <ng-container matColumnDef="notaFinal">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Nota Final</th>
                <td mat-cell *matCellDef="let g" class="font-bold text-success">
                  {{ g.notaFinal || 'N/A' }}
                </td>
              </ng-container>

              <ng-container matColumnDef="titulo">
                <th mat-header-cell *matHeaderCellDef>Título Tesis / Proyecto</th>
                <td mat-cell *matCellDef="let g" class="text-sm">
                  {{ g.tituloTesis || 'Título Registrado' }}
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumnsGraduados"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumnsGraduados"></tr>

              <tr class="mat-mdc-row" *matNoDataRow>
                <td [attr.colspan]="displayedColumnsGraduados.length" class="empty-state">
                  <i class="bi bi-journal-x"></i> No se encontraron registros de graduados
                  históricos.
                </td>
              </tr>
            </table>
          </div>
          <mat-paginator
            #paginatorGraduados
            [pageSizeOptions]="[10, 20, 50]"
            showFirstLastButtons
            aria-label="Paginación de graduados"
          ></mat-paginator>
        </div>
      }
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        font-family:
          'Plus Jakarta Sans',
          -apple-system,
          BlinkMacSystemFont,
          'Segoe UI',
          Roboto,
          sans-serif;
      }
      .alumnos-container {
        padding: 32px;
        max-width: 1400px;
        margin: 0 auto;
        background-color: var(--bg-canvas, #f0f2f5);
        min-height: 100vh;
      }
      .page-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 16px;
        flex-wrap: wrap;
        margin-bottom: 24px;
      }
      .header-title h1 {
        font-size: 1.625rem;
        font-weight: 700;
        color: var(--istpet-navy, #1b2a4a);
        letter-spacing: -0.01em;
        margin: 0;
      }
      .subtitle {
        color: var(--text-muted, #605e5c);
        margin-top: 4px;
        font-size: 0.875rem;
      }
      .btn-back {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        padding: 10px 18px;
        background-color: var(--bg-surface, #ffffff);
        color: var(--text-secondary, #323130);
        border: 1px solid var(--border-subtle, rgba(0, 0, 0, 0.08));
        text-decoration: none;
        border-radius: var(--radius-md, 6px);
        font-weight: 600;
        font-size: 0.8125rem;
        transition:
          background 150ms ease,
          border-color 150ms ease;
      }
      .btn-back:hover {
        background-color: var(--istpet-navy-subtle, rgba(27, 42, 74, 0.06));
        border-color: var(--istpet-navy, #1b2a4a);
      }
      .restriction-notice-banner {
        display: flex;
        align-items: flex-start;
        gap: 14px;
        padding: 14px 18px;
        background: rgba(183, 129, 3, 0.08);
        border: 1px solid rgba(183, 129, 3, 0.25);
        border-left: 4px solid #b78103;
        border-radius: var(--radius-md, 6px);
        margin-bottom: 20px;
      }
      .restriction-notice-banner .notice-icon {
        color: #b78103;
        flex-shrink: 0;
        display: flex;
        align-items: center;
        margin-top: 2px;
      }
      .restriction-notice-banner .notice-content strong {
        display: block;
        font-size: 0.875rem;
        color: var(--text-primary, #1a1a1a);
        margin-bottom: 2px;
      }
      .restriction-notice-banner .notice-content p {
        margin: 0;
        font-size: 0.8125rem;
        color: var(--text-secondary, #484644);
        line-height: 1.4;
      }
      [data-theme='dark'] .restriction-notice-banner {
        background: rgba(197, 155, 39, 0.1);
        border-color: rgba(197, 155, 39, 0.3);
      }
      [data-theme='dark'] .restriction-notice-banner .notice-content strong {
        color: var(--istpet-gold, #c59b27);
      }
      [data-theme='dark'] .restriction-notice-banner .notice-content p {
        color: #cbd5e1;
      }
      .tabs-nav {
        display: flex;
        gap: 4px;
        margin-bottom: 20px;
        border-bottom: 1px solid var(--border-subtle, rgba(0, 0, 0, 0.08));
        overflow-x: auto;
        -webkit-overflow-scrolling: touch;
        scrollbar-width: none;
      }
      .tabs-nav::-webkit-scrollbar {
        display: none;
      }
      .tab-btn {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        padding: 10px 18px;
        border: none;
        background: none;
        font-family: inherit;
        font-size: 0.875rem;
        font-weight: 600;
        color: var(--text-muted, #605e5c);
        cursor: pointer;
        flex-shrink: 0;
        white-space: nowrap;
        border-bottom: 2px solid transparent;
        margin-bottom: -1px;
        transition:
          color 150ms ease,
          border-color 150ms ease;
      }
      .tab-btn:hover {
        color: var(--text-primary, #1a1a1a);
      }
      .tab-btn.active {
        color: var(--istpet-navy, #1b2a4a);
        border-bottom-color: var(--istpet-gold, #c59b27);
      }
      [data-theme='dark'] .tab-btn.active {
        color: var(--istpet-gold, #c59b27);
      }
      .filter-card {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 16px;
        flex-wrap: wrap;
        background: var(--bg-surface, #ffffff);
        border: 1px solid var(--border-subtle, rgba(0, 0, 0, 0.08));
        padding: 16px 20px;
        border-radius: var(--radius-lg, 10px);
        box-shadow: var(--shadow-card, 0 2px 8px rgba(0, 0, 0, 0.06));
        margin-bottom: 20px;
      }
      .search-box {
        position: relative;
        flex: 1;
        max-width: 440px;
      }
      .search-icon {
        position: absolute;
        left: 14px;
        top: 50%;
        transform: translateY(-50%);
        color: var(--text-disabled, #a19f9d);
      }
      .input-search {
        width: 100%;
        height: 38px;
        padding: 0 14px 0 38px;
        border: 1px solid var(--border-color, rgba(0, 0, 0, 0.1));
        border-radius: var(--radius-md, 6px);
        font-size: 0.875rem;
        font-family: inherit;
        color: var(--text-primary, #1a1a1a);
        background: var(--bg-surface-alt, #f8f9fa);
        outline: none;
        transition:
          border-color 150ms ease,
          background 150ms ease;
      }
      .input-search:focus {
        border-color: var(--istpet-navy, #1b2a4a);
        background: var(--bg-surface, #ffffff);
      }
      .btn-refresh {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        height: 38px;
        padding: 0 18px;
        background: var(--istpet-navy, #1b2a4a);
        color: #ffffff;
        border: none;
        border-radius: var(--radius-md, 6px);
        font-family: inherit;
        font-size: 0.8125rem;
        font-weight: 600;
        cursor: pointer;
        transition: background 150ms ease;
      }
      .btn-refresh:hover {
        background: var(--istpet-navy-hover, #253d6b);
      }
      [data-theme='dark'] .btn-refresh {
        background: var(--istpet-gold, #c59b27);
        color: var(--istpet-navy-dark, #121d34);
      }
      [data-theme='dark'] .btn-refresh:hover {
        background: var(--istpet-gold-hover, #d4af37);
      }
      .table-card {
        background: var(--bg-surface, #ffffff);
        border: 1px solid var(--border-subtle, rgba(0, 0, 0, 0.08));
        border-radius: var(--radius-lg, 10px);
        box-shadow: var(--shadow-card, 0 2px 8px rgba(0, 0, 0, 0.06));
        overflow: hidden;
        animation: istpet-fade-in 220ms ease-out;
      }
      @keyframes istpet-fade-in {
        from {
          opacity: 0;
          transform: translateY(6px);
        }
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }
      @media (prefers-reduced-motion: reduce) {
        .table-card {
          animation: none !important;
        }
      }
      .table-responsive {
        overflow-x: auto;
      }
      .data-table {
        width: 100%;
        min-width: 720px;
        border-collapse: collapse;
        text-align: left;
        font-family: inherit;
      }
      .data-table .mat-mdc-header-cell {
        background: var(--bg-surface-alt, #f8f9fa);
        padding: 12px 20px;
        font-size: 0.75rem;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: var(--text-muted, #605e5c);
        font-weight: 700;
        border-bottom: 1px solid var(--border-subtle, rgba(0, 0, 0, 0.08));
        white-space: nowrap;
      }
      .data-table .mat-mdc-cell {
        padding: 14px 20px;
        border-bottom: 1px solid var(--border-subtle, rgba(0, 0, 0, 0.08));
        font-size: 0.875rem;
        color: var(--text-primary, #1a1a1a);
      }
      .data-table .mat-mdc-row:last-child .mat-mdc-cell {
        border-bottom: none;
      }
      .data-table .mat-mdc-row:hover .mat-mdc-cell {
        background-color: var(--bg-surface-alt, #f8f9fa);
      }
      .data-table .mat-sort-header-arrow {
        color: var(--istpet-gold, #c59b27);
      }
      .font-mono {
        font-family: 'SFMono-Regular', Consolas, monospace;
        font-weight: 600;
        color: var(--istpet-navy, #1b2a4a);
      }
      [data-theme='dark'] .font-mono {
        color: var(--istpet-gold, #c59b27);
      }
      .font-bold {
        font-weight: 600;
      }
      .text-sm {
        font-size: 0.8125rem;
        color: var(--text-secondary, #323130);
      }
      .text-accent {
        color: var(--istpet-navy, #1b2a4a);
      }
      [data-theme='dark'] .text-accent {
        color: var(--istpet-gold, #c59b27);
      }
      .text-success {
        color: var(--status-success, #0e703c);
      }
      .contact-info span {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 0.8125rem;
        color: var(--text-muted, #605e5c);
      }
      .badge-modalidad,
      .badge-status,
      .badge-acta {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        padding: 3px 10px;
        border-radius: var(--radius-pill, 999px);
        font-size: 0.75rem;
        font-weight: 600;
        white-space: nowrap;
      }
      .badge-modalidad {
        background: var(--status-info-bg, rgba(0, 120, 212, 0.08));
        color: var(--status-info, #0078d4);
      }
      .badge-status.disponible {
        background: var(--status-success-bg, rgba(14, 112, 60, 0.08));
        color: var(--status-success, #0e703c);
      }
      .badge-acta {
        background: var(--istpet-gold-subtle, rgba(197, 155, 39, 0.12));
        color: var(--istpet-gold-dark, #b08a20);
      }
      .empty-state {
        text-align: center;
        padding: 48px 24px;
        color: var(--text-disabled, #a19f9d);
        font-size: 0.9375rem;
      }
      .empty-state i {
        display: block;
        font-size: 1.75rem;
        margin-bottom: 8px;
        opacity: 0.6;
      }
      .loading-state {
        text-align: center;
        padding: 48px 24px;
        color: var(--text-muted, #605e5c);
      }
      .spinner {
        width: 36px;
        height: 36px;
        border: 3px solid var(--border-subtle, rgba(0, 0, 0, 0.08));
        border-top-color: var(--istpet-navy, #1b2a4a);
        border-radius: 50%;
        animation: spin 0.8s linear infinite;
        margin: 0 auto 12px;
      }
      [data-theme='dark'] .spinner {
        border-top-color: var(--istpet-gold, #c59b27);
      }
      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }

      @media (max-width: 640px) {
        .alumnos-container {
          padding: 16px;
        }
        .header-title h1 {
          font-size: 1.25rem;
        }
        .btn-back {
          width: 100%;
          justify-content: center;
        }
        .filter-card {
          padding: 12px 14px;
        }
        .search-box {
          max-width: none;
          width: 100%;
        }
        .filter-actions {
          width: 100%;
        }
        .btn-refresh {
          width: 100%;
          justify-content: center;
        }
      }
    `,
  ],
})
export class AlumnosTitulacionComponent implements OnInit {
  private service = inject(AlumnosTitulacionService);

  tabActiva = signal<'aptos' | 'graduados'>('aptos');
  cargando = signal<boolean>(false);
  alumnosAptos = signal<AlumnoApto[]>([]);
  alumnosGraduados = signal<GraduadoHistorico[]>([]);
  busquedaTexto = '';

  // --------------------------------------------------------
  // Mat-table: fuentes de datos, orden y paginación (cliente)
  // --------------------------------------------------------
  readonly displayedColumnsAptos = ['id', 'nombres', 'contacto', 'modalidad', 'periodo', 'estado'];
  dataSourceAptos = new MatTableDataSource<AlumnoApto>([]);
  private readonly sortAptos = viewChild<MatSort>('sortAptos');
  private readonly paginatorAptos = viewChild<MatPaginator>('paginatorAptos');

  readonly displayedColumnsGraduados = [
    'cedula',
    'acta',
    'fechaActa',
    'promedio',
    'notaFinal',
    'titulo',
  ];
  dataSourceGraduados = new MatTableDataSource<GraduadoHistorico>([]);
  private readonly sortGraduados = viewChild<MatSort>('sortGraduados');
  private readonly paginatorGraduados = viewChild<MatPaginator>('paginatorGraduados');

  private readonly _syncAptos = effect(() => {
    this.dataSourceAptos.data = this.alumnosAptos();
    const sort = this.sortAptos();
    const paginator = this.paginatorAptos();
    if (sort) this.dataSourceAptos.sort = sort;
    if (paginator) this.dataSourceAptos.paginator = paginator;
  });

  private readonly _syncGraduados = effect(() => {
    this.dataSourceGraduados.data = this.alumnosGraduados();
    const sort = this.sortGraduados();
    const paginator = this.paginatorGraduados();
    if (sort) this.dataSourceGraduados.sort = sort;
    if (paginator) this.dataSourceGraduados.paginator = paginator;
  });

  ngOnInit(): void {
    this.cargarDatos();
  }

  setTab(tab: 'aptos' | 'graduados'): void {
    this.tabActiva.set(tab);
    this.cargarDatos();
  }

  cargarDatos(): void {
    this.cargando.set(true);
    const busqueda = this.busquedaTexto.trim();

    if (this.tabActiva() === 'aptos') {
      this.service.getAlumnosAptos({ busqueda }).subscribe({
        next: (data) => {
          this.alumnosAptos.set(data);
          this.cargando.set(false);
        },
        error: () => this.cargando.set(false),
      });
    } else {
      this.service.getAlumnosGraduados({ busqueda }).subscribe({
        next: (data) => {
          this.alumnosGraduados.set(data);
          this.cargando.set(false);
        },
        error: () => this.cargando.set(false),
      });
    }
  }

  aplicarFiltros(): void {
    this.cargarDatos();
  }
}
