import { Routes } from '@angular/router';

export const CALENDAR_ROUTES: Routes = [
  { 
    path: '', 
    loadComponent: () => import('./components/calendar.component').then(m => m.CalendarComponent),
    title: 'ปฏิทินปฏิบัติงาน'
  }
];
