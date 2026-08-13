export interface ErrorApi {
  readonly estado: number;
  readonly titulo: string;
  readonly detalle?: string;
  readonly errores?: Record<string, string[]>;
}
