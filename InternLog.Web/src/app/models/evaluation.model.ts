export interface EvaluationResponseDto {
  evaluationID: number;
  internshipID: number;
  studentID: number;
  studentName: string;
  evaluatorRole: string; // 'supervisor_eval' or 'student_eval'
  criteriaJson?: string;
  totalScore?: number;
  grade?: string;
  comment?: string;
  createdAt: string;
}

export interface EvaluationRequestDto {
  internshipID: number;
  evaluatorRole: string;
  criteriaJson?: string;
  totalScore?: number;
  grade?: string;
  comment?: string;
}
