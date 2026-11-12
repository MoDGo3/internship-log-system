import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, forkJoin, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
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

  uploadFile(logId: number, file: File): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${logId}/attachments`, formData);
  }

  deleteAttachment(attId: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${environment.apiUrl}/attachments/${attId}`); 
  }

  createLog(data: Partial<WorkLog>, files: File[] = []): Observable<ApiResponse<WorkLog>> {
    const requestDto = {
      CategoryID: data.categoryID,
      LogDate: data.logDate,
      Title: data.title,
      Detail: data.description,
      Problem: data.problem,
      Solution: data.solution
    };

    return this.http.post<ApiResponse<WorkLog>>(this.baseUrl, requestDto, {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      // After log is created, upload files if any
      switchMap(res => {
        if (!res.success || !res.data) return of(res);
        const logId = res.data.logID || (res.data as any).id; // fallback id
        
        if (files.length === 0) return of(res);
        
        // Upload all files sequentially or in parallel
        const uploadTasks = files.map(file => this.uploadFile(logId, file));
        return forkJoin(uploadTasks).pipe(
          map(() => res) // Return the original create log response
        );
      })
    );
  }

  updateLog(id: number, data: Partial<WorkLog>, newFiles: File[] = [], removedAttachmentIds: number[] = []): Observable<ApiResponse<WorkLog>> {
    const requestDto = {
      CategoryID: data.categoryID,
      LogDate: data.logDate,
      Title: data.title,
      Detail: data.description,
      Problem: data.problem,
      Solution: data.solution
    };

    const deleteTasks = removedAttachmentIds.map(attId => this.deleteAttachment(attId));
    let delete$: Observable<any> = of(null);
    if (deleteTasks.length > 0) {
      delete$ = forkJoin(deleteTasks);
    }

    return delete$.pipe(
      switchMap(() => this.http.put<ApiResponse<WorkLog>>(`${this.baseUrl}/${id}`, requestDto, {
        headers: { 'Content-Type': 'application/json' }
      })),
      switchMap((res: ApiResponse<WorkLog>) => {
        if (!res.success || !res.data || newFiles.length === 0) return of(res);

        
        const uploadTasks = newFiles.map(file => this.uploadFile(id, file));
        return forkJoin(uploadTasks).pipe(
          map(() => res)
        );
      })
    );
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
