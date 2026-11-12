import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { startOfMonth, endOfMonth, startOfWeek, endOfWeek, addDays, subMonths, addMonths, format, isSameMonth, isSameDay } from 'date-fns';
import { th } from 'date-fns/locale';
import { DashboardService } from '../../../core/services/dashboard.service';
import { WorkLogService } from '../../work-logs/services/work-log.service';
import { forkJoin } from 'rxjs';
import { WorkLog } from '../../work-logs/models/work-log.model';

interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  status: 'approved' | 'pending' | 'absent' | 'leave' | 'holiday' | 'none';
  hasLog: boolean;
  workLog?: WorkLog; // Keep track of the log for the drawer
  events?: string[];
}

@Component({
  selector: 'app-calendar-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './calendar-page.component.html',
  styleUrls: ['./calendar-page.component.scss']
})
export class CalendarPageComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private workLogService = inject(WorkLogService);

  currentDate = new Date(); 
  calendarDays: CalendarDay[] = [];
  weekDays = ['อา.', 'จ.', 'อ.', 'พ.', 'พฤ.', 'ศ.', 'ส.'];
  
  selectedDay: CalendarDay | null = null;
  isDrawerOpen = false;

  ngOnInit() {
    this.generateCalendar();
  }

  generateCalendar() {
    const start = startOfWeek(startOfMonth(this.currentDate));
    const end = endOfWeek(endOfMonth(this.currentDate));
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth() + 1;
    
    forkJoin({
      calendar: this.dashboardService.getCalendarEvents(year, month),
      logs: this.workLogService.getStudentLogs(year, month)
    }).subscribe({
      next: ({ calendar, logs }) => {
        const statusMap: Record<string, string> = {};
        if (calendar.success && calendar.data) {
          calendar.data.forEach(c => {
             const d = (c as any).date.split('T')[0];
             statusMap[d] = c.status;
          });
        }

        const logsMap: Record<string, WorkLog> = {};
        if (logs.success && logs.data) {
          logs.data.forEach(log => {
             const d = log.logDate.split('T')[0];
             logsMap[d] = log;
          });
        }

        this.calendarDays = [];
        let day = start;
        
        while (day <= end) {
          // ensure YYYY-MM-DD
          const dStr = format(day, 'yyyy-MM-dd');
          const rawStatus = statusMap[dStr];
          let status: CalendarDay['status'] = 'none';
          
          if (rawStatus === 'Present') status = 'approved';
          else if (rawStatus === 'Absent') status = 'absent';
          else if (rawStatus === 'Leave') status = 'leave';
          else if (rawStatus === 'Holiday') status = 'holiday';

          const log = logsMap[dStr];

          this.calendarDays.push({
            date: day,
            isCurrentMonth: isSameMonth(day, this.currentDate),
            status: status,
            hasLog: !!log,
            workLog: log
          });
          day = addDays(day, 1);
        }
      }
    });
  }

  get currentMonthYear(): string {
    return format(this.currentDate, 'MMMM yyyy', { locale: th });
  }

  prevMonth() {
    this.currentDate = subMonths(this.currentDate, 1);
    this.generateCalendar();
  }

  nextMonth() {
    this.currentDate = addMonths(this.currentDate, 1);
    this.generateCalendar();
  }

  openDrawer(day: CalendarDay) {
    this.selectedDay = day;
    this.isDrawerOpen = true;
  }

  closeDrawer() {
    this.isDrawerOpen = false;
    setTimeout(() => {
      this.selectedDay = null;
    }, 300); // Wait for animation
  }

  isToday(date: Date): boolean {
    return isSameDay(date, new Date());
  }

  isMissingLog(day: CalendarDay): boolean {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const cellDate = new Date(day.date);
    cellDate.setHours(0, 0, 0, 0);

    // No warning for future dates
    if (cellDate > today) return false;

    // No warning for leave, absent, or holiday
    if (day.status === 'leave' || day.status === 'absent' || day.status === 'holiday') {
      return false;
    }

    // No warning for weekends unless they specifically have an approved/pending attendance
    const isWeekend = cellDate.getDay() === 0 || cellDate.getDay() === 6;
    if (isWeekend && day.status !== 'approved' && day.status !== 'pending') {
      return false;
    }

    // If no log is present, it's missing
    return !day.hasLog;
  }

  formatDateHeader(date: Date | undefined): string {
    if (!date) return '';
    return format(date, 'd MMMM yyyy', { locale: th });
  }
}
