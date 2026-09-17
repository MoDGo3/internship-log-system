import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { DashboardService as CoreDashboardService } from '../../../core/services/dashboard.service';

export interface CalendarEvent {
  id: string;
  title: string;
  date: string; // YYYY-MM-DD
  startTime?: string; // HH:mm
  endTime?: string;   // HH:mm
  isAllDay: boolean;
  type: 'attendance' | 'leave' | 'task' | 'visit' | 'holiday';
  status: 'approved' | 'pending' | 'late' | 'absent' | 'todo' | 'doing' | 'done';
  description?: string;
  refId?: string; 
}

@Injectable({ providedIn: 'root' })
export class CalendarService {
  private dashboardService = inject(CoreDashboardService);

  getEvents(year: number, month: number, viewType: string): Observable<CalendarEvent[]> {
    return this.dashboardService.getCalendarEvents(year, month).pipe(
      map(res => {
        const events: CalendarEvent[] = [];
        if (res.data) {
          res.data.forEach((day, index) => {
            const dateStr = day.date as unknown as string;
            if (day.status === 'Present') {
              events.push({ id: `att-${index}`, title: 'เข้างาน', date: dateStr, isAllDay: true, type: 'attendance', status: 'approved' });
            } else if (day.status === 'Absent') {
              events.push({ id: `abs-${index}`, title: 'ขาดงาน', date: dateStr, isAllDay: true, type: 'attendance', status: 'absent' });
            } else if (day.status === 'Leave') {
              events.push({ id: `leave-${index}`, title: 'ลา', date: dateStr, isAllDay: true, type: 'leave', status: 'approved' });
            } else if (day.status === 'Holiday') {
              events.push({ id: `hol-${index}`, title: 'วันหยุด', date: dateStr, isAllDay: true, type: 'holiday', status: 'approved' });
            }
          });
        }
        return events;
      })
    );
  }
}
