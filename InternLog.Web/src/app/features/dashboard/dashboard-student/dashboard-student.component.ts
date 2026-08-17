import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { format } from 'date-fns';
import { th } from 'date-fns/locale';

import { DashboardService } from '../../../core/services/dashboard.service';
import { AttendanceService } from '../../../core/services/attendance.service';
import { WorklogsService } from '../../../core/services/worklogs.service';
import { NotificationsService } from '../../../core/services/notifications.service';
import { LeavesService } from '../../../core/services/leaves.service';
import { AdvisorVisitsService } from '../../../core/services/advisor-visits.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-dashboard-student',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-student.component.html',
  styleUrls: ['./dashboard-student.component.scss']
})
export class DashboardStudentComponent implements OnInit, OnDestroy {
  currentTime = '';
  currentDate = '';
  private timer: any;
  studentId: number = 0;

  stats = {
    workedDays: 0,
    totalDays: 60, // Default requirement
    totalHours: 0,
    leavesLeft: 0,
    nextVisit: '-'
  };

  recentLogs: any[] = [];
  notifications: any[] = [];

  constructor(
    private dashboardService: DashboardService,
    private attendanceService: AttendanceService,
    private worklogsService: WorklogsService,
    private notificationsService: NotificationsService,
    private leavesService: LeavesService,
    private visitsService: AdvisorVisitsService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.updateClock();
    this.timer = setInterval(() => this.updateClock(), 1000);
    
    // Read from AuthService signal
    const user = this.authService.currentUser();
    if (user && user.id) {
      this.studentId = user.id;
      this.loadDashboardData();
    }
  }

  ngOnDestroy() {
    if (this.timer) {
      clearInterval(this.timer);
    }
  }

  loadDashboardData() {
    // Attendance Summary
    this.attendanceService.getSummary().subscribe(res => {
      const data = res.data;
      if (data) {
        this.stats.workedDays = data.totalDays || 0;
        this.stats.totalHours = data.totalHours || 0;
      }
    });

    // Leaves Quota
    this.leavesService.getQuota().subscribe(res => {
      const data = res.data;
      if (data) {
        // Assuming the API returns something like { remainingQuota: 5 }
        this.stats.leavesLeft = data.remainingQuota || 0;
      }
    });

    // Recent Worklogs
    this.worklogsService.getLogs().subscribe(res => {
      const data = res.data;
      if (data && data.length > 0) {
        // Take top 4 recent logs
        this.recentLogs = data.slice(0, 4).map((log: any) => ({
          date: log.logDate, // Assuming API returns logDate
          title: log.title,
          hours: log.hoursSpent,
          status: log.status
        }));
      }
    });

    // Notifications
    this.notificationsService.getNotifications().subscribe(res => {
      const data = res.data;
      if (data && data.length > 0) {
        this.notifications = data.slice(0, 3).map((n: any) => ({
          icon: n.type === 'system' ? '⚙️' : '🔔', // Simplified icon logic
          text: n.message,
          time: new Date(n.createdAt).toLocaleDateString() // Simplified time
        }));
      }
    });

    // Advisor Visits (Next visit)
    this.visitsService.getVisits().subscribe(res => {
      const data = res.data;
      if (data && data.length > 0) {
        const upcoming = data.filter((v: any) => new Date(v.visitDate) > new Date());
        if (upcoming.length > 0) {
          this.stats.nextVisit = new Date(upcoming[0].visitDate).toLocaleDateString('th-TH');
        }
      }
    });
  }

  updateClock() {
    const now = new Date();
    this.currentTime = format(now, 'HH:mm:ss');
    this.currentDate = format(now, 'd MMM yy', { locale: th });
  }

  checkIn() {
    this.attendanceService.checkIn({ note: 'เข้างาน' }).subscribe({
      next: () => alert('บันทึกเวลาเข้างานเรียบร้อยแล้ว'),
      error: (err) => alert('ไม่สามารถลงเวลาเข้าได้: ' + err.message)
    });
  }

  checkOut() {
    this.attendanceService.checkOut({ note: 'ออกงาน' }).subscribe({
      next: () => alert('บันทึกเวลาออกงานเรียบร้อยแล้ว'),
      error: (err) => alert('ไม่สามารถลงเวลาออกได้: ' + err.message)
    });
  }

  getStatusBadgeClass(status: string): string {
    switch(status?.toLowerCase()) {
      case 'approved': return 'badge-success';
      case 'pending': return 'badge-warning';
      case 'rejected': 
      case 'returned': return 'badge-danger';
      case 'draft': return 'badge-secondary';
      default: return 'badge-secondary';
    }
  }

  getStatusText(status: string): string {
    switch(status?.toLowerCase()) {
      case 'approved': return 'อนุมัติแล้ว';
      case 'pending': return 'รออนุมัติ';
      case 'rejected': 
      case 'returned': return 'ตีกลับ';
      case 'draft': return 'ร่าง';
      default: return status || 'ไม่มีสถานะ';
    }
  }
}
