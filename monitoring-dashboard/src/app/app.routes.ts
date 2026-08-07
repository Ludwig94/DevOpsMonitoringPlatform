import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'targets',
    loadComponent: () =>
      import('./features/targets/target-list/target-list.component').then(m => m.TargetListComponent)
  },
  {
    path: 'targets/new',
    loadComponent: () =>
      import('./features/targets/target-form/target-form.component').then(m => m.TargetFormComponent)
  },
  {
    path: 'targets/:id',
    loadComponent: () =>
      import('./features/targets/target-detail/target-detail.component').then(m => m.TargetDetailComponent)
  },
  {
    path: 'targets/:id/edit',
    loadComponent: () =>
      import('./features/targets/target-form/target-form.component').then(m => m.TargetFormComponent)
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
