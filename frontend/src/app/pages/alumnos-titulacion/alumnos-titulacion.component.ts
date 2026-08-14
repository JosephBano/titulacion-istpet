import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AlumnosTitulacionService } from '../../core/services/alumnos-titulacion.service';
import { AlumnoApto, GraduadoHistorico } from '../../core/models/alumno-filtro.model';

@Component({
  selector: 'app-alumnos-titulacion',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
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
            <table class="data-table">
              <thead>
                <tr>
                  <th>Identificación</th>
                  <th>Nombres Completos</th>
                  <th>Contacto</th>
                  <th>Modalidad</th>
                  <th>Período</th>
                  <th>Estado Titulación</th>
                </tr>
              </thead>
              <tbody>
                @for (alumno of alumnosAptos(); track alumno.idAlumno) {
                  <tr>
                    <td class="font-mono">{{ alumno.idAlumno }}</td>
                    <td class="font-bold">{{ alumno.nombresCompletos }}</td>
                    <td>
                      <div class="contact-info">
                        <span
                          ><i class="bi bi-envelope"></i>
                          {{ alumno.emailInstitucional || 'N/A' }}</span
                        >
                        <span><i class="bi bi-phone"></i> {{ alumno.celular || 'N/A' }}</span>
                      </div>
                    </td>
                    <td>
                      <span class="badge-modalidad">{{ alumno.modalidad }}</span>
                    </td>
                    <td>{{ alumno.idPeriodo }}</td>
                    <td>
                      <span class="badge-status disponible">
                        <i class="bi bi-check-circle-fill"></i> {{ alumno.estadoTitulacion }}
                      </span>
                    </td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="6" class="empty-state">
                      <i class="bi bi-inbox"></i> No se encontraron alumnos aptos disponibles.
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }

      <!-- Contenido Tab 2: Graduados Históricos -->
      @if (!cargando() && tabActiva() === 'graduados') {
        <div class="table-card">
          <div class="table-responsive">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Cédula</th>
                  <th>N° Acta</th>
                  <th>Fecha Acta</th>
                  <th>Promedio</th>
                  <th>Nota Final</th>
                  <th>Título Tesis / Proyecto</th>
                </tr>
              </thead>
              <tbody>
                @for (g of alumnosGraduados(); track g.idAlumno) {
                  <tr>
                    <td class="font-mono">{{ g.idAlumno }}</td>
                    <td>
                      <span class="badge-acta">{{ g.numeroActa }}</span>
                    </td>
                    <td>{{ g.fechaActa || 'N/A' }}</td>
                    <td class="font-bold text-accent">{{ g.promedioEstudios || 'N/A' }}</td>
                    <td class="font-bold text-success">{{ g.notaFinal || 'N/A' }}</td>
                    <td class="text-sm">{{ g.tituloTesis || 'Título Registrado' }}</td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="6" class="empty-state">
                      <i class="bi bi-journal-x"></i> No se encontraron registros de graduados
                      históricos.
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .alumnos-container {
        padding: 2rem;
        max-width: 1400px;
        margin: 0 auto;
      }
      .page-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 1.5rem;
      }
      .header-title h1 {
        font-size: 1.8rem;
        font-weight: 700;
        color: var(--color-primary, #002855);
        margin: 0;
      }
      .subtitle {
        color: #64748b;
        margin-top: 0.25rem;
        font-size: 0.95rem;
      }
      .btn-back {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.6rem 1.2rem;
        background-color: #f1f5f9;
        color: #334155;
        text-decoration: none;
        border-radius: 8px;
        font-weight: 600;
        transition: all 0.2s ease;
      }
      .btn-back:hover {
        background-color: #e2e8f0;
      }
      .tabs-nav {
        display: flex;
        gap: 1rem;
        margin-bottom: 1.5rem;
        border-bottom: 2px solid #e2e8f0;
        padding-bottom: 0.5rem;
      }
      .tab-btn {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.75rem 1.5rem;
        border: none;
        background: none;
        font-size: 1rem;
        font-weight: 600;
        color: #64748b;
        cursor: pointer;
        border-bottom: 3px solid transparent;
        transition: all 0.2s ease;
      }
      .tab-btn.active {
        color: #002855;
        border-bottom-color: #002855;
      }
      .filter-card {
        display: flex;
        justify-content: space-between;
        align-items: center;
        background: #ffffff;
        padding: 1rem 1.5rem;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
        margin-bottom: 1.5rem;
      }
      .search-box {
        position: relative;
        flex: 1;
        max-width: 500px;
      }
      .search-icon {
        position: absolute;
        left: 1rem;
        top: 50%;
        transform: translateY(-50%);
        color: #94a3b8;
      }
      .input-search {
        width: 100%;
        padding: 0.65rem 1rem 0.65rem 2.5rem;
        border: 1px solid #cbd5e1;
        border-radius: 8px;
        font-size: 0.95rem;
        outline: none;
        transition: border-color 0.2s ease;
      }
      .input-search:focus {
        border-color: #002855;
      }
      .btn-refresh {
        padding: 0.65rem 1.2rem;
        background: #002855;
        color: white;
        border: none;
        border-radius: 8px;
        font-weight: 600;
        cursor: pointer;
        transition: background 0.2s;
      }
      .btn-refresh:hover {
        background: #001f44;
      }
      .table-card {
        background: #ffffff;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
        overflow: hidden;
      }
      .data-table {
        width: 100%;
        border-collapse: collapse;
        text-align: left;
      }
      .data-table th {
        background: #f8fafc;
        padding: 1rem 1.25rem;
        font-size: 0.85rem;
        text-transform: uppercase;
        color: #475569;
        font-weight: 700;
        border-bottom: 1px solid #e2e8f0;
      }
      .data-table td {
        padding: 1rem 1.25rem;
        border-bottom: 1px solid #f1f5f9;
        font-size: 0.95rem;
        color: #1e293b;
      }
      .font-mono {
        font-family: monospace;
        font-weight: 600;
      }
      .font-bold {
        font-weight: 600;
      }
      .contact-info span {
        display: block;
        font-size: 0.85rem;
        color: #64748b;
      }
      .badge-modalidad {
        background: #e0f2fe;
        color: #0369a1;
        padding: 0.25rem 0.75rem;
        border-radius: 20px;
        font-size: 0.8rem;
        font-weight: 600;
      }
      .badge-status.disponible {
        background: #dcfce7;
        color: #15803d;
        padding: 0.25rem 0.75rem;
        border-radius: 20px;
        font-size: 0.8rem;
        font-weight: 600;
      }
      .badge-acta {
        background: #fef3c7;
        color: #b45309;
        padding: 0.25rem 0.6rem;
        border-radius: 6px;
        font-weight: 700;
      }
      .empty-state {
        text-align: center;
        padding: 3rem;
        color: #94a3b8;
        font-size: 1.1rem;
      }
      .loading-state {
        text-align: center;
        padding: 3rem;
      }
      .spinner {
        width: 40px;
        height: 40px;
        border: 4px solid #e2e8f0;
        border-top-color: #002855;
        border-radius: 50%;
        animation: spin 1s linear infinite;
        margin: 0 auto 1rem;
      }
      @keyframes spin {
        to {
          transform: rotate(360deg);
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
