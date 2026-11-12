import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardService } from '../../../core/services/dashboard.service';
import { AuthService } from '../../../core/auth/auth.service';
import { DashboardSummary } from '../../../models/dashboard.model';

interface StatCard {
  icon: string;
  label: string;
  value: (s: DashboardSummary) => number;
  unit: string;
  link: string;
}

/**
 * แดชบอร์ดของอาจารย์ (advisor) และผู้ดูแลระบบ (admin)
 * ตัวเลขมาจาก GET /api/dashboard ซึ่งคำนวณตามขอบเขตของ role อยู่แล้ว
 */
@Component({
  selector: 'app-dashboard-staff',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>{{ isAdmin() ? 'แดชบอร์ดผู้ดูแลระบบ' : 'แดชบอร์ดอาจารย์นิเทศก์' }}</h2>
        <p>{{ isAdmin() ? 'ภาพรวมการฝึกงานทั้งระบบ' : 'ภาพรวมนักศึกษาในความดูแลของคุณ' }}</p>
      </div>

      <div class="error-alert" *ngIf="errorMessage()">{{ errorMessage() }}</div>

      <div class="stats-grid" *ngIf="summary() as s">
        <div class="card stat-card clickable" *ngFor="let card of cards()" [routerLink]="card.link">
          <div class="stat-icon">{{ card.icon }}</div>
          <div class="stat-content">
            <h3>{{ card.label }}</h3>
            <div class="stat-value">{{ card.value(s) }} <span class="unit">{{ card.unit }}</span></div>
          </div>
        </div>
      </div>

      <div class="stats-grid" *ngIf="!summary() && !errorMessage()">
        <div class="card stat-card"><div class="stat-content"><h3>กำลังโหลดข้อมูล...</h3></div></div>
      </div>
    </div>
  `,
  styleUrls: ['../dashboard-student/dashboard-student.component.scss'],
  styles: [`
    .error-alert { background: #FEE2E2; color: #B91C1C; padding: 12px 16px; border-radius: 8px; margin-bottom: 16px; }
  `]
})
export class DashboardStaffComponent implements OnInit {
  summary = signal<DashboardSummary | null>(null);
  errorMessage = signal('');
  isAdmin = computed(() => this.authService.currentUser()?.role === 'admin');

  cards = computed<StatCard[]>(() => {
    const common: StatCard[] = [
      { icon: '🎓', label: 'การฝึกงานที่กำลังดำเนินการ', value: s => s.activeInternships, unit: 'คน',
        link: this.isAdmin() ? '/admin/internships' : '/attendance' },
      { icon: '🏖', label: 'ใบลารออนุมัติ', value: s => s.pendingLeaves, unit: 'รายการ', link: '/leaves' },
      { icon: '📄', label: 'รายงานรอตรวจ', value: s => s.pendingReports, unit: 'รายการ', link: '/reports' },
      { icon: '🔔', label: 'แจ้งเตือนที่ยังไม่อ่าน', value: s => s.unreadNotifications, unit: 'รายการ', link: '/notifications' },
    ];
    const total: StatCard = {
      icon: '📋', label: 'การฝึกงานทั้งหมด', value: s => s.totalInternships, unit: 'รายการ',
      link: this.isAdmin() ? '/admin/internships' : '/work-logs'
    };
    return [total, ...common];
  });

  constructor(private dashboardService: DashboardService, private authService: AuthService) {}

  ngOnInit() {
    this.dashboardService.getDashboardSummary().subscribe({
      next: res => {
        if (res.success && res.data) this.summary.set(res.data);
        else this.errorMessage.set(res.message || 'ไม่สามารถโหลดข้อมูลแดชบอร์ดได้');
      },
      error: err => this.errorMessage.set(err.error?.message || 'ไม่สามารถโหลดข้อมูลแดชบอร์ดได้')
    });
  }
}
