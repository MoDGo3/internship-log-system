import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-internships',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-internships.component.html',
  styleUrls: ['./admin-internships.component.scss']
})
export class AdminInternshipsComponent {
  internships = [
    { 
      id: 1, 
      student: 'นายสมชาย ใจดี', 
      company: 'บริษัท เอบีซี เทคโนโลยี จำกัด', 
      advisor: 'อ.วิชัย เก่งกาจ', 
      mentor: 'พี่โตโต้', 
      term: '1/2569', 
      period: 'ส.ค. - พ.ย. 69', 
      status: 'active' 
    },
    { 
      id: 2, 
      student: 'นางสาวสมหญิง น่ารัก', 
      company: 'บริษัท ซอฟต์แวร์ เฮ้าส์ จำกัด', 
      advisor: 'อ.ใจดี มีสุข', 
      mentor: 'พี่แมว', 
      term: '1/2569', 
      period: 'ส.ค. - พ.ย. 69', 
      status: 'pending' 
    }
  ];

  getStatusClass(status: string): string {
    return status === 'active' ? 'chip-success' : 'chip-warning';
  }

  getStatusText(status: string): string {
    return status === 'active' ? 'กำลังฝึก' : 'รอเริ่มฝึก';
  }
}
