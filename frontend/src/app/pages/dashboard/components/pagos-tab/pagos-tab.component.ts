import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PagosTitulacionService } from '../../../../core/services/pagos-titulacion.service';
import { TitulacionService } from '../../../../core/services/titulacion.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';
import {
  ConfiguracionPagoModalidad,
  GuardarConfiguracionPagoModalidad,
  HistorialCambioEstadoPago,
  PagoTitulacion,
  RegistrarPagoTitulacion,
  ResumenFinancieroEstudiante,
  RevisionPago,
} from '../../../../core/models/pagos-titulacion.models';
import { PostulacionResumen } from '../../../../core/models/titulacion.models';

type SubTab = 'bandeja' | 'registro' | 'configuracion';

@Component({
  selector: 'app-pagos-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pagos-tab.component.html',
  styleUrls: ['./pagos-tab.component.css'],
})
export class PagosTabComponent implements OnInit {
  private readonly pagosService = inject(PagosTitulacionService);
  private readonly titulacionService = inject(TitulacionService);
  private readonly notificationService = inject(NotificationService);
  private readonly authService = inject(AuthService);

  readonly skeletonRows = Array.from({ length: 5 }, (_, i) => i);

  // Sub-pestaña activa
  subTabActiva = signal<SubTab>('bandeja');

  // Roles y permisos
  isContabilidad = computed(() => this.authService.hasRole('CONTABILIDAD'));
  isSecretaria = computed(() => this.authService.hasRole('SECRETARIA'));
  isAdmin = computed(
    () =>
      this.authService.hasRole('ADMIN') ||
      this.authService.hasRole('TITULACION_ADMIN') ||
      this.authService.hasRole('ADMINISTRADOR') ||
      this.authService.hasRole('ADMIN_SIST') ||
      this.authService.hasRole('GESTOR'),
  );

  puedeAprobarRechazar = computed(() => this.isContabilidad());
  puedeRegistrar = computed(() => this.isSecretaria() || this.isAdmin());
  puedeConfigurar = computed(() => this.isAdmin());

  // === ESTADO: BANDEJA DE CONTABILIDAD ===
  pagosBandeja = signal<PagoTitulacion[]>([]);
  cargandoBandeja = signal<boolean>(false);
  filtroEstadoBandeja = signal<string>('TODOS');
  busquedaBandeja = signal<string>('');

  pagosFiltrados = computed(() => {
    let lista = this.pagosBandeja();
    const filtro = this.filtroEstadoBandeja();
    if (filtro !== 'TODOS') {
      lista = lista.filter((p) => p.estado.toUpperCase() === filtro);
    }
    const q = this.busquedaBandeja().trim().toLowerCase();
    if (q) {
      lista = lista.filter(
        (p) =>
          p.estudianteCedula.toLowerCase().includes(q) ||
          p.estudianteNombre.toLowerCase().includes(q) ||
          p.numeroFactura.toLowerCase().includes(q) ||
          p.modalidadNombre.toLowerCase().includes(q),
      );
    }
    return lista;
  });

  // Modal Rechazo
  pagoParaRechazar = signal<PagoTitulacion | null>(null);
  motivoRechazoTexto = signal<string>('');
  procesandoRevision = signal<boolean>(false);

  // Modal Historial
  pagoParaHistorial = signal<PagoTitulacion | null>(null);
  historialItems = signal<HistorialCambioEstadoPago[]>([]);
  cargandoHistorial = signal<boolean>(false);

  // === ESTADO: REGISTRO DE PAGOS (SECRETARÍA / ADMIN) ===
  postulaciones = signal<PostulacionResumen[]>([]);
  cargandoPostulaciones = signal<boolean>(false);
  busquedaPostulante = signal<string>('');
  postulacionSeleccionada = signal<PostulacionResumen | null>(null);
  resumenFinanciero = signal<ResumenFinancieroEstudiante | null>(null);
  cargandoResumenFinanciero = signal<boolean>(false);

  // Formulario de Registro de Pago
  nuevoPagoTipoRubro = signal<'BASE' | 'INVERSION'>('BASE');
  nuevoPagoFactura = signal<string>('');
  nuevoPagoValor = signal<number | null>(null);
  nuevoPagoObservacion = signal<string>('');
  guardandoPago = signal<boolean>(false);
  errorValidacionValor = signal<string | null>(null);

  // === ESTADO: CONFIGURACIÓN POR MODALIDAD ===
  configuraciones = signal<ConfiguracionPagoModalidad[]>([]);
  cargandoConfiguraciones = signal<boolean>(false);
  configuracionEnEdicion = signal<ConfiguracionPagoModalidad | null>(null);
  editValorBase = signal<number>(500);
  editMaximoInversion = signal<number>(200);
  guardandoConfiguracion = signal<boolean>(false);
  errorConfiguracion = signal<string | null>(null);

