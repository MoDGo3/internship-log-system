import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/user.model';

/** ผู้ใช้ในรูปแบบที่หน้า admin/users แสดง */
export interface AdminUser {
  id: string;
  code: string;
  name: string;
  firstName: string;
  lastName: string;
  email: string;
  username: string;
  role: 'student' | 'advisor' | 'admin';
  status: 'active' | 'suspended';
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  // --- Users ---
  /** รายชื่อผู้ใช้แปลงเป็น AdminUser สำหรับตาราง (API ส่ง role มาในชื่อ roleName) */
  getUserList(): Observable<AdminUser[]> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/users`).pipe(
      map(res => (res.data || []).map(u => ({
        id: u.userID?.toString(),
        code: u.studentID || '-',
        name: `${u.firstName} ${u.lastName}`,
        firstName: u.firstName || '',
        lastName: u.lastName || '',
        email: u.email || '',
        username: u.email || '',
        role: u.roleName ?? u.role,
        status: u.isActive ? 'active' : 'suspended',
        isActive: u.isActive
      } as AdminUser)))
    );
  }

  getUsers(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/users`).pipe(
      catchError(this.handleError)
    );
  }

  getUserById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/users/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  createUser(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/users`, data).pipe(
      catchError(this.handleError)
    );
  }

  updateUser(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/users/${id}`, data).pipe(
      catchError(this.handleError)
    );
  }

  deleteUser(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/users/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // --- Companies ---
  getCompanies(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/companies`).pipe(
      catchError(this.handleError)
    );
  }

  createCompany(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/companies`, data).pipe(
      catchError(this.handleError)
    );
  }

  updateCompany(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/companies/${id}`, data).pipe(
      catchError(this.handleError)
    );
  }

  deleteCompany(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/companies/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // --- Internships ---
  getInternships(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/internships`).pipe(
      catchError(this.handleError)
    );
  }

  createInternship(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/internships`, data).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    console.error('An error occurred:', error);
    return throwError(() => new Error(error.message || 'Server error'));
  }
}
