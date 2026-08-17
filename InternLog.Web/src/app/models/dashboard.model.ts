export interface DashboardSummary {
  role: string;
  totalInternships: number;
  activeInternships: number;
  pendingLeaves: number;
  pendingReports: number;
  unreadMessages: number;
  unreadNotifications: number;
}

export interface CalendarDay {
  date: string;
  status: string;
}
