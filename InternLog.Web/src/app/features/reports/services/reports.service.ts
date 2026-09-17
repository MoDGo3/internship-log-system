import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../models/user.model';

export interface ReportSummary {
  totalDays: number;
  onTimeDays: number;
  lateDays: number;
  leaveDays: number;
}

export interface ReportDetailRow {
  date: string;
  type: string;
  status: string;
  remark: string;
}

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  /** ดึงสรุปการเข้างาน — Backend ใช้ userId จาก JWT อัตโนมัติ */
  getSummary(): Observable<ReportSummary> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/attendance/summary`).pipe(
      map(res => {
        const data = res.data;
        return {
          totalDays:  data?.totalDays  || 0,
          onTimeDays: (data?.totalDays || 0) - (data?.lateCount || 0),
          lateDays:   data?.lateCount  || 0,
          leaveDays:  data?.leaveCount || 0
        };
      })
    );
  }

  /** ดึงรายละเอียดการเข้างานรายวัน */
  getDetails(month?: number, year?: number): Observable<ReportDetailRow[]> {
    let url = `${this.apiUrl}/attendance`;
    const params: string[] = [];
    if (month) params.push(`month=${month}`);
    if (year)  params.push(`year=${year}`);
    if (params.length) url += '?' + params.join('&');

    return this.http.get<ApiResponse<any[]>>(url).pipe(
      map(res =>
        (res.data || []).map(r => ({
          date:   r.attDate?.toString() || '',
          type:   'มาทำงาน',
          status: r.status || '-',
          remark: r.note   || '-'
        }))
      )
    );
  }
}
