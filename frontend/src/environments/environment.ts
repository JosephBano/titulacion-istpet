/**
 * Configuración de entorno.
 * Configuración activa: Desarrollo local (localhost).
 */
export const environment = {
  produccion: false,
  apiBaseUrl: 'http://localhost:5192', // Backend local .NET 8
} as const;
