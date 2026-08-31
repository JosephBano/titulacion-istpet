# 05. Componentes UI Modulares y Reutilizables (Clean Architecture & Apple HIG)

Este documento detalla la arquitectura de **componentes presentacionales (Dumb Components)** creados para desacoplar las vistas monolíticas, normalizar el sistema de diseño y garantizar el cumplimiento de los principios de **Clean Architecture**, **Apple Human Interface Guidelines (HIG)** y **Microsoft Fluent Design 2**.

---

## 1. Principios de Modularización y Normalización

1. **Patrón Smart vs. Dumb Components**:
   - **Smart Container (`DashboardComponent`)**: Orquesta estados, consume servicios y maneja eventos.
   - **Dumb Components (`src/app/shared/components/`)**: Componentes puramente presentacionales sin dependencias de servicios HTTP o estado global. Reciben datos mediante `input()` y notifican cambios mediante `output()`.
2. **Cero Emojis & Iconografía Vectorial Nítida**:
   - Sustitución de caracteres emoji o decoraciones ambiguas por SVG vectoriales (`16px`/`20px`) y micro-indicadores CSS `.status-dot`.
3. **Control Flow Moderno de Angular 22**:
   - Uso exclusivo de directivas nativas `@if`, `@for`, `@switch` con `track` obligatorio en todas las iteraciones.

---

## 2. Catálogo de Componentes Reutilizables

```
frontend/src/app/shared/components/
├── index.ts                                # Barrel file exportador
├── network-banner/
│   ├── network-banner.component.ts         # Banner de red y alertas globales
│   ├── network-banner.component.html
│   └── network-banner.component.css
├── convocatoria-card/
│   ├── convocatoria-card.component.ts      # Card de estado de convocatoria y corte
│   ├── convocatoria-card.component.html
│   └── convocatoria-card.component.css
├── stepper/
│   ├── stepper.component.ts                # Stepper dinámico de 5 etapas
│   ├── stepper.component.html
│   └── stepper.component.css
├── kpi-card/
│   ├── kpi-card.component.ts               # Tarjeta métrica de KPIs
│   ├── kpi-card.component.html
│   └── kpi-card.component.css
├── dictamen-modal/
│   ├── dictamen-modal.component.ts         # Modal de dictamen de postulaciones
│   ├── dictamen-modal.component.html
│   └── dictamen-modal.component.css
└── apertura-periodo-modal/
    ├── apertura-periodo-modal.component.ts # Modal de apertura de período masivo
    ├── apertura-periodo-modal.component.html
    └── apertura-periodo-modal.component.css
```

---

## 3. Especificación Técnica de los Componentes

### 3.1 `NetworkBannerComponent` (`<app-network-banner />`)
Encapsula el estado de conectividad (online/offline/2G/3G) y los mensajes de feedback del sistema.
* **Inputs:**
  * `isOnline = input<boolean>(true)`
  * `isLowBandwidth = input<boolean>(false)`
  * `connectionType = input<string>('4g')`
  * `mensajeAccion = input<MensajeAlerta | null>(null)`

### 3.2 `ConvocatoriaCardComponent` (`<app-convocatoria-card />`)
Visualiza de forma destacada el estado del período lectivo y la cuenta regresiva de días restantes.
* **Inputs:**
  * `estaAbierta = input<boolean>(false)`
  * `detalle = input<string>()`
  * `mensaje = input<string>()`
  * `diasRestantes = input<number | null>(null)`

### 3.3 `StepperComponent` (`<app-stepper />`)
Renderiza la barra de progreso de 5 etapas académicas con checkmarks SVG y resaltado reactivo.
* **Inputs:**
  * `etapaActual = input<number>(1)`
  * `estadoNombre = input<string>()`
  * `modalidadNombre = input<string>()`
  * `tienePostulacion = input<boolean>(false)`

### 3.4 `KpiCardComponent` (`<app-kpi-card />`)
Componente modular para tarjetas de métricas en cuadrícula 8px.
* **Inputs:**
  * `eyebrow = input.required<string>()`
  * `value = input.required<string | number>()`
  * `subtext = input<string>()`
  * `valueFontSize = input<string>('1.75rem')`

### 3.5 `DictamenModalComponent` (`<app-dictamen-modal />`)
Modal accesible con backdrop blur para emitir aprobaciones, observaciones o rechazos.
* **Inputs:** `data = input<DictamenModalData | null>(null)`
* **Outputs:** `close = output<void>()`, `confirm = output<DictamenPostulacionRequest>()`

### 3.6 `AperturaPeriodoModalComponent` (`<app-apertura-periodo-modal />`)
Formulario modal para habilitar un nuevo período lectivo masivo y definir fechas de corte.
* **Inputs:** `visible = input<boolean>(false)`
* **Outputs:** `close = output<void>()`, `confirm = output<AperturarPeriodoRequest>()`

---

## 4. Cobertura de Pruebas Unitarias (Vitest)

Todos los componentes cuentan con tests unitarios automatizados en [`shared-components.spec.ts`](file:///C:/Users/MEGABLODFIX/Desktop/titulacion-istpet/frontend/src/app/shared/components/shared-components.spec.ts):
- Renderizado condicional en desconexión.
- Cálculo de estados y clases CSS en el stepper.
- Emisión tipada de eventos en modales de dictamen y apertura de convocatorias.
- **Resultado:** 100% de tests aprobados (18 / 18 en frontend).
