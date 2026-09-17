import { environment } from '../../../../environments/environment';
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../core/models/api-response.model';
import { AdvisorVisitResponseDto, AdvisorVisitRequestDto } from '../../../models/visit.model';

@Injectable({ providedIn: 'root' })
export class VisitsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/visits`; // Backend progress report usually said /api/visits for AdvisorVisits

  getVisits(): Observable<ApiResponse<AdvisorVisitResponseDto[]>> {
    return this.http.get<ApiResponse<AdvisorVisitResponseDto[]>>(this.apiUrl);
  }

  saveVisit(visit: AdvisorVisitRequestDto): Observable<ApiResponse<AdvisorVisitResponseDto>> {
    return this.http.post<ApiResponse<AdvisorVisitResponseDto>>(this.apiUrl, visit);
  }

  updateVisit(id: number, visit: AdvisorVisitRequestDto): Observable<ApiResponse<AdvisorVisitResponseDto>> {
    return this.http.put<ApiResponse<AdvisorVisitResponseDto>>(`${this.apiUrl}/${id}`, visit);
  }
}


