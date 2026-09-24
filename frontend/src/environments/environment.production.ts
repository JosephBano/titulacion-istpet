/**
 * Configuracion NO secreta de build de produccion. Los secretos jamas viven en
 * el bundle del navegador: cualquier usuario puede leerlos.
 */
export const environment = {
  produccion: true,
  apiBaseUrl: 'https://localhost:7077',
} as const;
