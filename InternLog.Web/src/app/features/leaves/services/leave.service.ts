import { environment } from '../../../../environments/environment';
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../core/models/api-response.model';
import { LeaveQuota, LeaveRequest, LeaveResponse, LeaveApproval } from '../models/leave.model';

@Injectable({
  providedIn: 'root'
})
export class LeaveService {
  private http = inject(HttpClient);
  // Base URL pointing to the proxy configuration
  private apiUrl = `${environment.apiUrl}/leaves`;

  /**
   * 1. GET /api/leaves/quota
   * Fetch leave quota for the current student (used for Progress Bars)
   */
  getLeaveQuota(): Observable<ApiResponse<LeaveQuota[]>> {
    return this.http.get<ApiResponse<LeaveQuota[]>>(`${this.apiUrl}/quota`);
  }

  /**
   * 2. GET /api/leaves?status=
   * Fetch leaves history with optional status filter
   */
  getLeaves(status?: string): Observable<ApiResponse<LeaveResponse[]>> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<ApiResponse<LeaveResponse[]>>(this.apiUrl, { params });
  }

  /**
   * 3. POST /api/leaves
   * Create a new leave request (Supports FormData for IFormFile Attachment)
   */
  requestLeave(request: LeaveRequest): Observable<ApiResponse<LeaveResponse>> {
    const formData = new FormData();
    formData.append('leaveTypeID', request.leaveTypeID.toString());
    
    // We expect startDate and endDate to be valid ISO strings or Date formatted strings
    formData.append('startDate', request.startDate);
    formData.append('endDate', request.endDate);
    formData.append('reason', request.reason);
    
    // Append file if it exists
    if (request.attachment) {
      formData.append('attachment', request.attachment, request.attachment.name);
    }

    // HttpClient automatically handles boundary generation and Content-Type for FormData
    return this.http.post<ApiResponse<LeaveResponse>>(this.apiUrl, formData);
  }

  /**
   * 4. PUT /api/leaves/{id}/review
   * Advisor or Admin approves or rejects a leave request
   */
  reviewLeave(id: number, approval: LeaveApproval): Observable<ApiResponse<LeaveResponse>> {
    return this.http.put<ApiResponse<LeaveResponse>>(`${this.apiUrl}/${id}/review`, approval);
  }

  /**
   * 5. GET /api/leaves/pending
   * Fetch pending leave requests for the advisor queue
   */
  getPendingLeaves(): Observable<ApiResponse<LeaveResponse[]>> {
    return this.http.get<ApiResponse<LeaveResponse[]>>(`${this.apiUrl}/pending`);
  }
}


