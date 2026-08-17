import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile-page.component.html',
  styleUrls: ['./profile-page.component.scss']
})
export class ProfilePageComponent {
  
  profile = {
    firstName: 'สมชาย',
    lastName: 'ใจดี',
    studentId: '641234567',
    email: 'somchai.j@student.ac.th',
    phone: '081-234-5678',
    avatar: ''
  };

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

  saveProfile() {
    alert('บันทึกข้อมูลส่วนตัวสำเร็จ');
  }

  changePassword() {
    if (this.passwords.new !== this.passwords.confirm) {
      alert('รหัสผ่านใหม่ไม่ตรงกัน');
      return;
    }
    alert('เปลี่ยนรหัสผ่านสำเร็จ');
    this.passwords = { current: '', new: '', confirm: '' };
  }

  uploadAvatar() {
    alert('เปิดหน้าต่างเลือกไฟล์รูปภาพ');
  }
}
