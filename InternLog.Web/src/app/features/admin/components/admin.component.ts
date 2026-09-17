import { Component, inject, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, AdminUser, AdminCompany, AdminInternship } from '../services/admin.service';
import { finalize } from 'rxjs/operators';

type AdminTab = 'users' | 'companies' | 'internships';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-layout">
      <!-- Command Bar -->
      <div class="command-bar">
        <div class="cmd-left">
          <h2>จัดการระบบ (Admin Panel)</h2>
        </div>
        
        <div class="cmd-center">
          <div class="tabs">
            <button class="tab-btn" [class.active]="activeTab() === 'users'" (click)="setTab('users')">ผู้ใช้งาน</button>
            <button class="tab-btn" [class.active]="activeTab() === 'companies'" (click)="setTab('companies')">บริษัท</button>
            <button class="tab-btn" [class.active]="activeTab() === 'internships'" (click)="setTab('internships')">สถานะการฝึกงาน</button>
          </div>
        </div>

        <div class="cmd-right">
          <button class="btn-primary" (click)="openAddDrawer()">
            + เพิ่มข้อมูล
          </button>
        </div>
      </div>

      <!-- Main Content -->
      <div class="page-content">
        <div *ngIf="isLoading()" class="loading-overlay">
          <div class="spinner"></div>
          <span>กำลังโหลดข้อมูล...</span>
        </div>

        <div class="table-container shadow-card" *ngIf="!isLoading()">
          
          <!-- TAB 1: USERS -->
          <table class="data-table" *ngIf="activeTab() === 'users'">
            <thead>
              <tr>
                <th>รหัส</th>
                <th>ชื่อ-สกุล</th>
                <th>บทบาท (Role)</th>
                <th>สถานะ</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let user of users()">
                <td>{{ user.code }}</td>
                <td>{{ user.name }}</td>
                <td>
                  <span class="badge" 
                        [class.badge-primary]="user.role === 'student'"
                        [class.badge-info]="user.role === 'advisor'"
                        [class.badge-warning]="user.role === 'admin'">
                    {{ user.role | uppercase }}
                  </span>
                </td>
                <td>
                  <span class="status-dot" [class.active]="user.status === 'active'"></span>
                  {{ user.status === 'active' ? 'ใช้งานปกติ' : 'ระงับ' }}
                </td>
                <td>
                  <div class="action-btns">
                    <button class="btn-icon text-warning" title="แก้ไข" (click)="openEditDrawer(user)">✏️</button>
                    <button class="btn-icon text-danger" title="ระงับ/ลบ">🗑️</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>

          <!-- TAB 2: COMPANIES -->
          <table class="data-table" *ngIf="activeTab() === 'companies'">
            <thead>
              <tr>
                <th>ชื่อบริษัท</th>
                <th>สาขา</th>
                <th>ข้อมูลติดต่อ</th>
                <th class="text-center">นศ. ปัจจุบัน</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let comp of companies()">
                <td class="font-weight-600">{{ comp.name }}</td>
                <td>{{ comp.branch }}</td>
                <td>{{ comp.contact }}</td>
                <td class="text-center">{{ comp.activeStudents }} คน</td>
                <td>
                  <div class="action-btns">
                    <button class="btn-icon text-warning" title="แก้ไข" (click)="openEditDrawer(comp)">✏️</button>
                    <button class="btn-icon text-danger" title="ลบ">🗑️</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>

          <!-- TAB 3: INTERNSHIPS -->
          <table class="data-table" *ngIf="activeTab() === 'internships'">
            <thead>
              <tr>
                <th>ชื่อนักศึกษา</th>
                <th>บริษัทที่ฝึก</th>
                <th>ระยะเวลา</th>
                <th>สถานะ</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let intern of internships()">
                <td class="font-weight-600">{{ intern.studentName }}</td>
                <td>{{ intern.companyName }}</td>
                <td>{{ intern.startDate | date:'d MMM yy' }} - {{ intern.endDate | date:'d MMM yy' }}</td>
                <td>
                  <span class="badge" 
                        [class.badge-success]="intern.status === 'completed'"
                        [class.badge-info]="intern.status === 'ongoing'"
                        [class.badge-danger]="intern.status === 'cancelled'">
                    {{ intern.status | uppercase }}
                  </span>
                </td>
                <td>
                  <div class="action-btns">
                    <button class="btn-icon text-warning" title="แก้ไข" (click)="openEditDrawer(intern)">✏️</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>

        </div>
      </div>
    </div>

    <!-- Multi-purpose Side Drawer -->
    <div class="drawer-backdrop" *ngIf="isDrawerOpen()" (click)="closeDrawer()"></div>
    <div class="side-drawer shadow-drawer" [class.open]="isDrawerOpen()">
      <div class="drawer-header">
        <h3 class="m-0">{{ selectedItem() ? 'แก้ไขข้อมูล' : 'เพิ่มข้อมูลใหม่' }} ({{ activeTab() | uppercase }})</h3>
        <button class="btn-close" (click)="closeDrawer()">✖</button>
      </div>
      
      <div class="drawer-content-scroll">
        <!-- Mock Form - Adapts based on tab -->
        <form>
          <div *ngIf="activeTab() === 'users'">
            <div class="form-group">
              <label>รหัสประจำตัว</label>
              <input type="text" class="form-control" [value]="selectedItem()?.code || ''" placeholder="64xxxxxx">
            </div>
            <div class="form-group mt-3">
              <label>ชื่อ-นามสกุล</label>
              <input type="text" class="form-control" [value]="selectedItem()?.name || ''">
            </div>
            <div class="form-group mt-3">
              <label>บทบาท</label>
              <select class="form-control">
                <option value="student" [selected]="selectedItem()?.role === 'student'">นักศึกษา</option>
                <option value="advisor" [selected]="selectedItem()?.role === 'advisor'">อาจารย์</option>
                <option value="admin" [selected]="selectedItem()?.role === 'admin'">แอดมิน</option>
              </select>
            </div>
          </div>
          
          <!-- Mock Form fields for other tabs omitted for brevity, logic remains same -->
          <div *ngIf="activeTab() !== 'users'">
            <p class="text-muted">ฟอร์มสำหรับจัดการ {{ activeTab() }} จะแสดงที่นี่...</p>
          </div>
        </form>
      </div>

      <div class="drawer-footer">
        <button class="btn-outline" (click)="closeDrawer()">ยกเลิก</button>
        <button class="btn-primary ml-2" (click)="closeDrawer()">บันทึกข้อมูล</button>
      </div>
    </div>
  `,
  styles: [`
    /* Layout */
    .page-layout { display: flex; flex-direction: column; height: 100vh; background: #F5F6FA; font-family: 'Sarabun', sans-serif; overflow: hidden; }
    h2, h3 { font-family: 'Prompt', sans-serif; color: #1F2328; margin: 0; }
    .m-0 { margin: 0; }
    .mt-3 { margin-top: 16px; }
    .ml-2 { margin-left: 8px; }
    
    /* Command Bar & Tabs */
    .command-bar { display: flex; justify-content: space-between; align-items: center; padding: 12px 24px; background: #FFFFFF; border-bottom: 1px solid #E5E7EB; flex-shrink: 0; }
    .cmd-left h2 { font-size: 20px; font-weight: 600; }
    
    .tabs { display: flex; background: #F3F4F6; border-radius: 8px; padding: 4px; gap: 4px; }
    .tab-btn { background: transparent; border: none; padding: 8px 16px; font-family: 'Prompt'; font-size: 14px; font-weight: 500; color: #6B7280; border-radius: 6px; cursor: pointer; transition: 0.2s; }
    .tab-btn:hover { color: #1F2328; }
    .tab-btn.active { background: #FFFFFF; color: #5B5FC7; font-weight: 600; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
    
    .btn-primary { background: #5B5FC7; color: white; border: none; padding: 8px 16px; border-radius: 6px; font-weight: 600; font-family: 'Prompt'; cursor: pointer; transition: 0.2s; }
    .btn-primary:hover { background: #464AB9; }
    .btn-outline { background: #FFF; color: #374151; border: 1px solid #D1D5DB; padding: 8px 16px; border-radius: 6px; font-weight: 600; font-family: 'Prompt'; cursor: pointer; }
    .btn-outline:hover { background: #F3F4F6; }

    /* Content Area */
    .page-content { flex: 1; overflow-y: auto; padding: 24px; position: relative; }
    
    .loading-overlay { position: absolute; inset: 0; background: rgba(255,255,255,0.8); z-index: 10; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #5B5FC7; font-weight: 600; }
    .spinner { width: 32px; height: 32px; border: 3px solid #EEF0FF; border-top-color: #5B5FC7; border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 12px; }
    @keyframes spin { to { transform: rotate(360deg); } }

    /* Tables */
    .table-container { background: #FFFFFF; border-radius: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; overflow: hidden; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 14px; }
    .data-table th { background: #F9FAFB; padding: 12px 16px; text-align: left; font-family: 'Prompt'; font-weight: 600; color: #374151; border-bottom: 1px solid #E5E7EB; }
    .data-table td { padding: 16px; border-bottom: 1px solid #E5E7EB; color: #1F2328; }
    .data-table tbody tr:hover { background: #F9FAFB; }
    
    .text-center { text-align: center !important; }
    .font-weight-600 { font-weight: 600; }
    
    .badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: 700; }
    .badge-primary { background: #EEF2FF; color: #5B5FC7; }
    .badge-info { background: #E0F2FE; color: #0284C7; }
    .badge-warning { background: #FEF3C7; color: #D97706; }
    .badge-success { background: #DCFCE7; color: #16A34A; }
    .badge-danger { background: #FEE2E2; color: #DC2626; }
    
    .status-dot { display: inline-block; width: 8px; height: 8px; border-radius: 50%; background: #9CA3AF; margin-right: 6px; }
    .status-dot.active { background: #16A34A; }

    .action-btns { display: flex; gap: 8px; }
    .btn-icon { background: none; border: none; cursor: pointer; font-size: 16px; transition: 0.2s; padding: 4px; border-radius: 4px; opacity: 0.7; }
    .btn-icon:hover { background: #E5E7EB; opacity: 1; }

    /* Side Drawer */
    .drawer-backdrop { position: fixed; inset: 0; background: rgba(31, 35, 40, 0.4); z-index: 1000; animation: fadeIn 0.2s; backdrop-filter: blur(2px); }
    .side-drawer { position: fixed; top: 0; right: 0; bottom: 0; width: 400px; background: #FFFFFF; z-index: 1001; box-shadow: -4px 0 15px rgba(0,0,0,0.1); transform: translateX(100%); transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1); display: flex; flex-direction: column; }
    .side-drawer.open { transform: translateX(0); }
    
    .drawer-header { display: flex; justify-content: space-between; align-items: center; padding: 20px 24px; border-bottom: 1px solid #E5E7EB; background: #F9FAFB; }
    .btn-close { background: none; border: none; font-size: 16px; cursor: pointer; color: #6B7280; padding: 4px; }
    .btn-close:hover { color: #1F2328; }
    
    .drawer-content-scroll { flex: 1; overflow-y: auto; padding: 24px; }
    .drawer-footer { padding: 16px 24px; border-top: 1px solid #E5E7EB; display: flex; justify-content: flex-end; background: #F9FAFB; }
    
    .form-group label { display: block; font-size: 13px; font-weight: 600; color: #374151; margin-bottom: 8px; }
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #D1D5DB; border-radius: 6px; font-family: 'Sarabun'; font-size: 14px; box-sizing: border-box; }
    .text-muted { color: #6B7280; }

    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
  `]
})
export class AdminComponent implements OnInit {
  private adminService = inject(AdminService);
  
  activeTab = signal<AdminTab>('users');
  isLoading = signal(true);
  
  users = signal<AdminUser[]>([]);
  companies = signal<AdminCompany[]>([]);
  internships = signal<AdminInternship[]>([]);

  // Drawer
  isDrawerOpen = signal(false);
  selectedItem = signal<any>(null); // Use any for mock, should type properly in real app

  ngOnInit() {
    this.loadDataForTab('users');
  }

  setTab(tab: AdminTab) {
    if (this.activeTab() === tab) return;
    this.activeTab.set(tab);
    this.loadDataForTab(tab);
  }

  loadDataForTab(tab: AdminTab) {
    this.isLoading.set(true);
    
    if (tab === 'users') {
      this.adminService.getUsers().pipe(finalize(() => this.isLoading.set(false))).subscribe(d => this.users.set(d));
    } else if (tab === 'companies') {
      this.adminService.getCompanies().pipe(finalize(() => this.isLoading.set(false))).subscribe(d => this.companies.set(d));
    } else {
      this.adminService.getInternships().pipe(finalize(() => this.isLoading.set(false))).subscribe(d => this.internships.set(d));
    }
  }

  openAddDrawer() {
    this.selectedItem.set(null);
    this.isDrawerOpen.set(true);
  }

  openEditDrawer(item: any) {
    this.selectedItem.set(item);
    this.isDrawerOpen.set(true);
  }

  closeDrawer() {
    this.isDrawerOpen.set(false);
  }
}
