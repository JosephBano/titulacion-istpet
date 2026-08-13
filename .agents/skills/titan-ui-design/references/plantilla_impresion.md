# Plantilla de Impresión de Actas y Certificados de Titulación

Plantilla de impresión HTML/CSS sobria y oficial para reportes, actas de grado y certificados en PDF.

```css
@media print {
  @page {
    size: A4;
    margin: 15mm;
  }

  body {
    font-family: 'Inter', -apple-system, sans-serif;
    font-size: 11px;
    color: #111827 !important;
    background: #ffffff !important;
  }

  .no-print, nav, header, footer, .sidebar, .btn {
    display: none !important;
  }

  .print-table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 15px;
  }

  .print-table th {
    background: #f3f4f6 !important;
    border-bottom: 2px solid #e5e7eb;
    padding: 6px 8px;
    font-size: 10px;
    text-transform: uppercase;
  }

  .print-table td {
    border-bottom: 1px solid #e5e7eb;
    padding: 6px 8px;
  }
}
```
