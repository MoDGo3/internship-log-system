export interface Attendance {
  attendanceID: number;
  attDate: string;
  checkInTime?: string;
  checkOutTime?: string;
  totalHours?: number;
  status: 'ontime' | 'late' | 'leave' | 'absent';
}
export interface AttendanceSummary {
  workDays: number;
  totalHours: number;
  lateCount: number;
}
