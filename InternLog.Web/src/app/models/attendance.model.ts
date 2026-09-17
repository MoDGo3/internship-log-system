export interface AttendanceRecord {
  attendanceID: number;
  studentID: number;
  studentName: string;
  attDate: string; // 'yyyy-MM-dd'
  checkInTime?: string; // 'HH:mm:ss'
  checkOutTime?: string; // 'HH:mm:ss'
  totalHours?: number;
  status: string;
  note?: string;
}

export interface AttendanceSummary {
  totalDays: number;
  totalHours: number;
  lateCount: number;
  absentCount: number;
  leaveCount: number;
}
