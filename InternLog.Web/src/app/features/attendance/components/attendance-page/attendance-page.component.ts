import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { format } from 'date-fns';
import { th } from 'date-fns/locale';

import { AttendanceService } from '../../../../core/services/attendance.service';
import { AttendanceRecord, AttendanceSummary } from '../../../../models/attendance.model';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-attendance-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './attendance-page.component.html',
  styleUrls: ['./attendance-page.component.scss']
})
export class AttendancePageComponent implements OnInit, OnDestroy {
  currentTime = '';
  currentDate = '';
  private timer: any;
  isCheckedIn = false;
  isCompletedToday = false;
  
  stats: AttendanceSummary = {
    totalDays: 0,
    totalHours: 0,
    lateCount: 0,
    absentCount: 0,
    leaveCount: 0
  };

  months = [
    { value: '08-2026', label: 'สิงหาคม 2569' },
    { value: '07-2026', label: 'กรกฎาคม 2569' },
    { value: '06-2026', label: 'มิถุนายน 2569' }
  ];
  selectedMonth = '08-2026';

  history: AttendanceRecord[] = [];

  constructor(
    private attendanceService: AttendanceService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.updateClock();
    this.timer = setInterval(() => this.updateClock(), 1000);
    this.loadData();
  }

  ngOnDestroy() {
    if (this.timer) {
      clearInterval(this.timer);
    }
  }

  loadData() {
    this.attendanceService.getSummary(1).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.stats = res.data;
        }
      },
      error: (err) => console.error('Failed to load summary', err)
    });

    this.loadHistory();
  }

  onMonthChange() {
    this.loadHistory();
  }

  loadHistory() {
    const [monthStr, yearStr] = this.selectedMonth.split('-');
    const month = parseInt(monthStr, 10);
    const year = parseInt(yearStr, 10);

    this.attendanceService.getRecords(month, year).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.history = res.data;
          
          // Check today's status
          const todayStr = format(new Date(), 'yyyy-MM-dd');
          const todayRecord = this.history.find(r => r.attDate === todayStr);
          
          if (todayRecord) {
            if (todayRecord.checkInTime && !todayRecord.checkOutTime) {
              this.isCheckedIn = true;
              this.isCompletedToday = false;
            } else if (todayRecord.checkInTime && todayRecord.checkOutTime) {
              this.isCheckedIn = false;
              this.isCompletedToday = true;
            } else {
              this.isCheckedIn = false;
              this.isCompletedToday = false;
            }
          } else {
            this.isCheckedIn = false;
            this.isCompletedToday = false;
          }
        }
      },
      error: (err) => console.error('Failed to load history', err)
    });
  }

  updateClock() {
    const now = new Date();
    this.currentTime = format(now, 'HH:mm:ss');
    this.currentDate = format(now, 'd MMMM yyyy', { locale: th });
  }

  checkIn() {
    if (this.isCheckedIn) return;
    this.attendanceService.checkIn(null).subscribe({
      next: (res) => {
        if (res.success) {
          alert('ลงเวลาเข้าสำเร็จ');
          this.isCheckedIn = true;
          this.loadHistory();
        }
      },
      error: (err) => alert('เกิดข้อผิดพลาด: ' + (err.error?.message || err.message))
    });
  }

  checkOut() {
    if (!this.isCheckedIn) return;
    this.attendanceService.checkOut(null).subscribe({
      next: (res) => {
        if (res.success) {
          alert('ลงเวลาออกสำเร็จ');
          this.isCheckedIn = false;
          this.loadHistory();
        }
      },
      error: (err) => alert('เกิดข้อผิดพลาด: ' + (err.error?.message || err.message))
    });
  }

  requestEdit(item: AttendanceRecord) {
    const reason = prompt('กรุณาระบุเหตุผลการขอแก้ไขเวลา:');
    if (reason) {
      this.attendanceService.requestEdit(item.attendanceID, { note: reason }).subscribe({
        next: (res) => {
          if (res.success) {
            alert('ส่งคำขอแก้ไขเวลาเรียบร้อยแล้ว');
            this.loadHistory();
          }
        },
        error: (err) => alert('เกิดข้อผิดพลาด: ' + (err.error?.message || err.message))
      });
    }
  }

  getStatusBadgeClass(status: string): string {
    switch(status?.toLowerCase()) {
      case 'present': return 'badge-success';
      case 'late': return 'badge-danger';
      case 'leave': return 'badge-leave';
      case 'absent': return 'badge-secondary';
      case 'pending': return 'badge-warning';
      default: return 'badge-secondary';
    }
  }

  getStatusText(status: string): string {
    switch(status?.toLowerCase()) {
      case 'present': return 'ตรงเวลา';
      case 'late': return 'สาย';
      case 'leave': return 'ลา';
      case 'absent': return 'ขาด';
      case 'pending': return 'รอแก้ไข';
      default: return status || '-';
    }
  }
}
