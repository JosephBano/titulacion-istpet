---
name: titulacion-ui-design
description: Estándar de diseño UI/UX basado en Microsoft Fluent Design 2 para Titulación ISTPET (ISTPET), integrado con los colores oficiales del Instituto (Azul Marino #1b2a4a y Dorado Académico #c59b27) y soporte nativo para Modo Claro y Modo Oscuro.
---

# Estándar de Diseño Microsoft Fluent Design 2 — ISTPET Titulación ISTPET

Esta habilidad define las reglas estéticas, de componentes y de interfaz para el sistema **Titulación ISTPET** del Instituto Tecnológico Superior Traversari (ISTPET). El lenguaje visual es **Microsoft Fluent Design 2** — el mismo que usa Microsoft 365, Outlook, Teams y Azure Portal — adaptado con la identidad institucional de ISTPET.

---

## 1. Mentalidad de Diseñador Senior — Reglas de Oro

El agente actúa como un **Diseñador UI/UX Senior con 10+ años de experiencia en enterprise software real**. No como una IA generando templates.

### 1.1 Reglas Absolutas de Comportamiento

- Toda la UI debe ser coherente con este estándar desde el primer componente. Cero improvisaciones.
- Cero parches visuales: bordes de colores aleatorios, sombras excesivas, gradientes sin propósito o mezclas de design systems.
- Si una pantalla no está especificada en esta skill, el agente **pregunta antes de implementar**, no improvisa.
- Los cambios de UI se razonan contra este documento antes de escribir código. Si hay conflicto entre una petición del usuario y este estándar, se notifica y se propone la solución correcta.

### 1.2 Prohibiciones Absolutas — Sin Excepcion

**Emojis:**
- Prohibido en todo el sistema: código HTML, componentes Angular, mensajes de error, labels, tooltips, comentarios de código visible al usuario, y documentación interna de la UI.
- Única excepción: si el propio contenido de datos del usuario contiene emojis (no los generamos, los mostramos tal cual llegan del backend).

**Iconos SVG — uso restringido:**
- Los iconos SVG se usan **solo cuando aportan claridad funcional**: acciones que sin icono serían ambiguas, estados de validación (error, exito), indicadores de estado en tablas.
- Prohibido usar iconos como decoración: un icono junto a un label de formulario que ya es claro, iconos en títulos de secciones, iconos en botones de texto que no los necesitan.
- Máximo **un icono por elemento de UI**. Un botón con icono + texto es suficiente. Dos iconos en el mismo elemento = error de diseño.
- En formularios: iconos de campo **solo** en campo de búsqueda o cuando la función del campo sea ambigua sin él.
- Tamaño: `16px` inline, `20px` navegación, `24px` acciones primarias. Nunca `32px+` salvo en estados vacíos.

**Lo que hace que un diseño parezca genérico de IA:**
- Hero sections con gradientes de colores llamativos sin propósito informativo.
- Cards con demasiado padding y muy poco contenido (efecto "template vacío").
- Iconos grandes y centrados en la parte superior de cada tarjeta.
- Texto con `font-weight: 800` y letras en mayúsculas en cada título.
- Sombras `box-shadow` pronunciadas en cada elemento.
- Bordes redondeados excesivos (`border-radius: 16px+`) en elementos de datos.
- Paleta de colores con múltiples colores vivos sin jerarquía clara.
- Animaciones de entrada (fade-in, slide-up) en cada componente al cargar.
- Líneas divisorias doradas o de colores entre cada sección.
- Badges o chips de estado con recuadros cápsula o fondos recubrientes tipo píldora (`background` recubriente). Los estados deben ser texto plano nítido sin cápsula de fondo.
- Uso de glassmorphism sin razón estructural.

---

## 2. Anatomía Visual — Pantalla de Autenticación / Login

### 2.1 Fondo de Pantalla Completa

Basado en el login real de Microsoft 365 observado en producción:

| Propiedad | Light Mode | Dark Mode |
|---|---|---|
| Color base canvas | `#e8eaf0` — azul-lavanda suave | `#0d1117` — negro azulado profundo |
| Formas abstractas | Rombos/paralelogramos en `rgba(255,255,255,0.65)` — sin `border-radius`, sin `filter:blur` | Rombos en `rgba(27,42,74,0.45)` |
| Efecto | Formas nítidas superpuestas como capas de papel cortadas en diagonal | Identico |
| Patrón adicional | Ninguno. Fondo completamente limpio | Ninguno |

> Las formas NO tienen blur. Son polígonos nítidos con opacidad. El efecto difuminado percibido viene de la superposición, no de CSS.

**CSS correcto:**

```css
.fluent-bg-shape {
  position: absolute;
  background: rgba(255, 255, 255, 0.65);
  border-radius: 0;     /* Rombos nítidos sin redondeo */
}

.shape-1 { width: 480px; height: 480px; top: -180px; left: -60px;  transform: rotate(22deg);  opacity: 0.7; }
.shape-2 { width: 320px; height: 320px; bottom: -80px; right: -60px; transform: rotate(-18deg); opacity: 0.55; }
.shape-3 { width: 200px; height: 200px; top: 5%;  right: 10%;    transform: rotate(36deg);  opacity: 0.45; }
.shape-4 { width: 560px; height: 560px; top: 15%; right: -220px; transform: rotate(-12deg); opacity: 0.35; }
.shape-5 { width: 160px; height: 160px; bottom: 18%; left: 8%;   transform: rotate(28deg);  opacity: 0.4; }
```

### 2.2 Tarjeta de Formulario / Card

```css
.fluent-card {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.07);
  border-radius: 4px;
  padding: 44px 44px 36px;
  max-width: 440px;
  width: 100%;
  box-shadow: 0 2px 4px rgba(0,0,0,0.04), 0 4px 16px rgba(0,0,0,0.08);
}
```

Dark Mode:
```css
[data-theme='dark'] .fluent-card {
  background: #1f2937;
  border-color: rgba(255, 255, 255, 0.08);
  box-shadow: 0 4px 12px rgba(0,0,0,0.35), 0 12px 40px rgba(0,0,0,0.4);
}
```

### 2.3 Logo Institucional — Cuatro Cuadros

```html
<div class="istpet-logo">
  <div class="logo-grid">
    <span class="sq sq-gold-dark"></span>
    <span class="sq sq-gold"></span>
    <span class="sq sq-navy"></span>
    <span class="sq sq-navy-light"></span>
  </div>
  <span class="logo-ist">IST</span><span class="logo-pet">PET</span>
</div>
```

```css
.logo-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2.5px;
  width: 21px;
  height: 21px;
}
.logo-grid .sq { display: block; border-radius: 1px; }
.sq-gold-dark  { background: #b08a20; }
.sq-gold       { background: #c59b27; }
.sq-navy       { background: #1b2a4a; }
.sq-navy-light { background: #2b4070; }
```

### 2.4 Footer Legal — Esquina Inferior Derecha

```css
.fluent-legal {
  position: fixed;
  bottom: 12px;
  right: 16px;     /* DERECHA — como Microsoft 365 real */
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.6875rem;
  color: #605e5c;
  white-space: nowrap;
  z-index: 1;
}
```

---

## 3. Tipografia

```css
font-family: 'Segoe UI', 'Plus Jakarta Sans', system-ui, -apple-system, sans-serif;
```

| Elemento | Size | Weight | Color Light | Color Dark |
|---|---|---|---|---|
| Heading principal H1 | `1.5rem` | `600` | `#1a1a1a` | `#f9fafb` |
| Eyebrow / sección | `0.6875rem` | `600` | `#605e5c` | `#9ca3af` |
| Label de campo | `0.8125rem` | `600` | `#323130` | `#d1d5db` |
| Cuerpo / párrafo | `0.875rem` | `400` | `#323130` | `#e5e7eb` |
| Texto secundario | `0.875rem` | `400` | `#605e5c` | `#9ca3af` |
| Mensaje de error | `0.8125rem` | `400` | `#a4262c` | `#fca5a5` |
| Footer legal | `0.6875rem` | `400` | `#605e5c` | `#6b7280` |

**Reglas tipográficas adicionales:**
- `font-weight: 800` está prohibido en la UI. El máximo permitido es `700` y solo en el logo. Los títulos van en `600`.
- `text-transform: uppercase` permitido únicamente en eyebrows (`0.6875rem`, `letter-spacing: 0.05em`). Nunca en títulos principales ni botones.
- `letter-spacing` negativo solo en headings grandes (`-0.012em`). Nunca en cuerpo de texto.
- No usar más de dos pesos de fuente en una misma pantalla: `400` y `600`.

---

## 4. Sistema de Espaciado — Grid de 8px

Toda separación, padding y margen debe ser múltiplo de `8px`:

| Token | Valor | Uso |
|---|---|---|
| `--space-1` | `4px` | Gap mínimo entre elementos inline |
| `--space-2` | `8px` | Padding de inputs, gap entre label e input |
| `--space-3` | `12px` | Padding interno de badges, gap entre campos |
| `--space-4` | `16px` | Padding de botones, separación entre secciones menores |
| `--space-5` | `20px` | Separación estándar entre grupos de campos |
| `--space-6` | `24px` | Margen entre secciones de un card |
| `--space-8` | `32px` | Padding lateral de cards pequeños |
| `--space-10` | `40px` | Separación entre secciones de página |
| `--space-11` | `44px` | Padding interior de la tarjeta de login |

No usar valores arbitrarios como `13px`, `17px`, `22px`. Si no es múltiplo de `4px`, no se usa.

---

## 5. Componentes — Especificaciones Completas

### 5.1 Inputs

```css
.fluent-input {
  width: 100%;
  height: 32px;
  padding: 0 8px;
  font-size: 0.875rem;
  font-family: inherit;
  color: #323130;
  background: #ffffff;
  border: 1px solid #8a8886;
  border-radius: 2px;
  outline: none;
  transition: border-color 83ms ease;
}

.fluent-input:hover  { border-color: #323130; }

/* Tecnica Microsoft: borde inferior se engrosa en foco */
.fluent-input:focus {
  border-color: #1b2a4a;
  border-bottom: 2px solid #1b2a4a;
  outline: none;
}
```

Dark Mode:
```css
[data-theme='dark'] .fluent-input {
  background: #111827;
  border-color: #4a5568;
  color: #f9fafb;
}
[data-theme='dark'] .fluent-input:hover  { border-color: #6b7280; }
[data-theme='dark'] .fluent-input:focus  {
  border-color: #c59b27;
  border-bottom: 2px solid #c59b27;
}
```

### 5.2 Botones — Dos Variantes Según Contexto

#### Boton Rectangular (formularios, tablas, dialogs — UI interna)

```css
.fluent-btn-primary {
  height: 32px;
  padding: 0 16px;
  background: #1b2a4a;
  color: #ffffff;
  border: none;
  border-radius: 2px;
  font-size: 0.875rem;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  transition: background 167ms ease;
}
.fluent-btn-primary:hover:not(:disabled)  { background: #253d6b; }
.fluent-btn-primary:active:not(:disabled) { background: #121d34; }
.fluent-btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
```

#### Boton Pill (CTAs de pantallas principales — no UI interna)

```css
.fluent-btn-pill {
  height: 40px;
  padding: 0 24px;
  background: #1b2a4a;
  color: #ffffff;
  border: none;
  border-radius: 999px;
  font-size: 0.9375rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 167ms ease;
}
```

#### Boton Secundario / Outline

```css
.fluent-btn-secondary {
  height: 32px;
  padding: 0 16px;
  background: transparent;
  color: #1b2a4a;
  border: 1px solid #8a8886;
  border-radius: 2px;
  font-size: 0.875rem;
  font-weight: 400;
  cursor: pointer;
  transition: background 83ms ease, border-color 83ms ease;
}
.fluent-btn-secondary:hover { background: rgba(0,0,0,0.04); border-color: #323130; }
```

Dark Mode:
```css
[data-theme='dark'] .fluent-btn-primary {
  background: #c59b27;
  color: #0d1117;
}
[data-theme='dark'] .fluent-btn-primary:hover:not(:disabled) { background: #d4af37; }
```

### 5.3 Tablas — Fluent Data Grid

```css
.fluent-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.8125rem;
  font-family: inherit;
}

.fluent-table thead th {
  padding: 8px 12px;
  background: var(--bg-surface-alt);
  border-bottom: 2px solid var(--border-default);
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-secondary);
  text-align: left;
  white-space: nowrap;
  user-select: none;
}

.fluent-table tbody td {
  padding: 8px 12px;
  border-bottom: 1px solid var(--border-subtle);
  color: var(--text-primary);
  vertical-align: middle;
}

.fluent-table tbody tr:hover td { background: rgba(0,0,0,0.03); }

.fluent-table tbody tr.selected td {
  background: rgba(27,42,74,0.06);
  border-left: 2px solid #1b2a4a;
}
```

### 5.4 Badges / Estado

Sin fondos tipo cápsula o pill recubriendo el texto. Los indicadores de estado se muestran con texto limpio nítido (y punto indicador opcional), sin recuadros ni cápsulas de color de fondo:

```css
.fluent-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: transparent;
  padding: 0;
  font-size: 0.75rem;
  font-weight: 600;
  letter-spacing: 0.02em;
  white-space: nowrap;
}

.fluent-badge--success { background: transparent; color: #107c10; }
.fluent-badge--error   { background: transparent; color: #a4262c; }
.fluent-badge--warning { background: transparent; color: #b78103; }
.fluent-badge--neutral { background: transparent; color: var(--text-muted); border: none; }
.fluent-badge--info    { background: transparent; color: #0078d4; }
```

### 5.5 Navegacion de Tabs — Subrayado

```css
.fluent-tabs {
  display: flex;
  gap: 0;
  border-bottom: 1px solid var(--border-subtle);
}

.fluent-tab-item {
  padding: 10px 16px;
  font-size: 0.875rem;
  font-weight: 400;
  color: var(--text-muted);
  cursor: pointer;
  border-bottom: 2px solid transparent;
  margin-bottom: -1px;
  transition: color 83ms ease, border-color 83ms ease;
  user-select: none;
}

.fluent-tab-item:hover { color: var(--text-primary); }

.fluent-tab-item.active {
  color: var(--istpet-navy);
  font-weight: 600;
  border-bottom-color: var(--istpet-navy);
}
```

---

## 6. Shell de la Aplicacion — Microsoft 365 / Outlook Layout

Dos niveles de navegacion lateral, como en Outlook web:

```
+----------------------------------------------------------+
| [ISTPET] Titulación ISTPET     [---- Busqueda ----]    [notif avatar]| <- Topbar 48px navy
+----+----------+------------------------------------------+
|    |          |                                          |
| 48 | Sidebar  |  Area de contenido principal             |
| px | Secundaria|  bg: var(--bg-canvas) = #e8eaf0        |
|    | 220px    |                                          |
|Icon| bg:#fff  |  Cards, tablas, formularios              |
|Rail| border-r |                                          |
|    |          |                                          |
+----+----------+------------------------------------------+
```

### Topbar (48px)
- Background: `#1b2a4a`
- Logo: cuatro cuadros + "ISTPET Titulación ISTPET" en blanco
- Busqueda centrada: input fondo `rgba(255,255,255,0.12)`, borde `rgba(255,255,255,0.2)`
- Iconos derecha: notificaciones, configuracion, avatar en `rgba(255,255,255,0.85)`

### Icon Rail (48px)
- Background: `#12213a` (mas oscuro que topbar)
- Iconos: `20px`, color `rgba(255,255,255,0.7)`, activo `#ffffff`
- Item activo: borde izquierdo `3px solid #c59b27`
- Sin texto — solo iconos

### Sidebar Secundaria (220px)
- Background: `#ffffff` (Light) / `var(--bg-surface)` (Dark)
- Border right: `1px solid var(--border-subtle)`
- Encabezado de seccion: `0.6875rem`, `600`, color `var(--text-muted)`, uppercase, `letter-spacing: 0.05em`
- Item activo: `background: var(--istpet-gold-subtle); color: var(--istpet-navy); font-weight: 600`

### Contenido Principal
- Background: `var(--bg-canvas)` — `#e8eaf0` (Light) / `#0d1117` (Dark)
- Cards y paneles internos: `var(--bg-surface)` = `#ffffff`

---

## 7. Paleta Completa de Design Tokens

```css
:root {
  font-family: 'Segoe UI', 'Plus Jakarta Sans', system-ui, -apple-system, sans-serif;

  /* Marca ISTPET */
  --istpet-navy:         #1b2a4a;
  --istpet-navy-hover:   #253d6b;
  --istpet-navy-dark:    #121d34;
  --istpet-navy-rail:    #12213a;
  --istpet-gold:         #c59b27;
  --istpet-gold-hover:   #d4af37;
  --istpet-gold-subtle:  rgba(197, 155, 39, 0.12);

  /* Fondos */
  --bg-canvas:           #e8eaf0;
  --bg-surface:          #ffffff;
  --bg-surface-alt:      #faf9f8;
  --bg-shape:            rgba(255, 255, 255, 0.65);

  /* Texto */
  --text-primary:        #1a1a1a;
  --text-secondary:      #323130;
  --text-muted:          #605e5c;
  --text-disabled:       #a19f9d;
  --text-on-accent:      #ffffff;

  /* Bordes */
  --border-default:      #8a8886;
  --border-subtle:       rgba(0, 0, 0, 0.08);
  --border-strong:       #323130;

  /* Estados */
  --color-error:         #a4262c;   --color-error-bg:   #fde7e9;
  --color-success:       #107c10;   --color-success-bg: #dff6dd;
  --color-warning-text:  #7d5a00;   --color-warning-bg: #fff4ce;
  --color-info:          #0078d4;   --color-info-bg:    #e6f2fb;

  /* Radios */
  --radius-input:  2px;
  --radius-card:   4px;
  --radius-panel:  8px;
  --radius-badge:  12px;
  --radius-pill:   999px;

  /* Espaciado */
  --space-1:   4px;
  --space-2:   8px;
  --space-3:   12px;
  --space-4:   16px;
  --space-5:   20px;
  --space-6:   24px;
  --space-8:   32px;
  --space-10:  40px;
  --space-11:  44px;

  /* Motion */
  --ease-fluent:     cubic-bezier(0.1, 0.9, 0.2, 1);
  --duration-fast:   83ms;
  --duration-normal: 167ms;
  --duration-slow:   333ms;

  /* Shell */
  --topbar-height:           48px;
  --icon-rail-width:         48px;
  --sidebar-width:           220px;
  --sidebar-width-collapsed: 48px;
}

[data-theme='dark'] {
  --bg-canvas:           #0d1117;
  --bg-surface:          #1f2937;
  --bg-surface-alt:      #111827;
  --bg-shape:            rgba(27, 42, 74, 0.45);
  --text-primary:        #f9fafb;
  --text-secondary:      #e5e7eb;
  --text-muted:          #9ca3af;
  --text-disabled:       #6b7280;
  --border-default:      #4a5568;
  --border-subtle:       rgba(255, 255, 255, 0.08);
  --border-strong:       #9ca3af;
  --color-error:         #fca5a5;   --color-error-bg:   #2d1515;
  --color-success:       #6ee7b7;   --color-success-bg: #052e16;
  --color-warning-text:  #fde68a;   --color-warning-bg: #2d1b00;
  --color-info:          #60a5fa;   --color-info-bg:    #1e3a5f;
}
```

---

## 8. Animaciones — Fluent Motion

Animaciones rapidas y funcionales. Nunca decorativas.

```css
--ease-fluent: cubic-bezier(0.1, 0.9, 0.2, 1);
--duration-fast:   83ms;
--duration-normal: 167ms;
--duration-slow:   333ms;
```

- Nunca `animation-duration > 333ms`.
- Transiciones de color y borde: `var(--duration-fast) ease`.
- No `transform: scale()` en botones. Solo cambio de `background`.
- Modales y drawers: `transform: translateX/Y` + `opacity` simultaneos en `var(--duration-slow)`.
- Tablas sin animaciones en filas. El hover es instantaneo o `83ms`.
- Prohibido: fade-in, slide-up, bounce en elementos al cargar la pagina.

---

## 9. Iconografia — Fluent UI Icons

- Libreria: Fluent UI System Icons — SVG inline.
- Repositorio: https://github.com/microsoft/fluentui-system-icons
- Tamanos: `16px` (inline texto), `20px` (navegacion/toolbar), `24px` (acciones primarias).
- Color: `currentColor` siempre.
- No usar FontAwesome, Material Icons, Bootstrap Icons ni Heroicons.

**Cuando usar iconos:**
- Acciones de toolbar donde el label no cabe: Guardar, Exportar, Filtrar.
- Estados de validacion en inputs: error, exito.
- Navegacion lateral (icon rail) donde el espacio es limitado.
- Boton de cierre de modal/drawer.
- Indicadores de expansion/colapso.

**Cuando NO usar iconos:**
- Junto a labels de formularios que ya son descriptivos.
- En titulos de seccion o de pagina.
- Como decoracion dentro de cards de datos.
- En mensajes de error si el contexto ya indica el error.
- En botones de accion principal dentro de formularios (el texto es suficiente).

---

## 10. Reglas Anti-Genericas — Lo que Distingue Este Sistema

Estas reglas evitan que el resultado parezca un template de IA generico:

**Densidad de informacion correcta:**
- Las tablas van densas: `padding: 8px 12px` en celdas, no `20px+`. Un ERP academico muestra datos, no espacio en blanco.
- Los formularios de gestion van en grids de 2 columnas para datos relacionados (cedula + nombre, fecha + estado).
- No usar cards para envolver cada fila de un formulario — eso es decoracion, no estructura.

**Jerarquia visual sin colores:**
- La jerarquia se construye con tamano tipografico y peso, no con colores de fondo diferente en cada seccion.
- Los separadores horizontales se usan solo entre secciones logicamente distintas, no entre cada campo.

**Precision institucional:**
- Los textos de la UI van en espanol formal neutro, sin contracciones ni coloquialismos.
- Nombres de modulos: "Gestion de Titulacion", "Actas de Grado", "Nomina de Graduados" — no "Dashboard", "Home", "Overview".
- Los estados academicos usan la terminologia oficial: "Aprobado", "Pendiente de Revision", "Reprobado", "En Proceso" — no "Active", "Pending", "Done".

**Consistencia de componentes:**
- Todos los formularios del sistema usan los mismos inputs, botones y labels definidos aqui.
- Todos los modales siguen el mismo patron: header con titulo, cuerpo, footer con acciones alineadas a la derecha.
- Todos los estados vacios siguen el mismo patron: icono Fluent `48px` en gris, texto descriptivo `0.875rem`, boton de accion si aplica.

---

## 11. Anti-patrones Prohibidos — Lista Completa

| Prohibido | Correcto |
|---|---|
| Emojis en cualquier parte de la UI | Texto plano o icono Fluent funcional |
| Iconos SVG decorativos sin proposito | Solo iconos funcionales y necesarios |
| `border-radius > 4px` en cards | Cards `4px`, inputs `2px` |
| `border-radius: 0` en pill CTAs | CTAs de marketing usan `border-radius: 999px` |
| Gradientes en botones | Color solido plano |
| `filter: blur()` en formas de fondo | Formas nítidas con opacidad |
| `box-shadow` con colores de marca | Solo sombras neutras grises |
| `font-weight: 800` en la UI | Maximo `600` en titulos, `700` solo en el logo |
| `text-transform: uppercase` en botones o titulos | Solo en eyebrows de seccion |
| Multiples familias tipograficas | Solo Segoe UI / Plus Jakarta Sans |
| FontAwesome / Material Icons | SVG inline Fluent Icons |
| `animation-duration > 333ms` | 83ms-167ms para la mayoria |
| Fade-in / slide-up al cargar pagina | Sin animaciones de entrada por defecto |
| Inputs con iconos internos decorativos | Sin iconos en inputs salvo busqueda |
| Colores de marca en fondos de pagina | Solo en topbar, icon rail, botones y acentos |
| Footer legal centrado en login | Esquina inferior derecha |
| Un solo nivel de sidebar | Icon rail 48px + sidebar secundaria 220px |
| Padding de celdas de tabla mayor a 12px | `padding: 8px 12px` estandar |
| Cards con menos del 60% de superficie ocupada | Densidad de contenido apropiada |
| Valores de espaciado no multiplo de 4px | Solo multiplos de 4px o 8px |

---

## 12. Referencias

- [CSS Tokens — paleta_tokens.css](file:///c:/Users/DESARROLLADOR/Downloads/titan/.agents/skills/titulacion-ui-design/references/paleta_tokens.css)
- [Componentes de ejemplo — componentes_ejemplos.md](file:///c:/Users/DESARROLLADOR/Downloads/titan/.agents/skills/titulacion-ui-design/references/componentes_ejemplos.md)
- Fluent UI System Icons: https://github.com/microsoft/fluentui-system-icons
- Microsoft Fluent Design 2: https://learn.microsoft.com/design/


