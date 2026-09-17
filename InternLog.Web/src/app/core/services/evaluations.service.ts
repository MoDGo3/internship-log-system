import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EvaluationResponseDto, EvaluationRequestDto } from '../../models/evaluation.model';
import { ApiResponse } from '../../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class EvaluationsService {
  private apiUrl = `${environment.apiUrl}/evaluations`;

  constructor(private http: HttpClient) {}

  getEvaluations(internshipId: number): Observable<ApiResponse<EvaluationResponseDto[]>> {
    return this.http.get<ApiResponse<EvaluationResponseDto[]>>(`${this.apiUrl}?internshipId=${internshipId}`);
  }

  getEvaluationById(id: number): Observable<ApiResponse<EvaluationResponseDto>> {
    return this.http.get<ApiResponse<EvaluationResponseDto>>(`${this.apiUrl}/${id}`);
  }

  submitEvaluation(data: EvaluationRequestDto): Observable<ApiResponse<EvaluationResponseDto>> {
    return this.http.post<ApiResponse<EvaluationResponseDto>>(this.apiUrl, data);
  }
}
