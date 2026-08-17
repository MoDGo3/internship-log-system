export interface EvaluationRecord {
  evaluationID: number;
  studentID: number;
  evaluatorName: string;
  evaluationDate: string;
  score: number;
  comments: string;
}

export interface EvaluationSummary {
  averageScore: number;
  totalEvaluations: number;
}
