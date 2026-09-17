import { Routes } from '@angular/router';

export const VISITS_ROUTES: Routes = [
  { 
    path: '', 
    loadComponent: () => import('./components/visits.component').then(m => m.VisitsComponent),
    title: 'บันทึกการนิเทศ'
  }
];
