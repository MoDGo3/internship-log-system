import { Component, inject, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileService, UserProfile } from '../services/profile.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-layout">
      <!-- Command Bar -->
      <div class="command-bar">
        <div class="cmd-left">
          <h2>โปรไฟล์ส่วนตัว (Profile)</h2>
        </div>
      </div>

      <!-- Main Content -->
      <div class="page-content">
        
        <div *ngIf="isLoading()" class="loading-overlay">
          <div class="spinner"></div>
          <span>กำลังโหลดข้อมูลโปรไฟล์...</span>
        </div>

        <div class="content-wrapper" *ngIf="!isLoading()">
          <div class="grid-layout">
            
            <!-- Left Column: Personal Info -->
            <div class="card shadow-card">
              <div class="card-header">
                <h3>ข้อมูลทั่วไป</h3>
              </div>
              <div class="card-body">
                <form (ngSubmit)="saveProfile()">
                  
                  <div class="avatar-section">
                    <div class="avatar-circle">{{ getInitials() }}</div>
                    <div class="avatar-info">
                      <h4 class="m-0">{{ profile()?.firstName }} {{ profile()?.lastName }}</h4>
                      <span class="badge badge-primary mt-1">{{ profile()?.role | uppercase }}</span>
                    </div>
                  </div>

                  <div class="form-row mt-4">
                    <div class="form-group half">
                      <label>รหัสประจำตัว</label>
                      <input type="text" class="form-control" [value]="profile()?.code" disabled>
                    </div>
                    <div class="form-group half">
                      <label>คณะ/สาขา</label>
                      <input type="text" class="form-control" [value]="(profile()?.faculty || '') + ' / ' + (profile()?.major || '')" disabled>
                    </div>
                  </div>

                  <div class="form-row mt-3">
                    <div class="form-group half">
                      <label>ชื่อจริง</label>
                      <input type="text" class="form-control" [(ngModel)]="editData.firstName" name="firstName" required>
                    </div>
                    <div class="form-group half">
                      <label>นามสกุล</label>
                      <input type="text" class="form-control" [(ngModel)]="editData.lastName" name="lastName" required>
                    </div>
                  </div>

                  <div class="form-row mt-3">
                    <div class="form-group half">
                      <label>อีเมลติดต่อ</label>
                      <input type="email" class="form-control" [(ngModel)]="editData.email" name="email">
                    </div>
                    <div class="form-group half">
                      <label>เบอร์โทรศัพท์</label>
                      <input type="text" class="form-control" [(ngModel)]="editData.phone" name="phone">
                    </div>
                  </div>

                  <div class="action-footer mt-4">
                    <button type="submit" class="btn-primary" [disabled]="isSavingProfile()">
                      {{ isSavingProfile() ? 'กำลังบันทึก...' : 'บันทึกข้อมูลส่วนตัว' }}
                    </button>
                    <span class="text-success ml-2 success-msg" *ngIf="profileSaved()">บันทึกสำเร็จ!</span>
                  </div>
                </form>
              </div>
            </div>

            <!-- Right Column: Security -->
            <div class="card shadow-card">
              <div class="card-header">
                <h3>ความปลอดภัย (เปลี่ยนรหัสผ่าน)</h3>
              </div>
              <div class="card-body">
                <form (ngSubmit)="changePassword()">
                  
                  <div class="alert alert-info mb-4">
                    <strong>คำแนะนำ:</strong> รหัสผ่านควรมีความยาวอย่างน้อย 8 ตัวอักษร
                  </div>

                  <div class="form-group">
                    <label>รหัสผ่านเดิม</label>
                    <input type="password" class="form-control" [(ngModel)]="passData.old" name="oldPass" required placeholder="ลองพิมพ์ 123456">
                  </div>
                  
                  <div class="form-group mt-3">
                    <label>รหัสผ่านใหม่</label>
                    <input type="password" class="form-control" [(ngModel)]="passData.new1" name="newPass1" required>
                  </div>
                  
                  <div class="form-group mt-3">
                    <label>ยืนยันรหัสผ่านใหม่</label>
                    <input type="password" class="form-control" [(ngModel)]="passData.new2" name="newPass2" required>
                    <div class="text-danger mt-1 text-sm" *ngIf="passData.new1 && passData.new2 && passData.new1 !== passData.new2">
                      รหัสผ่านใหม่ไม่ตรงกัน
                    </div>
                  </div>

                  <div class="text-danger mt-2 text-sm" *ngIf="passError()">{{ passError() }}</div>

                  <div class="action-footer mt-4">
                    <button type="submit" class="btn-warning w-100" 
                            [disabled]="isChangingPass() || !passData.old || !passData.new1 || passData.new1 !== passData.new2">
                      {{ isChangingPass() ? 'กำลังเปลี่ยนรหัสผ่าน...' : 'อัปเดตรหัสผ่านใหม่' }}
                    </button>
                  </div>
                </form>
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    /* Layout */
    .page-layout { display: flex; flex-direction: column; height: 100vh; background: #F5F6FA; font-family: 'Sarabun', sans-serif; overflow: hidden; }
    h2, h3, h4 { font-family: 'Prompt', sans-serif; color: #1F2328; margin: 0; }
    .m-0 { margin: 0; }
    .mt-1 { margin-top: 4px; }
    .mt-2 { margin-top: 8px; }
    .mt-3 { margin-top: 16px; }
    .mt-4 { margin-top: 24px; }
    .mb-4 { margin-bottom: 24px; }
    .ml-2 { margin-left: 12px; }
    .w-100 { width: 100%; }
    .text-sm { font-size: 13px; }
    .text-danger { color: #DC2626; }
    .text-success { color: #16A34A; }
    
    /* Command Bar */
    .command-bar { display: flex; justify-content: space-between; align-items: center; padding: 12px 24px; background: #FFFFFF; border-bottom: 1px solid #E5E7EB; flex-shrink: 0; }
    .cmd-left h2 { font-size: 20px; font-weight: 600; }
    
    .btn-primary { background: #5B5FC7; color: white; border: none; padding: 10px 20px; border-radius: 6px; font-weight: 600; font-family: 'Prompt'; cursor: pointer; transition: 0.2s; }
    .btn-primary:hover:not([disabled]) { background: #464AB9; }
    .btn-primary[disabled] { background: #9CA3AF; cursor: not-allowed; }
    
    .btn-warning { background: #D97706; color: white; border: none; padding: 10px 20px; border-radius: 6px; font-weight: 600; font-family: 'Prompt'; cursor: pointer; transition: 0.2s; }
    .btn-warning:hover:not([disabled]) { background: #B45309; }
    .btn-warning[disabled] { background: #9CA3AF; cursor: not-allowed; }

    /* Content Area */
    .page-content { flex: 1; overflow-y: auto; padding: 24px; display: flex; justify-content: center; position: relative; }
    .content-wrapper { width: 100%; max-width: 1000px; }
    
    .loading-overlay { position: absolute; inset: 0; background: rgba(255,255,255,0.8); z-index: 10; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #5B5FC7; font-weight: 600; }
    .spinner { width: 32px; height: 32px; border: 3px solid #EEF0FF; border-top-color: #5B5FC7; border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 12px; }
    @keyframes spin { to { transform: rotate(360deg); } }

    /* Grid Layout */
    .grid-layout { display: grid; grid-template-columns: 1.5fr 1fr; gap: 24px; }
    @media (max-width: 768px) { .grid-layout { grid-template-columns: 1fr; } }

    /* Cards */
    .card { background: #FFFFFF; border-radius: 12px; display: flex; flex-direction: column; overflow: hidden; }
    .shadow-card { box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; }
    .card-header { padding: 20px 24px; border-bottom: 1px solid #E5E7EB; background: #F9FAFB; }
    .card-body { padding: 24px; flex: 1; }

    /* Avatar */
    .avatar-section { display: flex; align-items: center; gap: 20px; padding-bottom: 20px; border-bottom: 1px dashed #E5E7EB; }
    .avatar-circle { width: 80px; height: 80px; border-radius: 50%; background: #5B5FC7; color: white; display: flex; align-items: center; justify-content: center; font-size: 32px; font-weight: 700; font-family: 'Prompt'; }
    .avatar-info { display: flex; flex-direction: column; align-items: flex-start; }
    .badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: 700; }
    .badge-primary { background: #EEF2FF; color: #5B5FC7; border: 1px solid #C7D2FE; }

    /* Forms */
    .form-row { display: flex; gap: 16px; }
    .form-group.half { flex: 1; }
    .form-group label { display: block; font-size: 13px; font-weight: 600; color: #374151; margin-bottom: 8px; }
    
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #D1D5DB; border-radius: 6px; font-family: 'Sarabun'; font-size: 14px; box-sizing: border-box; }
    .form-control:focus { outline: none; border-color: #5B5FC7; box-shadow: 0 0 0 2px rgba(91,95,199,0.2); }
    .form-control[disabled] { background: #F3F4F6; color: #6B7280; cursor: not-allowed; border-color: #E5E7EB; }

    .action-footer { display: flex; align-items: center; }
    .success-msg { animation: fadeInOut 3s forwards; }

    .alert { padding: 12px 16px; border-radius: 6px; font-size: 14px; }
    .alert-info { background: #E0F2FE; color: #0369A1; border: 1px solid #BAE6FD; }

    @keyframes fadeInOut {
      0% { opacity: 0; transform: translateY(5px); }
      10% { opacity: 1; transform: translateY(0); }
      80% { opacity: 1; transform: translateY(0); }
      100% { opacity: 0; }
    }
  `]
})
export class ProfileComponent implements OnInit {
  private profileService = inject(ProfileService);
  
  isLoading = signal(true);
  profile = signal<UserProfile | null>(null);
  
  // Profile Form state
  editData = { firstName: '', lastName: '', email: '', phone: '' };
  isSavingProfile = signal(false);
  profileSaved = signal(false);

  // Password Form state
  passData = { old: '', new1: '', new2: '' };
  isChangingPass = signal(false);
  passError = signal<string | null>(null);

  ngOnInit() {
    this.isLoading.set(true);
    this.profileService.getProfile().pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe(data => {
      this.profile.set(data);
      this.editData = {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        phone: data.phone
      };
    });
  }

  getInitials(): string {
    const p = this.profile();
    if (!p) return 'U';
    return (p.firstName.charAt(0) + p.lastName.charAt(0)).toUpperCase();
  }

  saveProfile() {
    this.isSavingProfile.set(true);
    this.profileSaved.set(false);
    
    this.profileService.updateProfile(this.editData).pipe(
      finalize(() => this.isSavingProfile.set(false))
    ).subscribe({
      next: (updated) => {
        this.profileSaved.set(true);
        // อัปเดต signal จากข้อมูลจริงที่ API ส่งกลับมา
        this.profile.set(updated);
        setTimeout(() => this.profileSaved.set(false), 3000);
      },
      error: (err) => {
        alert('เกิดข้อผิดพลาด: ' + (err.error?.message || 'ไม่สามารถบันทึกข้อมูลได้'));
      }
    });
  }

  changePassword() {
    if (this.passData.new1 !== this.passData.new2) return;

    this.isChangingPass.set(true);
    this.passError.set(null);

    this.profileService.changePassword(this.passData.old, this.passData.new1).pipe(
      finalize(() => this.isChangingPass.set(false))
    ).subscribe({
      next: () => {
        alert('เปลี่ยนรหัสผ่านสำเร็จ! กรุณาเข้าสู่ระบบใหม่ในครั้งถัดไป');
        this.passData = { old: '', new1: '', new2: '' };
      },
      error: (err) => {
        this.passError.set(err.message);
      }
    });
  }
}
