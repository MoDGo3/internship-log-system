import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WorkLog, WorkCategory, LogComment } from '../models/work-log.model';
import { ApiResponse } from '../../../core/models/api-response.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class WorkLogService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/worklogs`;

  getCategories(): Observable<ApiResponse<WorkCategory[]>> {
    return this.http.get<ApiResponse<WorkCategory[]>>(`${this.baseUrl}/categories`);
  }

  getStudentLogs(year: number, month: number): Observable<ApiResponse<WorkLog[]>> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<ApiResponse<WorkLog[]>>(this.baseUrl, { params });
  }

  getLogById(id: number): Observable<ApiResponse<WorkLog>> {
    return this.http.get<ApiResponse<WorkLog>>(`${this.baseUrl}/${id}`);
  }

  createLog(data: Partial<WorkLog>, files: File[] = []): Observable<ApiResponse<WorkLog>> {
    const formData = new FormData();
    formData.append('logDate', data.logDate!);
    formData.append('title', data.title!);
    formData.append('description', data.description!);
    
    if (data.problem) formData.append('problem', data.problem);
    if (data.solution) formData.append('solution', data.solution);
    if (data.categoryID) formData.append('categoryID', data.categoryID.toString());
    if (data.hours) formData.append('hours', data.hours.toString());

    files.forEach(file => {
      formData.append('attachments', file, file.name);
    });

    return this.http.post<ApiResponse<WorkLog>>(this.baseUrl, formData);
  }

  updateLog(id: number, data: Partial<WorkLog>, newFiles: File[] = [], removedAttachmentIds: number[] = []): Observable<ApiResponse<WorkLog>> {
    const formData = new FormData();
    if (data.logDate) formData.append('logDate', data.logDate);
    if (data.title) formData.append('title', data.title);
    if (data.description) formData.append('description', data.description);
    if (data.problem) formData.append('problem', data.problem);
    if (data.solution) formData.append('solution', data.solution);
    if (data.categoryID) formData.append('categoryID', data.categoryID.toString());
    if (data.hours) formData.append('hours', data.hours.toString());

    newFiles.forEach(file => {
      formData.append('newAttachments', file, file.name);
    });

    removedAttachmentIds.forEach(attId => {
      formData.append('removedAttachmentIds', attId.toString());
    });

    return this.http.put<ApiResponse<WorkLog>>(`${this.baseUrl}/${id}`, formData);
  }

  deleteLog(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }

  // Advisor Methods
  getAllLogs(): Observable<ApiResponse<WorkLog[]>> {
    return this.http.get<ApiResponse<WorkLog[]>>(`${this.baseUrl}/all`);
  }

  getStudentLogsByAdvisor(studentId: number, year: number, month: number): Observable<ApiResponse<WorkLog[]>> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<ApiResponse<WorkLog[]>>(`${this.baseUrl}/student/${studentId}`, { params });
  }

  addComment(id: number, message: string): Observable<ApiResponse<LogComment>> {
    return this.http.post<ApiResponse<LogComment>>(`${this.baseUrl}/${id}/comments`, { message });
  }
}
