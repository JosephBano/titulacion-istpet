import { Injectable } from '@angular/core';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { EstudiantePendienteEgreso } from '../models/egresados.models';
import {
  ReporteCatalogoItem,
  MetadatosReporteInstitucional,
  FiltrosGeneracionReporte,
} from '../models/reportes.models';

@Injectable({
  providedIn: 'root',
})
export class ReportesService {
  // Catálogo de reportes disponibles (Modular y escalable a N reportes)
  public readonly catalogoReportes: ReporteCatalogoItem[] = [
    {
      id: 'posibles-postulantes',
      titulo: 'Nómina de Posibles Postulantes a Titulación',
      descripcion:
        'Estudiantes que completaron las materias de su malla curricular (pendientes de egreso) y están habilitados para postularse en el ciclo lectivo actual.',
      categoria: 'Estudiantes y Egresados',
      icono: 'report-students',
      formatosDisponibles: ['PDF'],
      requierePeriodo: true,
      requiereCarrera: true,
      requiereEstado: false,
      badge: 'Principal',
      disponible: true,
    },
    {
      id: 'expedientes-postulaciones',
      titulo: 'Bandeja General de Postulaciones y Dictámenes',
      descripcion:
        'Listado consolidado de solicitudes de titulación registradas en el período, incluyendo estado de aprobación y dictamen emitido.',
      categoria: 'Convocatorias y Postulaciones',
      icono: 'report-applications',
      formatosDisponibles: ['PDF'],
      requierePeriodo: true,
      requiereCarrera: true,
      requiereEstado: true,
      badge: 'Próximamente',
      disponible: false,
    },
    {
      id: 'avance-requisitos-docente',
      titulo: 'Consolidado de Validación de Requisitos por Docente',
      descripcion:
        'Seguimiento y estado de cumplimiento de requisitos evaluados por los docentes y secretaría académica.',
      categoria: 'Validación y Docencia',
      icono: 'report-evaluations',
      formatosDisponibles: ['PDF'],
      requierePeriodo: true,
      requiereCarrera: true,
      requiereEstado: false,
      badge: 'Próximamente',
      disponible: false,
    },
    {
      id: 'estadistica-modalidades',
      titulo: 'Distribución Estadística de Modalidades de Grado',
      descripcion:
        'Métricas institucionales de postulantes por examen complexivo, artículo científico y proyecto de investigación.',
      categoria: 'Actas y Dictámenes',
      icono: 'report-analytics',
      formatosDisponibles: ['PDF'],
      requierePeriodo: true,
      requiereCarrera: false,
      requiereEstado: false,
      badge: 'Próximamente',
      disponible: false,
    },
  ];

  /**
   * Genera el documento PDF oficial del Reporte de Posibles Postulantes a Titulación.
   */
  public generarReportePosiblesPostulantesPdf(
    estudiantes: EstudiantePendienteEgreso[],
    filtros: FiltrosGeneracionReporte,
    metadatos: MetadatosReporteInstitucional
  ): void {
    // Configuración de página: A4 Horizontal (Landscape) para máxima legibilidad de columnas
    const doc = new jsPDF({
      orientation: 'landscape',
      unit: 'mm',
      format: 'a4',
    });

    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();
    const marginX = 14;

    // 1. Franja Superior Azul Marino Institucional (#0d1b4c)
    doc.setFillColor(13, 27, 76);
    doc.rect(0, 0, pageWidth, 22, 'F');

    // 2. Línea de Acento Oro Académico (#c59b27)
    doc.setFillColor(197, 155, 39);
    doc.rect(0, 22, pageWidth, 2, 'F');

    // 3. Título Institucional en Cabecera
    doc.setTextColor(255, 255, 255);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(11);
    doc.text('INSTITUTO SUPERIOR TECNOLÓGICO TRAVERSARI — ISTPET', marginX, 9);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8.5);
    doc.setTextColor(230, 235, 245);
    doc.text('SISTEMA DE GESTIÓN DE TITULACIÓN ACADÉMICA', marginX, 15);

    // Fecha / Hora de Emisión a la derecha
    const fechaHoraStr = `Emisión: ${this.formatearFechaHora(metadatos.fechaEmision)}`;
    doc.setFontSize(8);
    doc.text(fechaHoraStr, pageWidth - marginX, 9, { align: 'right' });
    doc.text(`Generado por: ${metadatos.usuarioEmisor}`, pageWidth - marginX, 15, { align: 'right' });

    // 4. Encabezado del Reporte
    let currentY = 32;

    doc.setTextColor(13, 27, 76);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(14);
    doc.text(metadatos.tituloReporte.toUpperCase(), marginX, currentY);

