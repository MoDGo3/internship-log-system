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
    this.attendanceService.getSummary().subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data) this.stats = data;
      },
      error: (err: any) => console.error('Failed to load summary', err)
    });

    this.loadHistory();
  }

  loadHistory() {
    this.attendanceService.getRecords().subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data) {
          this.history = data;
          
          // Check today's status
          const todayStr = format(new Date(), 'yyyy-MM-dd');
          const todayRecord = this.history.find(r => r.attDate === todayStr);
          if (todayRecord && todayRecord.checkInTime && !todayRecord.checkOutTime) {
            this.isCheckedIn = true;
          } else {
            this.isCheckedIn = false;
          }
        }
      },
      error: (err: any) => console.error('Failed to load history', err)
    });
  }

  updateClock() {
    const now = new Date();
    this.currentTime = format(now, 'HH:mm:ss');
    this.currentDate = format(now, 'd MMMM yyyy', { locale: th });
  }

  checkIn() {
    if (this.isCheckedIn) return;
    this.attendanceService.checkIn({ note: '' }).subscribe({
      next: () => {
        alert('ลงเวลาเข้าสำเร็จ');
        this.isCheckedIn = true;
        this.loadHistory();
      },
      error: (err: any) => alert('เกิดข้อผิดพลาด: ' + err.message)
    });
  }

  checkOut() {
    if (!this.isCheckedIn) return;
    this.attendanceService.checkOut({ note: '' }).subscribe({
      next: () => {
        alert('ลงเวลาออกสำเร็จ');
        this.isCheckedIn = false;
        this.loadHistory();
      },
      error: (err: any) => alert('เกิดข้อผิดพลาด: ' + err.message)
    });
  }

  requestEdit(item: any) {
    const reason = prompt('กรุณาระบุเหตุผลการขอแก้ไขเวลา:');
    if (reason) {
      this.attendanceService.requestEdit(item.attendanceID, { note: reason }).subscribe({
        next: () => {
          alert('ส่งคำขอแก้ไขเวลาเรียบร้อยแล้ว');
          this.loadHistory();
        },
        error: (err: any) => alert('เกิดข้อผิดพลาด: ' + err.message)
      });
    }
  }

  getStatusBadgeClass(status: string): string {
    switch(status?.toLowerCase()) {
      case 'ontime': return 'badge-success';
      case 'late': return 'badge-danger';
      case 'leave': return 'badge-leave';
      case 'absent': return 'badge-secondary';
      case 'pending_edit': return 'badge-warning';
      default: return 'badge-secondary';
    }
  }

  getStatusText(status: string): string {
    switch(status?.toLowerCase()) {
      case 'ontime': return 'ตรงเวลา';
      case 'late': return 'สาย';
      case 'leave': return 'ลา';
      case 'absent': return 'ขาด';
      case 'pending_edit': return 'รอแก้ไข';
      default: return status || '-';
    }
  }
}
