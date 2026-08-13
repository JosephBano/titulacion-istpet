/**
 * Configuración NO secreta de build. Los secretos jamás viven en el bundle del navegador.
 */
export const environment = {
  produccion: false,
  apiBaseUrl: 'http://localhost:5032',
} as const;
