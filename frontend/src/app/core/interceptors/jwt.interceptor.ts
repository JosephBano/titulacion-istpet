import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  let authReq = req;
  if (token && !req.url.includes('/auth/login') && !req.url.includes('/auth/refresh-token')) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(authReq).pipe(
    catchError((error: unknown) => {
      const isHttpError = error instanceof HttpErrorResponse;
      const statusCode = isHttpError
        ? error.status
        : (error as { estado?: number; status?: number })?.estado ??
          (error as { estado?: number; status?: number })?.status;

      const isLogin = req.url.includes('/auth/login');
      const isRefresh = req.url.includes('/auth/refresh-token');
      const isLogout = req.url.includes('/auth/logout');

      if (statusCode === 401 && !isLogin && !isRefresh && !isLogout) {
        const refreshToken = authService.getRefreshToken();
        if (!refreshToken) {
          authService.logout();
          return throwError(() => error);
        }

        return authService.refreshToken().pipe(
          switchMap((response) => {
            const newReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${response.accessToken}`,
              },
            });
            return next(newReq);
          }),
          catchError((refreshErr) => {
            authService.logout();
            return throwError(() => refreshErr);
          }),
        );
      }

      if (statusCode === 401 && isRefresh) {
        authService.logout();
      }

      return throwError(() => error);
    }),
  );
};
