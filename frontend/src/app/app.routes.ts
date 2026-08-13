import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { AlumnosTitulacionComponent } from './pages/alumnos-titulacion/alumnos-titulacion.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  {
    path: 'dashboard/alumnos-titulacion',
    component: AlumnosTitulacionComponent,
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: 'login' },
];
