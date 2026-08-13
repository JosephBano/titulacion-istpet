import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'estudiantes',
    loadComponent: () =>
      import('./presentation/estudiantes/estudiantes-page').then((m) => m.EstudiantesPage),
    title: 'Estudiantes | Titulacion ISTPET',
  },
  { path: '', pathMatch: 'full', redirectTo: 'estudiantes' },
  { path: '**', redirectTo: 'estudiantes' },
];