    currentY += 5;
    doc.setTextColor(96, 94, 92);
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8.5);
    doc.text(
      'Nómina oficial de estudiantes que cumplen con el avance curricular para el proceso de graduación.',
      marginX,
      currentY
    );

    currentY += 6;

    // 5. Caja de Parámetros y KPIs de Resumen
    doc.setFillColor(250, 249, 248);
    doc.setDrawColor(225, 223, 221);
    doc.roundedRect(marginX, currentY, pageWidth - marginX * 2, 16, 2, 2, 'FD');

    // Columna 1: Período
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(7.5);
    doc.setTextColor(96, 94, 92);
    doc.text('PERÍODO ACADÉMICO:', marginX + 4, currentY + 6);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(8.5);
    doc.setTextColor(13, 27, 76);
    doc.text(metadatos.periodoAcademico || 'No especificado', marginX + 4, currentY + 11.5);

    // Columna 2: Carrera Filtro
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(7.5);
    doc.setTextColor(96, 94, 92);
    doc.text('CARRERA:', marginX + 70, currentY + 6);
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8.5);
    doc.setTextColor(26, 26, 26);
    const carreraStr = metadatos.carreraFiltro || 'Todas las carreras';
    doc.text(doc.splitTextToSize(carreraStr, 90)[0] || carreraStr, marginX + 70, currentY + 11.5);

    // Columna 3: Total Registros
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(7.5);
    doc.setTextColor(96, 94, 92);
    doc.text('TOTAL POSIBLES POSTULANTES:', marginX + 175, currentY + 6);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(10);
    doc.setTextColor(16, 124, 16);
    doc.text(`${estudiantes.length} estudiante(s)`, marginX + 175, currentY + 12);

    // Columna 4: Con Postulación
    const conPostulacionCount = estudiantes.filter((e) => e.tienePostulacionTitulacion).length;
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(7.5);
    doc.setTextColor(96, 94, 92);
    doc.text('CON POSTULACIÓN:', marginX + 235, currentY + 6);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(10);
    doc.setTextColor(0, 120, 212);
    doc.text(`${conPostulacionCount}`, marginX + 235, currentY + 12);

    currentY += 21;

    // 6. Preparar Datos para la Tabla
    const tableBody = estudiantes.map((e, index) => {
      const porcentajeAvance =
        e.totalMateriasMalla > 0
          ? Math.round((e.materiasAprobadas / e.totalMateriasMalla) * 100)
          : 100;

      const estadoPostulacionText = e.tienePostulacionTitulacion
        ? `Postulado (${e.estadoPostulacion || 'En Trámite'})`
        : 'Sin Postular';

      const finanzasText =
        e.noAdeuda === true
          ? 'Al Día'
          : e.noAdeuda === false
            ? `Adeuda $${(e.saldoPendiente || 0).toFixed(2)}`
            : 'Pendiente';

      return [
        (index + 1).toString(),
        e.idAlumno,
        e.nombreCompleto || `${e.apellidos} ${e.nombres}`.trim(),
        e.carrera,
        `${e.materiasAprobadas}/${e.totalMateriasMalla} (${porcentajeAvance}%)`,
        (e.promedioGeneral || 0).toFixed(2),
        e.ultimoPeriodo || '-',
        estadoPostulacionText,
        finanzasText,
      ];
    });

    // 7. Generar Tabla con jsPDF AutoTable
    autoTable(doc, {
      startY: currentY,
      head: [
        [
          '#',
          'CÉDULA / ID',
          'NOMBRES Y APELLIDOS',
          'CARRERA',
          'AVANCE MALLA',
          'PROM.',
          'ÚLT. PERÍODO',
          'ESTADO POSTULACIÓN',
          'FINANZAS',
        ],
      ],
      body: tableBody,
      theme: 'plain',
      styles: {
        font: 'helvetica',
        fontSize: 7.5,
        cellPadding: { top: 2.5, right: 3, bottom: 2.5, left: 3 },
        lineColor: [225, 223, 221],
        lineWidth: 0.1,
        textColor: [30, 41, 59],
        overflow: 'linebreak',
      },
      headStyles: {
        fillColor: [13, 27, 76],
        textColor: [255, 255, 255],
        fontStyle: 'bold',
        fontSize: 7.5,
        halign: 'left',
      },
      alternateRowStyles: {
        fillColor: [248, 250, 252],
      },
      columnStyles: {
        0: { cellWidth: 8, halign: 'center' },
        1: { cellWidth: 22, fontStyle: 'bold' },
        2: { cellWidth: 58, fontStyle: 'bold' },
        3: { cellWidth: 62 },
        4: { cellWidth: 28, halign: 'center' },
        5: { cellWidth: 15, halign: 'right' },
        6: { cellWidth: 24, halign: 'center' },
        7: { cellWidth: 32 },
        8: { cellWidth: 20, halign: 'center' },
      },
      didDrawPage: (data) => {
        // Pie de Página Institucional en cada página
        const totalPages = doc.getNumberOfPages();
        const currentPage = data.pageNumber;

        doc.setFillColor(197, 155, 39);
        doc.rect(marginX, pageHeight - 12, pageWidth - marginX * 2, 0.5, 'F');

        doc.setFont('helvetica', 'normal');
        doc.setFontSize(7);
        doc.setTextColor(100, 116, 139);

        // Izquierda
        doc.text(
          'Documento confidencial emitido por el Sistema de Titulación ISTPET — Válido para trámites internos.',
          marginX,
          pageHeight - 7
        );

        // Derecha: Paginación
        doc.text(
          `Página ${currentPage} de ${totalPages}`,
          pageWidth - marginX,
          pageHeight - 7,
          { align: 'right' }
        );
      },
      margin: { left: marginX, right: marginX, bottom: 16 },
    });

    // 8. Descargar el archivo PDF automáticamente
    const sanitizeStr = (s: string) => s.replace(/[^a-zA-Z0-9_-]/g, '_');
    const periodoTag = sanitizeStr(metadatos.periodoAcademico || 'CICLO_ACTUAL');
    const filename = `Reporte_Posibles_Postulantes_${periodoTag}_${new Date().toISOString().substring(0, 10)}.pdf`;

    doc.save(filename);
  }

  private formatearFechaHora(d: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');
    const dia = pad(d.getDate());
    const mes = pad(d.getMonth() + 1);
    const anio = d.getFullYear();
    const hora = pad(d.getHours());
    const min = pad(d.getMinutes());
    return `${dia}/${mes}/${anio} ${hora}:${min}`;
  }
}
