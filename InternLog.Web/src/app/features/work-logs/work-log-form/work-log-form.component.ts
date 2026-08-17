import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { WorklogsService } from '../../../core/services/worklogs.service';
import { format } from 'date-fns';

@Component({
  selector: 'app-work-log-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './work-log-form.component.html',
  styleUrls: ['./work-log-form.component.scss']
})
export class WorkLogFormComponent implements OnInit {
  logForm: FormGroup;
  isEditMode = false;
  logId: string | null = null;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private worklogsService: WorklogsService
  ) {
    this.logForm = this.fb.group({
      logDate: ['', Validators.required],
      title: ['', Validators.required],
      category: ['', Validators.required],
      hours: [8, [Validators.required, Validators.min(1)]],
      description: ['', Validators.required],
      problem: [''],
      solution: ['']
    });
  }

  ngOnInit() {
    this.logId = this.route.snapshot.paramMap.get('id');
    if (this.logId && this.logId !== 'new') {
      this.isEditMode = true;
      this.loadLogData(Number(this.logId));
    } else {
      // New log default to today
      this.logForm.patchValue({
        logDate: format(new Date(), 'yyyy-MM-dd')
      });
    }
  }

  loadLogData(id: number) {
    this.isLoading = true;
    this.worklogsService.getLogById(id.toString()).subscribe({
      next: (res) => {
        const data = res.data;
        if (data) {
          // Format date if needed, assuming API returns ISO format or similar
          const logDateFormatted = data.logDate ? format(new Date(data.logDate), 'yyyy-MM-dd') : '';
          this.logForm.patchValue({
            logDate: logDateFormatted,
            title: data.title,
            category: data.category,
            hours: data.hoursSpent,
            description: data.description,
          });
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load log data', err);
        this.isLoading = false;
        alert('โหลดข้อมูลไม่สำเร็จ');
      }
    });
  }

  saveDraft() {
    this.submitLogData('draft');
  }

  submitLog() {
    if (this.logForm.invalid) {
      this.logForm.markAllAsTouched();
      return;
    }
    this.submitLogData('pending');
  }

  private submitLogData(status: string) {
    this.isLoading = true;
    const formValue = this.logForm.value;
    
    const payload = {
      logDate: formValue.logDate,
      title: formValue.title,
      description: formValue.description + (formValue.problem ? `\nปัญหา: ${formValue.problem}` : '') + (formValue.solution ? `\nวิธีแก้: ${formValue.solution}` : ''),
      category: formValue.category,
      hoursSpent: formValue.hours,
      status: status
    };

    if (this.isEditMode && this.logId) {
      this.worklogsService.updateLog(this.logId, payload).subscribe({
        next: () => {
          this.isLoading = false;
          alert(status === 'draft' ? 'บันทึกแบบร่างสำเร็จ' : 'ส่งอนุมัติบันทึกงานสำเร็จ');
          this.router.navigate(['/work-logs']);
        },
        error: (err) => {
          this.isLoading = false;
          alert('เกิดข้อผิดพลาด: ' + err.message);
        }
      });
    } else {
      this.worklogsService.createLog(payload).subscribe({
        next: () => {
          this.isLoading = false;
          alert(status === 'draft' ? 'บันทึกแบบร่างสำเร็จ' : 'ส่งอนุมัติบันทึกงานสำเร็จ');
          this.router.navigate(['/work-logs']);
        },
        error: (err) => {
          this.isLoading = false;
          alert('เกิดข้อผิดพลาด: ' + err.message);
        }
      });
    }
  }
}
