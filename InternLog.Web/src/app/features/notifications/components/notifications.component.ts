import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationsService } from '../../../core/services/notifications.service';
import { NotificationResponseDto } from '../../../models/notification.model';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.Default,
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>การแจ้งเตือนทั้งหมด</h1>
        <button class="btn-text" *ngIf="unreadCount() > 0" (click)="markAllAsRead()">อ่านทั้งหมด</button>
      </div>

      <div class="notif-list-container shadow-card">
        <div class="empty-state" *ngIf="notifications().length === 0">
          <span class="empty-icon">📭</span>
          <p>ไม่มีการแจ้งเตือน</p>
        </div>

        <div *ngFor="let n of notifications()" 
             class="notif-item-lg" 
             [class.unread]="!n.isRead"
             (click)="onNotificationClick(n)">
          <div class="notif-icon-wrapper-lg type-info">
            <span class="notif-icon">🔔</span>
          </div>
          <div class="notif-content-lg">
            <div class="notif-title-row">
              <div class="notif-title">{{ n.title }}</div>
              <div class="notif-time">{{ n.createdAt | date:'short' }}</div>
            </div>
            <div class="notif-message">{{ n.message }}</div>
          </div>
          <div class="notif-unread-dot" *ngIf="!n.isRead"></div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 900px; margin: 0 auto; color: #1F2328; font-family: 'Sarabun', sans-serif; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    h1 { margin: 0; font-size: 24px; font-family: 'Prompt', sans-serif; font-weight: 700; }
    
    .btn-text { background: none; border: none; color: #5B5FC7; font-family: 'Sarabun', sans-serif; font-size: 14px; cursor: pointer; font-weight: 500; }
    .btn-text:hover { text-decoration: underline; }
    
    .shadow-card { background: #FFFFFF; border-radius: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; overflow: hidden; }
    
    .empty-state { padding: 48px; text-align: center; color: #6B7280; font-size: 16px; }
    .empty-icon { font-size: 48px; display: block; margin-bottom: 16px; }
    
    .notif-item-lg { display: flex; gap: 16px; padding: 24px; border-bottom: 1px solid #F3F4F6; cursor: pointer; transition: background 0.2s; position: relative; }
    .notif-item-lg:last-child { border-bottom: none; }
    .notif-item-lg:hover { background: #F9FAFB; }
    .notif-item-lg.unread { background: #EEF2FF; }
    .notif-item-lg.unread:hover { background: #E0E7FF; }
    
    .notif-icon-wrapper-lg { width: 48px; height: 48px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; }
    .type-approved { background: #DCFCE7; color: #16A34A; }
    .type-rejected { background: #FEE2E2; color: #DC2626; }
    .type-info { background: #DBEAFE; color: #2563EB; }
    .type-warning { background: #FEF3C7; color: #D97706; }
    
    .notif-content-lg { flex: 1; min-width: 0; }
    .notif-title-row { display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 6px; }
    .notif-title { font-weight: 600; font-size: 16px; color: #1F2328; }
    .notif-time { font-size: 12px; color: #6B7280; flex-shrink: 0; }
    .notif-message { font-size: 14px; color: #4B5563; line-height: 1.5; }
    
    .notif-unread-dot { position: absolute; right: 24px; top: 50%; transform: translateY(-50%); width: 10px; height: 10px; border-radius: 50%; background: #5B5FC7; }
  `]
})
export class NotificationsComponent implements OnInit {
  private notifService = inject(NotificationsService);
  
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

  onNotificationClick(notif: NotificationResponseDto) {
    if (!notif.isRead) {
      this.notifService.markAsRead(notif.notificationID).subscribe(() => {
        this.loadNotifications();
      });
    }
  }

  markAllAsRead() {
    this.notifService.markAllAsRead().subscribe(() => {
      this.loadNotifications();
    });
  }
}
