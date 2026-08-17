export interface TaskRecord {
  taskID: number;
  studentID: number;
  title: string;
  description: string;
  dueDate?: string;
  priority: string;
  status: string;
  assignedBy?: string; // from user
}
