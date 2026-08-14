import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { AlumnosTitulacionComponent } from './pages/alumnos-titulacion/alumnos-titulacion.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent, title: 'Iniciar Sesión | Titulación ISTPET' },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard],
    title: 'Panel de Control | Titulación ISTPET',
  },
  {
    path: 'dashboard/alumnos-titulacion',
    component: AlumnosTitulacionComponent,
    canActivate: [authGuard],
    title: 'Alumnos Titulación | Titulación ISTPET',
  },
  {
    path: 'estudiantes',
    loadComponent: () =>
      import('./presentation/estudiantes/estudiantes-page').then((m) => m.EstudiantesPage),
    title: 'Estudiantes | Titulación ISTPET',
  },
  { path: '**', redirectTo: 'login' },
];
