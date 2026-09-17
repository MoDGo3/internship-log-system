import { Routes } from '@angular/router';

export const LEAVES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/leave-page/leave-page.component').then(c => c.LeavePageComponent),
    title: 'วันลา (Leaves)'
  }
];
