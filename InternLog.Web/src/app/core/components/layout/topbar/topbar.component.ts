import { Component, ChangeDetectionStrategy, inject, signal, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NotificationsService } from '../../../services/notifications.service';
import { NotificationResponseDto } from '../../../../models/notification.model';
import { OnInit } from '@angular/core';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="topbar">
      <div class="search-container">
        <div class="search-box">
          <span class="search-icon">🔍</span>
          <input type="text" placeholder="ค้นหา... (Ctrl+E)">
        </div>
      </div>

      <div class="topbar-actions">
        <!-- Notification Button & Dropdown -->
        <div class="notification-wrapper">
          <button class="icon-btn notification-btn" (click)="toggleNotifications($event)">
            <span class="icon">🔔</span>
            <span class="badge" *ngIf="unreadCount() > 0">{{ unreadCount() > 99 ? '99+' : unreadCount() }}</span>
          </button>

          <!-- Notification Dropdown -->
          <div class="notification-dropdown shadow-lg" *ngIf="isNotifOpen()" (click)="$event.stopPropagation()">
            <div class="notif-header">
              <h3>การแจ้งเตือน</h3>
              <button class="btn-text" *ngIf="unreadCount() > 0" (click)="markAllAsRead()">อ่านทั้งหมด</button>
            </div>
            
            <div class="notif-body">
              <div class="notif-empty" *ngIf="notifications().length === 0">
                <span class="empty-icon">📭</span>
                <p>ไม่มีการแจ้งเตือนใหม่</p>
              </div>

              <div *ngFor="let n of notifications()" 
                   class="notif-item" 
                   [class.unread]="!n.isRead"
                   (click)="onNotificationClick(n)">
                <div class="notif-icon-wrapper type-info">
                  <span class="notif-icon">🔔</span>
                </div>
                <div class="notif-content">
                  <div class="notif-title">{{ n.title }}</div>
                  <div class="notif-message">{{ n.message }}</div>
                  <div class="notif-time">{{ n.createdAt | date:'short' }}</div>
                </div>
                <div class="notif-unread-dot" *ngIf="!n.isRead"></div>
              </div>
            </div>
            
            <div class="notif-footer" *ngIf="notifications().length > 0">
              <button class="btn-block" routerLink="/notifications" (click)="isNotifOpen.set(false)">ดูการแจ้งเตือนทั้งหมด</button>
            </div>
          </div>
        </div>

        <div class="profile-menu">
          <div class="avatar">สม</div>
          <div class="user-info">
            <span class="user-name">สมมติ ทดสอบ</span>
            <span class="user-role">นักศึกษา</span>
          </div>
          <span class="dropdown-icon">▼</span>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .topbar { height: 64px; background-color: #FFFFFF; border-bottom: 1px solid #E5E7EB; display: flex; align-items: center; justify-content: space-between; padding: 0 24px; font-family: 'Sarabun', sans-serif; position: relative; z-index: 50; }
    
    .search-container { flex: 1; max-width: 400px; }
    .search-box { display: flex; align-items: center; background-color: #F3F4F6; border-radius: 8px; padding: 0 12px; height: 36px; transition: all 0.2s; border: 1px solid transparent; }
    .search-box:focus-within { background-color: #FFFFFF; border-color: #5B5FC7; box-shadow: 0 0 0 3px rgba(91,95,199,0.1); }
    .search-icon { font-size: 14px; opacity: 0.6; margin-right: 8px; }
    .search-box input { border: none; background: transparent; width: 100%; font-family: inherit; font-size: 14px; outline: none; }
    
    .topbar-actions { display: flex; align-items: center; gap: 24px; }
    
    .notification-wrapper { position: relative; }
    .icon-btn { background: transparent; border: none; cursor: pointer; position: relative; padding: 8px; border-radius: 50%; display: flex; align-items: center; justify-content: center; transition: background 0.2s; }
    .icon-btn:hover { background-color: #F3F4F6; }
    .icon-btn .icon { font-size: 20px; }
    .badge { position: absolute; top: 4px; right: 4px; background-color: #DC2626; color: white; font-size: 10px; font-weight: bold; min-width: 16px; height: 16px; border-radius: 10px; padding: 0 4px; display: flex; align-items: center; justify-content: center; border: 2px solid #FFFFFF; box-sizing: border-box; }
    
    /* Notifications Dropdown (Teams Style) */
    .notification-dropdown { position: absolute; top: 50px; right: 0; width: 360px; background: #FFFFFF; border: 1px solid #E5E7EB; border-radius: 12px; box-shadow: 0 10px 25px rgba(0,0,0,0.1); display: flex; flex-direction: column; overflow: hidden; animation: slideDown 0.2s ease-out; z-index: 100; }
    .shadow-lg { box-shadow: 0 10px 25px rgba(0,0,0,0.1); }
    @keyframes slideDown { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }
    
    .notif-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #E5E7EB; background: #F9FAFB; }
    .notif-header h3 { margin: 0; font-family: 'Prompt'; font-size: 16px; font-weight: 600; color: #1F2328; }
    .btn-text { background: none; border: none; color: #5B5FC7; font-size: 13px; font-weight: 500; cursor: pointer; padding: 4px 8px; border-radius: 4px; font-family: 'Sarabun'; transition: 0.2s; }
    .btn-text:hover { background: #EEF0FF; }
    
    .notif-body { max-height: 400px; overflow-y: auto; }
    
    .notif-empty { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 40px 20px; color: #6B7280; }
    .empty-icon { font-size: 32px; margin-bottom: 8px; opacity: 0.5; }
    .notif-empty p { margin: 0; font-size: 14px; }
    
    .notif-item { display: flex; gap: 12px; padding: 16px 20px; border-bottom: 1px solid #F3F4F6; cursor: pointer; transition: 0.2s; position: relative; }
    .notif-item:hover { background: #F9FAFB; }
    .notif-item.unread { background: #F4F5FB; }
    
    .notif-icon-wrapper { width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; flex-shrink: 0; font-size: 18px; }
    .type-approved { background: #DCFCE7; color: #16A34A; }
    .type-rejected { background: #FEE2E2; color: #DC2626; }
    .type-info { background: #E0F2FE; color: #0284C7; }
    .type-warning { background: #FEF3C7; color: #D97706; }
    
    .notif-content { flex: 1; min-width: 0; }
    .notif-title { font-weight: 600; font-size: 14px; color: #1F2328; margin-bottom: 2px; }
    .notif-message { font-size: 13px; color: #4B5563; line-height: 1.4; margin-bottom: 6px; }
    .notif-time { font-size: 11px; color: #9CA3AF; }
    
    .notif-unread-dot { position: absolute; right: 20px; top: 50%; transform: translateY(-50%); width: 8px; height: 8px; border-radius: 50%; background: #5B5FC7; }
    
    .notif-footer { padding: 12px; border-top: 1px solid #E5E7EB; background: #FFFFFF; }
    .btn-block { width: 100%; padding: 8px; background: transparent; border: none; color: #4B5563; font-family: 'Prompt'; font-weight: 500; font-size: 14px; cursor: pointer; border-radius: 6px; transition: 0.2s; text-align: center; }
    .btn-block:hover { background: #F3F4F6; color: #1F2328; }
    
    /* Profile */
    .profile-menu { display: flex; align-items: center; gap: 12px; cursor: pointer; padding: 4px 8px; border-radius: 8px; transition: background 0.2s; }
    .profile-menu:hover { background-color: #F9FAFB; }
    
    .avatar { width: 36px; height: 36px; border-radius: 50%; background-color: #5B5FC7; color: white; display: flex; align-items: center; justify-content: center; font-family: 'Prompt', sans-serif; font-weight: 600; font-size: 14px; }
    
    .user-info { display: flex; flex-direction: column; }
    .user-name { font-weight: 600; font-size: 14px; color: #1F2328; }
    .user-role { font-size: 12px; color: #6B7280; }
    
    .dropdown-icon { font-size: 10px; color: #9CA3AF; margin-left: 4px; }
  `]
})
export class TopbarComponent implements OnInit {
  private notifService = inject(NotificationsService);
  private eRef = inject(ElementRef);
  
  isNotifOpen = signal(false);
  
  notifications = signal<NotificationResponseDto[]>([]);
  unreadCount = signal<number>(0);

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.notifService.getNotifications().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.notifications.set(res.data);
          this.unreadCount.set(res.data.filter(n => !n.isRead).length);
        }
      }
    });
  }

  toggleNotifications(event: Event) {
    event.stopPropagation();
    this.isNotifOpen.update(v => !v);
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.isNotifOpen.set(false);
    }
  }

  getIconForType(type: string): string {
    switch (type?.toLowerCase()) {
      case 'worklog': return '📝';
      case 'leave': return '📅';
      default: return '🔔';
    }
  }

  onNotificationClick(notif: NotificationResponseDto) {
    if (!notif.isRead) {
      this.notifService.markAsRead(notif.notificationID).subscribe(() => {
        this.loadNotifications();
      });
    }
    this.isNotifOpen.set(false);
  }

  markAllAsRead() {
    this.notifService.markAllAsRead().subscribe(() => {
      this.loadNotifications();
    });
  }
}
