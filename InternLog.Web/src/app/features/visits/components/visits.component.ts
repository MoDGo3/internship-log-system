import { Component, inject, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VisitsService } from '../services/visits.service';
import { AdvisorVisitResponseDto, AdvisorVisitRequestDto } from '../../../models/visit.model';
import { AuthService } from '../../../core/auth/auth.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-visits',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-layout">
      <!-- Command Bar -->
      <div class="command-bar">
        <div class="cmd-left">
          <h2>บันทึกการนิเทศ (Advisor Visits)</h2>
        </div>

        <div class="cmd-center">
          <div class="search-bar">
            <span class="search-icon">🔍</span>
            <input type="text" placeholder="ค้นหาการนิเทศ..." class="search-input">
          </div>
        </div>

        <div class="cmd-right">
          <!-- เฉพาะ Advisor ที่ควรเห็นปุ่มเพิ่ม -->
          <button class="btn-primary" *ngIf="userRole() === 'advisor'" (click)="openDrawer()">
            + บันทึกการนิเทศ
          </button>
        </div>
      </div>

      <!-- Main Content -->
      <div class="page-content">
        <div *ngIf="isLoading()" class="loading-overlay">
          <div class="spinner"></div>
          <span>กำลังโหลดข้อมูลการนิเทศ...</span>
        </div>

        <div class="table-container shadow-card" *ngIf="!isLoading()">
          <table class="data-table">
            <thead>
              <tr>
                <th>คนที่</th>
                <th>ครั้งที่</th>
                <th>วันที่นิเทศ</th>
                <th>ชื่ออาจารย์</th>
                <th>รูปแบบ</th>
                <th>สรุปผล</th>
                <th>คะแนน</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let visit of visits()">
                <td class="text-center">{{ visit.personNo }}</td>
                <td class="text-center">{{ visit.visitNo }}</td>
                <td>{{ visit.visitDate | date:'d MMM yyyy' }}</td>
                <td>{{ visit.advisorName }}</td>
                <td>
                  <span class="badge" [ngClass]="visit.visitType === 'Onsite' ? 'badge-primary' : 'badge-info'">
                    {{ visit.visitType }}
                  </span>
                </td>
                <td class="summary-cell">{{ visit.summary || '-' }}</td>
                <td class="text-center">
                  <span class="score-text" *ngIf="visit.score">{{ visit.score }}/100</span>
                  <span class="text-muted" *ngIf="!visit.score">-</span>
                </td>
                <td>
                  <div class="action-btns">
                    <button class="btn-icon text-primary" title="ดูรายละเอียด" (click)="viewDetails(visit)">👁️</button>
                    <button class="btn-icon text-warning" title="แก้ไข" *ngIf="userRole() === 'advisor'" (click)="openDrawer(visit)">✏️</button>
                  </div>
                </td>
              </tr>
              
              <tr *ngIf="visits().length === 0">
                <td colspan="8" class="empty-state">
                  <div class="empty-icon">🧑‍🏫</div>
                  <p>ยังไม่มีบันทึกการนิเทศ</p>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Side Drawer (Form & Details) -->
    <div class="drawer-backdrop" *ngIf="isDrawerOpen()" (click)="closeDrawer()"></div>
    <div class="side-drawer shadow-drawer" [class.open]="isDrawerOpen()">
      <div class="drawer-header">
        <h3 class="m-0">{{ viewMode() ? 'รายละเอียดการนิเทศ' : (editId() ? 'แก้ไขบันทึกการนิเทศ' : 'เพิ่มบันทึกการนิเทศ') }}</h3>
        <button class="btn-close" (click)="closeDrawer()">✖</button>
      </div>
      
      <div class="drawer-content-scroll">
        <!-- View Mode -->
        <div *ngIf="viewMode() && currentViewData" class="view-details">
          <div class="detail-group">
            <label>ลำดับ</label>
            <p>นักศึกษาคนที่: {{ currentViewData.personNo }} | การนิเทศครั้งที่: {{ currentViewData.visitNo }}</p>
          </div>
          <div class="detail-group">
            <label>วันที่นิเทศ</label>
            <p>{{ currentViewData.visitDate | date:'EEEE, d MMMM yyyy' }}</p>
          </div>
          <div class="detail-group">
            <label>ชื่ออาจารย์</label>
            <p>{{ currentViewData.advisorName }}</p>
          </div>
          <div class="detail-group">
            <label>รูปแบบ</label>
            <p><span class="badge" [ngClass]="currentViewData.visitType === 'Onsite' ? 'badge-primary' : 'badge-info'">{{ currentViewData.visitType }}</span></p>
          </div>
          <div class="detail-group">
            <label>สรุปผลการนิเทศ</label>
            <div class="summary-box">{{ currentViewData.summary || 'ไม่มีสรุปผล' }}</div>
          </div>
          <div class="detail-group" *ngIf="currentViewData.score">
            <label>คะแนน</label>
            <p class="score-large">{{ currentViewData.score }} / 100</p>
          </div>
        </div>

        <!-- Edit/Add Mode -->
        <form *ngIf="!viewMode()" (ngSubmit)="saveVisit()">
          
          <div class="form-group mt-3" *ngIf="!editId()">
             <label>รหัสนักศึกษา (Internship ID เบื้องต้นใส่ mock เลข 1 ไปก่อน เนื่องจาก UI เดิมไม่มี)</label>
             <input type="number" class="form-control" [(ngModel)]="formData.internshipID" name="internshipID" required>
          </div>

          <div class="form-row mt-3">
            <div class="form-group half">
              <label>นักศึกษาคนที่</label>
              <input type="number" class="form-control" [(ngModel)]="formData.personNo" name="personNo" required>
            </div>
            <div class="form-group half">
              <label>การนิเทศครั้งที่</label>
              <input type="number" class="form-control" [(ngModel)]="formData.visitNo" name="visitNo" required>
            </div>
          </div>

          <div class="form-group mt-3">
            <label>วันที่นิเทศ</label>
            <input type="date" class="form-control" [(ngModel)]="formData.visitDate" name="visitDate" required>
          </div>

          <div class="form-group mt-3">
            <label>รูปแบบการนิเทศ</label>
            <select class="form-control" [(ngModel)]="formData.visitType" name="visitType">
              <option value="Onsite">Onsite (เข้าพบ ณ สถานประกอบการ)</option>
              <option value="Online">Online (ผ่านระบบออนไลน์)</option>
            </select>
          </div>

          <div class="form-group mt-3">
            <label>สรุปผลการนิเทศ</label>
            <textarea class="form-control" rows="5" [(ngModel)]="formData.summary" name="summary" placeholder="พิมพ์สรุปผลการนิเทศที่นี่..."></textarea>
          </div>

          <div class="form-group mt-3">
            <label>คะแนน (ถ้ามี)</label>
            <input type="number" class="form-control" [(ngModel)]="formData.score" name="score" placeholder="0 - 100">
          </div>
        </form>
      </div>

      <div class="drawer-footer">
        <button class="btn-outline" (click)="closeDrawer()">ปิด</button>
        <button class="btn-primary ml-2" *ngIf="!viewMode()" (click)="saveVisit()" [disabled]="isSaving()">
          {{ isSaving() ? 'กำลังบันทึก...' : 'บันทึก' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    /* Layout */
    .page-layout { display: flex; flex-direction: column; height: 100vh; background: #F5F6FA; font-family: 'Sarabun', sans-serif; overflow: hidden; }
    h2, h3 { font-family: 'Prompt', sans-serif; color: #1F2328; }
    .m-0 { margin: 0; }
    .mt-3 { margin-top: 16px; }
    .ml-2 { margin-left: 8px; }
    
    /* Command Bar */
    .command-bar { display: flex; justify-content: space-between; align-items: center; padding: 12px 24px; background: #FFFFFF; border-bottom: 1px solid #E5E7EB; flex-shrink: 0; }
    .cmd-left h2 { margin: 0; font-size: 20px; font-weight: 600; }
    
    .search-bar { display: flex; align-items: center; background: #F9FAFB; border: 1px solid #D1D5DB; border-radius: 6px; padding: 6px 12px; width: 300px; }
    .search-icon { color: #6B7280; font-size: 14px; margin-right: 8px; }
    .search-input { border: none; background: transparent; outline: none; width: 100%; font-family: 'Sarabun'; font-size: 14px; }
    
    .btn-primary { background: #5B5FC7; color: white; border: none; padding: 8px 16px; border-radius: 6px; font-weight: 600; font-family: 'Prompt'; cursor: pointer; transition: 0.2s; }
    .btn-primary:hover { background: #464AB9; }
    .btn-primary:disabled { background: #9CA3AF; cursor: not-allowed; }
    .btn-outline { background: #FFF; color: #374151; border: 1px solid #D1D5DB; padding: 8px 16px; border-radius: 6px; font-weight: 600; font-family: 'Prompt'; cursor: pointer; }
    .btn-outline:hover { background: #F3F4F6; }

    /* Content & Table */
    .page-content { flex: 1; overflow-y: auto; padding: 24px; position: relative; }
    .shadow-card { background: #FFFFFF; border-radius: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; overflow: hidden; }
    
    .data-table { width: 100%; border-collapse: collapse; font-size: 14px; }
    .data-table th { background: #F9FAFB; padding: 12px 16px; text-align: left; font-family: 'Prompt'; font-weight: 600; color: #374151; border-bottom: 1px solid #E5E7EB; }
    .data-table td { padding: 16px; border-bottom: 1px solid #E5E7EB; color: #1F2328; }
    .data-table tbody tr:hover { background: #F9FAFB; }
    
    .text-center { text-align: center !important; }
    .text-muted { color: #6B7280; }
    .summary-cell { max-width: 250px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .score-text { font-family: 'Prompt'; font-weight: 600; color: #16A34A; }
    
    /* Badges */
    .badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 600; }
    .badge-primary { background: #EEF2FF; color: #5B5FC7; border: 1px solid #5B5FC7; }
    .badge-info { background: #E0F2FE; color: #0284C7; border: 1px solid #0284C7; }
    
    .action-btns { display: flex; gap: 8px; }
    .btn-icon { background: none; border: none; cursor: pointer; font-size: 16px; transition: 0.2s; padding: 4px; border-radius: 4px; }
    .btn-icon:hover { background: #E5E7EB; }
    
    /* Empty State */
    .empty-state { text-align: center; padding: 48px 0 !important; color: #6B7280; }
    .empty-icon { font-size: 48px; margin-bottom: 16px; opacity: 0.5; }
    
    /* Loading */
    .loading-overlay { position: absolute; inset: 0; background: rgba(255,255,255,0.8); z-index: 10; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #5B5FC7; font-weight: 600; }
    .spinner { width: 32px; height: 32px; border: 3px solid #EEF0FF; border-top-color: #5B5FC7; border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 12px; }
    @keyframes spin { to { transform: rotate(360deg); } }

    /* Side Drawer */
    .drawer-backdrop { position: fixed; inset: 0; background: rgba(31, 35, 40, 0.4); z-index: 1000; animation: fadeIn 0.2s; backdrop-filter: blur(2px); }
    .side-drawer { position: fixed; top: 0; right: 0; bottom: 0; width: 450px; background: #FFFFFF; z-index: 1001; box-shadow: -4px 0 15px rgba(0,0,0,0.1); transform: translateX(100%); transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1); display: flex; flex-direction: column; }
    .side-drawer.open { transform: translateX(0); }
    
    .drawer-header { display: flex; justify-content: space-between; align-items: center; padding: 20px 24px; border-bottom: 1px solid #E5E7EB; background: #F9FAFB; }
    .btn-close { background: none; border: none; font-size: 16px; cursor: pointer; color: #6B7280; padding: 4px; }
    .btn-close:hover { color: #1F2328; }
    
    .drawer-content-scroll { flex: 1; overflow-y: auto; padding: 24px; }
    .drawer-footer { padding: 16px 24px; border-top: 1px solid #E5E7EB; display: flex; justify-content: flex-end; background: #F9FAFB; }
    
    /* Forms */
    .form-row { display: flex; gap: 16px; }
    .half { flex: 1; }
    .form-group label { display: block; font-size: 13px; font-weight: 600; color: #374151; margin-bottom: 8px; }
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #D1D5DB; border-radius: 6px; font-family: 'Sarabun'; font-size: 14px; box-sizing: border-box; }
    .form-control:focus { border-color: #5B5FC7; outline: none; box-shadow: 0 0 0 3px rgba(91, 95, 199, 0.1); }
    
    /* View Details */
    .detail-group { margin-bottom: 20px; }
    .detail-group label { display: block; font-size: 13px; color: #6B7280; font-weight: 600; margin-bottom: 4px; }
    .detail-group p { margin: 0; font-size: 15px; color: #1F2328; }
    .summary-box { background: #F9FAFB; border: 1px solid #E5E7EB; padding: 12px; border-radius: 8px; font-size: 14px; color: #374151; min-height: 80px; }
    .score-large { font-family: 'Prompt'; font-size: 24px; font-weight: 700; color: #16A34A; }

    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
  `]
})
export class VisitsComponent implements OnInit {
  private visitsService = inject(VisitsService);
  private authService = inject(AuthService);

  // Read role from actual auth service
  userRole = computed(() => this.authService.currentUser()?.role || 'student'); 
  
  visits = signal<AdvisorVisitResponseDto[]>([]);
  isLoading = signal(true);
  isSaving = signal(false);

  // Drawer State
  isDrawerOpen = signal(false);
  viewMode = signal(false); 
  
  // Track edit mode
  editId = signal<number | null>(null);

  // Default form data matching Request DTO
  emptyForm: AdvisorVisitRequestDto = {
    internshipID: 1, // Need actual internship ID logic from UI context
    personNo: 1,
    visitNo: 1,
    visitDate: new Date().toISOString().split('T')[0],
    visitType: 'Onsite',
    summary: '',
    score: undefined
  };
  
  formData: AdvisorVisitRequestDto = { ...this.emptyForm };
  currentViewData: AdvisorVisitResponseDto | null = null;

  ngOnInit() {
    this.loadVisits();
  }

  loadVisits() {
    this.isLoading.set(true);
    this.visitsService.getVisits().pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (res) => {
        if (res.success) {
          this.visits.set(res.data || []);
        }
      },
      error: (err) => console.error(err)
    });
  }

  openDrawer(visit?: AdvisorVisitResponseDto) {
    this.viewMode.set(false);
    if (visit) {
      this.editId.set(visit.visitID);
      this.formData = {
        internshipID: visit.internshipID,
        personNo: visit.personNo,
        visitNo: visit.visitNo,
        visitDate: visit.visitDate,
        visitType: visit.visitType,
        summary: visit.summary,
        score: visit.score
      };
    } else {
      this.editId.set(null);
      this.formData = { ...this.emptyForm };
    }
    this.isDrawerOpen.set(true);
  }

  viewDetails(visit: AdvisorVisitResponseDto) {
    this.viewMode.set(true);
    this.currentViewData = visit;
    this.isDrawerOpen.set(true);
  }

  closeDrawer() {
    this.isDrawerOpen.set(false);
  }

  saveVisit() {
    this.isSaving.set(true);
    const id = this.editId();
    const request = id ? this.visitsService.updateVisit(id, this.formData) : this.visitsService.saveVisit(this.formData);

    request.pipe(
      finalize(() => this.isSaving.set(false))
    ).subscribe({
      next: (res) => {
        if (res.success) {
          this.loadVisits();
          this.closeDrawer();
        } else {
          alert('Error: ' + res.message);
        }
      },
      error: (err) => alert('API Error')
    });
  }
}
