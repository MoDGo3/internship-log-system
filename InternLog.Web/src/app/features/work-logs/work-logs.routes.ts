import { Routes } from '@angular/router';
export const WORKLOG_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./components/work-log-list/work-log-list.component').then(m => m.WorkLogListComponent), title: 'บันทึกงาน' },
  { path: 'new', loadComponent: () => import('./components/work-log-form/work-log-form.component').then(m => m.WorkLogFormComponent), title: 'เพิ่มบันทึกงาน' },
  { path: ':id', loadComponent: () => import('./components/work-log-form/work-log-form.component').then(m => m.WorkLogFormComponent), title: 'รายละเอียดบันทึกงาน' }
];
