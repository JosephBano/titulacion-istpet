import {
  EstiloPlantilla,
  PaginaPlantilla,
  FondoPlantilla,
} from '../../domain/models/plantilla-requisicion.model';

export interface OpcionesEstiloDocumento {
  estilo?: EstiloPlantilla;
  pagina?: PaginaPlantilla;
  fondoDataUrl?: string;
  fondoConfig?: FondoPlantilla;
}

/** Usa el mismo documento A4 para la vista previa y la impresión, respetando la configuración institucional. */
export function documentHtml(
  element: HTMLElement,
  title: string,
  preview = false,
  opciones?: OpcionesEstiloDocumento,
): string {
  const doc = document.implementation.createHTMLDocument(title);
  doc.documentElement.lang = 'es';

  const estilo = opciones?.estilo;
  const margenes = opciones?.pagina?.margenesMm ?? {
    superior: 14,
    derecho: 14,
    inferior: 14,
    izquierdo: 14,
  };

  let fontFamily = `'Segoe UI', Aptos, -apple-system, BlinkMacSystemFont, Arial, sans-serif`;
  if (estilo?.fuente === 'serif') {
    fontFamily = `'Times New Roman', Times, Georgia, serif`;
  } else if (estilo?.fuente === 'mono') {
    fontFamily = `'Courier New', Courier, Consolas, monospace`;
  }

  const colorTexto = estilo?.colorTexto || '#171717';
  const colorEncabezado = estilo?.colorEncabezado || '#002855';
  const colorDestacado = estilo?.colorDestacado || '#002855';
  const colorBorde = estilo?.colorBorde || '#cbd5e1';
  const grosorBordeMm = estilo?.grosorBordeMm ?? 0.35;
  const tamanoFuentePt = estilo?.tamanoFuentePt ?? 9;

  let fondoCss = '';
  if (opciones?.fondoDataUrl && (opciones.fondoConfig?.opacidad ?? 0) > 0) {
    const opacidad = Math.min(Math.max(opciones.fondoConfig?.opacidad ?? 0.08, 0), 1);
    const ajuste = opciones.fondoConfig?.ajuste === 'cover' ? 'cover' : 'contain';
    fondoCss = `
      .print-document {
        position: relative;
      }
      .print-document::before {
        content: '';
        position: absolute;
        top: 0; left: 0; right: 0; bottom: 0;
        background-image: url('${opciones.fondoDataUrl}');
        background-repeat: no-repeat;
        background-position: center;
        background-size: ${ajuste};
        opacity: ${opacidad};
        pointer-events: none;
        z-index: 0;
      }
      .print-document > * {
        position: relative;
        z-index: 1;
      }
    `;
  }

  const style = doc.createElement('style');
  style.textContent = `
    @page {
      size: A4;
      margin: ${margenes.superior}mm ${margenes.derecho}mm ${margenes.inferior}mm ${margenes.izquierdo}mm;
    }
    body {
      font: ${tamanoFuentePt}pt ${fontFamily};
      color: ${colorTexto};
      margin: 0;
      line-height: 1.35;
      -webkit-print-color-adjust: exact;
      print-color-adjust: exact;
    }
    h1 {
      font-size: ${tamanoFuentePt + 4}pt;
      font-weight: 800;
      color: ${colorEncabezado};
      text-align: left;
      margin: 0 0 4px;
      letter-spacing: 0.3px;
    }
    h2 {
      font-size: ${tamanoFuentePt + 1}pt;
      margin: 10px 0 6px;
      color: ${colorEncabezado};
    }
    h2.section-header {
      font-size: ${tamanoFuentePt}pt;
      font-weight: 700;
      color: ${colorEncabezado};
      background: #eaf0f8;
      border: ${grosorBordeMm}mm solid ${colorBorde};
      padding: 3.5px 8px;
      margin: 10px 0 6px;
      border-radius: 2px;
      text-transform: uppercase;
      letter-spacing: 0.2px;
    }
    h3 {
      font-size: ${tamanoFuentePt - 0.5}pt;
      margin: 8px 0 4px;
      color: ${colorEncabezado};
      font-weight: 700;
    }
    p { margin: 3px 0; }
    table { width: 100%; border-collapse: collapse; margin: 8px 0; }
    th, td { border: ${grosorBordeMm}mm solid ${colorBorde}; padding: 5px; text-align: left; vertical-align: top; }
    th { background: #eaf0f8; color: ${colorEncabezado}; }
    thead { display: table-header-group; }
    tr, .signature-block { break-inside: avoid; }

    /* Encabezado con identidad institucional */
    .document-header {
      border-bottom: ${grosorBordeMm * 1.5}mm solid ${colorDestacado};
      padding-bottom: 8px;
      margin-bottom: 12px;
    }
    .header-brand-row {
      display: flex;
      align-items: center;
      gap: 14px;
    }
    .header-logo-img {
      max-height: 48px;
      max-width: 90px;
      object-fit: contain;
    }
    .header-logo-badge {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      border-left: 3.5px solid ${colorDestacado};
      padding-left: 8px;
    }
    .badge-istpet {
      font-size: 13pt;
      font-weight: 800;
      color: ${colorDestacado};
      letter-spacing: 0.5px;
      line-height: 1;
    }
    .badge-sub {
      font-size: 6.5pt;
      font-weight: 600;
      color: #475569;
      letter-spacing: 0.2px;
      text-transform: uppercase;
      margin-top: 2px;
    }
    .header-titles {
      flex: 1;
    }
    .document-title {
      font-size: ${tamanoFuentePt + 3.5}pt;
      font-weight: 800;
      color: ${colorEncabezado};
      margin: 0;
      text-align: left;
      text-transform: uppercase;
      letter-spacing: 0.3px;
    }
    .document-subtitle {
      font-size: ${tamanoFuentePt - 0.5}pt;
      color: #475569;
      margin: 2px 0 0;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.2px;
    }

    /* Campos clave-valor */
    .document-fields {
      display: grid;
      grid-template-columns: 160px 1fr;
      gap: 3px 12px;
      margin-bottom: 6px;
    }
    .document-fields dt {
      font-weight: 700;
      color: ${colorTexto};
      font-size: ${tamanoFuentePt}pt;
    }
    .document-fields dd {
      margin: 0;
      border-bottom: 1px solid #cbd5e1;
      padding-bottom: 2px;
      font-size: ${tamanoFuentePt}pt;
    }
    .preserve-lines, dd {
      white-space: pre-wrap;
      overflow-wrap: anywhere;
    }

    /* Opciones y chips de selección */
    .document-options {
      display: flex;
      flex-wrap: wrap;
      gap: 4px 14px;
      margin: 4px 0 8px;
    }
    .document-options-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 4px 12px;
      margin: 4px 0 8px;
    }
    .doc-option-chip {
      display: inline-flex;
      align-items: baseline;
      gap: 5px;
      font-size: ${tamanoFuentePt - 0.5}pt;
    }
    .doc-check-mark {
      font-weight: bold;
      font-family: monospace;
      font-size: 1.05em;
    }
    .doc-check-mark.is-checked {
      color: ${colorDestacado};
    }

    /* Firmas */
    .signature-block {
      width: 100%;
      border-collapse: collapse;
      margin-top: 18px;
      break-inside: avoid;
    }
    .signature-block td {
      width: 33.33%;
      padding: 4px 10px;
      text-align: center;
      vertical-align: top;
      border: none;
      font-size: ${tamanoFuentePt - 1}pt;
    }
    .sig-title {
      display: block;
      font-weight: 700;
      color: ${colorEncabezado};
      margin-bottom: 4px;
      font-size: ${tamanoFuentePt - 0.5}pt;
      text-transform: uppercase;
    }
    .signature-line {
      height: 35px;
      border-bottom: 1px solid #64748b;
      margin: 0 auto 6px;
      width: 85%;
    }
    .signature-block p {
      margin: 2px 0;
      color: ${colorTexto};
      font-size: ${tamanoFuentePt - 1.5}pt;
      text-align: left;
      padding-left: 8%;
    }

    /* Nota final */
    .document-note {
      font-size: ${tamanoFuentePt - 1.5}pt;
      font-style: italic;
      color: #475569;
      margin-top: 14px;
      padding-top: 8px;
      border-top: 1px dashed #cbd5e1;
      line-height: 1.3;
    }

    [hidden] { display: none; }
    ${fondoCss}
    ${
      preview
        ? `
      @media screen {
        body {
          background: #e8ebef;
          padding: 24px;
          display: flex;
          justify-content: center;
        }
        .print-document {
          box-sizing: border-box;
          width: 210mm;
          min-height: 297mm;
          padding: ${margenes.superior}mm ${margenes.derecho}mm ${margenes.inferior}mm ${margenes.izquierdo}mm;
          margin: 0 auto;
          background: #ffffff;
          box-shadow: 0 4px 24px rgba(0, 0, 0, 0.15);
          border-radius: 2px;
        }
      }
    `
        : ''
    }
  `;
  doc.head.append(style);

  const copy = element.cloneNode(true) as HTMLElement;
  copy.removeAttribute('hidden');
  for (const img of copy.querySelectorAll('img')) {
    img.src = new URL(img.getAttribute('src') || '', document.baseURI).href;
  }
  doc.body.append(copy);
  return `<!doctype html>${doc.documentElement.outerHTML}`;
}

/** Imprime el documento en A4, con los controles del formulario fuera de la salida. */
export function printDocument(
  element: HTMLElement,
  title: string,
  opciones?: OpcionesEstiloDocumento,
): void {
  const frame = document.createElement('iframe');
  frame.title = title;
  frame.style.cssText = 'position:fixed;width:0;height:0;border:0;';
  frame.addEventListener(
    'load',
    () => {
      const target = frame.contentWindow;
      if (!target) {
        frame.remove();
        return;
      }
      target.addEventListener('afterprint', () => frame.remove(), { once: true });
      void target.document.fonts.ready.then(() => {
        target.focus();
        target.print();
      });
    },
    { once: true },
  );
  frame.srcdoc = documentHtml(element, title, false, opciones);
  document.body.append(frame);
}
