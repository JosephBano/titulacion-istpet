# Guia de contribucion

## Reglas cortas

1. Nadie hace push directo a `main` ni a `develop`. Todo entra por PR.
2. Cada desarrollador trabaja en su propia rama y abre PR contra `develop`.
3. Solo el dueño del repositorio abre y aprueba PRs de `develop` a `main`.
4. Un PR con conflictos o con checks en rojo no se puede mergear. Es una regla del
   servidor, no un acuerdo de equipo: GitHub bloquea el boton.

## Ciclo de trabajo

```bash
# 1. Partir siempre de develop actualizado
git checkout develop
git pull origin develop

# 2. Rama con nombre descriptivo
git checkout -b feature/registro-estudiantes

# 3. Trabajar y commitear (Conventional Commits)
git commit -m "feat(backend): endpoint para registrar estudiantes"

# 4. Antes de abrir el PR: reincorporar develop
git fetch origin
git rebase origin/develop        # aqui aparecen los conflictos, no en el PR

# 5. Verificar en local lo mismo que verifica el CI
cd backend  && dotnet build -warnaserror && dotnet test && dotnet format --verify-no-changes
cd frontend && npm run lint && npm test -- --watch=false && npm run build

# 6. Subir y abrir el PR
git push -u origin feature/registro-estudiantes
gh pr create --base develop --fill
```

## Nombres de rama

| Prefijo | Para que |
|---|---|
| `feature/` | funcionalidad nueva |
| `fix/` | correccion de bug |
| `refactor/` | reestructuracion sin cambio de comportamiento |
| `docs/` | solo documentacion |
| `chore/` | dependencias, tooling, configuracion |
| `release/` | preparacion de un release hacia `main` |
| `hotfix/` | correccion urgente sobre `main` |

## Commits y titulos de PR

Formato [Conventional Commits](https://www.conventionalcommits.org/):

```
<tipo>(<alcance opcional>): <descripcion en imperativo>
```

Tipos: `feat` `fix` `docs` `style` `refactor` `perf` `test` `build` `ci` `chore` `revert`.
Alcances habituales: `backend`, `frontend`, `ci`, `db`, `docs`.

El check **PR Gate / titulo** valida el titulo del PR contra este patron (se omite
si el PR esta en borrador), porque el titulo del PR es lo que termina en el
historial de `develop` al hacer squash merge.

## Que verifica el CI

| Check | Que hace | Bloquea merge |
|---|---|---|
| `Build y tests (.NET 8)` | restore, build con `-warnaserror`, `dotnet format --verify-no-changes`, tests | si |
| `Publish (verifica que despliega)` | `dotnet publish` de la WebApi | si |
| `Lint, tests y build (Angular)` | `npm ci`, lint, Prettier, vitest, build de produccion | si |
| `GitGuardian (ggshield)` | escaneo de secretos sobre todo el historial del PR | si |
| `Archivos que nunca deben commitearse` | busca appsettings/env/llaves versionados | si |
| `Sin conflictos de merge` | consulta la mergeabilidad a la API de GitHub | si |
| `Sin marcadores de conflicto en el codigo` | busca `<<<<<<<` en los archivos tocados | si |
| `Titulo con formato Conventional Commits` | valida el titulo del PR | si |

Los checks marcados como obligatorios son **required status checks** y estan en modo **strict**: si `develop` avanza
mientras tu PR espera, tenes que actualizar la rama y los checks corren de nuevo sobre
el resultado real del merge. Esto es lo que evita que dos PRs verdes por separado
rompan `develop` al juntarse.

## Resolver conflictos

```bash
git fetch origin
git rebase origin/develop
# resolver archivo por archivo, sin dejar marcadores <<<<<<< ======= >>>>>>>
git add <archivo>
git rebase --continue
git push --force-with-lease      # nunca --force a secas
```

`--force-with-lease` aborta si alguien mas subio commits a tu rama mientras trabajabas.
`--force` los borraria sin avisar.

## Secretos

- Nunca commitear `appsettings.json`, `.env`, `environment.prod.ts`, llaves ni certificados.
- Configuracion nueva: agregar la clave al `*.example.*` con un valor `CAMBIAME`, y el
  valor real por variable de entorno o `dotnet user-secrets`.
- Si te filtraste una credencial: **rotala inmediatamente** y avisa al dueño del repo.
  Limpiar el historial es el segundo paso, no el primero.

## Revision de codigo

- PRs chicos. Uno que toca 40 archivos no se revisa, se aprueba a ciegas.
- Un PR necesita **1 aprobacion** para entrar a `develop`.
- Un push nuevo invalida las aprobaciones previas: se revisa lo que se mergea.
- Preferir squash merge: un PR, un commit en `develop`.
