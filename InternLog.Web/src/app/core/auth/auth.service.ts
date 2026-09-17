import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, of } from 'rxjs';
import { AuthResponse, User, ApiResponse } from '../../models/user.model';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'accessToken';
  private readonly REFRESH_TOKEN_KEY = 'refreshToken';
  private readonly USER_INFO_KEY = 'currentUser';

  // Using Angular 16+ signals for reactive state
  public currentUser = signal<User | null>(null);
  public isAuthenticated = signal<boolean>(false);

  constructor(private http: HttpClient, private router: Router) {
    this.checkInitialAuthState();
  }

  private checkInitialAuthState() {
    const token = this.getAccessToken();
    if (token && !this.isTokenExpired(token)) {
      // Token มีอยู่และยังไม่หมดอายุ จึงถือว่า session ยังใช้ได้
      this.isAuthenticated.set(true);
      const userStr = localStorage.getItem(this.USER_INFO_KEY);
      if (userStr) {
        this.currentUser.set(JSON.parse(userStr));
      }
    } else if (token) {
      // Token หมดอายุแล้ว ลบทิ้งเพื่อป้องกัน 401 cascade
      // (ระบบจะพยายาม refresh เองผ่าน interceptor หรือให้ user login ใหม่)
      localStorage.removeItem(this.TOKEN_KEY);
      // ยังคง refreshToken ไว้เพื่อให้ interceptor ลองขอ token ใหม่
      this.isAuthenticated.set(false);
    }
  }

  /** ตรวจสอบว่า JWT Token หมดอายุแล้วหรือยัง (decode payload โดยไม่ต้องใช้ library) */
  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // exp คือวินาที (Unix timestamp) เทียบกับ Date.now() ซึ่งเป็น milliseconds
      const expiryMs = payload.exp * 1000;
      return Date.now() >= expiryMs;
    } catch {
      // ถ้า parse ไม่ได้ ให้ถือว่าหมดอายุแล้ว
      return true;
    }
  }

  login(credentials: any): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.API_URL}/login`, credentials).pipe(
      tap(res => this.handleAuthResponse(res))
    );
  }

  register(userData: any): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.API_URL}/register`, userData);
  }

  forgotPassword(email: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.API_URL}/forgot-password`, { email });
  }

  logout() {
    const refreshToken = this.getRefreshToken();
    
    // Call backend to revoke refresh token if needed
    if (refreshToken) {
      this.http.post(`${this.API_URL}/logout`, { refreshToken }).subscribe({
        next: () => this.clearSession(),
        error: () => this.clearSession()
      });
    } else {
      this.clearSession();
    }
  }

  private clearSession() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_INFO_KEY);
    this.isAuthenticated.set(false);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  refreshTokens(): Observable<ApiResponse<AuthResponse>> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      this.clearSession();
      return of({} as ApiResponse<AuthResponse>); // Throw error in real scenario
    }

    return this.http.post<ApiResponse<AuthResponse>>(`${this.API_URL}/refresh`, { refreshToken }).pipe(
      tap(res => this.handleAuthResponse(res))
    );
  }

  getMe(): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.API_URL}/me`).pipe(
      tap(res => {
        if (res.success && res.data) {
          localStorage.setItem(this.USER_INFO_KEY, JSON.stringify(res.data));
          this.currentUser.set(res.data);
        }
      })
    );
  }

  changePassword(data: any): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.API_URL}/change-password`, data);
  }

  private handleAuthResponse(res: ApiResponse<AuthResponse>) {
    if (res.success && res.data) {
      localStorage.setItem(this.TOKEN_KEY, res.data.accessToken);
      localStorage.setItem(this.REFRESH_TOKEN_KEY, res.data.refreshToken);
      localStorage.setItem(this.USER_INFO_KEY, JSON.stringify(res.data.user));

      this.currentUser.set(res.data.user);
      this.isAuthenticated.set(true);
    }
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  hasRole(allowedRoles: string[]): boolean {
    const user = this.currentUser();
    if (!user) return false;
    return allowedRoles.includes(user.role);
  }
}
