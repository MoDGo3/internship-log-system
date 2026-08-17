import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  // --- Users ---
  getUsers(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/users`).pipe(
      catchError(this.handleError)
    );
  }

  getUserById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/users/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  createUser(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/users`, data).pipe(
      catchError(this.handleError)
    );
  }

  updateUser(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/users/${id}`, data).pipe(
      catchError(this.handleError)
    );
  }

  deleteUser(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/users/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // --- Companies ---
  getCompanies(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/companies`).pipe(
      catchError(this.handleError)
    );
  }

  createCompany(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/companies`, data).pipe(
      catchError(this.handleError)
    );
  }

  updateCompany(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/companies/${id}`, data).pipe(
      catchError(this.handleError)
    );
  }

  deleteCompany(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/companies/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // --- Internships ---
  getInternships(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/internships`).pipe(
      catchError(this.handleError)
    );
  }

  createInternship(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/internships`, data).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    console.error('An error occurred:', error);
    return throwError(() => new Error(error.message || 'Server error'));
  }
}
