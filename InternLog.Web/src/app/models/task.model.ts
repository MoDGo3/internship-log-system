export interface TaskResponseDto {
  taskID: number;
  internshipID: number;
  studentID: number;
  studentName: string;
  assignedBy: number;
  assignerName: string;
  title: string;
  detail?: string;
  dueDate?: string; // DateOnly 'yyyy-MM-dd'
  priority: string;
  status: string;
  createdAt: string;
}

export interface TaskRequestDto {
  internshipID: number;
  title: string;
  detail?: string;
  dueDate?: string;
  priority: string;
}
