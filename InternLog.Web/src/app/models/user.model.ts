export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: 'student' | 'advisor' | 'supervisor' | 'admin';
  studentId?: string; // Only for students
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}
