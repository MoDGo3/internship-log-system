import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LeavesService } from '../../../core/services/leaves.service';
import { LeaveResponseDto, LeaveQuotaDto } from '../../../models/leave.model';
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

  quotas: LeaveQuotaDto[] = [];
  history: LeaveResponseDto[] = [];

  constructor(
    private fb: FormBuilder,
    private leavesService: LeavesService,
    private authService: AuthService
  ) {
    this.leaveForm = this.fb.group({
      leaveTypeID: ['', Validators.required],
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
      next: (res) => {
        if (res.success && res.data) {
          this.quotas = res.data;
        }
      },
      error: (err) => console.error('Failed to load quota', err)
    });

    this.leavesService.getLeaves().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.history = res.data;
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
      leaveTypeID: Number(formValue.leaveTypeID),
      startDate: formValue.startDate,
      endDate: formValue.endDate,
      reason: formValue.reason,
      attachment: formValue.attachment
    };

    this.leavesService.requestLeave(payload).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success) {
          alert('ส่งคำขอลาสำเร็จ');
          this.leaveForm.reset();
          this.loadData(); // reload history
        }
      },
      error: (err) => {
        this.isLoading = false;
        alert(err.error?.message || 'เกิดข้อผิดพลาดในการยื่นขอลา');
      }
    });
  }

  getQuotaPercentage(used: number, total: number): number {
    if (total === 0) return 0;
    return (used / total) * 100;
  }
  
  getQuotaColor(typeName: string): string {
    switch (typeName) {
      case 'ลากิจ': return 'info';
      case 'ลาป่วย': return 'danger';
      case 'ลาพักร้อน': return 'success';
      default: return 'primary';
    }
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
