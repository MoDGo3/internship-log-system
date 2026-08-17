import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Location } from '@angular/common';

@Component({
  selector: 'app-report-preview',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './report-preview.component.html',
  styleUrls: ['./report-preview.component.scss']
})
export class ReportPreviewComponent {

  constructor(private location: Location) {}

  goBack() {
    this.location.back();
  }

  downloadPdf() {
    alert('ระบบกำลังดาวน์โหลดไฟล์ PDF...');
  }
}
