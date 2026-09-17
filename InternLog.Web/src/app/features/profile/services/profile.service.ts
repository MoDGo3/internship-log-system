import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse, User } from '../../../models/user.model';

export interface UserProfile {
  code: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  role: 'student' | 'advisor' | 'admin';
  faculty?: string;
  major?: string;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/auth`;

  getProfile(): Observable<UserProfile> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/me`).pipe(
      map(res => {
        const user = res.data;
        return {
          code: user?.studentID || user?.userID?.toString() || '',
          firstName: user?.firstName || '',
          lastName: user?.lastName || '',
          email: user?.email || '',
          phone: user?.phone || '',
          role: user?.role as 'student' | 'advisor' | 'admin',
          faculty: '-',
          major: '-'
        };
      })
    );
  }

  updateProfile(data: Partial<UserProfile>): Observable<UserProfile> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/me`, {
      firstName: data.firstName,
      lastName: data.lastName,
      phone: data.phone
    }).pipe(
      map(res => {
        const user = res.data;
        return {
          code: user?.studentID || user?.userID?.toString() || '',
          firstName: user?.firstName || '',
          lastName: user?.lastName || '',
          email: user?.email || '',
          phone: user?.phone || '',
          role: user?.role as 'student' | 'advisor' | 'admin',
          faculty: '-',
          major: '-'
        };
      })
    );
  }

  changePassword(oldPass: string, newPass: string): Observable<boolean> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/change-password`, {
      oldPassword: oldPass,
      newPassword: newPass,
      confirmPassword: newPass
    }).pipe(
      map(res => res.success)
    );
  }
}
