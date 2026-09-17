import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../models/user.model';

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

export interface AdminCompany {
  id: string;
  name: string;
  branch: string;
  contact: string;
  activeStudents: number;
}

export interface AdminInternship {
  id: string;
  studentName: string;
  companyName: string;
  startDate: string;
  endDate: string;
  status: 'ongoing' | 'completed' | 'cancelled';
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/admin`;
  
  getUsers(): Observable<AdminUser[]> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/users`).pipe(
      map(res => (res.data || []).map(u => ({
        id: u.userID?.toString(),
        code: u.studentID || u.advisorID || u.adminID || '-',
        name: `${u.firstName} ${u.lastName}`,
        firstName: u.firstName || '',
        lastName: u.lastName || '',
        email: u.email || '',
        username: u.email || '',
        role: u.role,
        status: u.isActive ? 'active' : 'suspended',
        isActive: u.isActive
      })))
    );
  }

  getCompanies(): Observable<AdminCompany[]> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/companies`).pipe(
      map(res => (res.data || []).map(c => ({
        id: c.companyID?.toString(),
        name: c.companyName,
        branch: c.branch || '-',
        contact: c.contactName || c.phone || '-',
        activeStudents: 0 // Mocking active students if not provided by backend
      })))
    );
  }

  getInternships(): Observable<AdminInternship[]> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/internships`).pipe(
      map(res => (res.data || []).map(i => ({
        id: i.internshipID?.toString(),
        studentName: i.studentName || 'ไม่ระบุ',
        companyName: i.companyName || 'ไม่ระบุ',
        startDate: i.startDate,
        endDate: i.endDate,
        status: i.status === 'Completed' ? 'completed' : i.status === 'Cancelled' ? 'cancelled' : 'ongoing'
      })))
    );
  }

  updateUser(userId: string, payload: any): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/users/${userId}`, payload);
  }
}
