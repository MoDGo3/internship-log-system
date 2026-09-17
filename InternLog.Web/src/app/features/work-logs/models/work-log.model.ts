export interface WorkCategory {
  categoryID: number;
  categoryName: string;
}

export interface Attachment {
  attachmentID: number;
  fileName: string;
  filePath: string;
  fileType?: string;
  fileSize?: number;
  uploadedAt: string;
}

export interface LogComment {
  commentID: number;
  message: string;
  createdAt: string;
  userID: number;
  userName: string;
  userRole: string;
}

export interface WorkLog {
  logID: number;
  studentID: number;
  studentName: string;
  logDate: string;
  title: string;
  description: string;
  problem?: string;
  solution?: string;
  categoryID?: number;
  categoryName?: string;
  hours?: number;
  createdAt: string;
  updatedAt?: string;
  attachments: Attachment[];
  comments: LogComment[];
}
