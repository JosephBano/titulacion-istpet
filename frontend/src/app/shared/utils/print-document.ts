/** Usa el mismo documento A4 para la vista previa y la impresión. */
export function documentHtml(element: HTMLElement, title: string, preview = false): string {
  const doc = document.implementation.createHTMLDocument(title);
  doc.documentElement.lang = 'es';
  const style = doc.createElement('style');
  style.textContent = `
    @page { size: A4; margin: 14mm; }
    body { font: 9pt 'Segoe UI', Arial, sans-serif; color: #171717; margin: 0; }
    h1 { font-size: 14pt; text-align: center; margin: 0 0 16px; }
    h2 { font-size: 10.5pt; margin: 12px 0 6px; }
    h3 { font-size: 10pt; margin: 10px 0 5px; }
    p { margin: 4px 0; }
    table { width: 100%; border-collapse: collapse; margin: 8px 0; }
    th, td { border: 1px solid #6b7280; padding: 6px; text-align: left; vertical-align: top; }
    th { background: #eaf0f8; }
    thead { display: table-header-group; }
    tr, .signature-block { break-inside: avoid; }
    .document-fields { display: grid; grid-template-columns: 145px 1fr; gap: 4px 10px; }
    .document-fields dt { font-weight: 600; }
    .document-fields dd { margin: 0; border-bottom: 1px solid #a3a3a3; padding-bottom: 3px; }
    .preserve-lines, dd { white-space: pre-wrap; overflow-wrap: anywhere; }
    .document-options { display: flex; flex-wrap: wrap; gap: 6px 16px; margin: 6px 0; }
    .signature-block { margin-top: 22px; }
    .signature-block td { width: 33.33%; font-size: 9pt; }
    .signature-line { height: 40px; border-bottom: 1px solid #666; margin-bottom: 6px; }
    .document-note { font-size: 8pt; margin-top: 12px; }
    .document-header { border-bottom: 2px solid #002855; padding-bottom: 12px; margin-bottom: 15px; }
    [hidden] { display: none; }
    ${
      preview
        ? `
      @media screen {
        body { background: #e8ebef; padding: 24px; }
        .print-document { box-sizing: border-box; width: 210mm; min-height: 297mm;
          padding: 14mm; margin: 0 auto; background: white; box-shadow: 0 4px 24px #0002; }
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
export function printDocument(element: HTMLElement, title: string): void {
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
  frame.srcdoc = documentHtml(element, title);
  document.body.append(frame);
}
