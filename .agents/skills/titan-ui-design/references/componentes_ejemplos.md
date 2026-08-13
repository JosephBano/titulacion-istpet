# Referencia de Componentes para Titán ERP System

Ejemplos de componentes alineados estrictamente al estándar **ERP Limpio / Holded / Xero**.

---

## 1. Tabla de Alta Densidad para Titulación (`titan-table`)

```html
<div class="table-wrapper">
  <table class="erp-table">
    <thead>
      <tr>
        <th style="width: 40px;"><input type="checkbox" /></th>
        <th>Estudiante / Identificación</th>
        <th>Carrera</th>
        <th>Cohorte</th>
        <th class="text-right">Promedio General</th>
        <th>Estado Requisitos</th>
        <th class="text-right">Acciones</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><input type="checkbox" /></td>
        <td>
          <div class="student-name font-semibold">Doicela Jorge</div>
          <div class="student-id font-tabular text-muted">1723456789</div>
        </td>
        <td>Desarrollo de Software</td>
        <td>2026-T1</td>
        <td class="text-right font-tabular font-semibold">9.45</td>
        <td><span class="badge badge-active">APTO</span></td>
        <td class="text-right">
          <button class="btn-cell">Generar Acta</button>
        </td>
      </tr>
    </tbody>
  </table>
</div>
```

---

## 2. Panel Contable / Resumen de Métricas Akademikas

```html
<aside class="academic-summary-panel">
  <div class="panel-header">
    <h3>Resumen de Titulación</h3>
  </div>
  <div class="summary-row">
    <span class="label">Postulaciones Registradas</span>
    <span class="value font-tabular">154</span>
  </div>
  <div class="summary-row">
    <span class="label">Aprobados para Acta</span>
    <span class="value font-tabular">112</span>
  </div>
  <div class="summary-row">
    <span class="label">Pendientes por Documentación</span>
    <span class="value font-tabular">42</span>
  </div>
</aside>
```
