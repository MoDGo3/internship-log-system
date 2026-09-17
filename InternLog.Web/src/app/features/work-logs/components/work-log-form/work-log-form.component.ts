import { Component, inject, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { WorkLogService } from '../../services/work-log.service';
import { WorkCategory, WorkLog } from '../../models/work-log.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-work-log-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <header class="page-header">
        <div>
          <button class="btn-back" (click)="goBack()">← ย้อนกลับ</button>
          <h1>{{ isEditMode() ? 'แก้ไขบันทึกงาน' : 'เพิ่มบันทึกงานใหม่' }}</h1>
        </div>
      </header>

      <div *ngIf="errorMessage()" class="alert-error">{{ errorMessage() }}</div>

      <div *ngIf="isInitializing()" class="loading-state shadow-card">
        <div class="spinner-large"></div>
        <p>กำลังโหลดข้อมูล...</p>
      </div>

      <form *ngIf="!isInitializing()" [formGroup]="logForm" (ngSubmit)="onSubmit()" class="form-grid">
        
        <!-- Left Column: Main Form -->
        <div class="form-main shadow-card">
          <div class="card-title">รายละเอียดงาน</div>
          
          <div class="form-row">
            <div class="form-group flex-1">
              <label>วันที่ปฏิบัติงาน <span class="req">*</span></label>
              <input type="date" formControlName="logDate" [class.is-invalid]="hasError('logDate')">
            </div>
            <div class="form-group flex-1">
              <label>จำนวนชั่วโมง</label>
              <input type="number" formControlName="hours" min="0" step="0.5" placeholder="เช่น 8">
            </div>
          </div>

          <div class="form-group">
            <label>หัวข้อเรือง <span class="req">*</span></label>
            <input type="text" formControlName="title" placeholder="ชื่องานที่ทำสั้นๆ" [class.is-invalid]="hasError('title')">
          </div>

          <div class="form-group">
            <label>หมวดหมู่งาน</label>
            <select formControlName="categoryID">
              <option value="">-- เลือกหมวดหมู่ --</option>
              <option *ngFor="let cat of categories()" [value]="cat.categoryID">{{ cat.categoryName }}</option>
            </select>
          </div>

          <div class="form-group">
            <label>รายละเอียดงานที่ทำ <span class="req">*</span></label>
            <textarea formControlName="description" rows="4" placeholder="อธิบายขั้นตอนหรือรายละเอียดงาน" [class.is-invalid]="hasError('description')"></textarea>
          </div>

          <div class="form-group">
            <label>ปัญหาที่พบ (ถ้ามี)</label>
            <textarea formControlName="problem" rows="3" placeholder="ระบุปัญหาที่เกิดขึ้นระหว่างการทำงาน"></textarea>
          </div>

          <div class="form-group">
            <label>วิธีแก้ไข (ถ้ามี)</label>
            <textarea formControlName="solution" rows="3" placeholder="ระบุวิธีที่คุณใช้แก้ปัญหา"></textarea>
          </div>
        </div>

        <!-- Right Column: Attachments & Actions -->
        <div class="form-side">
          
          <!-- Attachments -->
          <div class="shadow-card p-24 mb-24">
            <div class="card-title">ไฟล์แนบ (ถ้ามี)</div>
            
            <div class="upload-zone" (click)="fileInput.click()" (dragover)="onDragOver($event)" (dragleave)="onDragLeave($event)" (drop)="onDrop($event)" [class.drag-over]="isDragOver()">
              <input type="file" #fileInput multiple style="display:none" (change)="onFileSelected($event)">
              <div class="icon">📁</div>
              <p>คลิกเพื่อเลือกไฟล์ หรือลากไฟล์มาวางที่นี่</p>
              <small>รองรับไฟล์รูปภาพและเอกสาร (Max: 5MB/ไฟล์)</small>
            </div>

            <!-- Existing Files -->
            <div class="file-list" *ngIf="existingAttachments().length > 0">
              <label class="list-label">ไฟล์ที่อัปโหลดไว้แล้ว</label>
              <div class="file-item" *ngFor="let file of existingAttachments()">
                <span class="file-name">📎 {{ file.fileName }}</span>
                <button type="button" class="btn-remove" (click)="removeExistingFile(file.attachmentID)">ลบ</button>
              </div>
            </div>

            <!-- New Files -->
            <div class="file-list" *ngIf="newFiles().length > 0">
              <label class="list-label">ไฟล์ใหม่ที่จะแนบ</label>
              <div class="file-item new-file" *ngFor="let file of newFiles(); let i = index">
                <span class="file-name">🆕 {{ file.name }}</span>
                <button type="button" class="btn-remove" (click)="removeNewFile(i)">ลบ</button>
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="actions">
            <button type="submit" class="btn-primary w-100" [disabled]="isSubmitting()">
              <span *ngIf="isSubmitting()" class="spinner"></span>
              บันทึก (Save)
            </button>
          </div>

        </div>
      </form>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 1440px; margin: 0 auto; color: #1F2328; font-family: 'Sarabun', sans-serif; }
    .page-header { margin-bottom: 24px; }
    .btn-back { background: transparent; border: none; color: #6B7280; font-family: 'Prompt', sans-serif; font-size: 14px; cursor: pointer; padding: 0 0 8px 0; font-weight: 500; }
    .btn-back:hover { color: #1F2328; }
    h1 { margin: 0; font-size: 24px; color: #1F2328; font-family: 'Prompt', sans-serif; font-weight: 700; }
    
    .alert-error { background: #FEE2E2; border-left: 4px solid #DC2626; color: #B91C1C; padding: 12px 16px; border-radius: 8px; font-size: 14px; margin-bottom: 24px; font-weight: 500; }
    
    .form-grid { display: grid; grid-template-columns: 2fr 1fr; gap: 24px; }
    @media (max-width: 900px) { .form-grid { grid-template-columns: 1fr; } }
    
    .shadow-card { background: #FFFFFF; border-radius: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; }
    .form-main { padding: 32px; }
    .p-24 { padding: 24px; }
    .mb-24 { margin-bottom: 24px; }
    .mb-12 { margin-bottom: 12px; }
    
    .card-title { font-family: 'Prompt', sans-serif; font-weight: 600; font-size: 18px; margin-bottom: 24px; color: #374151; padding-bottom: 12px; border-bottom: 1px solid #F3F4F6; }
    
    .form-row { display: flex; gap: 16px; }
    .flex-1 { flex: 1; }
    
    .form-group { margin-bottom: 20px; }
    label { display: block; margin-bottom: 8px; font-weight: 600; color: #374151; font-size: 14px; }
    .req { color: #DC2626; }
    
    input, select, textarea { width: 100%; padding: 12px 16px; border: 1px solid #D1D5DB; border-radius: 8px; font-family: inherit; font-size: 15px; box-sizing: border-box; outline: none; transition: 0.2s; background: #fff; }
    input:focus, select:focus, textarea:focus { border-color: #5B5FC7; box-shadow: 0 0 0 3px rgba(91,95,199,0.1); }
    input.is-invalid, textarea.is-invalid { border-color: #DC2626; }
    
    /* Upload Zone */
    .upload-zone { border: 2px dashed #D1D5DB; border-radius: 12px; padding: 32px 20px; text-align: center; background: #F9FAFB; cursor: pointer; transition: 0.2s; margin-bottom: 20px; }
    .upload-zone:hover, .upload-zone.drag-over { border-color: #5B5FC7; background: #EEF2FF; }
    .upload-zone .icon { font-size: 32px; margin-bottom: 12px; }
    .upload-zone p { margin: 0 0 4px 0; font-weight: 500; color: #4B5563; }
    .upload-zone small { color: #9CA3AF; font-size: 12px; }
    
    /* File Lists */
    .file-list { margin-bottom: 16px; }
    .list-label { font-size: 12px; color: #6B7280; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 8px; font-weight: 600; }
    .file-item { display: flex; justify-content: space-between; align-items: center; padding: 10px 12px; background: #F3F4F6; border-radius: 6px; margin-bottom: 8px; font-size: 14px; }
    .file-item.new-file { background: #EEF2FF; }
    .file-name { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 200px; }
    .btn-remove { background: transparent; border: none; color: #DC2626; cursor: pointer; font-size: 12px; font-weight: 600; }
    .btn-remove:hover { text-decoration: underline; }
    
    /* Buttons */
    .btn-primary { background: #5B5FC7; color: white; border: none; padding: 14px 20px; border-radius: 8px; font-weight: 600; font-family: 'Prompt', sans-serif; font-size: 15px; cursor: pointer; transition: 0.2s; display: inline-flex; align-items: center; justify-content: center; gap: 8px; }
    .btn-primary:hover:not(:disabled) { background: #464AB9; }
    .btn-primary:disabled { background: #A5A7DB; cursor: not-allowed; }
    
    .btn-outline { background: transparent; color: #374151; border: 1px solid #D1D5DB; padding: 14px 20px; border-radius: 8px; font-weight: 600; font-family: 'Prompt', sans-serif; font-size: 15px; cursor: pointer; transition: 0.2s; display: inline-flex; align-items: center; justify-content: center; gap: 8px; }
    .btn-outline:hover:not(:disabled) { background: #F3F4F6; }
    .btn-outline:disabled { opacity: 0.5; cursor: not-allowed; }
    
    .w-100 { width: 100%; }
    
    /* States */
    .loading-state { text-align: center; padding: 60px 20px; color: #6B7280; }
    .spinner { width: 16px; height: 16px; border: 2px solid rgba(255,255,255,0.3); border-radius: 50%; border-top-color: #fff; animation: spin 1s linear infinite; }
    .spinner-large { width: 40px; height: 40px; border: 4px solid #F5F6FA; border-top: 4px solid #5B5FC7; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 16px; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class WorkLogFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private logService = inject(WorkLogService);
  private route = inject(ActivatedRoute);
  private location = inject(Location);

  logForm = this.fb.group({
    logDate: [new Date().toLocaleDateString('en-CA'), Validators.required],
    title: ['', Validators.required],
    description: ['', Validators.required],
    problem: [''],
    solution: [''],
    categoryID: [''],
    hours: ['']
  });

  logId = signal<number | null>(null);
  isEditMode = computed(() => this.logId() !== null);
  
  categories = signal<WorkCategory[]>([]);
  isInitializing = signal(true);
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  // File handling
  newFiles = signal<File[]>([]);
  existingAttachments = signal<any[]>([]);
  removedAttachmentIds = signal<number[]>([]);
  isDragOver = signal(false);

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam && idParam !== 'new') {
      this.logId.set(Number(idParam));
    }
    this.loadInitialData();
  }

  loadInitialData() {
    this.logService.getCategories().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.categories.set(res.data);
        }
        
        if (this.isEditMode()) {
          this.loadLogData();
        } else {
          this.isInitializing.set(false);
        }
      },
      error: () => {
        this.errorMessage.set('ไม่สามารถโหลดหมวดหมู่งานได้');
        this.isInitializing.set(false);
      }
    });
  }

  loadLogData() {
    const id = this.logId();
    if (!id) return;

    this.logService.getLogById(id).pipe(finalize(() => this.isInitializing.set(false))).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const data = res.data;
          this.logForm.patchValue({
            logDate: data.logDate ? data.logDate.split('T')[0] : '',
            title: data.title,
            description: data.description,
            problem: data.problem,
            solution: data.solution,
            categoryID: data.categoryID ? data.categoryID.toString() : '',
            hours: data.hours ? data.hours.toString() : ''
          });
          if (data.attachments) {
            this.existingAttachments.set(data.attachments);
          }
        }
      },
      error: () => this.errorMessage.set('ไม่สามารถโหลดข้อมูลบันทึกงานได้')
    });
  }

  hasError(field: string): boolean {
    const control = this.logForm.get(field);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  // --- Drag & Drop File Uploads ---
  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragOver.set(false);
    if (event.dataTransfer?.files) {
      this.handleFiles(Array.from(event.dataTransfer.files));
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(Array.from(input.files));
    }
  }

  handleFiles(files: File[]) {
    this.newFiles.update(current => [...current, ...files]);
  }

  removeNewFile(index: number) {
    this.newFiles.update(current => current.filter((_, i) => i !== index));
  }

  removeExistingFile(attachmentId: number) {
    this.existingAttachments.update(current => current.filter(a => a.attachmentID !== attachmentId));
    this.removedAttachmentIds.update(current => [...current, attachmentId]);
  }
  // -------------------------------

  onSubmit() {
    if (this.logForm.invalid) {
      this.logForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const formData = this.logForm.value;
    const logData: Partial<WorkLog> = {
      ...formData,
      categoryID: formData.categoryID ? Number(formData.categoryID) : undefined,
      hours: formData.hours ? Number(formData.hours) : undefined
    } as any;

    const request$ = this.isEditMode()
      ? this.logService.updateLog(this.logId()!, logData, this.newFiles(), this.removedAttachmentIds())
      : this.logService.createLog(logData, this.newFiles());

    request$.pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: (res) => {
        if (res.success) {
          this.goBack();
        } else {
          this.errorMessage.set(res.message);
        }
      },
      error: (err) => this.errorMessage.set(err.error?.message || 'เกิดข้อผิดพลาดในการบันทึกข้อมูล')
    });
  }

  goBack() {
    this.location.back();
  }
}
