import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { format } from 'date-fns';
import { th } from 'date-fns/locale';

import { DashboardService } from '../../../core/services/dashboard.service';
import { AttendanceService } from '../../../core/services/attendance.service';
import { WorkLogService } from '../../work-logs/services/work-log.service';
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
    private workLogService: WorkLogService,
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
    if (user && user.userID) {
      this.studentId = user.userID;
      this.loadDashboardData();
    }
  }

  ngOnDestroy() {
    if (this.timer) {
      clearInterval(this.timer);
    }
  }

  hasCheckedInToday = false;
  hasCheckedOutToday = false;

  loadDashboardData() {
    const now = new Date();
    
    // 1. Dashboard Summary
    this.dashboardService.getDashboardSummary().subscribe({
      next: (res) => {}
    });

    // 2. Attendance Summary
    this.attendanceService.getSummary(1).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.stats.workedDays = res.data.totalDays || 0;
          this.stats.totalHours = res.data.totalHours || 0;
        }
      }
    });

    // 3. Leaves Quota
    this.leavesService.getQuota().subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data && Array.isArray(data)) {
          this.stats.leavesLeft = data.reduce((sum, item) => sum + (item.remainingDays || 0), 0);
        }
      }
    });

    // 4. Recent Worklogs
    this.workLogService.getStudentLogs(now.getFullYear(), now.getMonth() + 1).subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data && data.length > 0) {
          this.recentLogs = data.slice(0, 4).map((log: any) => ({
            date: log.logDate,
            title: log.title,
            hours: log.hours
          }));
        }
      }
    });

    // 5. Notifications
    this.notificationsService.getNotifications().subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data && data.length > 0) {
          this.notifications = data.slice(0, 3).map((n: any) => ({
            icon: '🔔',
            text: n.message,
            time: new Date(n.createdAt).toLocaleDateString('th-TH')
          }));
        }
      }
    });

    // 6. Advisor Visits
    this.visitsService.getVisits(1).subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data && data.length > 0) {
          const upcoming = data.filter((v: any) => new Date(v.visitDate) > new Date());
          if (upcoming.length > 0) {
            this.stats.nextVisit = new Date(upcoming[0].visitDate).toLocaleDateString('th-TH');
          }
        }
      }
    });

    // 7. Today's Attendance Status
    this.attendanceService.getRecords(now.getMonth() + 1, now.getFullYear()).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          const todayStr = format(now, 'yyyy-MM-dd');
          const todayRecord = res.data.find((a: any) => a.attDate && a.attDate.startsWith(todayStr));
          if (todayRecord) {
            this.hasCheckedInToday = !!todayRecord.checkInTime;
            this.hasCheckedOutToday = !!todayRecord.checkOutTime;
          } else {
            this.hasCheckedInToday = false;
            this.hasCheckedOutToday = false;
          }
        }
      }
    });
  }

  updateClock() {
    const now = new Date();
    this.currentTime = format(now, 'HH:mm:ss');
    this.currentDate = format(now, 'd MMM yy', { locale: th });
  }

  actionMessage: { type: 'success' | 'error', text: string } | null = null;

  showMessage(type: 'success' | 'error', text: string) {
    this.actionMessage = { type, text };
    setTimeout(() => {
      this.actionMessage = null;
    }, 5000); // 5 วินาทีแล้วค่อยซ่อน
  }

  checkIn() {
    this.attendanceService.checkIn(null).subscribe({
      next: (res) => {
        const timeStr = res.data?.checkInTime || this.currentTime;
        this.showMessage('success', `ลงชื่อเข้าแล้ว เวลา: ${timeStr}`);
        this.loadDashboardData();
      },
      error: (err) => {
        const errorMsg = err.error?.message || err.message;
        this.showMessage('error', `ไม่สามารถลงเวลาเข้าได้: ${errorMsg}`);
      }
    });
  }

  checkOut() {
    this.attendanceService.checkOut(null).subscribe({
      next: (res) => {
        const timeStr = res.data?.checkOutTime || this.currentTime;
        this.showMessage('success', `ลงชื่อออกแล้ว เวลา: ${timeStr}`);
        this.loadDashboardData();
      },
      error: (err) => {
        const errorMsg = err.error?.message || err.message;
        this.showMessage('error', `ไม่สามารถลงเวลาออกได้: ${errorMsg}`);
      }
    });
  }
}
