import { Component, inject, OnInit, signal, computed, ChangeDetectionStrategy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CalendarService, CalendarEvent } from '../services/calendar.service';
import { finalize } from 'rxjs/operators';

type ViewMode = 'month' | 'week' | 'workWeek' | 'day';

interface CalendarDay {
  date: Date;
  dateStr: string;
  isCurrentMonth: boolean;
  isToday: boolean;
  allDayEvents: CalendarEvent[];
  timeEvents: CalendarEvent[];
}

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="calendar-layout">
      <!-- 3. Command Bar (Teams Style) -->
      <div class="command-bar">
        <div class="cmd-left">
          <h2>ปฏิทิน</h2>
          <div class="nav-actions">
            <button class="btn-icon" (click)="prev()">◀</button>
            <button class="btn-today" (click)="goToToday()">วันนี้</button>
            <button class="btn-icon" (click)="next()">▶</button>
            <span class="current-date-display">{{ displayDateRange() }}</span>
          </div>
        </div>

        <div class="cmd-center">
          <div class="filter-bar">
            <label class="filter-check"><input type="checkbox" [(ngModel)]="filters.attendance" (change)="applyFilters()"> ลงเวลา</label>
            <label class="filter-check"><input type="checkbox" [(ngModel)]="filters.leaves" (change)="applyFilters()"> วันลา</label>
            <label class="filter-check"><input type="checkbox" [(ngModel)]="filters.tasks" (change)="applyFilters()"> งานมอบหมาย</label>
            <label class="filter-check"><input type="checkbox" [(ngModel)]="filters.visits" (change)="applyFilters()"> การนิเทศ</label>
          </div>
        </div>

        <div class="cmd-right">
          <div class="view-toggle">
            <select [(ngModel)]="currentView" (change)="generateCalendar()">
              <option value="month">เดือน (Month)</option>
              <option value="week">สัปดาห์ (Week)</option>
              <option value="workWeek">วันทำงาน (Mon-Fri)</option>
              <option value="day">วัน (Day)</option>
            </select>
          </div>
          <button class="btn-primary">+ เพิ่มบันทึก</button>
        </div>
      </div>

      <!-- Main Calendar Area -->
      <div class="calendar-content">
        <div *ngIf="isLoading()" class="loading-overlay">
          <div class="spinner"></div>
          <span>กำลังโหลดข้อมูล...</span>
        </div>

        <!-- MONTH VIEW -->
        <div class="month-grid-container" *ngIf="currentView() === 'month' && !isLoading()">
          <div class="weekday-header-row">
            <div class="wh-cell text-danger">อาทิตย์</div>
            <div class="wh-cell">จันทร์</div>
            <div class="wh-cell">อังคาร</div>
            <div class="wh-cell">พุธ</div>
            <div class="wh-cell">พฤหัสบดี</div>
            <div class="wh-cell">ศุกร์</div>
            <div class="wh-cell text-primary">เสาร์</div>
          </div>
          <div class="month-grid">
            <div *ngFor="let day of calendarDays()" 
                 class="month-cell" 
                 [class.other-month]="!day.isCurrentMonth"
                 [class.is-today]="day.isToday"
                 (click)="openDrawerForNew(day.date)">
              
              <div class="mc-header">
                <span class="mc-date" [class.active]="day.isToday">{{ day.date | date:'d' }}</span>
              </div>
              
              <div class="mc-events">
                <!-- All-day events -->
                <div *ngFor="let evt of day.allDayEvents" 
                     class="evt-allday" 
                     [ngClass]="getEventColorClass(evt)"
                     (click)="openDrawer(evt, $event)"
                     [title]="evt.title">
                  {{ evt.title }}
                </div>
                <!-- Time events -->
                <div *ngFor="let evt of day.timeEvents" 
                     class="evt-time" 
                     (click)="openDrawer(evt, $event)"
                     [title]="evt.startTime + ' - ' + evt.title">
                  <span class="evt-dot" [ngClass]="getEventColorClass(evt)"></span>
                  <span class="evt-time-text">{{ evt.startTime }}</span>
                  <span class="evt-title-text">{{ evt.title }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- WEEK / WORK-WEEK / DAY VIEW (Time Slots) -->
        <div class="time-grid-container" *ngIf="(currentView() === 'week' || currentView() === 'workWeek' || currentView() === 'day') && !isLoading()">
          
          <div class="tg-header-row">
            <div class="tg-time-axis-header">GMT+07</div>
            <div *ngFor="let day of calendarDays()" class="tg-day-header" [class.is-today]="day.isToday">
              <div class="day-name">{{ day.date | date:'EEE' }}</div>
              <div class="day-num" [class.active]="day.isToday">{{ day.date | date:'d' }}</div>
              
              <!-- All day events area in header -->
              <div class="allday-zone">
                <div *ngFor="let evt of day.allDayEvents" 
                     class="evt-allday" 
                     [ngClass]="getEventColorClass(evt)"
                     (click)="openDrawer(evt, $event)">
                  {{ evt.title }}
                </div>
              </div>
            </div>
          </div>

          <div class="tg-body" #scrollBody>
            <div class="tg-time-axis">
              <div *ngFor="let hr of hours" class="time-slot-label">
                <span>{{ hr }}:00</span>
              </div>
            </div>
            <div class="tg-columns">
              <!-- Background grid lines -->
              <div class="grid-lines">
                <div *ngFor="let hr of hours" class="grid-line-row"></div>
              </div>
              
              <!-- Day columns -->
              <div *ngFor="let day of calendarDays()" class="tg-day-col" (click)="openDrawerForNew(day.date)">
                <!-- Absolute positioned time events -->
                <div *ngFor="let evt of day.timeEvents" 
                     class="evt-absolute" 
                     [ngStyle]="getEventPosition(evt)"
                     [ngClass]="getEventColorClass(evt)"
                     (click)="openDrawer(evt, $event)">
                  <div class="evt-abs-time">{{ evt.startTime }}</div>
                  <div class="evt-abs-title">{{ evt.title }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- 4. Side Drawer (Teams Style Overlay) -->
    <div class="drawer-backdrop" *ngIf="isDrawerOpen()" (click)="closeDrawer()"></div>
    <div class="side-drawer" [class.open]="isDrawerOpen()">
      <div class="drawer-header">
        <h3 class="m-0">{{ selectedEvent() ? selectedEvent()?.title : 'เพิ่มบันทึกใหม่' }}</h3>
        <button class="btn-close" (click)="closeDrawer()">✖</button>
      </div>
      
      <div class="drawer-content-scroll" *ngIf="selectedEvent() as evt">
        <div class="detail-group">
          <label>วันที่</label>
          <p>{{ evt.date | date:'EEEE, d MMMM yyyy' }}</p>
        </div>
        <div class="detail-group" *ngIf="!evt.isAllDay">
          <label>เวลา</label>
          <p>{{ evt.startTime }} - {{ evt.endTime || '...' }}</p>
        </div>
        <div class="detail-group">
          <label>ประเภท</label>
          <p class="badge" [ngClass]="getEventColorClass(evt)">{{ evt.type | uppercase }}</p>
        </div>
        <div class="detail-group">
          <label>สถานะ</label>
          <p>{{ evt.status | uppercase }}</p>
        </div>
      </div>

      <div class="drawer-content-scroll" *ngIf="!selectedEvent()">
        <p class="text-muted">สร้างบันทึกสำหรับวันที่ {{ selectedDate() | date:'d MMM yyyy' }}</p>
        <!-- Form placeholders -->
        <div class="form-group mt-3">
          <label>ประเภทบันทึก</label>
          <select class="form-control"><option>บันทึกงาน (Work Log)</option><option>ขอลา (Leave)</option></select>
        </div>
      </div>

      <div class="drawer-footer">
        <button class="btn-outline" (click)="closeDrawer()">ปิด</button>
        <button class="btn-primary ml-2" *ngIf="!selectedEvent()">บันทึก</button>
        <button class="btn-primary ml-2" *ngIf="selectedEvent()?.status === 'pending'">✅ อนุมัติ</button>
      </div>
    </div>
  `,
  styles: [`
    /* Core Layout */
    .calendar-layout { display: flex; flex-direction: column; height: 100vh; background: #F5F6FA; font-family: 'Sarabun', sans-serif; overflow: hidden; }
    h2, h3 { font-family: 'Prompt', sans-serif; color: #1F2328; }
    .m-0 { margin: 0; }
    .mt-3 { margin-top: 16px; }
    .ml-2 { margin-left: 8px; }
    
    /* 3. Command Bar */
    .command-bar { display: flex; justify-content: space-between; align-items: center; padding: 12px 24px; background: #FFFFFF; border-bottom: 1px solid #E5E7EB; flex-shrink: 0; }
    .cmd-left { display: flex; align-items: center; gap: 24px; }
    .cmd-left h2 { margin: 0; font-size: 20px; font-weight: 600; }
    .nav-actions { display: flex; align-items: center; gap: 8px; }
    .btn-icon, .btn-today { background: #FFFFFF; border: 1px solid #D1D5DB; border-radius: 6px; cursor: pointer; color: #374151; font-family: 'Sarabun'; font-size: 14px; padding: 6px 12px; transition: 0.2s; }
    .btn-icon { padding: 6px 10px; font-size: 12px; }
    .btn-icon:hover, .btn-today:hover { background: #F3F4F6; }
    .current-date-display { font-family: 'Prompt', sans-serif; font-weight: 500; font-size: 16px; margin-left: 12px; min-width: 150px; }
    
    .filter-bar { display: flex; gap: 16px; align-items: center; background: #F9FAFB; padding: 6px 16px; border-radius: 8px; border: 1px solid #E5E7EB; }
    .filter-check { font-size: 14px; color: #4B5563; display: flex; align-items: center; gap: 6px; cursor: pointer; }
    
    .cmd-right { display: flex; align-items: center; gap: 16px; }
    .view-toggle select { padding: 8px 12px; border: 1px solid #D1D5DB; border-radius: 6px; background: #FFF; font-family: 'Sarabun'; cursor: pointer; outline: none; }
    .btn-primary { background: #5B5FC7; color: white; border: none; padding: 8px 16px; border-radius: 6px; font-weight: 600; font-family: 'Prompt'; cursor: pointer; transition: 0.2s; }
    .btn-primary:hover { background: #464AB9; }
    .btn-outline { background: #FFF; color: #374151; border: 1px solid #D1D5DB; padding: 8px 16px; border-radius: 6px; font-weight: 600; font-family: 'Prompt'; cursor: pointer; }
    .btn-outline:hover { background: #F3F4F6; }

    /* Calendar Content Area */
    .calendar-content { flex: 1; overflow: hidden; position: relative; display: flex; flex-direction: column; background: #FFFFFF; }
    
    .loading-overlay { position: absolute; inset: 0; background: rgba(255,255,255,0.8); z-index: 10; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #5B5FC7; font-weight: 600; }
    .spinner { width: 32px; height: 32px; border: 3px solid #EEF0FF; border-top-color: #5B5FC7; border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 12px; }
    @keyframes spin { to { transform: rotate(360deg); } }

    /* --- Month View --- */
    .month-grid-container { display: flex; flex-direction: column; height: 100%; }
    .weekday-header-row { display: grid; grid-template-columns: repeat(7, 1fr); border-bottom: 1px solid #E5E7EB; }
    .wh-cell { padding: 12px; text-align: center; font-family: 'Prompt'; font-size: 14px; font-weight: 500; color: #4B5563; }
    .text-danger { color: #DC2626; } .text-primary { color: #5B5FC7; }
    
    .month-grid { flex: 1; display: grid; grid-template-columns: repeat(7, 1fr); grid-auto-rows: minmax(100px, 1fr); background: #E5E7EB; gap: 1px; border-bottom: 1px solid #E5E7EB; }
    .month-cell { background: #FFFFFF; display: flex; flex-direction: column; cursor: pointer; transition: background 0.2s; overflow: hidden; }
    .month-cell:hover { background: #F9FAFB; }
    .month-cell.other-month { background: #F9FAFB; opacity: 0.5; }
    .mc-header { padding: 8px 8px 4px 8px; display: flex; justify-content: flex-end; }
    .mc-date { width: 24px; height: 24px; display: flex; align-items: center; justify-content: center; border-radius: 50%; font-size: 14px; color: #1F2328; }
    .mc-date.active { background: #5B5FC7; color: #FFF; font-weight: 600; }
    
    .mc-events { padding: 0 4px; display: flex; flex-direction: column; gap: 2px; overflow-y: auto; max-height: calc(100% - 32px); }
    
    /* Event Items */
    .evt-allday { padding: 2px 6px; border-radius: 4px; font-size: 11px; font-weight: 500; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin-bottom: 2px; }
    .evt-time { display: flex; align-items: center; gap: 4px; padding: 2px 4px; font-size: 11px; border-radius: 4px; }
    .evt-time:hover { background: #F3F4F6; }
    .evt-dot { width: 6px; height: 6px; border-radius: 50%; flex-shrink: 0; }
    .evt-time-text { color: #6B7280; font-weight: 500; }
    .evt-title-text { color: #1F2328; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

    /* --- Time Grid View (Week/Day) --- */
    .time-grid-container { display: flex; flex-direction: column; height: 100%; }
    .tg-header-row { display: flex; border-bottom: 1px solid #E5E7EB; background: #FFFFFF; flex-shrink: 0; }
    .tg-time-axis-header { width: 60px; border-right: 1px solid #E5E7EB; display: flex; align-items: flex-end; justify-content: center; padding-bottom: 8px; font-size: 11px; color: #6B7280; flex-shrink: 0; }
    .tg-day-header { flex: 1; padding: 12px 8px; text-align: center; border-right: 1px solid #E5E7EB; display: flex; flex-direction: column; align-items: center; min-width: 0; }
    .day-name { font-size: 12px; color: #6B7280; text-transform: uppercase; }
    .day-num { font-family: 'Prompt'; font-size: 24px; color: #1F2328; margin: 4px 0; width: 36px; height: 36px; display: flex; align-items: center; justify-content: center; border-radius: 50%; }
    .tg-day-header.is-today .day-num { background: #5B5FC7; color: #FFF; }
    
    .allday-zone { width: 100%; display: flex; flex-direction: column; gap: 2px; margin-top: 4px; min-height: 20px; }
    
    .tg-body { flex: 1; overflow-y: auto; display: flex; background: #FFFFFF; }
    .tg-time-axis { width: 60px; border-right: 1px solid #E5E7EB; flex-shrink: 0; background: #FFFFFF; }
    .time-slot-label { height: 60px; position: relative; }
    .time-slot-label span { position: absolute; top: -8px; right: 8px; font-size: 11px; color: #6B7280; }
    
    .tg-columns { flex: 1; position: relative; display: flex; }
    .grid-lines { position: absolute; inset: 0; z-index: 1; pointer-events: none; }
    .grid-line-row { height: 60px; border-bottom: 1px solid #F3F4F6; }
    
    .tg-day-col { flex: 1; border-right: 1px solid #E5E7EB; position: relative; z-index: 2; cursor: pointer; }
    .tg-day-col:hover { background: rgba(249, 250, 251, 0.5); }
    
    .evt-absolute { position: absolute; left: 4px; right: 4px; border-radius: 4px; padding: 4px 6px; font-size: 11px; overflow: hidden; box-shadow: 0 1px 2px rgba(0,0,0,0.1); border-left-width: 3px; border-left-style: solid; z-index: 10; cursor: pointer; }
    .evt-absolute:hover { filter: brightness(0.95); z-index: 11; }
    .evt-abs-time { font-weight: 600; margin-bottom: 2px; }
    .evt-abs-title { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

    /* 5. Color Coding (Tokens) */
    .bg-success { background: #DCFCE7; color: #14532D; border-color: #16A34A; }
    .bg-warning { background: #FEF3C7; color: #78350F; border-color: #D97706; }
    .bg-danger { background: #FEE2E2; color: #7F1D1D; border-color: #DC2626; }
    .bg-leave { background: #F3E8FF; color: #581C87; border-color: #9333EA; }
    .bg-info { background: #E0F2FE; color: #0C4A6E; border-color: #2563EB; }
    .bg-visit { background: #FFEDD5; color: #7C2D12; border-color: #F97316; }
    .bg-border { background: #F3F4F6; color: #374151; border-color: #E5E7EB; }

    /* 4. Side Drawer Overlay & Animation */
    .drawer-backdrop { position: fixed; inset: 0; background: rgba(31, 35, 40, 0.4); z-index: 1000; animation: fadeIn 0.2s; backdrop-filter: blur(2px); }
    .side-drawer { position: fixed; top: 0; right: 0; bottom: 0; width: 400px; background: #FFFFFF; z-index: 1001; box-shadow: -4px 0 15px rgba(0,0,0,0.1); transform: translateX(100%); transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1); display: flex; flex-direction: column; }
    .side-drawer.open { transform: translateX(0); }
    
    .drawer-header { display: flex; justify-content: space-between; align-items: center; padding: 20px 24px; border-bottom: 1px solid #E5E7EB; background: #F9FAFB; }
    .btn-close { background: none; border: none; font-size: 16px; cursor: pointer; color: #6B7280; padding: 4px; }
    .btn-close:hover { color: #1F2328; }
    
    .drawer-content-scroll { flex: 1; overflow-y: auto; padding: 24px; }
    .drawer-footer { padding: 16px 24px; border-top: 1px solid #E5E7EB; display: flex; justify-content: flex-end; background: #F9FAFB; }
    
    .detail-group { margin-bottom: 16px; }
    .detail-group label { display: block; font-size: 12px; color: #6B7280; margin-bottom: 4px; font-weight: 500; }
    .detail-group p { margin: 0; font-size: 15px; color: #1F2328; }
    .badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 600; }
    .form-group label { display: block; font-size: 13px; font-weight: 500; margin-bottom: 8px; }
    .form-control { width: 100%; padding: 8px 12px; border: 1px solid #D1D5DB; border-radius: 6px; font-family: 'Sarabun'; }
    
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
  `]
})
export class CalendarComponent implements OnInit {
  private calendarService = inject(CalendarService);

  // State
  currentView = signal<ViewMode>('month');
  baseDate = signal<Date>(new Date());
  
  calendarDays = signal<CalendarDay[]>([]);
  allRawEvents = signal<CalendarEvent[]>([]);
  isLoading = signal(true);
  
  // Filters
  filters = { attendance: true, leaves: true, tasks: true, visits: true };

  // Drawer
  isDrawerOpen = signal(false);
  selectedEvent = signal<CalendarEvent | null>(null);
  selectedDate = signal<Date | null>(null);

  hours = Array.from({length: 24}, (_, i) => i);

  ngOnInit() {
    this.generateCalendar();
  }

  generateCalendar() {
    this.isLoading.set(true);
    const date = this.baseDate();
    const year = date.getFullYear();
    const month = date.getMonth();

    // Generate grid based on view
    let days: CalendarDay[] = [];
    const today = new Date();

    if (this.currentView() === 'month') {
      const firstDay = new Date(year, month, 1);
      const startingDay = firstDay.getDay();
      const totalDays = new Date(year, month + 1, 0).getDate();
      const prevMonthDays = new Date(year, month, 0).getDate();
      
      for (let i = startingDay - 1; i >= 0; i--) {
        days.push(this.createDayObj(new Date(year, month - 1, prevMonthDays - i), false, today));
      }
      for (let i = 1; i <= totalDays; i++) {
        days.push(this.createDayObj(new Date(year, month, i), true, today));
      }
      const remain = 42 - days.length;
      for (let i = 1; i <= remain; i++) {
        days.push(this.createDayObj(new Date(year, month + 1, i), false, today));
      }
    } 
    else if (this.currentView() === 'week' || this.currentView() === 'workWeek') {
      const dayOfWeek = date.getDay();
      const diff = date.getDate() - dayOfWeek;
      const startOfWeek = new Date(date.setDate(diff));
      
      let numDays = this.currentView() === 'week' ? 7 : 5;
      let offset = this.currentView() === 'workWeek' ? 1 : 0; // start on Monday for workWeek

      for (let i = 0; i < numDays; i++) {
        const d = new Date(startOfWeek);
        d.setDate(d.getDate() + i + offset);
        days.push(this.createDayObj(d, true, today));
      }
    }
    else if (this.currentView() === 'day') {
      days.push(this.createDayObj(new Date(date), true, today));
    }

    // Fetch API
    this.calendarService.getEvents(year, month + 1, this.currentView()).pipe(finalize(() => this.isLoading.set(false))).subscribe(events => {
      this.allRawEvents.set(events);
      this.mapEventsToDays(days);
    });
  }

  createDayObj(d: Date, isCurrent: boolean, today: Date): CalendarDay {
    return {
      date: d,
      dateStr: this.formatDateStr(d),
      isCurrentMonth: isCurrent,
      isToday: d.getDate() === today.getDate() && d.getMonth() === today.getMonth() && d.getFullYear() === today.getFullYear(),
      allDayEvents: [],
      timeEvents: []
    };
  }

  mapEventsToDays(days: CalendarDay[]) {
    // Apply filters
    const filteredEvents = this.allRawEvents().filter(e => {
      if (e.type === 'attendance' && !this.filters.attendance) return false;
      if (e.type === 'leave' && !this.filters.leaves) return false;
      if (e.type === 'task' && !this.filters.tasks) return false;
      if (e.type === 'visit' && !this.filters.visits) return false;
      return true;
    });

    days.forEach(day => {
      day.allDayEvents = filteredEvents.filter(e => e.date === day.dateStr && e.isAllDay);
      day.timeEvents = filteredEvents.filter(e => e.date === day.dateStr && !e.isAllDay)
                                     .sort((a,b) => (a.startTime || '').localeCompare(b.startTime || ''));
    });
    this.calendarDays.set(days);
  }

  applyFilters() {
    const days = [...this.calendarDays()];
    this.mapEventsToDays(days);
  }

  formatDateStr(date: Date): string {
    return date.getFullYear() + '-' + (date.getMonth() + 1).toString().padStart(2, '0') + '-' + date.getDate().toString().padStart(2, '0');
  }

  // Navigation
  prev() {
    const d = new Date(this.baseDate());
    if (this.currentView() === 'month') d.setMonth(d.getMonth() - 1);
    else if (this.currentView() === 'week' || this.currentView() === 'workWeek') d.setDate(d.getDate() - 7);
    else d.setDate(d.getDate() - 1);
    this.baseDate.set(d);
    this.generateCalendar();
  }

  next() {
    const d = new Date(this.baseDate());
    if (this.currentView() === 'month') d.setMonth(d.getMonth() + 1);
    else if (this.currentView() === 'week' || this.currentView() === 'workWeek') d.setDate(d.getDate() + 7);
    else d.setDate(d.getDate() + 1);
    this.baseDate.set(d);
    this.generateCalendar();
  }

  goToToday() {
    this.baseDate.set(new Date());
    this.generateCalendar();
  }

  displayDateRange(): string {
    const mNames = ['มกราคม', 'กุมภาพันธ์', 'มีนาคม', 'เมษายน', 'พฤษภาคม', 'มิถุนายน', 'กรกฎาคม', 'สิงหาคม', 'กันยายน', 'ตุลาคม', 'พฤศจิกายน', 'ธันวาคม'];
    const d = this.baseDate();
    if (this.currentView() === 'month') {
      return mNames[d.getMonth()] + ' ' + d.getFullYear();
    }
    return d.getDate() + ' ' + mNames[d.getMonth()] + ' ' + d.getFullYear();
  }

  // Visuals
  getEventColorClass(evt: CalendarEvent): string {
    // Color mapping based on specs
    if (evt.status === 'approved' || evt.status === 'done') return 'bg-success';
    if (evt.status === 'pending' || evt.status === 'doing') return 'bg-warning';
    if (evt.status === 'late' || evt.status === 'absent' || evt.status === 'todo') return 'bg-danger';
    if (evt.type === 'leave') return 'bg-leave';
    if (evt.type === 'task') return 'bg-info';
    if (evt.type === 'visit') return 'bg-visit';
    if (evt.type === 'holiday') return 'bg-border';
    return 'bg-border';
  }

  getEventPosition(evt: CalendarEvent): any {
    if (!evt.startTime) return {};
    const [h1, m1] = evt.startTime.split(':').map(Number);
    const startMins = h1 * 60 + m1;
    
    let endMins = startMins + 60; // default 1 hr
    if (evt.endTime) {
      const [h2, m2] = evt.endTime.split(':').map(Number);
      endMins = h2 * 60 + m2;
    }
    
    // Each hour is 60px height
    const top = startMins;
    const height = Math.max(endMins - startMins, 20); // min 20px
    return { top: top + 'px', height: height + 'px' };
  }

  // Drawer
  openDrawer(evt: CalendarEvent, mouseEvent: Event) {
    mouseEvent.stopPropagation();
    this.selectedEvent.set(evt);
    this.isDrawerOpen.set(true);
  }

  openDrawerForNew(date: Date) {
    this.selectedEvent.set(null);
    this.selectedDate.set(date);
    this.isDrawerOpen.set(true);
  }

  closeDrawer() {
    this.isDrawerOpen.set(false);
  }
}
