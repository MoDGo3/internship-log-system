import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ReportsService } from '../../../core/services/reports.service';
import { ReportRecord, ReportSummaryDto } from '../../../models/report.model';
import { format } from 'date-fns';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './reports-page.component.html',
  styleUrls: ['./reports-page.component.scss']
})
export class ReportsPageComponent implements OnInit {
  selectedPeriod = 'all';
  
  // Summary Data
  summary: ReportSummaryDto = {
    totalHours: 0,
    attendanceDays: 0,
    leaveDays: 0,
    categoryBreakdown: {}
  };

  chartData: any[] = [];
  reportsList: ReportRecord[] = [];

  constructor(private reportsService: ReportsService) {}

  ngOnInit() {
    this.loadSummary();
    this.loadReports();
  }

  loadSummary() {
    this.reportsService.getSummary().subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data) {
          this.summary = data;
          this.prepareChartData(data.categoryBreakdown);
        }
      },
      error: (err: any) => console.error('Failed to load report summary', err)
    });
  }

  loadReports() {
    this.reportsService.getReports().subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data) {
          this.reportsList = data.map((r: any) => ({
            ...r,
            id: r.reportID,
            reportDateFormatted: r.reportDate ? format(new Date(r.reportDate), 'dd MMM yyyy') : '-'
          }));
        }
      },
      error: (err: any) => console.error('Failed to load reports', err)
    });
  }

  prepareChartData(breakdown: { [category: string]: number }) {
    if (!breakdown) return;
    const colors = ['var(--primary)', '#8B5CF6', '#F59E0B', '#10B981', '#6B7280'];
    let index = 0;
    
    // Find max value to calculate percentage for CSS bar height, or just use sum if you want relative to total hours
    const maxHours = Math.max(...Object.values(breakdown), 50); // Fallback to 50 if empty

    this.chartData = Object.keys(breakdown).map(key => {
      const hours = breakdown[key];
      const color = colors[index % colors.length];
      index++;
      return {
        label: key,
        hours: hours,
        percent: (hours / maxHours) * 100, // height percentage relative to max
        color: color
      };
    });
  }

  generateReport() {
    alert('กำลังสร้างรายงานใหม่...');
    this.reportsService.generateReport({}).subscribe({
      next: () => {
        alert('สร้างรายงานสำเร็จ');
        this.loadReports();
      },
      error: (err: any) => alert('เกิดข้อผิดพลาดในการสร้างรายงาน: ' + err.message)
    });
  }
}
