import { Component, inject, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { WorkLogService } from '../../services/work-log.service';
import { WorkLog } from '../../models/work-log.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-work-log-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <header class="page-header">
        <div>
          <h1>บันทึกการปฏิบัติงานรายวัน (Work Logs)</h1>
          <p>บันทึกรายละเอียดงานที่ทำ ปัญหาที่พบ และวิธีแก้ไขในแต่ละวัน</p>
        </div>
        <button class="btn-primary" routerLink="/work-logs/new">
          <span class="icon">➕</span> เพิ่มบันทึกงาน
        </button>
      </header>

      <div *ngIf="errorMessage()" class="alert-error">
        {{ errorMessage() }}
      </div>

      <!-- Filters -->
      <div class="filter-section shadow-card">
        <div class="filter-group">
          <label>เดือน</label>
          <select [value]="currentMonth()" (change)="onMonthChange($event)">
            <option *ngFor="let m of months" [value]="m.value">{{ m.label }}</option>
          </select>
        </div>
        <div class="filter-group">
          <label>ปี</label>
          <select [value]="currentYear()" (change)="onYearChange($event)">
            <option *ngFor="let y of years" [value]="y">{{ y }}</option>
          </select>
        </div>

      </div>

      <!-- List Content -->
      <div *ngIf="isLoading()" class="loading-state shadow-card">
        <div class="spinner-large"></div>
        <p>กำลังโหลดข้อมูล...</p>
      </div>

      <div class="empty-state shadow-card" *ngIf="!isLoading() && filteredLogs().length === 0">
        <div class="icon-placeholder">📝</div>
        <h3>ไม่มีบันทึกงาน</h3>
        <p>คุณยังไม่มีบันทึกงานในเดือนที่เลือก หรือไม่พบข้อมูลตามตัวกรอง</p>
        <button class="btn-outline mt-3" routerLink="/work-logs/new">เริ่มบันทึกงานแรก</button>
      </div>

      <div class="log-grid" *ngIf="!isLoading() && filteredLogs().length > 0">
        <div class="log-card shadow-card" *ngFor="let log of filteredLogs(); trackBy: trackById">
          <div class="card-head">
            <span class="date">{{ log.logDate | date:'dd MMM yyyy' }}</span>

          </div>
          <div class="card-body">
            <h3 class="log-title">{{ log.title }}</h3>
            <p class="log-desc">{{ log.description }}</p>
            <div class="tags">
              <span class="tag category" *ngIf="log.categoryName">📁 {{ log.categoryName }}</span>
              <span class="tag hours" *ngIf="log.hours">⏱️ {{ log.hours }} ชม.</span>
              <span class="tag attachment" *ngIf="log.attachments?.length">📎 {{ log.attachments.length }} ไฟล์</span>
              <span class="tag comment" *ngIf="log.comments?.length">💬 {{ log.comments.length }}</span>
            </div>
          </div>
          <div class="card-foot">
            <a [routerLink]="['/work-logs', log.logID]" class="btn-text">ดูรายละเอียด / แก้ไข →</a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 1440px; margin: 0 auto; color: #1F2328; font-family: 'Sarabun', sans-serif; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; flex-wrap: wrap; gap: 16px; }
    h1 { margin: 0 0 8px 0; font-size: 24px; color: #1F2328; font-family: 'Prompt', sans-serif; font-weight: 700; }
    p { margin: 0; color: #6B7280; font-size: 15px; }
    
    .alert-error { background: #FEE2E2; border-left: 4px solid #DC2626; color: #B91C1C; padding: 12px 16px; border-radius: 8px; font-size: 14px; margin-bottom: 24px; font-weight: 500; }
    .shadow-card { background: #FFFFFF; border-radius: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; }
    
    .filter-section { display: flex; gap: 16px; padding: 16px 24px; margin-bottom: 24px; flex-wrap: wrap; align-items: flex-end; }
    .filter-group { display: flex; flex-direction: column; gap: 6px; min-width: 150px; flex: 1; }
    .filter-group label { font-size: 13px; font-weight: 600; color: #4B5563; }
    select { padding: 10px 12px; border: 1px solid #D1D5DB; border-radius: 8px; font-family: inherit; font-size: 14px; background-color: #fff; outline: none; }
    select:focus { border-color: #5B5FC7; box-shadow: 0 0 0 2px rgba(91,95,199,0.1); }
    
    .log-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 20px; }
    .log-card { display: flex; flex-direction: column; transition: transform 0.2s, box-shadow 0.2s; }
    .log-card:hover { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.08); }
    
    .card-head { padding: 16px 20px; border-bottom: 1px solid #F3F4F6; display: flex; justify-content: space-between; align-items: center; }
    .date { font-weight: 600; color: #374151; font-family: 'Prompt', sans-serif; font-size: 15px; }
    
    .card-body { padding: 20px; flex: 1; }
    .log-title { font-size: 16px; font-weight: 700; color: #1F2328; margin: 0 0 8px 0; font-family: 'Prompt', sans-serif; }
    .log-desc { color: #6B7280; font-size: 14px; margin: 0 0 16px 0; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; line-height: 1.5; }
    
    .tags { display: flex; flex-wrap: wrap; gap: 8px; }
    .tag { font-size: 12px; padding: 4px 10px; border-radius: 6px; font-weight: 500; background: #F3F4F6; color: #4B5563; display: inline-flex; align-items: center; gap: 4px; }
    .tag.category { background: #EEF2FF; color: #4338CA; }
    
    .card-foot { padding: 16px 20px; border-top: 1px solid #F3F4F6; background: #F9FAFB; border-radius: 0 0 12px 12px; text-align: right; }
    .btn-text { color: #5B5FC7; text-decoration: none; font-weight: 600; font-size: 14px; transition: 0.2s; }
    .btn-text:hover { color: #4338CA; text-decoration: underline; }
    
    /* Badges */
    .status-badge { padding: 4px 12px; border-radius: 20px; font-size: 11px; font-weight: 600; font-family: 'Prompt', sans-serif; }
    .status-badge.draft, .status-badge.DRAFT { background: #F3F4F6; color: #4B5563; border: 1px solid #E5E7EB; }
    .status-badge.pending, .status-badge.PENDING { background: #FEF3C7; color: #D97706; }
    .status-badge.approved, .status-badge.APPROVED { background: #DCFCE7; color: #16A34A; }
    .status-badge.rejected, .status-badge.REJECTED { background: #FEE2E2; color: #DC2626; }
    
    /* Buttons */
    .btn-primary { background: #5B5FC7; color: white; border: none; padding: 10px 20px; border-radius: 8px; font-weight: 600; font-family: 'Prompt', sans-serif; font-size: 15px; cursor: pointer; transition: 0.2s; display: inline-flex; align-items: center; gap: 8px; text-decoration: none; }
    .btn-primary:hover { background: #464AB9; }
    
    .btn-outline { background: transparent; color: #5B5FC7; border: 1px solid #5B5FC7; padding: 10px 20px; border-radius: 8px; font-weight: 600; font-family: 'Prompt', sans-serif; font-size: 15px; cursor: pointer; transition: 0.2s; text-decoration: none; display: inline-block; }
    .btn-outline:hover { background: #F5F6FA; }
    
    /* States */
    .loading-state, .empty-state { text-align: center; padding: 60px 20px; color: #6B7280; }
    .icon-placeholder { font-size: 48px; margin-bottom: 16px; opacity: 0.5; }
    .empty-state h3 { font-family: 'Prompt', sans-serif; color: #374151; margin-bottom: 8px; }
    .spinner-large { width: 40px; height: 40px; border: 4px solid #F5F6FA; border-top: 4px solid #5B5FC7; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 16px; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .mt-3 { margin-top: 16px; }
  `]
})
export class WorkLogListComponent implements OnInit {
  private logService = inject(WorkLogService);

  logs = signal<WorkLog[]>([]);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  currentMonth = signal(new Date().getMonth() + 1);
  currentYear = signal(new Date().getFullYear());

  months = [
    { value: 1, label: 'มกราคม' }, { value: 2, label: 'กุมภาพันธ์' }, { value: 3, label: 'มีนาคม' },
    { value: 4, label: 'เมษายน' }, { value: 5, label: 'พฤษภาคม' }, { value: 6, label: 'มิถุนายน' },
    { value: 7, label: 'กรกฎาคม' }, { value: 8, label: 'สิงหาคม' }, { value: 9, label: 'กันยายน' },
    { value: 10, label: 'ตุลาคม' }, { value: 11, label: 'พฤศจิกายน' }, { value: 12, label: 'ธันวาคม' }
  ];
  years = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - i);

  filteredLogs = computed(() => {
    return this.logs();
  });

  ngOnInit() {
    this.loadLogs();
  }

  loadLogs() {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.logService.getStudentLogs(this.currentYear(), this.currentMonth())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.logs.set(res.data);
          } else {
            this.logs.set([]);
          }
        },
        error: (err) => this.errorMessage.set(err.error?.message || 'ไม่สามารถโหลดข้อมูลได้')
      });
  }

  onMonthChange(event: Event) {
    const v = (event.target as HTMLSelectElement).value;
    this.currentMonth.set(Number(v));
    this.loadLogs();
  }

  onYearChange(event: Event) {
    const v = (event.target as HTMLSelectElement).value;
    this.currentYear.set(Number(v));
    this.loadLogs();
  }


  trackById(index: number, log: WorkLog): number {
    return log.logID;
  }
}
