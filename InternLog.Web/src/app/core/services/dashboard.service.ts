import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardSummary, CalendarDay } from '../../models/dashboard.model';
import { ApiResponse } from '../../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getDashboardSummary(): Observable<ApiResponse<DashboardSummary>> {
    return this.http.get<ApiResponse<DashboardSummary>>(this.apiUrl);
  }

  getCalendarEvents(year: number, month: number): Observable<ApiResponse<CalendarDay[]>> {
    let params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());
      
    return this.http.get<ApiResponse<CalendarDay[]>>(`${this.apiUrl}/calendar`, { params });
  }
}
