export interface ReportResponseDto {
  reportID: number;
  internshipID: number;
  studentID: number;
  studentName: string;
  periodLabel: string;
  startDate: string; // DateOnly 'yyyy-MM-dd'
  endDate: string; // DateOnly 'yyyy-MM-dd'
  status: string; // 'draft', 'submitted', 'approved', 'returned'
  reviewNote?: string;
  filePath?: string;
  createdAt: string;
}

export interface ReportGenerateRequestDto {
  internshipID: number;
  periodLabel: string;
  startDate: string;
  endDate: string;
}

export interface ReportReviewRequestDto {
  status: string;
  reviewNote?: string;
}
