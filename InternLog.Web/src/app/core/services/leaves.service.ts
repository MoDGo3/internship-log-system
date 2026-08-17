import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LeavesService {
  private apiUrl = `${environment.apiUrl}/leaves`;

  constructor(private http: HttpClient) {}

  getLeaves(): Observable<any> {
    return this.http.get<any>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getQuota(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/quota`).pipe(
      catchError(this.handleError)
    );
  }

  getPendingLeaves(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/pending`).pipe(
      catchError(this.handleError)
    );
  }

  requestLeave(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data).pipe(
      catchError(this.handleError)
    );
  }

  reviewLeave(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/review`, data).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    console.error('An error occurred:', error);
    return throwError(() => new Error(error.message || 'Server error'));
  }
}
