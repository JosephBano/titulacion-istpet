import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { API_BASE_URL } from '../config/api.config';
import {
  LoginRequest,
  LoginResponse,
  UserPermissions,
  RefreshTokenRequest,
} from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  private readonly API_URL = `${this.apiBaseUrl}/api/v1/auth`;
  private readonly TOKEN_KEY = 'titulacion_access_token';
  private readonly REFRESH_TOKEN_KEY = 'titulacion_refresh_token';
  private readonly USER_INFO_KEY = 'titulacion_user_info';

  // State Management utilizando Angular Signals
  public currentUser = signal<UserPermissions | null>(this.loadUserFromStorage());
  public isAuthenticated = computed(() => !!this.currentUser() && !!this.getAccessToken());

  private loadUserFromStorage(): UserPermissions | null {
    const data = localStorage.getItem(this.USER_INFO_KEY);
    return data ? JSON.parse(data) : null;
  }

  public getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  public getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  public login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.API_URL}/login`, credentials).pipe(
      tap((response) => this.handleAuthSuccess(response)),
      catchError((error) => throwError(() => error)),
    );
  }

  public refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    const payload: RefreshTokenRequest = { refreshToken };
    return this.http.post<LoginResponse>(`${this.API_URL}/refresh-token`, payload).pipe(
      tap((response) => this.handleAuthSuccess(response)),
      catchError((error) => {
        this.logout();
        return throwError(() => error);
      }),
    );
  }

  public logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.API_URL}/logout`, { refreshToken }).subscribe({
        next: () => {},
        error: () => {},
      });
    }

    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_INFO_KEY);

    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  public hasPermission(moduleName: string, operationName: string): boolean {
    const user = this.currentUser();
    if (!user) return false;

    const modulo = user.modulos?.find(
      (m) => m.nombreModulo.toLowerCase() === moduleName.toLowerCase(),
    );
    if (!modulo) return false;

    return modulo.operaciones?.some((op) => op.toLowerCase() === operationName.toLowerCase()) ?? false;
  }

  public hasRole(roleCode: string): boolean {
    const user = this.currentUser();
    if (!user || !user.roles) return false;
    const cleanTarget = roleCode.toUpperCase().replace(/^TITULACION_/, '');
    return user.roles.some((r) => {
      const upperRole = r.toUpperCase();
      const cleanRole = upperRole.replace(/^TITULACION_/, '');
      return (
        upperRole === roleCode.toUpperCase() ||
        cleanRole === cleanTarget ||
        (cleanTarget === 'ESTUDIANTE' && upperRole === 'ALUMNO') ||
        (cleanTarget === 'DOCENTE' && upperRole === 'PROFESOR') ||
        (cleanTarget === 'ADMIN' && (upperRole === 'ADMINISTRADOR' || upperRole === 'TITULACION_ADMIN'))
      );
    });
  }

  public hasAnyRole(roles: string[]): boolean {
    return roles.some((role) => this.hasRole(role));
  }

  private handleAuthSuccess(response: LoginResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(this.USER_INFO_KEY, JSON.stringify(response.userInfo));
    this.currentUser.set(response.userInfo);
  }
}
