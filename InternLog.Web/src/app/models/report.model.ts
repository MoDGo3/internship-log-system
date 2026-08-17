export interface ReportRecord {
  reportID: number;
  studentID: number;
  title: string;
  reportDate: string;
  pdfUrl?: string;
  id?: number;
  reportDateFormatted?: string;
}

export interface ReportSummaryDto {
  totalHours: number;
  attendanceDays: number;
  leaveDays: number;
  categoryBreakdown: { [category: string]: number };
}
