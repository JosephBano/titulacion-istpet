import { jsPDF } from 'jspdf';
import autoTable, { Color } from 'jspdf-autotable';

/** Conserva los saltos de línea del contenido y de los bloques del documento. */
function texto(elemento: Element | null): string {
  if (!elemento) return '';
  const copia = elemento.cloneNode(true) as Element;
  for (const salto of copia.querySelectorAll('br')) salto.replaceWith('\n');
  for (const bloque of copia.querySelectorAll('p, div')) {
    bloque.prepend('\n');
    bloque.append('\n');
  }
  return (copia.textContent ?? '')
    .split('\n')
    .map((linea) => linea.trim())
    .filter(Boolean)
    .join('\n');
}

/** PDF vectorial: los bordes y la paginación no dependen de la impresión del HTML. */
export function crearPlanificacionPdf(documento: HTMLElement): jsPDF {
  const pdf = new jsPDF({ format: 'a4', unit: 'mm', orientation: 'portrait' });
  pdf.setProperties({ title: 'Planificación del examen complexivo — ISTPET', author: 'ISTPET' });
  const margen = 14;
  const ancho = 182;
  const azul: Color = [84, 130, 181];
  const borde: Color = [51, 65, 85];
  const colores: Color[] = [
    [201, 229, 241],
    [242, 201, 208],
    [217, 232, 198],
  ];
  let y = margen;

  const logo = documento.querySelector<HTMLImageElement>('.planning-brand img');
  if (logo?.complete && logo.naturalWidth > 0) {
    const canvas = document.createElement('canvas');
    canvas.width = logo.naturalWidth;
    canvas.height = logo.naturalHeight;
    canvas.getContext('2d')!.drawImage(logo, 0, 0);
    pdf.addImage(canvas, 'PNG', margen, y + 2, 18, (18 * logo.naturalHeight) / logo.naturalWidth);
  } else {
    pdf.setFontSize(9);
    pdf.text('ISTPET', margen, y + 9);
  }
  pdf.setTextColor(80, 126, 175);
  pdf.setFont('helvetica', 'bold');
  pdf.setFontSize(13);
  pdf.text(['UNIDAD DE', 'TITULACIÓN'], 36, y + 5);
  pdf.setTextColor(23, 23, 23);
  pdf.setFontSize(10);
  pdf.text(texto(documento.querySelector('.planning-period strong')), 151, y + 3, {
    align: 'center',
  });
  pdf.setFont('helvetica', 'normal');
  pdf.setFontSize(9);
  const periodo: string[] = pdf.splitTextToSize(
    texto(documento.querySelector('.planning-period p')),
    88,
  );
  pdf.text(periodo, 151, y + 8, { align: 'center' });
  y += Math.max(18, 8 + periodo.length * 4);

  pdf.setFont('helvetica', 'bold');
  pdf.setFontSize(11);
  const titulo: string[] = pdf.splitTextToSize(
    texto(documento.querySelector('.planning-title h1')),
    ancho,
  );
  pdf.text(titulo, 105, y + 4, { align: 'center' });
  y += titulo.length * 4.5 + 6;
  pdf.setFontSize(9);
  for (const parrafo of documento.querySelectorAll('.planning-title p')) {
    const lineas: string[] = pdf.splitTextToSize(texto(parrafo), ancho);
    pdf.text(lineas, 105, y, { align: 'center' });
    y += lineas.length * 4;
    pdf.setFont('helvetica', 'normal');
  }

  function tabla(selector: string, anchos: number[]): void {
    const elemento = documento.querySelector<HTMLTableElement>(selector);
    if (!elemento) throw new Error('No se encontró la tabla de planificación.');
    autoTable(pdf, {
      html: elemento,
      includeHiddenHtml: true,
      startY: y + 2,
      margin: margen,
      theme: 'grid',
      rowPageBreak: 'avoid',
      styles: {
        font: 'helvetica',
        fontSize: 8.5,
        cellPadding: 2,
        lineWidth: 0.35,
        lineColor: borde,
        textColor: [23, 23, 23],
        valign: 'middle',
        overflow: 'linebreak',
      },
      headStyles: {
        fillColor: azul,
        textColor: [255, 255, 255],
        halign: 'center',
        fontStyle: 'bold',
        fontSize: 8,
      },
      columnStyles: Object.fromEntries(
        anchos.map((cellWidth, i) => [
          i,
          {
            cellWidth,
            halign:
              (i === 1 && selector === '.curriculum-table') ||
              (i === 2 && selector === '.schedules-table')
                ? ('left' as const)
                : ('center' as const),
          },
        ]),
      ),
      didParseCell: ({ cell, column, section }) => {
        if (cell.raw instanceof HTMLElement) cell.text = texto(cell.raw).split('\n');
        if (section === 'head' && selector === '.curriculum-table') {
          if (column.index === 2) cell.text = ['HORAS', 'POR', 'TEMA'];
          if (column.index === 5) cell.text = ['TOTAL', 'DE', 'HORAS'];
        }
        if (section !== 'body') return;
        const elementoFila = cell.raw instanceof HTMLElement ? cell.raw.closest('tr') : null;
        if (elementoFila?.classList.contains('course-total')) {
          cell.styles.fillColor = [233, 239, 246];
          cell.styles.fontStyle = 'bold';
          if (column.index === 0) cell.styles.halign = 'left';
          return;
        }
        const indice = colores.findIndex((_, i) =>
          elementoFila?.classList.contains(`module-tone-${i}`),
        );
        if (
          indice >= 0 &&
          (column.index === 0 || (selector === '.schedules-table' && column.index === 1))
        ) {
          cell.styles.fillColor = colores[indice];
          if (selector === '.schedules-table') cell.styles.fontStyle = 'bold';
        }
        if (selector === '.schedules-table' && column.index === 0) {
          cell.styles.fillColor = azul;
          cell.styles.textColor = indice >= 0 ? colores[indice] : [255, 255, 255];
        }
      },
      didDrawPage: ({ cursor }) => {
        if (cursor) y = cursor.y;
      },
    });
  }

  tabla('.curriculum-table', [27, 64, 18, 30, 27, 16]);
  if (y + 40 > 297 - margen) {
    pdf.addPage();
    y = margen;
  }
  y += 8;
  pdf.setFont('helvetica', 'normal');
  pdf.setFontSize(9);
  for (const parrafo of documento.querySelectorAll('.course-schedules > p')) {
    const lineas: string[] = pdf.splitTextToSize(texto(parrafo), ancho);
    pdf.text(lineas, margen, y);
    y += lineas.length * 4;
  }
  tabla('.schedules-table', [31, 43, 108]);
  y += 8;
  pdf.setFont('helvetica', 'bold');
  pdf.setFontSize(9);
  if (y + 22 > 297 - margen) {
    pdf.addPage();
    y = margen + 4;
  }
  pdf.text('Observaciones:', margen, y);
  y += 6;
  pdf.setFont('helvetica', 'normal');
  for (const nota of documento.querySelectorAll('.course-schedules li')) {
    const lineas: string[] = pdf.splitTextToSize(texto(nota), ancho - 5);
    if (y + lineas.length * 4 > 297 - margen) {
      pdf.addPage();
      y = margen + 4;
    }
    pdf.text('•', margen, y);
    pdf.text(lineas, margen + 5, y);
    y += lineas.length * 4 + 2;
  }
  return pdf;
}

export function imprimirPlanificacion(documento: HTMLElement): void {
  const pdf = crearPlanificacionPdf(documento);
  pdf.autoPrint();
  const url = URL.createObjectURL(pdf.output('blob'));
  window.open(url, '_blank', 'noopener');
  window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
}
