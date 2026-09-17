export interface AdvisorVisitResponseDto {
  visitID: number;
  internshipID: number;
  studentID: number;
  studentName: string;
  personNo: number;
  visitNo: number;
  visitDate: string; // DateOnly 'yyyy-MM-dd'
  advisorID: number;
  advisorName: string;
  visitType: string;
  summary?: string;
  score?: number;
  createdAt: string;
}

export interface AdvisorVisitRequestDto {
  internshipID: number;
  personNo: number;
  visitNo: number;
  visitDate: string;
  visitType: string;
  summary?: string;
  score?: number;
}
