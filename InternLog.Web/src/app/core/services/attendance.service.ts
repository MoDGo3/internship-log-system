import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private apiUrl = `${environment.apiUrl}/attendance`;

  constructor(private http: HttpClient) {}

  getRecords(): Observable<any> {
    return this.http.get<any>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getSummary(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/summary`).pipe(
      catchError(this.handleError)
    );
  }

  checkIn(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/check-in`, data).pipe(
      catchError(this.handleError)
    );
  }

  checkOut(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/check-out`, data).pipe(
      catchError(this.handleError)
    );
  }

  requestEdit(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/request-edit`, data).pipe(
      catchError(this.handleError)
    );
  }

  reviewEdit(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/review`, data).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    console.error('An error occurred:', error);
    return throwError(() => new Error(error.message || 'Server error'));
  }
}
