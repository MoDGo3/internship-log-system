import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/auth/auth.service';
import { DashboardStudentComponent } from './dashboard-student/dashboard-student.component';
import { DashboardStaffComponent } from './dashboard-staff/dashboard-staff.component';

/** /dashboard — เลือกแดชบอร์ดตาม role ของผู้ใช้ */
@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, DashboardStudentComponent, DashboardStaffComponent],
  template: `
    <app-dashboard-student *ngIf="isStudent(); else staff"></app-dashboard-student>
    <ng-template #staff><app-dashboard-staff></app-dashboard-staff></ng-template>
  `
})
export class DashboardPageComponent {
  isStudent = computed(() => this.authService.currentUser()?.role === 'student');

  constructor(private authService: AuthService) {}
}
