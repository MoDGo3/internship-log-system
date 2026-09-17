export interface User {
  userID: number;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  studentID?: string;
  phone?: string;
  profileImage?: string;
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
