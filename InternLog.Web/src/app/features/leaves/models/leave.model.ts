export interface LeaveQuota {
  leaveTypeID: number;
  typeName: string;
  maxDays: number;
  usedDays: number;
  remainingDays: number;
}

export interface LeaveRequest {
  leaveTypeID: number;
  startDate: string; // ISO string format (e.g., '2026-08-18T00:00:00Z')
  endDate: string; // ISO string format
  reason: string;
  attachment?: File | null; // For FormData
}

export interface LeaveResponse {
  leaveID: number;
  studentID: number;
  studentName: string;
  leaveTypeID: number;
  leaveTypeName: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason: string;
  attachmentPath?: string | null;
  status: 'pending' | 'approved' | 'rejected';
  approvedBy?: number | null;
  approverName?: string | null;
  approvedAt?: string | null;
  createdAt: string;
}

export interface LeaveApproval {
  status: 'approved' | 'rejected';
}
