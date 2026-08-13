## Que cambia

<!-- Una frase. Si necesitas un parrafo, probablemente el PR es demasiado grande. -->

## Por que

<!-- El problema que resuelve. Enlaza el issue: Closes #123 -->

## Como probarlo

1.
2.

## Checklist

- [ ] Mi rama esta actualizada con `develop` (`git fetch origin && git rebase origin/develop`)
- [ ] No hay conflictos de merge
- [ ] `dotnet test` pasa en local (si toque el backend)
- [ ] `npm run lint && npm test` pasan en local (si toque el frontend)
- [ ] **No agregue secretos, credenciales ni `appsettings.json` reales**
- [ ] Si agregue configuracion nueva, actualice el `*.example.*` correspondiente
- [ ] El titulo del PR sigue Conventional Commits (`feat:`, `fix:`, `docs:`...)
