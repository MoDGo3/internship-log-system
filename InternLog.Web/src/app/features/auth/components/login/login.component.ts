import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="auth-layout">
      <div class="auth-card">
        <div class="auth-header">
          <div class="logo-box">
            <span class="icon">🎓</span>
          </div>
          <h1>เข้าสู่ระบบ</h1>
          <p>ระบบจดบันทึกการฝึกงาน</p>
        </div>

        <div *ngIf="errorMessage()" class="alert-error">
          {{ errorMessage() }}
        </div>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>อีเมล</label>
            <input type="email" formControlName="email" placeholder="name@example.com"
                   [class.is-invalid]="loginForm.controls.email.invalid && loginForm.controls.email.touched">
            <div class="error-msg" *ngIf="loginForm.controls.email.hasError('required') && loginForm.controls.email.touched">กรุณากรอกอีเมล</div>
            <div class="error-msg" *ngIf="loginForm.controls.email.hasError('email') && loginForm.controls.email.touched">รูปแบบอีเมลไม่ถูกต้อง</div>
          </div>

          <div class="form-group">
            <label>รหัสผ่าน</label>
            <input type="password" formControlName="password" placeholder="รหัสผ่านอย่างน้อย 8 ตัวอักษร"
                   [class.is-invalid]="loginForm.controls.password.invalid && loginForm.controls.password.touched">
          </div>

          <div class="form-options">
            <a routerLink="/auth/forgot-password" class="forgot-link">ลืมรหัสผ่าน?</a>
          </div>

          <button type="submit" class="btn-primary w-100" [disabled]="isLoading()">
            <span *ngIf="isLoading()" class="spinner"></span>
            {{ isLoading() ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบ' }}
          </button>
        </form>

        <div class="auth-footer">
          <p>ยังไม่มีบัญชี? <a routerLink="/auth/register">สมัครสมาชิก</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-layout { display: flex; align-items: center; justify-content: center; min-height: 100vh; background-color: #F5F6FA; font-family: 'Sarabun', sans-serif; padding: 20px; }
    .auth-card { background: #FFFFFF; width: 100%; max-width: 440px; padding: 40px; border-radius: 16px; box-shadow: 0 4px 24px rgba(0,0,0,0.06); }
    .auth-header { text-align: center; margin-bottom: 32px; }
    .logo-box { width: 64px; height: 64px; background: rgba(91, 95, 199, 0.1); border-radius: 16px; display: flex; align-items: center; justify-content: center; font-size: 32px; margin: 0 auto 16px; }
    h1 { font-family: 'Prompt', sans-serif; font-size: 24px; font-weight: 700; color: #1F2328; margin: 0 0 8px 0; }
    p { color: #6B7280; margin: 0; font-size: 15px; }
    
    .alert-error { background: #FEE2E2; border-left: 4px solid #DC2626; color: #B91C1C; padding: 12px 16px; border-radius: 8px; font-size: 14px; margin-bottom: 24px; font-weight: 500; }
    
    .form-group { margin-bottom: 20px; }
    label { display: block; margin-bottom: 8px; font-weight: 500; color: #1F2328; font-size: 14px; }
    input { width: 100%; padding: 12px 16px; border: 1px solid #E5E7EB; border-radius: 8px; font-family: inherit; font-size: 15px; transition: 0.2s; box-sizing: border-box; }
    input:focus { border-color: #5B5FC7; outline: none; box-shadow: 0 0 0 3px rgba(91,95,199,0.1); }
    input.is-invalid { border-color: #DC2626; }
    .error-msg { color: #DC2626; font-size: 12px; margin-top: 6px; }
    
    .form-options { display: flex; justify-content: flex-end; margin-bottom: 24px; }
    .forgot-link { color: #5B5FC7; text-decoration: none; font-size: 14px; font-weight: 500; }
    .forgot-link:hover { text-decoration: underline; }
    
    .btn-primary { background: #5B5FC7; color: white; border: none; padding: 14px 24px; border-radius: 8px; font-weight: 600; font-family: 'Prompt', sans-serif; font-size: 16px; cursor: pointer; transition: 0.2s; display: flex; align-items: center; justify-content: center; gap: 8px; }
    .btn-primary:hover { background: #464AB9; }
    .btn-primary:disabled { background: #A5A7DB; cursor: not-allowed; }
    .w-100 { width: 100%; }
    
    .auth-footer { text-align: center; margin-top: 32px; padding-top: 24px; border-top: 1px solid #F3F4F6; }
    .auth-footer p { font-size: 14px; }
    .auth-footer a { color: #5B5FC7; font-weight: 600; text-decoration: none; }
    .auth-footer a:hover { text-decoration: underline; }

    .spinner { width: 18px; height: 18px; border: 2px solid rgba(255,255,255,0.3); border-radius: 50%; border-top-color: #fff; animation: spin 1s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.loginForm.getRawValue())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.router.navigate(['/attendance']);
          } else {
            this.errorMessage.set(res.message);
          }
        },
        error: (err) => this.errorMessage.set(err.error?.message || 'ไม่สามารถเข้าสู่ระบบได้')
      });
  }
}
