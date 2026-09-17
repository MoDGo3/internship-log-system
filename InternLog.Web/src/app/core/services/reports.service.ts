import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ReportResponseDto, ReportGenerateRequestDto, ReportReviewRequestDto } from '../../models/report.model';
import { ApiResponse } from '../../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private apiUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  getReports(internshipId: number): Observable<ApiResponse<ReportResponseDto[]>> {
    return this.http.get<ApiResponse<ReportResponseDto[]>>(`${this.apiUrl}?internshipId=${internshipId}`);
  }

  generateReport(data: ReportGenerateRequestDto): Observable<ApiResponse<ReportResponseDto>> {
    return this.http.post<ApiResponse<ReportResponseDto>>(`${this.apiUrl}/generate`, data);
  }

  submitReport(id: number): Observable<ApiResponse<ReportResponseDto>> {
    return this.http.put<ApiResponse<ReportResponseDto>>(`${this.apiUrl}/${id}/submit`, {});
  }

  reviewReport(id: number, data: ReportReviewRequestDto): Observable<ApiResponse<ReportResponseDto>> {
    return this.http.put<ApiResponse<ReportResponseDto>>(`${this.apiUrl}/${id}/review`, data);
  }

  exportPdf(id: number): Observable<ApiResponse<string>> {
    return this.http.get<ApiResponse<string>>(`${this.apiUrl}/${id}/export-pdf`);
  }
}
