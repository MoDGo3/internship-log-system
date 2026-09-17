export interface User {
  userID: number;
  email: string;
  firstName: string;
  lastName: string;
  roleName: string;
}
export interface AuthResponse {
  accessToken: string;
  user: User;
}
