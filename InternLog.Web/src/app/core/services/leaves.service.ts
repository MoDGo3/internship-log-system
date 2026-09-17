import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LeaveResponseDto, LeaveQuotaDto, LeaveRequestDto, ReviewLeaveDto } from '../../models/leave.model';
import { ApiResponse } from '../../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class LeavesService {
  private apiUrl = `${environment.apiUrl}/leaves`;

  constructor(private http: HttpClient) {}

  getLeaves(): Observable<ApiResponse<LeaveResponseDto[]>> {
    return this.http.get<ApiResponse<LeaveResponseDto[]>>(this.apiUrl);
  }

  getQuota(): Observable<ApiResponse<LeaveQuotaDto[]>> {
    return this.http.get<ApiResponse<LeaveQuotaDto[]>>(`${this.apiUrl}/quota`);
  }

  getPendingLeaves(): Observable<ApiResponse<LeaveResponseDto[]>> {
    return this.http.get<ApiResponse<LeaveResponseDto[]>>(`${this.apiUrl}/pending`);
  }

  requestLeave(data: LeaveRequestDto): Observable<ApiResponse<LeaveResponseDto>> {
    const formData = new FormData();
    formData.append('leaveTypeID', data.leaveTypeID.toString());
    formData.append('startDate', data.startDate.toString());
    formData.append('endDate', data.endDate.toString());
    formData.append('reason', data.reason);

    if (data.attachment) {
      formData.append('attachment', data.attachment, data.attachment.name);
    }

    return this.http.post<ApiResponse<LeaveResponseDto>>(this.apiUrl, formData);
  }

  reviewLeave(id: number, data: ReviewLeaveDto): Observable<ApiResponse<LeaveResponseDto>> {
    return this.http.put<ApiResponse<LeaveResponseDto>>(`${this.apiUrl}/${id}/review`, data);
  }
}
