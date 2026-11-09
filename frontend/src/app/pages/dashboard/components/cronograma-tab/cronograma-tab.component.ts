import { Component, computed, input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ConvocatoriaDetalle, PortalEstudiante } from '../../../../core/models/titulacion.models';

export interface EtapaCronogramaReal {
  numero: number;
  titulo: string;
  subtitulo: string;
  fechaInicio: Date;
  fechaFin: Date;
  duracionDias: number;
  estado: 'En Curso' | 'Próxima' | 'Finalizada';
  estadoClase: 'pill--active' | 'pill--upcoming' | 'pill--completed';
  esActiva: boolean;
  diasRestantes: number;
  porcentajeProgreso: number;
}

@Component({
  selector: 'app-cronograma-tab',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './cronograma-tab.component.html',
  styleUrls: ['./cronograma-tab.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CronogramaTabComponent {
  // Inputs reales provenientes del dashboard / servicios
  convocatoria = input<ConvocatoriaDetalle | null>(null);
  portalEstudiante = input<PortalEstudiante | null>(null);
  periodoAmigable = input<string>('');

  private limpiarHora(d: Date): Date {
    return new Date(d.getFullYear(), d.getMonth(), d.getDate());
  }

  private fromYMD(s: string): Date {
    const parts = s.includes('T') ? s.split('T')[0].split('-') : s.split('-');
    const y = parseInt(parts[0], 10);
    const m = parseInt(parts[1], 10) - 1;
    const d = parseInt(parts[2], 10);
    return new Date(y, m, d);
  }

  private diffDias(d1: Date, d2: Date): number {
    const msPorDia = 24 * 60 * 60 * 1000;
    return Math.round((d2.getTime() - d1.getTime()) / msPorDia);
  }

  // Datos reales consolidados sin invenciones
  cronogramaData = computed(() => {
    const conv = this.convocatoria();
    const portal = this.portalEstudiante();
    const portalConv = portal?.convocatoria;

    const fechaInicioStr = conv?.fechaInicio || portalConv?.fechaInicio || null;
    const fechaFinStr = conv?.fechaFin || portalConv?.fechaCierre || null;
    const diasPermitidos = conv?.diasPermitidos ?? null;
    const diasExtension = conv?.diasExtension ?? 0;
    const detalleConvocatoria =
      conv?.detalle || portalConv?.detalle || 'Convocatoria de Titulación';
    const periodo = conv?.idPeriodo || portalConv?.periodo || '';

    // Si la BD no contiene fechas configuradas, no inventamos fechas relativas
    if (!fechaInicioStr || !fechaFinStr) {
      return {
        tieneDatosReales: false,
        periodo,
        detalleConvocatoria,
        diasPermitidos,
        diasExtension,
        etapas: [] as EtapaCronogramaReal[],
        resumenEstado: null,
      };
    }

    const hoy = this.limpiarHora(new Date());
    const dInicioPost = this.limpiarHora(this.fromYMD(fechaInicioStr));
    const dFinPost = this.limpiarHora(this.fromYMD(fechaFinStr));
    const duracionPostDias = Math.max(1, this.diffDias(dInicioPost, dFinPost) + 1);

    const etapas: EtapaCronogramaReal[] = [];

    // Etapa 1: Postulación y Recepción de Expedientes (fechas directas de BD)
    const esPostActiva = hoy >= dInicioPost && hoy <= dFinPost;
    const esPostFutura = hoy < dInicioPost;
    const diasRestantesPost = Math.max(0, this.diffDias(hoy, dFinPost));
    const diasTranscurridosPost = Math.max(0, this.diffDias(dInicioPost, hoy) + 1);
    const progresoPost = esPostActiva
      ? Math.min(100, Math.max(0, Math.round((diasTranscurridosPost / duracionPostDias) * 100)))
      : esPostFutura
        ? 0
        : 100;

    etapas.push({
      numero: 1,
      titulo: 'Recepción de Postulaciones y Expedientes',
      subtitulo: 'Plazo oficial para registro y entrega de requisitos de la cohorte.',
      fechaInicio: dInicioPost,
      fechaFin: dFinPost,
      duracionDias: duracionPostDias,
      estado: esPostActiva ? 'En Curso' : esPostFutura ? 'Próxima' : 'Finalizada',
      estadoClase: esPostActiva
        ? 'pill--active'
        : esPostFutura
          ? 'pill--upcoming'
          : 'pill--completed',
      esActiva: esPostActiva,
      diasRestantes: diasRestantesPost,
      porcentajeProgreso: progresoPost,
    });

    // Etapa 2: Desarrollo y Ejecución de Titulación (si existen días permitidos configurados en BD)
    if (diasPermitidos && diasPermitidos > 0) {
      const dInicioDes = new Date(dFinPost.getTime() + 24 * 60 * 60 * 1000);
      const dFinDes = new Date(dInicioDes.getTime() + (diasPermitidos - 1) * 24 * 60 * 60 * 1000);

      const esDesActivo = hoy >= dInicioDes && hoy <= dFinDes;
      const esDesFuturo = hoy < dInicioDes;
      const diasRestantesDes = Math.max(0, this.diffDias(hoy, dFinDes));
      const diasTranscurridosDes = Math.max(0, this.diffDias(dInicioDes, hoy) + 1);
      const progresoDes = esDesActivo
        ? Math.min(100, Math.max(0, Math.round((diasTranscurridosDes / diasPermitidos) * 100)))
        : esDesFuturo
          ? 0
          : 100;

      etapas.push({
        numero: 2,
        titulo: 'Desarrollo del Proceso de Titulación',
        subtitulo: 'Plazo reglamentario asignado por la institución para tutorías o complexivo.',
        fechaInicio: dInicioDes,
        fechaFin: dFinDes,
        duracionDias: diasPermitidos,
        estado: esDesActivo ? 'En Curso' : esDesFuturo ? 'Próxima' : 'Finalizada',
        estadoClase: esDesActivo
          ? 'pill--active'
          : esDesFuturo
            ? 'pill--upcoming'
            : 'pill--completed',
        esActiva: esDesActivo,
        diasRestantes: diasRestantesDes,
        porcentajeProgreso: progresoDes,
      });

      // Etapa 3: Prórroga Reglamentaria (solo si días de extensión > 0 en BD)
      if (diasExtension > 0) {
        const dInicioPro = new Date(dFinDes.getTime() + 24 * 60 * 60 * 1000);
        const dFinPro = new Date(dInicioPro.getTime() + (diasExtension - 1) * 24 * 60 * 60 * 1000);

        const esProActiva = hoy >= dInicioPro && hoy <= dFinPro;
        const esProFutura = hoy < dInicioPro;
        const diasRestantesPro = Math.max(0, this.diffDias(hoy, dFinPro));
        const diasTranscurridosPro = Math.max(0, this.diffDias(dInicioPro, hoy) + 1);
        const progresoPro = esProActiva
          ? Math.min(100, Math.max(0, Math.round((diasTranscurridosPro / diasExtension) * 100)))
          : esProFutura
            ? 0
            : 100;

        etapas.push({
          numero: 3,
          titulo: 'Prórroga Reglamentaria',
          subtitulo: 'Extensión oficial autorizada para la culminación del proceso.',
          fechaInicio: dInicioPro,
          fechaFin: dFinPro,
          duracionDias: diasExtension,
          estado: esProActiva ? 'En Curso' : esProFutura ? 'Próxima' : 'Finalizada',
          estadoClase: esProActiva
            ? 'pill--active'
            : esProFutura
              ? 'pill--upcoming'
              : 'pill--completed',
          esActiva: esProActiva,
          diasRestantes: diasRestantesPro,
          porcentajeProgreso: progresoPro,
        });
      }
    }

    // Resumen del estado actual
    const etapaActiva = etapas.find((e) => e.esActiva) || null;
    let resumenTexto: string;
    let resumenBadge: string;
    let resumenClase: string;

    if (etapaActiva) {
      resumenTexto = `${etapaActiva.titulo} está actualmente en curso.`;
      resumenBadge =
        etapaActiva.diasRestantes === 0
          ? '¡Vence Hoy!'
          : `${etapaActiva.diasRestantes} días restantes`;
      resumenClase = 'badge--active';
    } else if (hoy < dInicioPost) {
      const diasParaInicio = this.diffDias(hoy, dInicioPost);
      resumenTexto = `La postulación iniciará el ${fechaInicioStr}.`;
      resumenBadge = `Inicia en ${diasParaInicio} días`;
      resumenClase = 'badge--upcoming';
    } else {
      resumenTexto = 'Los plazos de este período de titulación han culminado.';
      resumenBadge = 'Concluido';
      resumenClase = 'badge--completed';
    }

    return {
      tieneDatosReales: true,
      periodo,
      detalleConvocatoria,
      diasPermitidos,
      diasExtension,
      etapas,
      resumenEstado: {
        texto: resumenTexto,
        badge: resumenBadge,
        clase: resumenClase,
        etapaActiva,
      },
    };
  });
}
