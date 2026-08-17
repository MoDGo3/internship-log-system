export interface LeaveRecord {
  leaveID: number;
  studentID: number;
  leaveType: string;
  startDate: string;
  endDate: string;
  reason: string;
  documentUrl?: string;
  status: string;
  approverID?: number;
  approvedDate?: string;
  rejectionReason?: string;
}

export interface LeaveQuota {
  studentID: number;
  totalQuota: number;
  usedQuota: number;
  remainingQuota: number;
}
