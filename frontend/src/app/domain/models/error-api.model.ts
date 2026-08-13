/** Forma normalizada de cualquier fallo que cruce la frontera HTTP. */
export interface ErrorApi {
  readonly titulo: string;
  readonly detalle?: string;
  readonly estado: number;
  /** Errores por campo cuando el backend devuelve un ValidationProblemDetails. */
  readonly errores?: Readonly<Record<string, readonly string[]>>;
}
