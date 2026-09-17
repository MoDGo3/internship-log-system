export interface NotificationResponseDto {
  notificationID: number;
  userID: number;
  title: string;
  message: string;
  link?: string;
  isRead: boolean;
  createdAt: string;
}
