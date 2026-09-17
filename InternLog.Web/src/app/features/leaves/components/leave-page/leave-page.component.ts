import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LeaveService } from '../../services/leave.service';
import { LeaveQuota, LeaveResponse, LeaveRequest } from '../../models/leave.model';

@Component({
  selector: 'app-leave-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-container">
      <header class="page-header">
        <div>
          <h1>วันลา (Leaves)</h1>
          <p>ดูโควตาวันลาและยื่นขอลาสำหรับนักศึกษา</p>
        </div>
      </header>

      <!-- Loading & Error States -->
      <div *ngIf="isLoading()" class="loading-state">
        <div class="spinner"></div>
        <p>กำลังโหลดข้อมูล...</p>
      </div>
      <div *ngIf="errorMsg()" class="error-state">
        <p>{{ errorMsg() }}</p>
        <button class="btn-outline" (click)="loadData()">ลองใหม่</button>
      </div>

      <ng-container *ngIf="!isLoading() && !errorMsg()">
        <div class="main-grid">
          
          <!-- Left Column (8 cols): Quota Cards vertically -->
          <div class="left-column">
            <section class="quota-list">
              <div class="quota-card" *ngFor="let q of quotas()">
                <div class="quota-header">
                  <div class="quota-icon">
                    <span *ngIf="q.leaveTypeID === 1">🤒</span>
                    <span *ngIf="q.leaveTypeID === 2">📝</span>
                    <span *ngIf="q.leaveTypeID === 3">🏖️</span>
                  </div>
                  <h3>{{ q.typeName }}</h3>
                </div>
                <div class="quota-stats">
                  <div class="stat-box">
                    <span class="label">ใช้ไป</span>
                    <span class="value used">{{ q.usedDays }}</span>
                  </div>
                  <div class="stat-box">
                    <span class="label">คงเหลือ</span>
                    <span class="value remain">{{ q.remainingDays }}</span>
                  </div>
                </div>
                <div class="progress-bar-container">
                  <div class="progress-bar" [style.width.%]="(q.usedDays / q.maxDays) * 100" 
                       [ngClass]="{'danger': q.remainingDays === 0}"></div>
                </div>
                <div class="quota-total">โควตาทั้งหมด {{ q.maxDays }} วัน</div>
              </div>
            </section>
          </div>

          <!-- Right Column (4 cols): Leave Form -->
          <div class="right-column">
            <div class="form-card">
              <h2>แบบฟอร์มยื่นขอลา</h2>
              <form [formGroup]="leaveForm" (ngSubmit)="onSubmit()">
                <div class="form-group">
                  <label>ประเภทการลา</label>
                  <select formControlName="leaveTypeID" [class.is-invalid]="leaveForm.get('leaveTypeID')?.invalid && leaveForm.get('leaveTypeID')?.touched">
                    <option [value]="null" disabled>-- เลือกประเภทการลา --</option>
                    <option [value]="q.leaveTypeID" *ngFor="let q of quotas()">{{ q.typeName }}</option>
                  </select>
                  <small class="error" *ngIf="leaveForm.get('leaveTypeID')?.invalid && leaveForm.get('leaveTypeID')?.touched">
                    กรุณาเลือกประเภทการลา
                  </small>
                </div>

                <div class="form-row">
                  <div class="form-group">
                    <label>ตั้งแต่วันที่</label>
                    <input type="date" formControlName="startDate" [class.is-invalid]="leaveForm.get('startDate')?.invalid && leaveForm.get('startDate')?.touched">
                  </div>
                  <div class="form-group">
                    <label>ถึงวันที่</label>
                    <input type="date" formControlName="endDate" [class.is-invalid]="leaveForm.get('endDate')?.invalid && leaveForm.get('endDate')?.touched">
                  </div>
                </div>
                <small class="error" *ngIf="leaveForm.errors?.['dateRange'] && leaveForm.get('endDate')?.touched">
                  วันที่สิ้นสุดต้องไม่ก่อนวันที่เริ่มต้น
                </small>

                <div class="form-group">
                  <label>เหตุผลการลา</label>
                  <textarea formControlName="reason" rows="3" placeholder="ระบุเหตุผลการลา..." [class.is-invalid]="leaveForm.get('reason')?.invalid && leaveForm.get('reason')?.touched"></textarea>
                  <small class="error" *ngIf="leaveForm.get('reason')?.invalid && leaveForm.get('reason')?.touched">
                    กรุณาระบุเหตุผลการลา
                  </small>
                </div>

                <div class="form-group">
                  <label>ใบรับรองแพทย์ / ไฟล์แนบ</label>
                  <div class="file-upload-zone">
                    <input type="file" (change)="onFileSelected($event)" accept="image/*,.pdf" id="fileUpload">
                    <label for="fileUpload" class="file-label">
                      📎 <span *ngIf="!selectedFile">เลือกไฟล์ (ถ้ามี)</span><span *ngIf="selectedFile">{{ selectedFile.name }}</span>
                    </label>
                  </div>
                </div>

                <button type="submit" class="btn-primary w-100" [disabled]="leaveForm.invalid || isSubmitting()">
                  {{ isSubmitting() ? 'กำลังส่งข้อมูล...' : 'ส่งคำขอลา' }}
                </button>
              </form>
            </div>
          </div>

        </div>

        <!-- Bottom: History Table -->
        <section class="history-section">
          <div class="section-header">
            <h2>ประวัติคำขอลา</h2>
            <div class="filters">
              <button class="filter-btn" [class.active]="currentFilter() === ''" (click)="setFilter('')">ทั้งหมด</button>
              <button class="filter-btn" [class.active]="currentFilter() === 'pending'" (click)="setFilter('pending')">รออนุมัติ</button>
              <button class="filter-btn" [class.active]="currentFilter() === 'approved'" (click)="setFilter('approved')">อนุมัติ</button>
              <button class="filter-btn" [class.active]="currentFilter() === 'rejected'" (click)="setFilter('rejected')">ไม่อนุมัติ</button>
            </div>
          </div>

          <div *ngIf="filteredLeaves().length === 0" class="empty-state">
            <p>ไม่พบประวัติการลาในสถานะที่คุณเลือก</p>
          </div>

          <div class="table-container" *ngIf="filteredLeaves().length > 0">
            <table>
              <thead>
                <tr>
                  <th>ประเภท</th>
                  <th>ตั้งแต่ – ถึง</th>
                  <th>จำนวนวัน</th>
                  <th>เหตุผล</th>
                  <th>ไฟล์แนบ</th>
                  <th>สถานะ</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let leave of filteredLeaves(); trackBy: trackByLeaveId">
                  <td><span class="type-badge">{{ leave.leaveTypeName }}</span></td>
                  <td>{{ leave.startDate | date:'dd/MM/yyyy' }} – {{ leave.endDate | date:'dd/MM/yyyy' }}</td>
                  <td>{{ leave.totalDays }} วัน</td>
                  <td class="reason-cell" [title]="leave.reason">{{ leave.reason }}</td>
                  <td>
                    <a *ngIf="leave.attachmentPath" [href]="leave.attachmentPath" target="_blank" class="link">📎 ดูไฟล์</a>
                    <span *ngIf="!leave.attachmentPath" class="text-muted">-</span>
                  </td>
                  <td>
                    <span class="status-badge" [ngClass]="leave.status">
                      {{ getStatusText(leave.status) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </ng-container>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 1440px; margin: 0 auto; color: #1F2328; font-family: 'Sarabun', sans-serif; }
    .page-header { margin-bottom: 24px; }
    h1 { margin: 0 0 8px 0; font-size: 24px; color: #1F2328; font-family: 'Prompt', sans-serif; font-weight: 700; }
    h2 { font-size: 20px; font-family: 'Prompt', sans-serif; font-weight: 600; margin-top: 0; margin-bottom: 20px; }
    p { margin: 0; color: #6B7280; }
    
    .main-grid { display: grid; grid-template-columns: 8fr 4fr; gap: 24px; margin-bottom: 24px; }
    @media (max-width: 900px) { .main-grid { grid-template-columns: 1fr; } }
    
    .quota-list { display: flex; flex-direction: column; gap: 16px; }
    .quota-card { background: #FFFFFF; border-radius: 12px; padding: 20px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; }
    .quota-header { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; }
    .quota-icon { font-size: 24px; width: 40px; height: 40px; background: #F5F6FA; border-radius: 8px; display: flex; align-items: center; justify-content: center; }
    .quota-card h3 { margin: 0; font-size: 16px; font-family: 'Prompt', sans-serif; color: #1F2328; }
    
    .quota-stats { display: flex; gap: 32px; margin-bottom: 16px; }
    .stat-box { display: flex; flex-direction: column; }
    .stat-box .label { font-size: 14px; color: #6B7280; }
    .stat-box .value { font-size: 24px; font-weight: 700; font-family: 'Prompt', sans-serif; }
    .stat-box .used { color: #5B5FC7; }
    .stat-box .remain { color: #16A34A; }
    
    .progress-bar-container { height: 8px; background: #E5E7EB; border-radius: 4px; overflow: hidden; margin-bottom: 8px; }
    .progress-bar { height: 100%; background: #5B5FC7; border-radius: 4px; transition: width 0.3s ease; }
    .progress-bar.danger { background: #DC2626; }
    .quota-total { font-size: 12px; color: #6B7280; text-align: right; }

    .form-card { background: #FFFFFF; border-radius: 12px; padding: 24px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; height: 100%; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; font-size: 14px; color: #1F2328; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 10px; border: 1px solid #E5E7EB; border-radius: 8px; font-family: inherit; font-size: 14px; }
    .form-group input:focus, .form-group select:focus, .form-group textarea:focus { border-color: #5B5FC7; outline: none; }
    .form-group input.is-invalid, .form-group select.is-invalid, .form-group textarea.is-invalid { border-color: #DC2626; }
    .form-row { display: flex; gap: 16px; }
    .form-row .form-group { flex: 1; }
    .error { color: #DC2626; font-size: 12px; margin-top: 4px; display: block; }
    
    .file-upload-zone { border: 2px dashed #5B5FC7; background: rgba(91,95,199,0.05); border-radius: 8px; text-align: center; position: relative; }
    .file-upload-zone input[type="file"] { opacity: 0; position: absolute; top: 0; left: 0; width: 100%; height: 100%; cursor: pointer; }
    .file-label { display: block; padding: 20px; color: #5B5FC7; font-weight: 500; pointer-events: none; }

    .btn-primary { padding: 12px 16px; background: #5B5FC7; color: white; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; font-family: inherit; transition: 0.2s; }
    .btn-primary:hover { background: #464AB9; }
    .btn-primary:disabled { background: #a5a7db; cursor: not-allowed; }
    .w-100 { width: 100%; }

    .history-section { background: #FFFFFF; border-radius: 12px; padding: 24px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; }
    .section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .filters { display: flex; gap: 8px; }
    .filter-btn { padding: 6px 16px; border: 1px solid #E5E7EB; background: white; border-radius: 20px; cursor: pointer; font-size: 14px; color: #6B7280; }
    .filter-btn.active { background: #5B5FC7; color: white; border-color: #5B5FC7; }

    .table-container { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; text-align: left; }
    th { padding: 12px 16px; background: #F5F6FA; color: #6B7280; font-weight: 600; font-size: 12px; text-transform: uppercase; border-bottom: 1px solid #E5E7EB; }
    td { padding: 16px; border-bottom: 1px solid #E5E7EB; font-size: 14px; vertical-align: middle; }
    
    .type-badge { background: #F5F6FA; padding: 4px 10px; border-radius: 12px; font-size: 12px; color: #1F2328; }
    .reason-cell { max-width: 250px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .link { color: #5B5FC7; text-decoration: none; font-weight: 500; }
    .link:hover { text-decoration: underline; }
    .text-muted { color: #6B7280; }
    
    .status-badge { padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 500; }
    .status-badge.pending { background: #FEF3C7; color: #D97706; }
    .status-badge.approved { background: #DCFCE7; color: #16A34A; }
    .status-badge.rejected { background: #FEE2E2; color: #DC2626; }

    .loading-state, .empty-state, .error-state { text-align: center; padding: 60px 20px; color: #6B7280; }
    .spinner { border: 4px solid #F5F6FA; border-top: 4px solid #5B5FC7; border-radius: 50%; width: 40px; height: 40px; animation: spin 1s linear infinite; margin: 0 auto 16px; }
    @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
    .btn-outline { margin-top: 16px; padding: 8px 16px; border: 1px solid #5B5FC7; background: transparent; color: #5B5FC7; border-radius: 8px; cursor: pointer; }
  `]
})
export class LeavePageComponent implements OnInit {
  private leaveService = inject(LeaveService);
  private fb = inject(FormBuilder);

  // State
  quotas = signal<LeaveQuota[]>([]);
  leaves = signal<LeaveResponse[]>([]);
  isLoading = signal(true);
  errorMsg = signal<string | null>(null);
  currentFilter = signal<string>('');
  isSubmitting = signal(false);

  // Form
  selectedFile: File | null = null;
  leaveForm = this.fb.group({
    leaveTypeID: [null as number | null, Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    reason: ['', [Validators.required, Validators.maxLength(500)]]
  }, { validators: this.dateRangeValidator });

  dateRangeValidator(group: any) {
    const start = group.get('startDate')?.value;
    const end = group.get('endDate')?.value;
    if (start && end && new Date(start) > new Date(end)) {
      return { dateRange: true };
    }
    return null;
  }

  // Computed
  filteredLeaves = computed(() => {
    const filter = this.currentFilter();
    const all = this.leaves();
    return filter ? all.filter(l => l.status === filter) : all;
  });

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading.set(true);
    this.errorMsg.set(null);
    
    this.leaveService.getLeaveQuota().subscribe({
      next: (res) => { if (res.success && res.data) this.quotas.set(res.data); },
      error: () => { this.errorMsg.set('ไม่สามารถโหลดข้อมูลโควตาการลาได้'); this.isLoading.set(false); }
    });

    this.leaveService.getLeaves().subscribe({
      next: (res) => { 
        if (res.success && res.data) this.leaves.set(res.data); 
        this.isLoading.set(false); 
      },
      error: () => { 
        this.errorMsg.set('ไม่สามารถโหลดประวัติการลาได้'); 
        this.isLoading.set(false); 
      }
    });
  }

  setFilter(status: string) {
    this.currentFilter.set(status);
  }

  trackByLeaveId(index: number, leave: LeaveResponse): number {
    return leave.leaveID;
  }

  getStatusText(status: string): string {
    switch(status) {
      case 'pending': return 'รออนุมัติ';
      case 'approved': return 'อนุมัติ';
      case 'rejected': return 'ไม่อนุมัติ';
      default: return status;
    }
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) this.selectedFile = file;
  }

  onSubmit() {
    if (this.leaveForm.invalid) {
      this.leaveForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    
    const request: LeaveRequest = {
      leaveTypeID: Number(this.leaveForm.get('leaveTypeID')?.value),
      startDate: this.leaveForm.get('startDate')?.value as string,
      endDate: this.leaveForm.get('endDate')?.value as string,
      reason: this.leaveForm.get('reason')?.value as string,
      attachment: this.selectedFile
    };

    this.leaveService.requestLeave(request).subscribe({
      next: (res) => {
        if (res.success) {
          alert('ยื่นคำขอลาสำเร็จ');
          this.leaveForm.reset();
          this.selectedFile = null;
          this.loadData(); // reload quotas and history
        }
        this.isSubmitting.set(false);
      },
      error: (err) => {
        alert(err.error?.message || 'เกิดข้อผิดพลาดในการยื่นขอลา');
        this.isSubmitting.set(false);
      }
    });
  }
}
