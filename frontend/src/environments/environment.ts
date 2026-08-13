/**
 * Configuracion NO secreta de build. Los secretos jamas viven en el bundle del
 * navegador: cualquier usuario puede leerlos. Lo sensible se queda en el backend.
 */
export const environment = {
  produccion: false,
  apiBaseUrl: 'https://localhost:7077',
} as const;
