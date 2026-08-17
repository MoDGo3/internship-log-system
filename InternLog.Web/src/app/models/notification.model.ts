export interface NotificationRecord {
  notificationID: number;
  userID: number;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}
