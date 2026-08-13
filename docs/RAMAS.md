# Estrategia de ramas y protecciones

## Modelo

```
main      ●───────────────────────●──────────────►   produccion, solo el dueño
           \                     /
develop     ●──●──●──●──●──●──●─●                     integracion del equipo
             \    /    \    /
feature/x     ●──●       │                            rama personal
feature/y                ●──●                         rama personal
```

- **`main`** — rama por defecto. Refleja lo que esta en produccion. Solo recibe PRs
  desde `develop` (o `hotfix/*`), y solo los abre y aprueba el dueño del repositorio.
- **`develop`** — donde se integra el trabajo del equipo. Todo desarrollador abre PRs
  aqui desde su rama.
- **`feature/*`, `fix/*`, ...** — ramas personales, sin proteccion. Se borran al mergear.

## Protecciones configuradas

### `main`

| Regla | Valor | Por que |
|---|---|---|
| Push directo | bloqueado | ningun commit entra sin revision |
| PR requerido | si | |
| Aprobaciones | 1, **de un code owner** | `.github/CODEOWNERS` asigna todo a `@JorgeDoicela`, asi que ningun PR entra a `main` sin el visto bueno del dueño |
| Aprobaciones obsoletas | se descartan al hacer push | se revisa lo que se mergea, no una version anterior |
| Checks requeridos | los 8, en modo **strict** | |
| Resolucion de conversaciones | obligatoria | ningun comentario queda sin cerrar |
| Force push / borrado | bloqueados | el historial de produccion es inmutable |

### `develop`

| Regla | Valor | Por que |
|---|---|---|
| Push directo | bloqueado | |
| PR requerido | si | |
| Aprobaciones | 1 | revision entre pares, sin cuello de botella en el dueño |
| Aprobaciones obsoletas | se descartan al hacer push | |
| Checks requeridos | los 8, en modo **strict** | |
| Force push / borrado | bloqueados | nadie reescribe la historia compartida |

## Como se garantiza que no entra codigo roto ni con conflictos

Son cuatro mecanismos distintos, y hacen falta los cuatro:

1. **GitHub bloquea nativamente los PRs con conflictos.** Si `mergeable` es `false`,
   el boton de merge se deshabilita. No hay forma de saltarlo desde la interfaz.

2. **El check `Sin conflictos de merge`** consulta ese mismo estado por API y lo
   convierte en un check rojo visible. Sin el, un PR conflictivo simplemente muestra
   un boton gris sin explicar por que.

3. **`strict: true` en los checks requeridos** ("Require branches to be up to date
   before merging") es la pieza central. Sin esto, dos PRs pueden estar verdes por
   separado y romper `develop` al juntarse: cada uno se testeo contra un `develop`
   anterior. Con `strict`, la rama debe estar al dia y los checks corren sobre el
   resultado real de la fusion.

4. **El evento `pull_request` compila el merge commit, no tu rama.** GitHub Actions
   hace checkout de `refs/pull/N/merge`, o sea el resultado de fusionar tu rama con
   la base. Lo que el CI aprueba es exactamente lo que va a quedar en `develop`.

Ademas, `Sin marcadores de conflicto en el codigo` cubre el caso de un conflicto
"resuelto" a mano dejando los `<<<<<<<` adentro del archivo: en algunos lenguajes eso
compila y pasa desapercibido.

## Aplicar o reconstruir las protecciones

Las reglas viven en `scripts/proteger-ramas.sh`, versionado. Reejecutarlo es idempotente:

```bash
./scripts/proteger-ramas.sh
```

Requiere `gh` autenticado con permisos de admin sobre el repositorio.

## Limitacion conocida en repos personales

"Restringir quien puede pushear a esta rama" solo existe en repositorios de
**organizacion**. En un repo personal no se puede listar explicitamente quien puede
mergear a `main`.

El equivalente funcional que si aplica: **`require_code_owner_reviews` en `main`**.
Como `CODEOWNERS` asigna `*` a `@JorgeDoicela`, ningun PR a `main` puede mergearse sin
su aprobacion. Un colaborador podria abrir el PR, pero no cerrarlo.

Si mas adelante el proyecto se mueve a una organizacion, agregar en
`scripts/proteger-ramas.sh` el bloque `restrictions` para `main`.

