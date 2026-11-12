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
  user: User; // refresh token อยู่ใน httpOnly cookie (JavaScript อ่านไม่ได้)
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}
