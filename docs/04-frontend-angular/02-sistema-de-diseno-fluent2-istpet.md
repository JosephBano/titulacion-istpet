# Sistema de Diseño Fluent Design 2 e Identidad ISTPET — Sistema Titán

## 1. Estándar Visual Microsoft Fluent Design 2

El cliente web de Titán implementa el sistema de diseño **Microsoft Fluent Design 2**, adaptado con la paleta de colores oficiales e identidad corporativa del **Instituto Tecnológico Superior Traversari (ISTPET)**.

---

## 2. Paleta de Colores y Tokens Institucionales

```css
:root {
  /* Colores Principales Institucionales ISTPET */
  --istpet-navy-primary: #002855;      /* Azul Marino Oficial */
  --istpet-navy-hover: #001f42;
  --istpet-gold-accent: #FCB813;       /* Amarillo Institucional Accent */
  --istpet-gold-hover: #e0a310;

  /* Neutros Fluent Design 2 */
  --fluent-bg-canvas: #F8FAFC;
  --fluent-card-bg: #FFFFFF;
  --fluent-text-primary: #0F172A;
  --fluent-text-secondary: #475569;
  --fluent-border: #E2E8F0;

  /* Colores de Estado */
  --fluent-success: #10B981;
  --fluent-warning: #F59E0B;
  --fluent-error: #EF4444;

  /* Elevación y Sombras */
  --fluent-shadow-card: 0 4px 14px 0 rgba(0, 40, 85, 0.08);
  --fluent-radius: 8px;
}
```

---

## 3. Tipografía y Estructura Jerárquica

- **Familia Tipográfica:** Segoe UI / Inter (`font-family: 'Segoe UI', system-ui, -apple-system, sans-serif`).
- **Encabezados:** Peso semibold (`font-weight: 600`), color `--istpet-navy-primary`.
- **Cuerpo de Texto:** Peso regular (`font-weight: 400`), `--fluent-text-primary`.

---

## 4. Lineamientos de UX y Componentes

1. **Tarjetas (Cards):** Fondo blanco con borde sutil `#E2E8F0`, radio de borde de 8px y sombra difusa `--fluent-shadow-card`.
2. **Botones Primarios:** Fondo `--istpet-navy-primary`, texto blanco, efecto de elevación al pasar el cursor (`hover`) y transición suave de 200ms.
3. **Indicadores de Estado (Badges):** Etiquetas redondeadas para estados de postulación (`PENDIENTE`: Amarillo `#F59E0B`, `APROBADA`: Verde `#10B981`, `RECHAZADA`: Rojo `#EF4444`).
