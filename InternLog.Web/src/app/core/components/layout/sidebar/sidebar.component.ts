import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <aside class="sidebar" [class.collapsed]="isCollapsed()">
      <div class="sidebar-header">
        <div class="logo">
          <span class="logo-icon">🎓</span>
          <span class="logo-text" *ngIf="!isCollapsed()">InternLog</span>
        </div>
        <button class="btn-toggle" (click)="toggleSidebar()">
          {{ isCollapsed() ? '▶' : '◀' }}
        </button>
      </div>

      <nav class="sidebar-nav">
        <a routerLink="/dashboard" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">🏠</span>
          <span class="nav-text" *ngIf="!isCollapsed()">แดชบอร์ด</span>
        </a>
        <a routerLink="/attendance" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">⏰</span>
          <span class="nav-text" *ngIf="!isCollapsed()">ลงเวลา</span>
        </a>
        <a routerLink="/leaves" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">🏖️</span>
          <span class="nav-text" *ngIf="!isCollapsed()">วันลา</span>
        </a>
        <a routerLink="/calendar" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">📅</span>
          <span class="nav-text" *ngIf="!isCollapsed()">ปฏิทิน</span>
        </a>
        <a routerLink="/work-logs" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">📝</span>
          <span class="nav-text" *ngIf="!isCollapsed()">บันทึกงาน</span>
        </a>
        <a routerLink="/tasks" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">📋</span>
          <span class="nav-text" *ngIf="!isCollapsed()">งานมอบหมาย</span>
        </a>
        
        <!-- Pending Features Placeholders -->
        <a routerLink="/visits" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">🧑</span>
          <span class="nav-text" *ngIf="!isCollapsed()">การนิเทศ</span>
        </a>
        <a routerLink="/chat" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">💬</span>
          <span class="nav-text" *ngIf="!isCollapsed()">แชท</span>
        </a>
        <a routerLink="/reports" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">📊</span>
          <span class="nav-text" *ngIf="!isCollapsed()">รายงาน</span>
        </a>
        <a routerLink="/profile" routerLinkActive="active" class="nav-item">
          <span class="nav-icon">⚙️</span>
          <span class="nav-text" *ngIf="!isCollapsed()">โปรไฟล์</span>
        </a>
        
        <a *ngIf="userRole() === 'admin'" routerLink="/admin" routerLinkActive="active" class="nav-item admin-link">
          <span class="nav-icon">🛠️</span>
          <span class="nav-text" *ngIf="!isCollapsed()">จัดการระบบ</span>
        </a>
      </nav>

      <div class="sidebar-footer">
        <a href="javascript:void(0)" class="nav-item logout-link" (click)="logout()">
          <span class="nav-icon">🚪</span>
          <span class="nav-text" *ngIf="!isCollapsed()">ออกจากระบบ</span>
        </a>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar { width: 256px; height: 100vh; background-color: #FFFFFF; border-right: 1px solid #E5E7EB; display: flex; flex-direction: column; font-family: 'Sarabun', sans-serif; transition: width 0.3s ease; }
    .sidebar.collapsed { width: 64px; }
    
    .sidebar-header { height: 64px; display: flex; align-items: center; justify-content: space-between; padding: 0 16px; border-bottom: 1px solid #E5E7EB; }
    .logo { display: flex; align-items: center; gap: 12px; overflow: hidden; }
    .logo-icon { font-size: 24px; min-width: 24px; text-align: center; }
    .logo-text { font-family: 'Prompt', sans-serif; font-weight: 700; font-size: 18px; color: #5B5FC7; letter-spacing: 0.5px; white-space: nowrap; }
    
    .btn-toggle { background: transparent; border: none; font-size: 14px; cursor: pointer; color: #6B7280; padding: 4px; display: flex; align-items: center; justify-content: center; }
    .btn-toggle:hover { color: #1F2328; }
    
    .sidebar-nav { flex: 1; overflow-y: auto; overflow-x: hidden; padding: 16px 0; display: flex; flex-direction: column; gap: 4px; }
    
    .nav-item { display: flex; align-items: center; padding: 12px 20px; text-decoration: none; color: #1F2328; font-weight: 500; font-size: 15px; border-left: 3px solid transparent; transition: all 0.2s ease; white-space: nowrap; }
    .nav-item .nav-icon { color: #6B7280; }
    
    .nav-item:hover { background-color: #F9FAFB; }
    .nav-item.active { background-color: #EEF0FF; color: #5B5FC7; border-left-color: #5B5FC7; font-weight: 600; }
    .nav-item.active .nav-icon { color: #5B5FC7; }
    
    .nav-icon { font-size: 18px; min-width: 24px; text-align: center; margin-right: 12px; }
    .sidebar.collapsed .nav-item { padding: 12px 16px; justify-content: center; }
    .sidebar.collapsed .nav-icon { margin-right: 0; }
    
    .sidebar-footer { padding: 16px 0; border-top: 1px solid #E5E7EB; overflow-x: hidden; }
    .logout-link { color: #DC2626; }
    .logout-link .nav-icon { color: #DC2626; }
    .logout-link:hover { background-color: #FEF2F2; color: #B91C1C; }
  `]
})
export class SidebarComponent {
  isCollapsed = signal(false);
  
  // Hardcoded for mockup. In real app, inject AuthService and read role from signal
  userRole = signal<'student' | 'advisor' | 'admin'>('student');

  toggleSidebar() {
    this.isCollapsed.update(v => !v);
  }

  logout() {
    console.log('Logging out...');
    // Real app: this.authService.logout();
  }
}
