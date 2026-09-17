export interface LeaveQuotaDto {
  leaveTypeID: number;
  typeName: string;
  maxDays: number;
  usedDays: number;
  pendingDays: number;
  remainingDays: number;
}

export interface LeaveResponseDto {
  leaveID: number;
  studentID: number;
  studentName: string;
  leaveTypeID: number;
  leaveTypeName: string;
  startDate: string; // 'yyyy-MM-dd'
  endDate: string; // 'yyyy-MM-dd'
  dayCount: number;
  reason: string;
  attachmentUrl?: string;
  status: string;
  reviewNote?: string;
  createdAt: string;
}

export interface LeaveRequestDto {
  leaveTypeID: number;
  startDate: string;
  endDate: string;
  reason: string;
  attachment?: File | null;
}

export interface ReviewLeaveDto {
  isApproved: boolean;
  reviewNote?: string;
}
