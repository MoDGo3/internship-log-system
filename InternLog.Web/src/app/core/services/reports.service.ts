import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private apiUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  getReports(): Observable<any> {
    return this.http.get<any>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getSummary(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/summary`).pipe(
      catchError(this.handleError)
    );
  }

  generateReport(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/generate`, data).pipe(
      catchError(this.handleError)
    );
  }

  submitReport(id: string): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/submit`, {}).pipe(
      catchError(this.handleError)
    );
  }

  reviewReport(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/review`, data).pipe(
      catchError(this.handleError)
    );
  }

  exportPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/export-pdf`, { responseType: 'blob' }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    console.error('An error occurred:', error);
    return throwError(() => new Error(error.message || 'Server error'));
  }
}