  ngOnInit(): void {
    if (this.isContabilidad() && !this.isAdmin() && !this.isSecretaria()) {
      this.subTabActiva.set('bandeja');
    } else if (this.isSecretaria() && !this.isAdmin() && !this.isContabilidad()) {
      this.subTabActiva.set('registro');
    } else {
      this.subTabActiva.set('bandeja');
    }

    this.cargarBandeja();
    if (this.puedeRegistrar()) {
      this.cargarPostulaciones();
    }
    if (this.puedeConfigurar()) {
      this.cargarConfiguraciones();
    }
  }

  setSubTab(tab: SubTab): void {
    this.subTabActiva.set(tab);
    if (tab === 'bandeja') {
      this.cargarBandeja();
    } else if (tab === 'registro') {
      if (this.postulaciones().length === 0) {
        this.cargarPostulaciones();
      }
    } else if (tab === 'configuracion') {
      this.cargarConfiguraciones();
    }
  }

  // ==========================================
  // BANDEJA DE CONTABILIDAD
  // ==========================================
  cargarBandeja(): void {
    this.cargandoBandeja.set(true);
    const estado = this.filtroEstadoBandeja() === 'TODOS' ? undefined : this.filtroEstadoBandeja();
    this.pagosService.getBandejaContabilidad(estado).subscribe({
      next: (items) => {
        this.pagosBandeja.set(items || []);
        this.cargandoBandeja.set(false);
      },
      error: () => {
        this.notificationService.error('Error al cargar la bandeja de pagos.');
        this.cargandoBandeja.set(false);
      },
    });
  }

  cambiarFiltroEstado(nuevoEstado: string): void {
    this.filtroEstadoBandeja.set(nuevoEstado);
    this.cargarBandeja();
  }

  aprobarPago(pago: PagoTitulacion): void {
    if (!this.puedeAprobarRechazar()) {
      this.notificationService.error(
        'Solo personal de Contabilidad puede aprobar o rechazar pagos.',
      );
      return;
    }

    if (
      !confirm(
        `¿Confirma la aprobación del pago de $${pago.valor} (Factura: ${pago.numeroFactura})?`,
      )
    ) {
      return;
    }

    this.procesandoRevision.set(true);
    const revision: RevisionPago = {
      aprobar: true,
      observacion: 'Aprobado formalmente por Contabilidad',
    };

    this.pagosService.procesarRevisionContabilidad(pago.idPagoTitulacion, revision).subscribe({
      next: () => {
        this.notificationService.success('Pago aprobado satisfactoriamente por Contabilidad.');
        this.procesandoRevision.set(false);
        this.cargarBandeja();
      },
      error: (err) => {
        const mensaje = err?.error?.mensaje || 'Error al procesar la aprobación del pago.';
        this.notificationService.error(mensaje);
        this.procesandoRevision.set(false);
      },
    });
  }

  abrirModalRechazo(pago: PagoTitulacion): void {
    if (!this.puedeAprobarRechazar()) {
      this.notificationService.error(
        'Solo personal de Contabilidad puede aprobar o rechazar pagos.',
      );
      return;
    }
    this.pagoParaRechazar.set(pago);
    this.motivoRechazoTexto.set('');
  }

  cerrarModalRechazo(): void {
    this.pagoParaRechazar.set(null);
    this.motivoRechazoTexto.set('');
  }

  confirmarRechazo(): void {
    const pago = this.pagoParaRechazar();
    const motivo = this.motivoRechazoTexto().trim();

    if (!pago) return;
    if (!motivo || motivo.length < 5) {
      this.notificationService.warning(
        'El motivo de rechazo es obligatorio (mínimo 5 caracteres).',
      );
      return;
    }

    this.procesandoRevision.set(true);
    const revision: RevisionPago = {
      aprobar: false,
      motivoRechazo: motivo,
    };

    this.pagosService.procesarRevisionContabilidad(pago.idPagoTitulacion, revision).subscribe({
      next: () => {
        this.notificationService.success(
          'Pago rechazado. Ha dejado de contar como pagado y el cupo fue liberado.',
        );
        this.procesandoRevision.set(false);
        this.cerrarModalRechazo();
        this.cargarBandeja();
      },
      error: (err) => {
        const mensaje = err?.error?.mensaje || 'Error al rechazar el pago.';
        this.notificationService.error(mensaje);
        this.procesandoRevision.set(false);
      },
    });
  }

