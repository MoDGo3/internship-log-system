import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { startOfMonth, endOfMonth, startOfWeek, endOfWeek, addDays, subMonths, addMonths, format, isSameMonth, isSameDay } from 'date-fns';
import { th } from 'date-fns/locale';

interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  status: 'approved' | 'pending' | 'absent' | 'leave' | 'holiday' | 'none';
  hasLog: boolean;
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
  currentDate = new Date(); // Aug 2026 as base if we want, or real current date
  calendarDays: CalendarDay[] = [];
  weekDays = ['อา.', 'จ.', 'อ.', 'พ.', 'พฤ.', 'ศ.', 'ส.'];
  
  selectedDay: CalendarDay | null = null;
  isDrawerOpen = false;

  ngOnInit() {
    // Override to August 2026 for consistent mockup
    this.currentDate = new Date(2026, 7, 14); // 14 Aug 2026
    this.generateCalendar();
  }

  generateCalendar() {
    const start = startOfWeek(startOfMonth(this.currentDate));
    const end = endOfWeek(endOfMonth(this.currentDate));
    
    this.calendarDays = [];
    let day = start;
    
    while (day <= end) {
      // Mock statuses for visual variety
      let status: CalendarDay['status'] = 'none';
      let hasLog = false;
      const dayNum = day.getDate();
      const monthNum = day.getMonth();

      if (monthNum === 7) { // Aug
        if (dayNum >= 1 && dayNum <= 10 && day.getDay() !== 0 && day.getDay() !== 6) {
          status = 'approved';
          hasLog = true;
        } else if (dayNum === 11) {
          status = 'leave';
        } else if (dayNum === 12) {
          status = 'pending';
          hasLog = true;
        } else if (dayNum === 13) {
          status = 'pending';
          hasLog = true;
        } else if (dayNum === 14) {
          status = 'none'; // Today, maybe no log yet
        }
      }

      this.calendarDays.push({
        date: day,
        isCurrentMonth: isSameMonth(day, this.currentDate),
        status: status,
        hasLog: hasLog
      });
      day = addDays(day, 1);
    }
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
    return isSameDay(date, new Date(2026, 7, 14)); // Mock today as 14 Aug 2026
  }

  formatDateHeader(date: Date | undefined): string {
    if (!date) return '';
    return format(date, 'd MMMM yyyy', { locale: th });
  }
}
