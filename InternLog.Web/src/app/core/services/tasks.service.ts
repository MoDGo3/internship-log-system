import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TaskResponseDto, TaskRequestDto } from '../../models/task.model';
import { ApiResponse } from '../../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class TasksService {
  private apiUrl = `${environment.apiUrl}/tasks`;

  constructor(private http: HttpClient) {}

  getTasks(): Observable<ApiResponse<TaskResponseDto[]>> {
    return this.http.get<ApiResponse<TaskResponseDto[]>>(this.apiUrl);
  }

  getTaskById(id: number): Observable<ApiResponse<TaskResponseDto>> {
    return this.http.get<ApiResponse<TaskResponseDto>>(`${this.apiUrl}/${id}`);
  }

  createTask(data: TaskRequestDto): Observable<ApiResponse<TaskResponseDto>> {
    return this.http.post<ApiResponse<TaskResponseDto>>(this.apiUrl, data);
  }

  updateTask(id: number, data: TaskRequestDto): Observable<ApiResponse<TaskResponseDto>> {
    return this.http.put<ApiResponse<TaskResponseDto>>(`${this.apiUrl}/${id}`, data);
  }

  updateTaskStatus(id: number, status: string): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}/status`, { status });
  }

  deleteTask(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }
}
