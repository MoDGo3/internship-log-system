import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AttendanceRecord, AttendanceSummary } from '../../models/attendance.model';
import { ApiResponse } from '../../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private apiUrl = `${environment.apiUrl}/attendance`;

  constructor(private http: HttpClient) {}

  getRecords(month?: number, year?: number): Observable<ApiResponse<AttendanceRecord[]>> {
    let params = new HttpParams();
    if (month) params = params.set('month', month.toString());
    if (year) params = params.set('year', year.toString());
    
    return this.http.get<ApiResponse<AttendanceRecord[]>>(this.apiUrl, { params });
  }

  getSummary(internshipId: number): Observable<ApiResponse<AttendanceSummary>> {
    return this.http.get<ApiResponse<AttendanceSummary>>(`${this.apiUrl}/summary?internshipId=${internshipId}`);
  }

  checkIn(note: string | null): Observable<ApiResponse<AttendanceRecord>> {
    // Backend API accepts a string body (the note) or null
    return this.http.post<ApiResponse<AttendanceRecord>>(`${this.apiUrl}/check-in`, note ? JSON.stringify(note) : 'null', {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  checkOut(note: string | null): Observable<ApiResponse<AttendanceRecord>> {
    return this.http.post<ApiResponse<AttendanceRecord>>(`${this.apiUrl}/check-out`, note ? JSON.stringify(note) : 'null', {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  requestEdit(id: number, data: any): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}/request-edit`, data);
  }

  reviewEdit(id: number, data: any): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}/review`, data);
  }
}
