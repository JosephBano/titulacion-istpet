// commitlint.config.mjs
// Configuracion de Conventional Commits para el equipo de Titan ISTPET
// Referencia: https://www.conventionalcommits.org/

export default {
  extends: ['@commitlint/config-conventional'],

  rules: {
    // Tipos de commit permitidos
    'type-enum': [
      2,
      'always',
      [
        'feat',      // Nueva funcionalidad
        'fix',       // Correccion de bug
        'refactor',  // Refactorizacion sin cambio de comportamiento
        'docs',      // Documentacion
        'style',     // Formato / estilos sin logica
        'test',      // Tests unitarios o de integracion
        'chore',     // Tareas de mantenimiento (deps, config, ci)
        'perf',      // Mejora de rendimiento
        'ci',        // Configuracion de CI/CD
        'revert',    // Revertir un commit anterior
      ],
    ],

    // El tipo debe estar en minusculas
    'type-case': [2, 'always', 'lower-case'],

    // El tipo es obligatorio
    'type-empty': [2, 'never'],

    // El alcance (scope) es opcional pero si se incluye, en minusculas
    'scope-case': [2, 'always', 'lower-case'],

    // La descripcion es obligatoria
    'subject-empty': [2, 'never'],

    // La descripcion no debe terminar con punto
    'subject-full-stop': [2, 'never', '.'],

    // La descripcion en minusculas
    'subject-case': [2, 'always', 'lower-case'],

    // Longitud maxima de la linea del encabezado: 100 caracteres
    'header-max-length': [2, 'always', 100],

    // Longitud minima del encabezado: 10 caracteres (evita commits vacios)
    'header-min-length': [2, 'always', 10],
  },

  // Mensaje de ayuda personalizado cuando falla la validacion
  helpUrl:
    'https://github.com/conventional-changelog/commitlint/#what-is-commitlint\n\n' +
    'Formato requerido:\n' +
    '  tipo(alcance): descripcion en minusculas\n\n' +
    'Tipos permitidos: feat, fix, refactor, docs, style, test, chore, perf, ci, revert\n' +
    'Alcances sugeridos: auth, titulacion, graduados, actas, rbac, frontend, backend, ui, db\n\n' +
    'Ejemplos validos:\n' +
    '  feat(titulacion): agregar endpoint de aprobacion de acta de grado\n' +
    '  fix(auth): corregir expiracion de refresh token\n' +
    '  refactor(frontend): migrar BehaviorSubject a signal en auth.service\n' +
    '  docs(skills): actualizar titan-ui-design con reglas de iconos\n' +
    '  chore(deps): actualizar angular a 19.3.0',
};
