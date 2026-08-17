import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { WorklogsService } from '../../../core/services/worklogs.service';
import { WorkLogRecord } from '../../../models/worklog.model';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-work-log-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './work-log-list.component.html',
  styleUrls: ['./work-log-list.component.scss']
})
export class WorkLogListComponent implements OnInit {
  filters = {
    month: '08-2026',
    status: '',
    category: '',
    search: ''
  };

  logs: WorkLogRecord[] = [];
  filteredLogs: WorkLogRecord[] = [];

  constructor(
    private worklogsService: WorklogsService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.loadLogs();
  }

  loadLogs() {
    this.worklogsService.getLogs().subscribe({
      next: (data) => {
        if (data) {
          this.logs = data;
          this.applyFilters();
        }
      },
      error: (err) => console.error('Failed to load worklogs', err)
    });
  }

  applyFilters() {
    this.filteredLogs = this.logs.filter(log => {
      const matchStatus = !this.filters.status || log.status === this.filters.status;
      const matchCategory = !this.filters.category || log.category === this.filters.category;
      const matchSearch = !this.filters.search || log.title.toLowerCase().includes(this.filters.search.toLowerCase());
      return matchStatus && matchCategory && matchSearch;
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return 'badge-success';
      case 'pending': return 'badge-warning';
      case 'returned': return 'badge-danger';
      case 'draft': return 'badge-secondary';
      default: return 'badge-secondary';
    }
  }

  getStatusText(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return 'อนุมัติแล้ว';
      case 'pending': return 'รออนุมัติ';
      case 'returned': return 'ตีกลับ';
      case 'draft': return 'ฉบับร่าง';
      default: return status || '-';
    }
  }

  deleteLog(id: number) {
    if (confirm('คุณแน่ใจหรือไม่ว่าต้องการลบบันทึกงานนี้?')) {
      this.worklogsService.deleteLog(id).subscribe({
        next: () => {
          this.logs = this.logs.filter(log => log.logID !== id);
          this.applyFilters();
        },
        error: (err) => alert('ลบไม่สำเร็จ: ' + err.message)
      });
    }
  }
}
