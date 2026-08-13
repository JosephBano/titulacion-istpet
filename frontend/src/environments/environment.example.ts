/**
 * Plantilla de environment.prod.ts (git-ignored). Copiala como environment.prod.ts
 * y apunta apiBaseUrl al backend desplegado.
 *
 * Recordatorio: esto termina en el bundle publico. Nunca pongas aqui llaves de API,
 * cadenas de conexion ni secretos de ningun tipo.
 */
export const environment = {
  produccion: true,
  apiBaseUrl: 'https://CAMBIAME.istpet.edu.ec',
} as const;