  abrirHistorial(pago: PagoTitulacion): void {
    this.pagoParaHistorial.set(pago);
    this.historialItems.set([]);
    this.cargandoHistorial.set(true);

    this.pagosService.getHistorial(pago.idPagoTitulacion).subscribe({
      next: (items) => {
        this.historialItems.set(items || []);
        this.cargandoHistorial.set(false);
      },
      error: () => {
        this.notificationService.error('Error al consultar el historial de auditoría.');
        this.cargandoHistorial.set(false);
      },
    });
  }

  cerrarHistorial(): void {
    this.pagoParaHistorial.set(null);
    this.historialItems.set([]);
  }

  // ==========================================
  // REGISTRO DE PAGOS (SECRETARÍA / ADMIN)
  // ==========================================
  cargarPostulaciones(): void {
    this.cargandoPostulaciones.set(true);
    this.titulacionService
      .getPostulaciones(1, 100, undefined, undefined, undefined, this.busquedaPostulante().trim())
      .subscribe({
        next: (res) => {
          this.postulaciones.set(res.items || []);
          this.cargandoPostulaciones.set(false);
        },
        error: () => {
          this.cargandoPostulaciones.set(false);
        },
      });
  }

  buscarPostulantes(): void {
    this.cargarPostulaciones();
  }

  seleccionarPostulacion(postulacion: PostulacionResumen): void {
    this.postulacionSeleccionada.set(postulacion);
    this.cargarResumenFinanciero(postulacion.idPostulacionAlumnos);
  }

  cargarResumenFinanciero(idPostulacion: number): void {
    this.cargandoResumenFinanciero.set(true);
    this.pagosService.getResumenFinanciero(idPostulacion).subscribe({
      next: (res) => {
        this.resumenFinanciero.set(res);
        this.cargandoResumenFinanciero.set(false);
        // Si no tiene cupo de inversión disponible o la modalidad tiene $0 de inversión, fijar BASE
        if (res.inversionMaxima === 0) {
          this.nuevoPagoTipoRubro.set('BASE');
        }
        this.validarMontoEnTiempoReal();
      },
      error: () => {
        this.notificationService.error('Error al consultar el resumen financiero del estudiante.');
        this.cargandoResumenFinanciero.set(false);
      },
    });
  }

  onTipoRubroChange(rubro: 'BASE' | 'INVERSION'): void {
    this.nuevoPagoTipoRubro.set(rubro);
    this.validarMontoEnTiempoReal();
  }

  onValorInput(val: number | null): void {
    this.nuevoPagoValor.set(val);
    this.validarMontoEnTiempoReal();
  }

  validarMontoEnTiempoReal(): void {
    const val = this.nuevoPagoValor();
    const resumen = this.resumenFinanciero();
    const rubro = this.nuevoPagoTipoRubro();

    if (!resumen || val === null || val === undefined || isNaN(val)) {
      this.errorValidacionValor.set(null);
      return;
    }

    if (val <= 0) {
      this.errorValidacionValor.set('El valor debe ser mayor a $0.00.');
      return;
    }

    if (rubro === 'BASE') {
      if (val > resumen.saldoBaseDisponible) {
        this.errorValidacionValor.set(
          `El valor ingresado de $${val.toFixed(2)} supera el saldo disponible de $${resumen.saldoBaseDisponible.toFixed(2)} para valor base (Tope modalidad: $${resumen.valorBaseMaximo.toFixed(2)}, ya registrado: $${resumen.totalBasePagado.toFixed(2)}).`,
        );
        return;
      }
    } else {
      if (resumen.inversionMaxima === 0) {
        this.errorValidacionValor.set(
          `La modalidad ${resumen.modalidadNombre} no contempla rubro de inversión (Tope: $0.00).`,
        );
        return;
      }
      if (val > resumen.saldoInversionDisponible) {
        this.errorValidacionValor.set(
          `El valor ingresado de $${val.toFixed(2)} supera el saldo disponible de $${resumen.saldoInversionDisponible.toFixed(2)} para inversión (Tope modalidad: $${resumen.inversionMaxima.toFixed(2)}, ya registrado: $${resumen.totalInversionPagado.toFixed(2)}).`,
        );
        return;
      }
    }

    if (val > resumen.saldoTotalPendiente) {
      this.errorValidacionValor.set(
        `El valor de $${val.toFixed(2)} supera el saldo total pendiente de $${resumen.saldoTotalPendiente.toFixed(2)} (Total titulación: $${resumen.totalTitulacion.toFixed(2)}).`,
      );
      return;
    }

    this.errorValidacionValor.set(null);
  }

