// .lintstagedrc.mjs
// Lint-staged: ejecuta linters SOLO sobre archivos en staging area
// Se activa en el hook pre-commit de Husky

export default {
  // Archivos TypeScript, HTML y CSS del frontend — Prettier (formato)
  'frontend/src/**/*.{ts,html,css}': [
    'npx prettier --write',
  ],

  // Archivos CSS del frontend — Prettier (formato)
  'frontend/src/**/*.css': [
    'npx prettier --write',
  ],

  // Archivos C# del backend — dotnet format
  // dotnet format analiza los archivos pasados por lint-staged
  'backend/**/*.cs': [
    () => 'dotnet format backend/Titan.slnx --severity warn --no-restore',
  ],
};
