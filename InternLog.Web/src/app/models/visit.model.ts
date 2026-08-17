export interface VisitRecord {
  visitID: number;
  studentID: number;
  advisorID: number;
  visitNo: number;
  visitDate: string;
  visitFormat: 'Onsite' | 'Online';
  summary: string;
  score: number | null;
  status: string;
  advisorName?: string;
}
