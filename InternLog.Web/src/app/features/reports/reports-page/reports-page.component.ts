import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ReportsService } from '../../../core/services/reports.service';
import { ReportResponseDto, ReportGenerateRequestDto } from '../../../models/report.model';
import { format } from 'date-fns';

interface ReportVM extends ReportResponseDto {
  reportDateFormatted: string;
}

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './reports-page.component.html',
  styleUrls: ['./reports-page.component.scss']
})
export class ReportsPageComponent implements OnInit {
  selectedPeriod = 'all';
  internshipID = 1; // Temporary hardcoded or from state
  
  // Summary Data Mocked for UI since Backend doesn't provide it directly in Reports API
  summary = {
    totalHours: 320,
    attendanceDays: 45,
    leaveDays: 2,
    categoryBreakdown: {
      'Frontend': 120,
      'Backend': 100,
      'Database': 40,
      'Meeting': 60
    }
  };

  chartData: any[] = [];
  reportsList: ReportVM[] = [];

  constructor(private reportsService: ReportsService) {}

  ngOnInit() {
    this.prepareChartData(this.summary.categoryBreakdown);
    this.loadReports();
  }

  loadReports() {
    this.reportsService.getReports(this.internshipID).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.reportsList = res.data.map((r: ReportResponseDto) => ({
            ...r,
            reportDateFormatted: r.createdAt ? format(new Date(r.createdAt), 'dd MMM yyyy') : '-'
          }));
        }
      },
      error: (err) => console.error('Failed to load reports', err)
    });
  }

  prepareChartData(breakdown: { [category: string]: number }) {
    if (!breakdown) return;
    const colors = ['var(--primary)', '#8B5CF6', '#F59E0B', '#10B981', '#6B7280'];
    let index = 0;
    
    const maxHours = Math.max(...Object.values(breakdown), 50); 

    this.chartData = Object.keys(breakdown).map(key => {
      const hours = breakdown[key];
      const color = colors[index % colors.length];
      index++;
      return {
        label: key,
        hours: hours,
        percent: (hours / maxHours) * 100,
        color: color
      };
    });
  }

  generateReport() {
    const today = new Date();
    const req: ReportGenerateRequestDto = {
      internshipID: this.internshipID,
      periodLabel: `Week of ${format(today, 'dd MMM yyyy')}`,
      startDate: format(new Date(today.getFullYear(), today.getMonth(), today.getDate() - 7), 'yyyy-MM-dd'),
      endDate: format(today, 'yyyy-MM-dd')
    };

    this.reportsService.generateReport(req).subscribe({
      next: (res) => {
        if (res.success) {
          alert('สร้างรายงานสำเร็จ');
          this.loadReports();
        }
      },
      error: (err) => alert('เกิดข้อผิดพลาดในการสร้างรายงาน: ' + err.message)
    });
  }
}
