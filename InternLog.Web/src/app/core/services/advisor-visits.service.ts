import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdvisorVisitResponseDto, AdvisorVisitRequestDto } from '../../models/visit.model';
import { ApiResponse } from '../../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AdvisorVisitsService {
  private apiUrl = `${environment.apiUrl}/advisorvisits`;

  constructor(private http: HttpClient) {}

  getVisits(internshipId: number): Observable<ApiResponse<AdvisorVisitResponseDto[]>> {
    return this.http.get<ApiResponse<AdvisorVisitResponseDto[]>>(`${this.apiUrl}?internshipId=${internshipId}`);
  }

  getVisitById(id: number): Observable<ApiResponse<AdvisorVisitResponseDto>> {
    return this.http.get<ApiResponse<AdvisorVisitResponseDto>>(`${this.apiUrl}/${id}`);
  }

  createVisit(data: AdvisorVisitRequestDto): Observable<ApiResponse<AdvisorVisitResponseDto>> {
    return this.http.post<ApiResponse<AdvisorVisitResponseDto>>(this.apiUrl, data);
  }

  updateVisit(id: number, data: AdvisorVisitRequestDto): Observable<ApiResponse<AdvisorVisitResponseDto>> {
    return this.http.put<ApiResponse<AdvisorVisitResponseDto>>(`${this.apiUrl}/${id}`, data);
  }
}
