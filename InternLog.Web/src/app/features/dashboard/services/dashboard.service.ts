import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { DashboardService as CoreDashboardService } from '../../../core/services/dashboard.service';

export interface DashboardStats {
  daysCompleted?: number;
  totalDays?: number;
  totalHours?: number;
  leaveDaysLeft?: number;
  nextVisitDate?: string;
  
  // Advisor stats
  totalStudents?: number;
  pendingApprovals?: number;
  approvedThisWeek?: number;
  unreadMessages?: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private coreDashboardService = inject(CoreDashboardService);

  getStudentStats(): Observable<DashboardStats> {
    return this.coreDashboardService.getDashboardSummary().pipe(
      map(res => {
        const data = res.data;
        return {
          daysCompleted: data?.activeInternships || 0,
          totalDays: data?.totalInternships || 0,
          totalHours: 0,
          leaveDaysLeft: data?.pendingLeaves || 0,
          nextVisitDate: '-'
        };
      })
    );
  }

  getAdvisorStats(): Observable<DashboardStats> {
    return this.coreDashboardService.getDashboardSummary().pipe(
      map(res => {
        const data = res.data;
        return {
          totalStudents: data?.totalInternships || 0,
          pendingApprovals: data?.pendingLeaves || 0,
          approvedThisWeek: 0,
          unreadMessages: data?.unreadMessages || 0
        };
      })
    );
  }
}
