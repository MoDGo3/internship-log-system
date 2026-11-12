import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, of, map, catchError, finalize, share } from 'rxjs';
import { AuthResponse, User, ApiResponse } from '../../models/user.model';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'accessToken';
  private readonly USER_INFO_KEY = 'currentUser';
  /** key เก่าที่เคยเก็บ refresh token — ลบทิ้งเมื่อเจอ (ปัจจุบันใช้ httpOnly cookie) */
  private readonly LEGACY_REFRESH_TOKEN_KEY = 'refreshToken';
  /** ให้ browser ส่ง/รับ refresh token cookie กับ endpoint /auth */
  private readonly withCookie = { withCredentials: true };

  // Using Angular 16+ signals for reactive state
  public currentUser = signal<User | null>(null);
  public isAuthenticated = signal<boolean>(false);

  /** refresh ที่กำลังทำอยู่ — ใช้ร่วมกันทุกผู้เรียก (refresh token หมุนทุกครั้ง ยิงซ้ำพร้อมกันจะล้มเหลว) */
  private refreshInFlight$: Observable<ApiResponse<AuthResponse>> | null = null;

  constructor(private http: HttpClient, private router: Router) {
    this.checkInitialAuthState();
  }

  private checkInitialAuthState() {
    localStorage.removeItem(this.LEGACY_REFRESH_TOKEN_KEY);
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
      // authGuard จะลอง refresh ด้วย cookie ก่อน ถ้าไม่สำเร็จจึงพาไปหน้า login
      localStorage.removeItem(this.TOKEN_KEY);
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
    return this.http.post<ApiResponse<AuthResponse>>(`${this.API_URL}/login`, credentials, this.withCookie).pipe(
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
    // ให้ backend revoke refresh token และลบ cookie
    this.http.post(`${this.API_URL}/logout`, {}, this.withCookie).subscribe({
      next: () => this.clearSession(),
      error: () => this.clearSession()
    });
  }

  private clearSession() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.LEGACY_REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_INFO_KEY);
    this.isAuthenticated.set(false);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  /** ขอ access token ใหม่ด้วย refresh token ใน cookie — error (401) ถ้า cookie ไม่มีหรือหมดอายุ */
  refreshTokens(): Observable<ApiResponse<AuthResponse>> {
    if (!this.refreshInFlight$) {
      this.refreshInFlight$ = this.http.post<ApiResponse<AuthResponse>>(`${this.API_URL}/refresh`, {}, this.withCookie).pipe(
        tap(res => this.handleAuthResponse(res)),
        finalize(() => (this.refreshInFlight$ = null)),
        share()
      );
    }
    return this.refreshInFlight$;
  }

  /** true ถ้ามี session ที่ใช้ได้ — ถ้าไม่มี access token จะลอง refresh ด้วย cookie ก่อน (ใช้ใน guard) */
  ensureSession(): Observable<boolean> {
    if (this.isAuthenticated()) return of(true);
    return this.refreshTokens().pipe(
      map(res => res.success),
      catchError(() => of(false))
    );
  }

  /** ตั้งรหัสผ่านใหม่ด้วย token จากลิงก์ในอีเมล */
  resetPassword(token: string, newPassword: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.API_URL}/reset-password`, { token, newPassword });
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
      localStorage.setItem(this.USER_INFO_KEY, JSON.stringify(res.data.user));

      this.currentUser.set(res.data.user);
      this.isAuthenticated.set(true);
    }
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  hasRole(allowedRoles: string[]): boolean {
    const user = this.currentUser();
    if (!user) return false;
    return allowedRoles.includes(user.role);
  }
}
