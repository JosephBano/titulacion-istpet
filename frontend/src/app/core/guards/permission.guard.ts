import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const permissionGuard = (moduleName: string, operationName: string): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.hasPermission(moduleName, operationName)) {
      return true;
    }

    router.navigate(['/unauthorized']);
    return false;
  };
};
