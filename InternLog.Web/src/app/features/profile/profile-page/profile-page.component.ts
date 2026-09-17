import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { User } from '../../../models/user.model';
import { DashboardService } from '../../../core/services/dashboard.service';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile-page.component.html',
  styleUrls: ['./profile-page.component.scss']
})
export class ProfilePageComponent implements OnInit {
  
  profile: Partial<User> = {};
  isLoading = false;

  internship = {
    company: 'บริษัท เอบีซี เทคโนโลยี จำกัด',
    period: 'สิงหาคม 2569 - พฤศจิกายน 2569',
    advisor: 'อ.วิชัย เก่งกาจ',
    mentor: 'พี่โตโต้ (Senior Developer)'
  };

  passwords = {
    current: '',
    new: '',
    confirm: ''
  };

  constructor(
    private authService: AuthService,
    private dashboardService: DashboardService
  ) {}

  ngOnInit() {
    this.authService.getMe().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.profile = res.data;
        }
      },
      error: (err) => console.error('Failed to load profile', err)
    });
  }

  saveProfile() {
    alert('ระบบยังไม่รองรับการแก้ไขข้อมูลส่วนตัวในเวอร์ชันนี้');
  }

  changePassword() {
    if (!this.passwords.current || !this.passwords.new || !this.passwords.confirm) {
      alert('กรุณากรอกข้อมูลให้ครบถ้วน');
      return;
    }
    if (this.passwords.new !== this.passwords.confirm) {
      alert('รหัสผ่านใหม่ไม่ตรงกัน');
      return;
    }

    this.isLoading = true;
    this.authService.changePassword({
      oldPassword: this.passwords.current,
      newPassword: this.passwords.new,
      confirmPassword: this.passwords.confirm
    }).subscribe({
      next: (res) => {
        this.isLoading = false;
        alert(res.message || 'เปลี่ยนรหัสผ่านสำเร็จ');
        this.passwords = { current: '', new: '', confirm: '' };
        this.authService.logout(); // Typically force re-login
      },
      error: (err) => {
        this.isLoading = false;
        alert('เปลี่ยนรหัสผ่านไม่สำเร็จ: ' + err.message);
      }
    });
  }

  uploadAvatar() {
    alert('ฟังก์ชันอัปโหลดรูปภาพกำลังอยู่ระหว่างการพัฒนา');
  }
}