  registrarPago(): void {
    const post = this.postulacionSeleccionada();
    const resumen = this.resumenFinanciero();
    const rubro = this.nuevoPagoTipoRubro();
    const factura = this.nuevoPagoFactura().trim();
    const valor = this.nuevoPagoValor();

    if (!post || !resumen) {
      this.notificationService.warning('Seleccione un estudiante primero.');
      return;
    }

    if (!factura) {
      this.notificationService.warning('Ingrese el número de factura o comprobante.');
      return;
    }

    if (!valor || valor <= 0) {
      this.notificationService.warning('Ingrese un valor mayor a cero.');
      return;
    }

    this.validarMontoEnTiempoReal();
    if (this.errorValidacionValor()) {
      this.notificationService.error(this.errorValidacionValor()!);
      return;
    }

    const dto: RegistrarPagoTitulacion = {
      idPostulacionAlumno: post.idPostulacionAlumnos,
      tipoRubro: rubro,
      numeroFactura: factura,
      valor: valor,
      observacion: this.nuevoPagoObservacion().trim() || null,
    };

    this.guardandoPago.set(true);
    this.pagosService.registrarPago(dto).subscribe({
      next: () => {
        this.notificationService.success(
          `Pago de $${valor.toFixed(2)} registrado correctamente. Se refleja como pagado de inmediato.`,
        );
        this.guardandoPago.set(false);
        this.nuevoPagoFactura.set('');
        this.nuevoPagoValor.set(null);
        this.nuevoPagoObservacion.set('');
        this.errorValidacionValor.set(null);
        // Recargar datos actualizados
        this.cargarResumenFinanciero(post.idPostulacionAlumnos);
        this.cargarBandeja();
      },
      error: (err) => {
        const mensaje = err?.error?.mensaje || 'Error al registrar el pago.';
        this.notificationService.error(mensaje);
        this.guardandoPago.set(false);
      },
    });
  }

  // ==========================================
  // CONFIGURACIÓN DE TOPES POR MODALIDAD
  // ==========================================
  cargarConfiguraciones(): void {
    this.cargandoConfiguraciones.set(true);
    this.pagosService.getConfiguraciones().subscribe({
      next: (items) => {
        this.configuraciones.set(items || []);
        this.cargandoConfiguraciones.set(false);
      },
      error: () => {
        this.notificationService.error('Error al cargar la configuración de modalidades.');
        this.cargandoConfiguraciones.set(false);
      },
    });
  }

  abrirEdicionConfiguracion(cfg: ConfiguracionPagoModalidad): void {
    this.configuracionEnEdicion.set(cfg);
    this.editValorBase.set(cfg.valorBase);
    this.editMaximoInversion.set(cfg.maximoInversion);
    this.errorConfiguracion.set(null);
  }

  cerrarEdicionConfiguracion(): void {
    this.configuracionEnEdicion.set(null);
    this.errorConfiguracion.set(null);
  }

  onEditValoresChange(): void {
    const base = Number(this.editValorBase()) || 0;
    const inv = Number(this.editMaximoInversion()) || 0;
    const total = base + inv;

    if (inv < 0 || inv > 200) {
      this.errorConfiguracion.set(
        `El rubro de inversión no puede superar el tope normativo de $200.00 (Actual: $${inv.toFixed(2)}).`,
      );
      return;
    }

    if (total !== 700) {
      this.errorConfiguracion.set(
        `La suma de Valor Base ($${base.toFixed(2)}) e Inversión ($${inv.toFixed(2)}) debe ser exactamente igual a $700.00 (Total actual: $${total.toFixed(2)}).`,
      );
      return;
    }

    this.errorConfiguracion.set(null);
  }

  guardarConfiguracion(): void {
    const cfg = this.configuracionEnEdicion();
    if (!cfg) return;

    this.onEditValoresChange();
    if (this.errorConfiguracion()) {
      this.notificationService.error(this.errorConfiguracion()!);
      return;
    }

    const base = Number(this.editValorBase());
    const inv = Number(this.editMaximoInversion());

    const dto: GuardarConfiguracionPagoModalidad = {
      idModalidadTitulacion: cfg.idModalidadTitulacion,
      idPeriodo: cfg.idPeriodo,
      valorBase: base,
      maximoInversion: inv,
      totalTitulacion: 700,
      idEspecie: cfg.idEspecie,
      activo: cfg.activo,
    };

    this.guardandoConfiguracion.set(true);
    this.pagosService.guardarConfiguracion(dto).subscribe({
      next: () => {
        this.notificationService.success(
          `Configuración de topes para ${cfg.modalidadNombre} actualizada correctamente.`,
        );
        this.guardandoConfiguracion.set(false);
        this.cerrarEdicionConfiguracion();
        this.cargarConfiguraciones();
      },
      error: (err) => {
        const mensaje = err?.error?.mensaje || 'Error al guardar la configuración.';
        this.notificationService.error(mensaje);
        this.guardandoConfiguracion.set(false);
      },
    });
  }
}
