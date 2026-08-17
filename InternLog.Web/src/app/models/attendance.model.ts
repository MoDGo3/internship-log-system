export interface AttendanceRecord {
  attendanceID: number;
  studentID: number;
  studentName: string;
  attDate: string;
  checkInTime?: string;
  checkOutTime?: string;
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
