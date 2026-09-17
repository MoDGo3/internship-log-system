import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationsService } from '../../../core/services/notifications.service';
import { NotificationResponseDto } from '../../../models/notification.model';
import { formatDistanceToNow } from 'date-fns';
import { th } from 'date-fns/locale';

interface NotificationVM extends NotificationResponseDto {
  time: string;
}

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications-page.component.html',
  styleUrls: ['./notifications-page.component.scss']
})
export class NotificationsPageComponent implements OnInit {
  notifications: NotificationVM[] = [];

  constructor(private notificationsService: NotificationsService) {}

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.notificationsService.getNotifications().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.notifications = res.data.map((n: NotificationResponseDto) => ({
            ...n,
            time: n.createdAt ? formatDistanceToNow(new Date(n.createdAt), { addSuffix: true, locale: th }) : ''
          }));
        }
      },
      error: (err) => console.error('Failed to load notifications', err)
    });
  }

  getIcon(title: string): string {
    const t = (title || '').toLowerCase();
    if (t.includes('อนุมัติ') || t.includes('approve')) return '✅';
    if (t.includes('งาน') || t.includes('task')) return '📋';
    if (t.includes('ข้อความ') || t.includes('message')) return '💬';
    if (t.includes('ระบบ') || t.includes('system')) return '⚙️';
    return '🔔';
  }

  markAsRead(notif: NotificationVM) {
    if (notif.isRead) return;

    this.notificationsService.markAsRead(notif.notificationID).subscribe({
      next: (res) => {
        if (res.success) {
          notif.isRead = true;
        }
      },
      error: (err) => console.error('Failed to mark as read', err)
    });
  }

  markAllAsRead() {
    if (confirm('ต้องการทำเครื่องหมายอ่านแล้วทั้งหมดหรือไม่?')) {
      this.notificationsService.markAllAsRead().subscribe({
        next: (res) => {
          if (res.success) {
            this.notifications.forEach(n => n.isRead = true);
          }
        },
        error: (err) => console.error('Failed to mark all as read', err)
      });
    }
  }

  deleteNotif(id: number, event: Event) {
    event.stopPropagation();
    // Assuming backend has a delete endpoint. If not, just hide it locally
    this.notifications = this.notifications.filter(n => n.notificationID !== id);
  }
}
