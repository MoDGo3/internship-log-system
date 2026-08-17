import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class WorklogsService {
  private apiUrl = `${environment.apiUrl}/worklogs`;

  constructor(private http: HttpClient) {}

  getLogs(): Observable<any> {
    return this.http.get<any>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getPendingLogs(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/pending`).pipe(
      catchError(this.handleError)
    );
  }

  getLogById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  createLog(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data).pipe(
      catchError(this.handleError)
    );
  }

  updateLog(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data).pipe(
      catchError(this.handleError)
    );
  }

  submitLog(id: string): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/submit`, {}).pipe(
      catchError(this.handleError)
    );
  }

  reviewLog(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/review`, data).pipe(
      catchError(this.handleError)
    );
  }

  addAttachment(id: string, data: FormData): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/attachments`, data).pipe(
      catchError(this.handleError)
    );
  }

  deleteLog(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  deleteAttachment(attachmentId: string): Observable<any> {
    return this.http.delete<any>(`${environment.apiUrl}/attachments/${attachmentId}`).pipe(
      catchError(this.handleError)
    );
  }

  addComment(id: string, data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/comments`, data).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    console.error('An error occurred:', error);
    return throwError(() => new Error(error.message || 'Server error'));
  }
}
