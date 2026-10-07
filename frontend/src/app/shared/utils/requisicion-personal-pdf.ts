import { jsPDF } from 'jspdf';
import {
  PlantillaRequisicionDefinicion,
  DatosFormularioRequisicion,
} from '../../domain/models/plantilla-requisicion.model';

export interface RecursosPdfPlantilla {
  logoDataUrl?: string;
  fondoDataUrl?: string;
}

function hexToRgb(hex: string): [number, number, number] {
  let c = hex.replace('#', '').trim();
  if (c.length === 3) {
    c = c
      .split('')
      .map((x) => x + x)
      .join('');
  }
  const num = parseInt(c, 16);
  if (isNaN(num)) return [23, 23, 23];
  return [(num >> 16) & 255, (num >> 8) & 255, num & 255];
}

function obtenerNombreFuente(fuente: string): string {
  switch (fuente) {
    case 'serif':
      return 'times';
    case 'mono':
      return 'courier';
    default:
      return 'helvetica';
  }
}

export async function crearRequisicionPdf(
  definicionOriginal: PlantillaRequisicionDefinicion,
  datosOriginal: Partial<DatosFormularioRequisicion>,
  recursos?: RecursosPdfPlantilla,
): Promise<jsPDF> {
  // Copia inmutable para consistencia durante todo el render
  const definicion = structuredClone(definicionOriginal);
  const datos = structuredClone(datosOriginal);

  const docConfig = definicion.documento;
  const estilo = docConfig.estilo;
  const margenes = docConfig.pagina.margenesMm;

  const pdf = new jsPDF({
    format: 'a4',
    unit: 'mm',
    orientation: 'portrait',
  });

  const fontName = obtenerNombreFuente(estilo.fuente);
  const anchoPagina = 210;
  const altoPagina = 297;
  const anchoUtil = anchoPagina - margenes.izquierdo - margenes.derecho;
  const colorTexto = hexToRgb(estilo.colorTexto);
  const colorEncabezado = hexToRgb(estilo.colorEncabezado);
  const colorDestacado = hexToRgb(estilo.colorDestacado);
  const colorBorde = hexToRgb(estilo.colorBorde);
  const grosorBorde = estilo.grosorBordeMm;

  pdf.setProperties({
    title: definicion.documento.titulo,
    author: 'ISTPET - Unidad de Titulación',
  });

  const dibujarFondo = () => {
    if (recursos?.fondoDataUrl && docConfig.fondo.opacidad > 0) {
      try {
        const opacidad = Math.min(Math.max(docConfig.fondo.opacidad, 0), 1);
        const GState = (pdf as unknown as { GState: new (options: { opacity: number }) => unknown })
          .GState;
        if (GState) {
          pdf.setGState(new GState({ opacity: opacidad }));
        }
        if (docConfig.fondo.ajuste === 'cover') {
          pdf.addImage(recursos.fondoDataUrl, 'PNG', 0, 0, anchoPagina, altoPagina);
        } else {
          const fondoAncho = anchoUtil * 0.75;
          const fondoAlto = fondoAncho;
          const xFondo = (anchoPagina - fondoAncho) / 2;
          const yFondo = (altoPagina - fondoAlto) / 2;
          pdf.addImage(recursos.fondoDataUrl, 'PNG', xFondo, yFondo, fondoAncho, fondoAlto);
        }
        if (GState) {
          pdf.setGState(new GState({ opacity: 1.0 }));
        }
      } catch {
        // En caso de que GState falle, continuar sin opacidad
      }
    }
  };

  dibujarFondo();

  let y = margenes.superior;

  const asegurarEspacio = (alturaRequerida: number) => {
    if (y + alturaRequerida > altoPagina - margenes.inferior) {
      pdf.addPage();
      dibujarFondo();
      y = margenes.superior;
    }
  };

  // Encabezado con Logo y Título
  const logoAncho = docConfig.logo.anchoMm || 24;
  if (recursos?.logoDataUrl) {
    try {
      pdf.addImage(recursos.logoDataUrl, 'PNG', margenes.izquierdo, y, logoAncho, logoAncho * 0.75);
    } catch {
      // Si la imagen falla se dibuja marca de texto
    }
  } else if (docConfig.logo.usarLogoInstitucional) {
    pdf.setFont(fontName, 'bold');
    pdf.setFontSize(11);
    pdf.setTextColor(...colorDestacado);
    pdf.text('ISTPET', margenes.izquierdo, y + 6);
  }

  // Título del documento
  pdf.setFont(fontName, 'bold');
  pdf.setFontSize(estilo.tamanoFuentePt + 4);
  pdf.setTextColor(...colorEncabezado);
  const tituloLineas = pdf.splitTextToSize(docConfig.titulo, anchoUtil - logoAncho - 5);
  pdf.text(tituloLineas, margenes.izquierdo + (logoAncho > 0 ? logoAncho + 4 : 0), y + 6);
  y += Math.max(logoAncho * 0.75, tituloLineas.length * 6) + 4;

  if (docConfig.subtitulo) {
    pdf.setFont(fontName, 'normal');
    pdf.setFontSize(estilo.tamanoFuentePt);
    pdf.setTextColor(...colorTexto);
    pdf.text(docConfig.subtitulo, margenes.izquierdo, y);
    y += 5;
  }

  // Línea divisoria del encabezado
  pdf.setDrawColor(...colorDestacado);
  pdf.setLineWidth(grosorBorde * 1.5);
  pdf.line(margenes.izquierdo, y, margenes.izquierdo + anchoUtil, y);
  y += 4;

  const dibujarTituloSeccion = (titulo: string) => {
    asegurarEspacio(9);
    pdf.setFillColor(234, 240, 248);
    pdf.rect(margenes.izquierdo, y, anchoUtil, 6, 'F');
    pdf.setDrawColor(...colorBorde);
    pdf.setLineWidth(grosorBorde);
    pdf.rect(margenes.izquierdo, y, anchoUtil, 6, 'S');

    pdf.setFont(fontName, 'bold');
    pdf.setFontSize(estilo.tamanoFuentePt);
    pdf.setTextColor(...colorEncabezado);
    pdf.text(titulo, margenes.izquierdo + 3, y + 4.2);
    y += 7.5;
  };

  const dibujarCheckbox = (
    x: number,
    yPos: number,
    marcado: boolean,
    etiqueta: string,
    anchoMax: number,
  ) => {
    const boxSize = 3.2;
    pdf.setDrawColor(...colorBorde);
    pdf.setLineWidth(0.25);
    pdf.rect(x, yPos - boxSize + 0.8, boxSize, boxSize);

    if (marcado) {
      pdf.setFillColor(...colorDestacado);
      pdf.rect(x + 0.6, yPos - boxSize + 1.4, boxSize - 1.2, boxSize - 1.2, 'F');
    }

    pdf.setFont(fontName, marcado ? 'bold' : 'normal');
    pdf.setFontSize(estilo.tamanoFuentePt - 0.5);
    pdf.setTextColor(...colorTexto);
    const lineas = pdf.splitTextToSize(etiqueta, anchoMax - boxSize - 2);
    pdf.text(lineas, x + boxSize + 2, yPos);
    return Math.max(boxSize + 1, lineas.length * 4);
  };

  const dibujarCampo = (
    etiqueta: string,
    valor: string | number | null | undefined,
    anchoTotal = anchoUtil,
  ) => {
    const textoValor = valor !== null && valor !== undefined ? String(valor) : '';
    pdf.setFont(fontName, 'bold');
    pdf.setFontSize(estilo.tamanoFuentePt);
    pdf.setTextColor(...colorTexto);

    const anchoEtiqueta = 55;
    const lineasEtiqueta = pdf.splitTextToSize(etiqueta + ':', anchoEtiqueta);

    pdf.setFont(fontName, 'normal');
    const anchoValor = anchoTotal - anchoEtiqueta - 2;
    const lineasValor = pdf.splitTextToSize(textoValor || '—', anchoValor);

    const alturaFila = Math.max(lineasEtiqueta.length, lineasValor.length) * 4.2 + 2;
    asegurarEspacio(alturaFila);

    pdf.setFont(fontName, 'bold');
    pdf.text(lineasEtiqueta, margenes.izquierdo + 2, y + 3.5);

    pdf.setFont(fontName, 'normal');
    pdf.text(lineasValor, margenes.izquierdo + anchoEtiqueta + 2, y + 3.5);

    pdf.setDrawColor(200, 200, 200);
    pdf.setLineWidth(0.2);
    pdf.line(
      margenes.izquierdo + anchoEtiqueta + 2,
      y + alturaFila - 0.5,
      margenes.izquierdo + anchoTotal,
      y + alturaFila - 0.5,
    );

    y += alturaFila;
  };

  // 1. INFORMACIÓN GENERAL
  const sec1 =
    definicion.secciones.find((s) => s.id === 'general')?.titulo ?? '1. INFORMACIÓN GENERAL';
  dibujarTituloSeccion(sec1);
  dibujarCampo(
    definicion.campos['fechaSolicitud']?.etiqueta ?? 'Fecha de solicitud',
    datos.fechaSolicitud,
  );
  dibujarCampo(definicion.campos['area']?.etiqueta ?? 'Área / Departamento', datos.area);
  dibujarCampo(
    definicion.campos['solicitante']?.etiqueta ?? 'Nombre del solicitante',
    datos.solicitante,
  );
  y += 2;

  // 2. INSTITUCIÓN SOLICITANTE
  const sec2 =
    definicion.secciones.find((s) => s.id === 'institucion')?.titulo ??
    '2. INSTITUCIÓN SOLICITANTE';
  dibujarTituloSeccion(sec2);
  const opcionesInst = definicion.opciones.institucion ?? [];
  for (const opt of opcionesInst) {
    asegurarEspacio(6);
    const marcada = datos.institucion === opt.id || datos.institucion === opt.etiqueta;
    const salto = dibujarCheckbox(
      margenes.izquierdo + 3,
      y + 3,
      marcada,
      opt.etiqueta,
      anchoUtil - 6,
    );
    y += salto + 1.5;
  }
  y += 2;

  // 3. INFORMACIÓN DE LA VACANTE
  const sec3 =
    definicion.secciones.find((s) => s.id === 'vacante')?.titulo ?? '3. INFORMACIÓN DE LA VACANTE';
  dibujarTituloSeccion(sec3);
  dibujarCampo(definicion.campos['cargo']?.etiqueta ?? 'Cargo solicitado', datos.cargo);
  dibujarCampo(
    definicion.campos['numeroVacantes']?.etiqueta ?? 'Número de vacantes',
    datos.numeroVacantes,
  );

  // Tipo de contratación
  asegurarEspacio(6);
  pdf.setFont(fontName, 'bold');
  pdf.setFontSize(estilo.tamanoFuentePt);
  pdf.text(
    (definicion.campos['contratacion']?.etiqueta ?? 'Tipo de contratación') + ':',
    margenes.izquierdo + 2,
    y + 3.5,
  );
  let xOffset = margenes.izquierdo + 52;
  const opcionesCont = definicion.opciones.contratacion ?? [];
  for (const opt of opcionesCont) {
    const marcada = datos.contratacion === opt.id || datos.contratacion === opt.etiqueta;
    dibujarCheckbox(xOffset, y + 3.5, marcada, opt.etiqueta, 40);
    xOffset += 42;
  }
  y += 6.5;

  // Jornada laboral
  asegurarEspacio(6);
  pdf.setFont(fontName, 'bold');
  pdf.setFontSize(estilo.tamanoFuentePt);
  pdf.text(
    (definicion.campos['jornada']?.etiqueta ?? 'Jornada laboral') + ':',
    margenes.izquierdo + 2,
    y + 3.5,
  );
  xOffset = margenes.izquierdo + 52;
  const opcionesJor = definicion.opciones.jornada ?? [];
  for (const opt of opcionesJor) {
    const marcada = datos.jornada === opt.id || datos.jornada === opt.etiqueta;
    dibujarCheckbox(xOffset, y + 3.5, marcada, opt.etiqueta, 40);
    xOffset += 42;
  }
  y += 6.5;

  // Remuneración
  asegurarEspacio(6);
  pdf.setFont(fontName, 'bold');
  pdf.setFontSize(estilo.tamanoFuentePt);
  pdf.text(
    (definicion.campos['remuneracion']?.etiqueta ?? 'Remuneración') + ':',
    margenes.izquierdo + 2,
    y + 3.5,
  );
  xOffset = margenes.izquierdo + 52;
  const opcionesRem = definicion.opciones.remuneracion ?? [];
  for (const opt of opcionesRem) {
    const marcada = datos.remuneracion === opt.id || datos.remuneracion === opt.etiqueta;
    dibujarCheckbox(xOffset, y + 3.5, marcada, opt.etiqueta, 35);
    xOffset += 38;
  }
  y += 6.5;

  dibujarCampo(
    definicion.campos['valorRemuneracion']?.etiqueta ?? 'Sueldo / valor por hora',
    datos.valorRemuneracion !== null && datos.valorRemuneracion !== undefined
      ? `$ ${datos.valorRemuneracion}`
      : '',
  );
  dibujarCampo(
    definicion.campos['valorTotal']?.etiqueta ?? 'Valor total',
    datos.valorTotal !== null && datos.valorTotal !== undefined ? `$ ${datos.valorTotal}` : '',
  );
  dibujarCampo(definicion.campos['horario']?.etiqueta ?? 'Horario requerido', datos.horario);
  dibujarCampo(definicion.campos['lugar']?.etiqueta ?? 'Lugar de trabajo', datos.lugar);
  dibujarCampo(
    definicion.campos['fechaIngreso']?.etiqueta ?? 'Fecha requerida de ingreso',
    datos.fechaIngreso,
  );
  y += 2;

  // 4. MOTIVO DE LA VACANTE
  const sec4 =
    definicion.secciones.find((s) => s.id === 'motivo')?.titulo ?? '4. MOTIVO DE LA VACANTE';
  dibujarTituloSeccion(sec4);
  const opcionesMotivo = definicion.opciones.motivo ?? [];
  const columnasMotivo = 2;
  const anchoCol = anchoUtil / columnasMotivo;
  for (let i = 0; i < opcionesMotivo.length; i += columnasMotivo) {
    asegurarEspacio(5.5);
    const m1 = opcionesMotivo[i];
    const marcada1 = datos.motivo === m1.id || datos.motivo === m1.etiqueta;
    dibujarCheckbox(margenes.izquierdo + 3, y + 3, marcada1, m1.etiqueta, anchoCol - 6);

    if (i + 1 < opcionesMotivo.length) {
      const m2 = opcionesMotivo[i + 1];
      const marcada2 = datos.motivo === m2.id || datos.motivo === m2.etiqueta;
      dibujarCheckbox(
        margenes.izquierdo + anchoCol + 3,
        y + 3,
        marcada2,
        m2.etiqueta,
        anchoCol - 6,
      );
    }
    y += 5.5;
  }
  if (datos.otroMotivo) {
    dibujarCampo(definicion.campos['otroMotivo']?.etiqueta ?? 'Otro motivo', datos.otroMotivo);
  }
  dibujarCampo(
    definicion.campos['detalleMotivo']?.etiqueta ?? 'Detalle del motivo',
    datos.detalleMotivo,
  );
  y += 2;

  // 5. PERFIL REQUERIDO
  const sec5 = definicion.secciones.find((s) => s.id === 'perfil')?.titulo ?? '5. PERFIL REQUERIDO';
  dibujarTituloSeccion(sec5);
  dibujarCampo(definicion.campos['formacion']?.etiqueta ?? 'Formación académica', datos.formacion);
  dibujarCampo(
    definicion.campos['experiencia']?.etiqueta ?? 'Experiencia requerida',
    datos.experiencia,
  );
  dibujarCampo(
    definicion.campos['conocimientos']?.etiqueta ?? 'Conocimientos específicos',
    datos.conocimientos,
  );

  // Competencias
  asegurarEspacio(8);
  pdf.setFont(fontName, 'bold');
  pdf.setFontSize(estilo.tamanoFuentePt);
  pdf.text(
    (definicion.campos['competencias']?.etiqueta ?? 'Competencias requeridas') + ':',
    margenes.izquierdo + 2,
    y + 3.5,
  );
  y += 5;

  const competenciasSeleccionadas = new Set(datos.competencias ?? []);
  const opcionesComp = definicion.opciones.competencias ?? [];
  for (let i = 0; i < opcionesComp.length; i += columnasMotivo) {
    asegurarEspacio(5.5);
    const c1 = opcionesComp[i];
    const marcada1 =
      competenciasSeleccionadas.has(c1.id) || competenciasSeleccionadas.has(c1.etiqueta);
    dibujarCheckbox(margenes.izquierdo + 3, y + 3, marcada1, c1.etiqueta, anchoCol - 6);

    if (i + 1 < opcionesComp.length) {
      const c2 = opcionesComp[i + 1];
      const marcada2 =
        competenciasSeleccionadas.has(c2.id) || competenciasSeleccionadas.has(c2.etiqueta);
      dibujarCheckbox(
        margenes.izquierdo + anchoCol + 3,
        y + 3,
        marcada2,
        c2.etiqueta,
        anchoCol - 6,
      );
    }
    y += 5.5;
  }
  if (datos.otraCompetencia) {
    dibujarCampo(
      definicion.campos['otraCompetencia']?.etiqueta ?? 'Otra competencia',
      datos.otraCompetencia,
    );
  }
  y += 2;

  // 6. OBSERVACIONES
  if (datos.observaciones) {
    const sec6 =
      definicion.secciones.find((s) => s.id === 'observaciones')?.titulo ?? '6. OBSERVACIONES';
    dibujarTituloSeccion(sec6);
    dibujarCampo(
      definicion.campos['observaciones']?.etiqueta ?? 'Observaciones',
      datos.observaciones,
    );
    y += 2;
  }

  // Firmas: Se mantienen juntas
  const alturaFirmas = 38;
  asegurarEspacio(alturaFirmas + 15);

  const firmas = definicion.firmas ?? [];
  const anchoBloqueFirma = anchoUtil / 3;

  y += 8;
  for (let idx = 0; idx < firmas.length && idx < 3; idx++) {
    const firma = firmas[idx];
    const xFirma = margenes.izquierdo + idx * anchoBloqueFirma;

    pdf.setFont(fontName, 'bold');
    pdf.setFontSize(estilo.tamanoFuentePt - 0.5);
    pdf.setTextColor(...colorEncabezado);
    pdf.text(firma.titulo, xFirma + anchoBloqueFirma / 2, y, { align: 'center' });

    // Línea de firma
    pdf.setDrawColor(120, 120, 120);
    pdf.setLineWidth(0.3);
    const lineaY = y + 18;
    pdf.line(xFirma + 5, lineaY, xFirma + anchoBloqueFirma - 5, lineaY);

    pdf.setFont(fontName, 'normal');
    pdf.setFontSize(estilo.tamanoFuentePt - 1.5);
    pdf.setTextColor(...colorTexto);
    pdf.text(`${firma.etiquetaNombre}:`, xFirma + 5, lineaY + 4);
    pdf.text(`${firma.etiquetaCargo}:`, xFirma + 5, lineaY + 8);
  }

  y += 30;

  // Nota final
  if (docConfig.notaFinal) {
    asegurarEspacio(12);
    pdf.setFont(fontName, 'italic');
    pdf.setFontSize(estilo.tamanoFuentePt - 1.5);
    pdf.setTextColor(90, 90, 90);
    const lineasNota = pdf.splitTextToSize(docConfig.notaFinal, anchoUtil);
    pdf.text(lineasNota, margenes.izquierdo, y);
  }

  return pdf;
}
