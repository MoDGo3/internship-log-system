import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Location } from '@angular/common';
import { ReportsService } from '../../../core/services/reports.service';

@Component({
  selector: 'app-report-preview',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './report-preview.component.html',
  styleUrls: ['./report-preview.component.scss']
})
export class ReportPreviewComponent {
  isDownloading = false;

  constructor(
    private location: Location,
    private route: ActivatedRoute,
    private reportsService: ReportsService
  ) {}

  goBack() {
    this.location.back();
  }

  downloadPdf() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id || this.isDownloading) return;

    this.isDownloading = true;
    this.reportsService.downloadPdf(id).subscribe({
      next: () => (this.isDownloading = false),
      error: () => {
        this.isDownloading = false;
        alert('ไม่สามารถดาวน์โหลดไฟล์ PDF ได้');
      }
    });
  }
}
