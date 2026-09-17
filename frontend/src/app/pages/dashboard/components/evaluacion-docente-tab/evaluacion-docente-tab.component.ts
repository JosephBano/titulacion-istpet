import { Component, computed, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { RequisitoEvaluacionDocente } from '../../../../core/models/titulacion.models';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { environment } from '../../../../../environments/environment';

export interface GuardarEvaluacionEvento {
  item: RequisitoEvaluacionDocente;
  aprobado: boolean;
  observaciones: string;
  archivo?: File;
}

export interface DocumentoVisorInfo {
  nombre: string;
  url: string;
  safeUrl: SafeResourceUrl;
  esPdf: boolean;
  esImagen: boolean;
  esLocal: boolean;
}

@Component({
  selector: 'app-evaluacion-docente-tab',
  standalone: true,
  imports: [CommonModule, FormsModule, DrawerComponent],
  templateUrl: './evaluacion-docente-tab.component.html',
  styleUrls: ['./evaluacion-docente-tab.component.css'],
})
export class EvaluacionDocenteTabComponent {
  private readonly sanitizer = inject(DomSanitizer);

  items = input<RequisitoEvaluacionDocente[]>([]);
  cargando = input<boolean>(false);
  guardandoId = input<number | null>(null);

  guardarEvaluacion = output<GuardarEvaluacionEvento>();

  // Filtros de UI
  busqueda = signal<string>('');
  filtroCumplimiento = signal<'TODOS' | 'PENDIENTES' | 'APROBADOS'>('TODOS');
  filtroCarrera = signal<string>('TODAS');

  // Paginación
  paginaActual = signal<number>(1);
  tamanoPagina = signal<number>(10);

  // Estado del Drawer de Revisión Enfocada
  drawerAbierto = signal<boolean>(false);
  casoSeleccionado = signal<RequisitoEvaluacionDocente | null>(null);

  // Estado local de edición por ID de requisito
  archivosPorFila = signal<Record<number, File>>({});
  observacionesPorFila = signal<Record<number, string>>({});
  aprobadoPorFila = signal<Record<number, boolean>>({});

  // Lista única de carreras para filtro
  carrerasDisponibles = computed(() => {
    const list = this.items()
      .map((i) => i.carrera?.trim())
      .filter((c): c is string => !!c);
    return Array.from(new Set(list)).sort();
  });

  // Métricas rápidas
  totalPendientes = computed(() => {
    return this.items().filter((item) => !this.getAprobado(item)).length;
  });

  totalAprobados = computed(() => {
    return this.items().filter((item) => this.getAprobado(item)).length;
  });

  // Items filtrados
  itemsFiltrados = computed(() => {
    const rawItems = this.items();
    const query = this.busqueda().toLowerCase().trim();
    const filtro = this.filtroCumplimiento();
    const carreraSel = this.filtroCarrera();

    return rawItems
      .filter((item) => {
        const esAprob = this.getAprobado(item);
        if (filtro === 'PENDIENTES' && esAprob) return false;
        if (filtro === 'APROBADOS' && !esAprob) return false;

        if (carreraSel !== 'TODAS' && item.carrera !== carreraSel) {
          return false;
        }

        if (query) {
          const matchAlumno = (item.nombreAlumno || '').toLowerCase().includes(query);
          const matchCedula = (item.cedulaAlumno || '').toLowerCase().includes(query);
          const matchReq = (item.nombreRequisito || '').toLowerCase().includes(query);
          const matchCarrera = (item.carrera || '').toLowerCase().includes(query);
          const matchExp = item.idPostulacionAlumnos.toString().includes(query);
          if (!matchAlumno && !matchCedula && !matchReq && !matchCarrera && !matchExp) {
            return false;
          }
        }
        return true;
      })
      .sort((a, b) => {
        const aAprob = this.getAprobado(a) ? 1 : 0;
        const bAprob = this.getAprobado(b) ? 1 : 0;
        if (aAprob !== bAprob) return aAprob - bAprob;
        return b.idPostulacionAlumnos - a.idPostulacionAlumnos;
      });
  });

  totalFiltrados = computed(() => this.itemsFiltrados().length);

  totalPaginas = computed(() => {
    const t = this.totalFiltrados();
    const size = this.tamanoPagina() || 10;
    return Math.max(1, Math.ceil(t / size));
  });

  itemsPaginados = computed(() => {
    const start = (this.paginaActual() - 1) * this.tamanoPagina();
    return this.itemsFiltrados().slice(start, start + this.tamanoPagina());
  });

  rangoInicio = computed(() => {
    if (this.totalFiltrados() === 0) return 0;
    return (this.paginaActual() - 1) * this.tamanoPagina() + 1;
  });

  rangoFin = computed(() => {
    return Math.min(this.paginaActual() * this.tamanoPagina(), this.totalFiltrados());
  });

  // Navegación dentro del Drawer
  indiceActual = computed(() => {
    const sel = this.casoSeleccionado();
    if (!sel) return -1;
    return this.itemsFiltrados().findIndex(
      (i) => i.idPostulacionAlumnoRequisitoModalidad === sel.idPostulacionAlumnoRequisitoModalidad
    );
  });

  hayAnterior = computed(() => this.indiceActual() > 0);
  haySiguiente = computed(() => {
    const idx = this.indiceActual();
    return idx >= 0 && idx < this.itemsFiltrados().length - 1;
  });

  // Visor del caso seleccionado
  visorCasoActual = computed<DocumentoVisorInfo | null>(() => {
    const caso = this.casoSeleccionado();
    if (!caso) return null;

    // Verificar si hay archivo recién subido
    const localFile = this.archivosPorFila()[caso.idPostulacionAlumnoRequisitoModalidad];
    if (localFile) {
      const objectUrl = URL.createObjectURL(localFile);
      const lower = localFile.name.toLowerCase();
      const esPdf = localFile.type === 'application/pdf' || lower.endsWith('.pdf');
      const esImagen = localFile.type.startsWith('image/') || lower.endsWith('.png') || lower.endsWith('.jpg') || lower.endsWith('.jpeg');
      return {
        nombre: localFile.name,
        url: objectUrl,
        safeUrl: this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl),
        esPdf,
        esImagen,
        esLocal: true,
      };
    }

    if (!caso.rutaArchivoAdjunto) return null;
    const url = this.getArchivoUrl(caso.rutaArchivoAdjunto);
    const nombre = caso.nombreArchivoAdjunto || 'Documento adjunto';
    const lower = (nombre || caso.rutaArchivoAdjunto).toLowerCase();
    const esPdf = lower.endsWith('.pdf');
    const esImagen = lower.endsWith('.png') || lower.endsWith('.jpg') || lower.endsWith('.jpeg') || lower.endsWith('.webp');

    return {
      nombre,
      url,
      safeUrl: this.sanitizer.bypassSecurityTrustResourceUrl(url),
      esPdf,
      esImagen,
      esLocal: false,
    };
  });

  // Acciones de UI
  setFiltro(tipo: 'PENDIENTES' | 'APROBADOS' | 'TODOS'): void {
    this.filtroCumplimiento.set(tipo);
    this.paginaActual.set(1);
  }

  setFiltroCarrera(carrera: string): void {
    this.filtroCarrera.set(carrera);
    this.paginaActual.set(1);
  }

  onSearchChange(val: string): void {
    this.busqueda.set(val);
    this.paginaActual.set(1);
  }

  irAPagina(p: number): void {
    if (p >= 1 && p <= this.totalPaginas()) {
      this.paginaActual.set(p);
    }
  }

  // Apertura y Navegación del Drawer
  abrirRevision(item: RequisitoEvaluacionDocente): void {
    this.casoSeleccionado.set(item);
    // Si aún no tenía estado modificado, inicializarlo con el actual
    if (this.aprobadoPorFila()[item.idPostulacionAlumnoRequisitoModalidad] === undefined) {
      this.setAprobado(item, item.aprobado);
    }
    if (this.observacionesPorFila()[item.idPostulacionAlumnoRequisitoModalidad] === undefined) {
      this.setObservaciones(item, item.observaciones || '');
    }
    this.drawerAbierto.set(true);
  }

  cerrarRevision(): void {
    this.drawerAbierto.set(false);
    this.casoSeleccionado.set(null);
  }

  irAnterior(): void {
    const idx = this.indiceActual();
    if (idx > 0) {
      const prevItem = this.itemsFiltrados()[idx - 1];
      this.abrirRevision(prevItem);
    }
  }

  irSiguiente(): void {
    const idx = this.indiceActual();
    if (idx >= 0 && idx < this.itemsFiltrados().length - 1) {
      const nextItem = this.itemsFiltrados()[idx + 1];
      this.abrirRevision(nextItem);
    }
  }

  // Getters y Setters de formulario
  getAprobado(item: RequisitoEvaluacionDocente): boolean {
    const custom = this.aprobadoPorFila()[item.idPostulacionAlumnoRequisitoModalidad];
    if (custom !== undefined) return custom;
    return item.aprobado;
  }

  setAprobado(item: RequisitoEvaluacionDocente, valor: boolean): void {
    this.aprobadoPorFila.update((map) => ({
      ...map,
      [item.idPostulacionAlumnoRequisitoModalidad]: valor,
    }));
  }

  getObservaciones(item: RequisitoEvaluacionDocente): string {
    const custom = this.observacionesPorFila()[item.idPostulacionAlumnoRequisitoModalidad];
    if (custom !== undefined) return custom;
    return item.observaciones || '';
  }

  setObservaciones(item: RequisitoEvaluacionDocente, valor: string): void {
    this.observacionesPorFila.update((map) => ({
      ...map,
      [item.idPostulacionAlumnoRequisitoModalidad]: valor,
    }));
  }

  onFileChange(idPostulacionAlumnoRequisitoModalidad: number, event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    if (inputEl.files && inputEl.files.length > 0) {
      const file = inputEl.files[0];
      this.archivosPorFila.update((map) => ({
        ...map,
        [idPostulacionAlumnoRequisitoModalidad]: file,
      }));
    }
  }

  removerArchivoLocal(idPostulacionAlumnoRequisitoModalidad: number): void {
    this.archivosPorFila.update((map) => {
      const next = { ...map };
      delete next[idPostulacionAlumnoRequisitoModalidad];
      return next;
    });
  }

  // Guardar evaluación desde el Drawer o directamente
  guardarCasoActual(): void {
    const caso = this.casoSeleccionado();
    if (!caso) return;

    this.enviarGuardado(caso);
  }

  aprobarDirecto(item: RequisitoEvaluacionDocente, event: MouseEvent): void {
    event.stopPropagation();
    this.setAprobado(item, true);
    this.enviarGuardado(item);
  }

  enviarGuardado(item: RequisitoEvaluacionDocente): void {
    const aprobado = this.getAprobado(item);
    const observaciones = this.getObservaciones(item);
    const archivo = this.archivosPorFila()[item.idPostulacionAlumnoRequisitoModalidad];

    this.guardarEvaluacion.emit({
      item,
      aprobado,
      observaciones,
      archivo,
    });
  }

  // URLs y Helpers
  getArchivoUrl(ruta?: string | null): string {
    if (!ruta) return '';
    if (ruta.startsWith('http://') || ruta.startsWith('https://') || ruta.startsWith('blob:')) {
      return ruta;
    }
    const cleanPath = ruta.startsWith('/') ? ruta : '/' + ruta;
    return `${environment.apiBaseUrl}${cleanPath}`;
  }

  getIniciales(nombreCompleto: string): string {
    if (!nombreCompleto) return 'AL';
    const partes = nombreCompleto.trim().split(/\s+/);
    if (partes.length >= 2) {
      return `${partes[0].charAt(0)}${partes[1].charAt(0)}`.toUpperCase();
    }
    return partes[0].substring(0, 2).toUpperCase();
  }
}
