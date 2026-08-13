#!/usr/bin/env bash
#
# Aplica las protecciones de rama de main y develop.
#
# Idempotente: reejecutarlo deja el repositorio en el mismo estado. Es la fuente de
# verdad de las reglas; si alguien las cambia a mano en la interfaz de GitHub, correr
# este script las restaura.
#
# Requiere: gh autenticado con permisos de admin sobre el repositorio.
# No requiere jq: el JSON se arma con heredocs y `gh -q` trae su propio jq embebido.
#
# Uso: ./scripts/proteger-ramas.sh [owner/repo]

set -euo pipefail

REPO="${1:-$(gh repo view --json nameWithOwner -q .nameWithOwner)}"
echo "Aplicando protecciones a ${REPO}"
echo

# Los nombres deben coincidir EXACTAMENTE con el campo `name:` de cada job en los
# workflows. Un nombre mal escrito no da error: el check queda "pendiente" para
# siempre y bloquea todos los merges sin explicar por que.
# El check de GitGuardian se activa con una variable, no editando la lista: hacerlo
# obligatorio sin el secreto GITGUARDIAN_API_KEY configurado bloquearia todos los
# merges, porque el job falla a proposito cuando la key no existe.
#
#   1. Crear la key en https://dashboard.gitguardian.com/api/personal-access-tokens
#   2. gh secret set GITGUARDIAN_API_KEY
#   3. GGSHIELD_REQUERIDO=1 ./scripts/proteger-ramas.sh
#
GGSHIELD_REQUERIDO="${GGSHIELD_REQUERIDO:-0}"

leer_checks() {
  cat <<'JSON'
    "Build y tests (.NET 8)",
    "Publish (verifica que despliega)",
    "Lint, tests y build (Angular)",
    "Archivos que nunca deben commitearse",
    "Sin conflictos de merge",
    "Sin marcadores de conflicto en el codigo",
    "Titulo con formato Conventional Commits"
JSON
  if [ "$GGSHIELD_REQUERIDO" = "1" ]; then
    printf ',\n    "GitGuardian (ggshield)"\n'
  fi
}

proteger() {
  local rama="$1" code_owners="$2"

  echo "  -> ${rama} (require_code_owner_reviews: ${code_owners})"

  # strict:true = "require branches to be up to date before merging".
  # Es lo que impide que dos PRs verdes por separado rompan la rama al fusionarse.
  gh api -X PUT "repos/${REPO}/branches/${rama}/protection" \
    -H "Accept: application/vnd.github+json" \
    --input - > /dev/null <<JSON
{
  "required_status_checks": {
    "strict": true,
    "contexts": [
$(leer_checks)
    ]
  },
  "enforce_admins": false,
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": ${code_owners},
    "require_last_push_approval": false
  },
  "restrictions": null,
  "required_linear_history": false,
  "allow_force_pushes": false,
  "allow_deletions": false,
  "block_creations": false,
  "required_conversation_resolution": true,
  "lock_branch": false,
  "allow_fork_syncing": false
}
JSON
}

# main exige aprobacion de code owner: CODEOWNERS asigna todo a @JosephBano,
# asi que nada entra a produccion sin el dueño.
proteger main true

# develop se revisa entre pares: 1 aprobacion de cualquier colaborador.
proteger develop false

echo
echo "Estado resultante:"
for rama in main develop; do
  echo "--- ${rama} ---"
  gh api "repos/${REPO}/branches/${rama}/protection" -q '
    "  checks estrictos : \(.required_status_checks.strict)
  checks requeridos: \(.required_status_checks.contexts | length)
  aprobaciones     : \(.required_pull_request_reviews.required_approving_review_count)
  code owners      : \(.required_pull_request_reviews.require_code_owner_reviews)
  descarta stale   : \(.required_pull_request_reviews.dismiss_stale_reviews)
  force push       : \(.allow_force_pushes.enabled)
  borrado          : \(.allow_deletions.enabled)
  conversaciones   : \(.required_conversation_resolution.enabled)"'
done
