import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell/shell.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  // TODO: Add authGuard when implemented
  {
    path: '',
    component: ShellComponent, 
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        // เลือกแดชบอร์ดตาม role (student / advisor+admin)
        loadComponent: () => import('./features/dashboard/dashboard-page.component').then(m => m.DashboardPageComponent)
      },
      {
        path: 'attendance',
        loadComponent: () => import('./features/attendance/components/attendance-page/attendance-page.component').then(m => m.AttendancePageComponent)
      },
      {
        path: 'leaves',
        loadComponent: () => import('./features/leaves/leaves-page/leaves-page.component').then(m => m.LeavesPageComponent)
      },
      {
        path: 'calendar',
        loadComponent: () => import('./features/calendar/calendar-page/calendar-page.component').then(m => m.CalendarPageComponent)
      },
      {
        path: 'tasks',
        loadComponent: () => import('./features/tasks/tasks-kanban/tasks-kanban.component').then(m => m.TasksKanbanComponent)
      },
      {
        path: 'visits',
        loadComponent: () => import('./features/visits/visits-page/visits-page.component').then(m => m.VisitsPageComponent)
      },

      {
        path: 'notifications',
        loadComponent: () => import('./features/notifications/notifications-page/notifications-page.component').then(m => m.NotificationsPageComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports-page/reports-page.component').then(m => m.ReportsPageComponent)
      },
      {
        path: 'reports/:id/preview',
        loadComponent: () => import('./features/reports/report-preview/report-preview.component').then(m => m.ReportPreviewComponent)
      },
      {
        path: 'evaluations',
        loadComponent: () => import('./features/evaluations/evaluations-page/evaluations-page.component').then(m => m.EvaluationsPageComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile-page/profile-page.component').then(m => m.ProfilePageComponent)
      },
      {
        path: 'companies',
        loadComponent: () => import('./features/companies/components/companies.component').then(m => m.CompaniesComponent),
        canActivate: [roleGuard],
        data: { roles: ['advisor', 'admin'] }
      },
      {
        path: 'admin/users',
        loadComponent: () => import('./features/admin/admin-users/admin-users.component').then(m => m.AdminUsersComponent),
        canActivate: [roleGuard],
        data: { roles: ['admin'] }
      },
      {
        path: 'admin/companies',
        loadComponent: () => import('./features/admin/admin-companies/admin-companies.component').then(m => m.AdminCompaniesComponent),
        canActivate: [roleGuard],
        data: { roles: ['admin'] }
      },
      {
        path: 'admin/internships',
        loadComponent: () => import('./features/admin/admin-internships/admin-internships.component').then(m => m.AdminInternshipsComponent),
        canActivate: [roleGuard],
        data: { roles: ['admin'] }
      },
      {
        path: 'work-logs',
        loadComponent: () => import('./features/work-logs/components/work-log-list/work-log-list.component').then(m => m.WorkLogListComponent)
      },
      {
        path: 'work-logs/new',
        loadComponent: () => import('./features/work-logs/components/work-log-form/work-log-form.component').then(m => m.WorkLogFormComponent)
      },
      {
        path: 'work-logs/:id',
        loadComponent: () => import('./features/work-logs/components/work-log-form/work-log-form.component').then(m => m.WorkLogFormComponent)
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  { path: '**', redirectTo: 'login' }
];
