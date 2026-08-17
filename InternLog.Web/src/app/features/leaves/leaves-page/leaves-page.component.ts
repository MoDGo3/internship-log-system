import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LeavesService } from '../../../core/services/leaves.service';
import { LeaveRecord, LeaveQuota } from '../../../models/leave.model';
import { AuthService } from '../../../core/auth/auth.service';
import { format } from 'date-fns';

@Component({
  selector: 'app-leaves-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './leaves-page.component.html',
  styleUrls: ['./leaves-page.component.scss']
})
export class LeavesPageComponent implements OnInit {
  leaveForm: FormGroup;
  isLoading = false;

  quotas: any[] = [
    { type: 'ลากิจ', total: 3, used: 0, color: 'info' },
    { type: 'ลาป่วย', total: 30, used: 0, color: 'danger' },
    { type: 'ลาพักร้อน', total: 6, used: 0, color: 'success' }
  ];

  history: LeaveRecord[] = [];

  constructor(
    private fb: FormBuilder,
    private leavesService: LeavesService,
    private authService: AuthService
  ) {
    this.leaveForm = this.fb.group({
      leaveType: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      reason: ['', Validators.required],
      attachment: [null]
    });
  }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.leavesService.getQuota().subscribe({
      next: (data) => {
        if (data) {
          // Assuming API returns a generic quota. You might map specific ones if API provides it.
          // For now, mapping the total quota to 'ลาป่วย' or distributing it as mock logic if API only returns one combined quota.
          // Assuming `data` matches LeaveQuota model
          this.quotas[0].total = 3; // mock
          this.quotas[0].used = 0;
        }
      },
      error: (err) => console.error('Failed to load quota', err)
    });

    this.leavesService.getLeaves().subscribe({
      next: (data) => {
        if (data) {
          this.history = data;
        }
      },
      error: (err) => console.error('Failed to load history', err)
    });
  }

  onSubmit() {
    if (this.leaveForm.invalid) {
      this.leaveForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formValue = this.leaveForm.value;

    const payload = {
      leaveType: formValue.leaveType,
      startDate: formValue.startDate,
      endDate: formValue.endDate,
      reason: formValue.reason,
      status: 'pending'
    };

    this.leavesService.requestLeave(payload).subscribe({
      next: () => {
        this.isLoading = false;
        alert('ส่งคำขอลาสำเร็จ');
        this.leaveForm.reset();
        this.loadData(); // reload history
      },
      error: (err) => {
        this.isLoading = false;
        alert('เกิดข้อผิดพลาด: ' + err.message);
      }
    });
  }

  getQuotaPercentage(used: number, total: number): number {
    if (total === 0) return 0;
    return (used / total) * 100;
  }

  getStatusBadgeClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return 'badge-success';
      case 'pending': return 'badge-warning';
      case 'rejected': return 'badge-danger';
      default: return 'badge-secondary';
    }
  }

  getStatusText(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return 'อนุมัติแล้ว';
      case 'pending': return 'รออนุมัติ';
      case 'rejected': return 'ไม่อนุมัติ';
      default: return status || '-';
    }
  }
}
