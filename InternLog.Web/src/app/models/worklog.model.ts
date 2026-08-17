export interface WorkLogRecord {
  logID: number;
  studentID: number;
  logDate: string;
  title: string;
  description: string;
  category: string;
  hoursSpent: number;
  status: string;
  advisorFeedback?: string;
  supervisorFeedback?: string;
}
