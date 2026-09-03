import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ErrorApi } from '../../domain/models/error-api.model';

/**
 * Traduce cualquier HttpErrorResponse al ErrorApi del dominio, de modo que ninguna
 * capa por encima tenga que conocer el formato ProblemDetails de ASP.NET.
 */
export const errorApiInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      const cuerpo = error.error as
        { title?: string; detail?: string; message?: string; errors?: Record<string, string[]> } | string | null;

      const problema = typeof cuerpo === 'object' && cuerpo !== null ? cuerpo : null;

      const detalleTexto = problema?.detail || problema?.message || (typeof cuerpo === 'string' ? cuerpo : undefined);

      const normalizado: ErrorApi = {
        estado: error.status,
        titulo:
          problema?.title ??
          (error.status === 0
            ? 'No se pudo contactar al servidor.'
            : 'Ocurrio un error inesperado.'),
        detalle: detalleTexto,
        errores: problema?.errors,
      };

      return throwError(() => normalizado);
    }),
  );
