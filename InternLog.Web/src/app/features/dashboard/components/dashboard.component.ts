import { Component, inject, OnInit, signal, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardService, DashboardStats } from '../services/dashboard.service';
import { AuthService } from '../../../core/auth/auth.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-container">
      <header class="page-header">
        <div>
          <h1>ภาพรวมระบบ (Dashboard)</h1>
          <p>ยินดีต้อนรับกลับมา, {{ userName() }}</p>
        </div>
      </header>

      <div *ngIf="isLoading()" class="loading-state shadow-card">
        <div class="spinner-large"></div>
        <p>กำลังโหลดข้อมูลแดชบอร์ด...</p>
      </div>

      <ng-container *ngIf="!isLoading()">
        
        <!-- ================= STUDENT DASHBOARD (S4) ================= -->
        <ng-container *ngIf="userRole()?.role === 'student'">
          <!-- StatCards แถวบน -->
          <div class="stat-grid mb-24">
            <div class="stat-card shadow-card">
              <div class="stat-icon bg-primary-light">📅</div>
              <div class="stat-info">
                <p class="stat-label">ฝึกแล้ว (วัน)</p>
                <h3 class="stat-value">{{ stats()?.daysCompleted }} / {{ stats()?.totalDays }}</h3>
                <div class="progress-bar">
                  <div class="progress-fill" [style.width.%]="(stats()?.daysCompleted || 0) / (stats()?.totalDays || 1) * 100"></div>
                </div>
              </div>
            </div>
            
            <div class="stat-card shadow-card">
              <div class="stat-icon bg-success-light">⏱️</div>
              <div class="stat-info">
                <p class="stat-label">ชั่วโมงสะสม</p>
                <h3 class="stat-value">{{ stats()?.totalHours }} ชม.</h3>
              </div>
            </div>

            <div class="stat-card shadow-card">
              <div class="stat-icon bg-warning-light">🏖️</div>
              <div class="stat-info">
                <p class="stat-label">วันลาคงเหลือ</p>
                <h3 class="stat-value">{{ stats()?.leaveDaysLeft }} วัน</h3>
              </div>
            </div>

            <div class="stat-card shadow-card">
              <div class="stat-icon bg-info-light">🧑</div>
              <div class="stat-info">
                <p class="stat-label">นัดนิเทศถัดไป</p>
                <h3 class="stat-value text-sm">{{ stats()?.nextVisitDate | date:'dd MMM yyyy' }}</h3>
              </div>
            </div>
          </div>

          <!-- 2 Columns Layout -->
          <div class="two-col-grid mb-24">
            
            <!-- การ์ดซ้าย: ลงเวลาวันนี้ -->
            <div class="dashboard-panel shadow-card text-center">
              <h3 class="panel-title text-left">ลงเวลาปฏิบัติงานวันนี้</h3>
              <div class="clock-display">
                <div class="time">{{ currentTime | date:'HH:mm:ss' }}</div>
                <div class="date">{{ currentTime | date:'EEEE, dd MMMM yyyy' }}</div>
              </div>
              <div class="action-buttons mt-4">
                <button class="btn-primary w-100 mb-12">ลงเวลาเข้า (Check In)</button>
                <button class="btn-outline-danger w-100">ลงเวลาออก (Check Out)</button>
              </div>
            </div>

            <!-- การ์ดขวา: แจ้งเตือนล่าสุด -->
            <div class="dashboard-panel shadow-card">
              <div class="panel-header">
                <h3 class="panel-title m-0">แจ้งเตือนล่าสุด</h3>
                <a routerLink="/notifications" class="text-link">ดูทั้งหมด</a>
              </div>
              <div class="noti-list mt-3">
                <div class="noti-item">
                  <div class="noti-icon bg-success-light">✅</div>
                  <div class="noti-content">
                    <p class="noti-title">บันทึกงาน วันที่ 18 ส.ค. ได้รับการอนุมัติแล้ว</p>
                    <small class="noti-time">10 นาทีที่แล้ว</small>
                  </div>
                </div>
                <div class="noti-item">
                  <div class="noti-icon bg-warning-light">📋</div>
                  <div class="noti-content">
                    <p class="noti-title">คุณได้รับมอบหมายงานใหม่ "ทดสอบระบบ"</p>
                    <small class="noti-time">2 ชั่วโมงที่แล้ว</small>
                  </div>
                </div>
              </div>
            </div>
            
          </div>
        </ng-container>

        <!-- ================= ADVISOR DASHBOARD (S5) ================= -->
        <ng-container *ngIf="userRole()?.role === 'advisor'">
          <!-- StatCards แถวบน -->
          <div class="stat-grid mb-24">
            <div class="stat-card shadow-card">
              <div class="stat-icon bg-primary-light">👨‍🎓</div>
              <div class="stat-info">
                <p class="stat-label">นศ. ที่ดูแล</p>
                <h3 class="stat-value">{{ stats()?.totalStudents }} คน</h3>
              </div>
            </div>
            
            <div class="stat-card shadow-card">
              <div class="stat-icon bg-danger-light">⏳</div>
              <div class="stat-info">
                <p class="stat-label">คิวรออนุมัติ</p>
                <h3 class="stat-value text-danger">{{ stats()?.pendingApprovals }} รายการ</h3>
              </div>
            </div>

            <div class="stat-card shadow-card">
              <div class="stat-icon bg-success-light">✅</div>
              <div class="stat-info">
                <p class="stat-label">อนุมัติสัปดาห์นี้</p>
                <h3 class="stat-value">{{ stats()?.approvedThisWeek }} รายการ</h3>
              </div>
            </div>

            <div class="stat-card shadow-card">
              <div class="stat-icon bg-info-light">💬</div>
              <div class="stat-info">
                <p class="stat-label">ข้อความใหม่</p>
                <h3 class="stat-value">{{ stats()?.unreadMessages }} ข้อความ</h3>
              </div>
            </div>
          </div>

          <!-- 2 Columns Layout -->
          <div class="two-col-grid mb-24">
            
            <!-- การ์ดซ้าย: คิวรออนุมัติ -->
            <div class="dashboard-panel shadow-card">
              <h3 class="panel-title">คิวรออนุมัติด่วน</h3>
              <div class="approval-list mt-3">
                <div class="approval-item">
                  <div class="appr-info">
                    <strong>สมหญิง เรียนดี</strong>
                    <span class="badge badge-warning">ขอลาป่วย</span>
                  </div>
                  <div class="appr-actions">
                    <button class="btn-sm btn-success">✅</button>
                    <button class="btn-sm btn-danger">❌</button>
                  </div>
                </div>
                <div class="approval-item">
                  <div class="appr-info">
                    <strong>สมชาย ขยันมาก</strong>
                    <span class="badge badge-primary">บันทึกงาน</span>
                  </div>
                  <div class="appr-actions">
                    <button class="btn-sm btn-success">✅</button>
                    <button class="btn-sm btn-danger">❌</button>
                  </div>
                </div>
              </div>
            </div>

            <!-- การ์ดขวา: ความคืบหน้า นศ. -->
            <div class="dashboard-panel shadow-card">
              <h3 class="panel-title">ความคืบหน้า นศ.</h3>
              <div class="student-list mt-3">
                <div class="student-item">
                  <div class="stu-avatar">ส</div>
                  <div class="stu-info w-100">
                    <div class="d-flex justify-content-between">
                      <strong>สมชาย ขยันมาก</strong>
                      <small>80%</small>
                    </div>
                    <div class="progress-bar mt-1">
                      <div class="progress-fill bg-success" style="width: 80%"></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            
          </div>
        </ng-container>

      </ng-container>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 1440px; margin: 0 auto; color: #1F2328; font-family: 'Sarabun', sans-serif; }
    .page-header { margin-bottom: 24px; }
    h1 { margin: 0 0 8px 0; font-size: 24px; color: #1F2328; font-family: 'Prompt', sans-serif; font-weight: 700; }
    p { margin: 0; color: #6B7280; font-size: 15px; }
    
    .shadow-card { background: #FFFFFF; border-radius: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; padding: 24px; }
    .mb-24 { margin-bottom: 24px; }
    .mt-3 { margin-top: 16px; }
    .mt-4 { margin-top: 24px; }
    .m-0 { margin: 0; }
    .w-100 { width: 100%; }
    .mb-12 { margin-bottom: 12px; }
    
    /* Stat Grid */
    .stat-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 20px; }
    .stat-card { display: flex; align-items: center; gap: 16px; padding: 20px !important; }
    .stat-icon { width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; }
    .bg-primary-light { background: #EEF2FF; color: #5B5FC7; }
    .bg-success-light { background: #DCFCE7; color: #16A34A; }
    .bg-warning-light { background: #FEF3C7; color: #D97706; }
    .bg-info-light { background: #E0F2FE; color: #0284C7; }
    .bg-danger-light { background: #FEE2E2; color: #DC2626; }
    
    .stat-info { flex: 1; }
    .stat-label { margin: 0 0 4px 0; font-size: 13px; color: #6B7280; font-weight: 600; }
    .stat-value { margin: 0; font-size: 20px; font-family: 'Prompt', sans-serif; font-weight: 700; color: #1F2328; }
    .text-sm { font-size: 16px !important; }
    .text-danger { color: #DC2626 !important; }
    
    .progress-bar { width: 100%; height: 6px; background: #F3F4F6; border-radius: 3px; margin-top: 8px; overflow: hidden; }
    .progress-fill { height: 100%; background: #5B5FC7; border-radius: 3px; transition: width 0.3s; }
    .bg-success { background: #16A34A !important; }
    
    /* Layouts */
    .two-col-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; }
    @media (max-width: 900px) { .two-col-grid { grid-template-columns: 1fr; } }
    
    .dashboard-panel { display: flex; flex-direction: column; }
    .panel-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #F3F4F6; padding-bottom: 12px; }
    .panel-title { font-family: 'Prompt', sans-serif; font-size: 18px; font-weight: 600; color: #374151; margin: 0; }
    
    /* Student: Clock */
    .text-center { text-align: center; }
    .text-left { text-align: left; border-bottom: 1px solid #F3F4F6; padding-bottom: 12px; }
    .clock-display { padding: 32px 0; }
    .time { font-family: 'Prompt', sans-serif; font-size: 48px; font-weight: 700; color: #1F2328; line-height: 1; letter-spacing: 2px; }
    .date { font-size: 16px; color: #6B7280; margin-top: 8px; }
    
    /* Buttons */
    .btn-primary { background: #5B5FC7; color: white; border: none; padding: 14px 20px; border-radius: 8px; font-weight: 600; font-family: 'Prompt', sans-serif; font-size: 15px; cursor: pointer; transition: 0.2s; }
    .btn-primary:hover { background: #464AB9; }
    .btn-outline-danger { background: transparent; color: #DC2626; border: 1px solid #DC2626; padding: 14px 20px; border-radius: 8px; font-weight: 600; font-family: 'Prompt', sans-serif; font-size: 15px; cursor: pointer; transition: 0.2s; }
    .btn-outline-danger:hover { background: #FEF2F2; }
    
    /* Lists */
    .text-link { color: #5B5FC7; text-decoration: none; font-size: 14px; font-weight: 600; }
    .text-link:hover { text-decoration: underline; }
    
    .noti-item, .approval-item, .student-item { display: flex; gap: 16px; padding: 16px 0; border-bottom: 1px solid #F3F4F6; align-items: center; }
    .noti-item:last-child, .approval-item:last-child, .student-item:last-child { border-bottom: none; padding-bottom: 0; }
    
    .noti-icon { width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 18px; flex-shrink: 0; }
    .noti-content p { margin: 0 0 4px 0; font-weight: 600; color: #1F2328; font-size: 14px; }
    .noti-time { color: #9CA3AF; font-size: 12px; }
    
    /* Advisor Specific */
    .appr-info { flex: 1; display: flex; flex-direction: column; gap: 4px; }
    .badge { padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; align-self: flex-start; }
    .badge-warning { background: #FEF3C7; color: #D97706; }
    .badge-primary { background: #EEF2FF; color: #5B5FC7; }
    
    .appr-actions { display: flex; gap: 8px; }
    .btn-sm { width: 32px; height: 32px; border-radius: 8px; border: none; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 14px; transition: 0.2s; }
    .btn-sm.btn-success { background: #DCFCE7; color: #16A34A; }
    .btn-sm.btn-success:hover { background: #BBF7D0; }
    .btn-sm.btn-danger { background: #FEE2E2; color: #DC2626; }
    .btn-sm.btn-danger:hover { background: #FECACA; }
    
    .stu-avatar { width: 40px; height: 40px; border-radius: 50%; background: #5B5FC7; color: white; display: flex; align-items: center; justify-content: center; font-family: 'Prompt', sans-serif; font-weight: 600; flex-shrink: 0; }
    .d-flex { display: flex; }
    .justify-content-between { justify-content: space-between; }
    
    .loading-state { text-align: center; padding: 60px 20px; color: #6B7280; }
    .spinner-large { width: 40px; height: 40px; border: 4px solid #F5F6FA; border-top: 4px solid #5B5FC7; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 16px; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  private dashboardService = inject(DashboardService);
  private authService = inject(AuthService);

  // อ่าน role และชื่อจาก AuthService จริง
  userRole = this.authService.currentUser;
  
  userName = signal<string>('');

  stats = signal<DashboardStats | null>(null);
  isLoading = signal(true);

  currentTime = new Date();
  private timer: any;

  ngOnInit() {
    // ตั้งชื่อจาก currentUser
    const user = this.authService.currentUser();
    if (user) {
      this.userName.set(`${user.firstName} ${user.lastName}`);
    }
    this.startClock();
    this.loadStats();
  }

  ngOnDestroy() {
    if (this.timer) {
      clearInterval(this.timer);
    }
  }

  startClock() {
    this.timer = setInterval(() => {
      this.currentTime = new Date();
    }, 1000);
  }

  loadStats() {
    this.isLoading.set(true);
    const role = this.authService.currentUser()?.role;
    const req$ = role === 'student'
      ? this.dashboardService.getStudentStats()
      : this.dashboardService.getAdvisorStats();

    req$.pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: (data) => this.stats.set(data),
      error: (err) => console.error(err)
    });
  }
}
