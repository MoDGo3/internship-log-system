import { Routes } from '@angular/router';
export const ATTENDANCE_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./components/attendance-page/attendance-page.component').then(m => m.AttendancePageComponent), title: 'ลงเวลา' }
];
