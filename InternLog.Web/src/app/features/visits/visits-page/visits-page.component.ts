import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvisorVisitsService } from '../../../core/services/advisor-visits.service';
import { AuthService } from '../../../core/auth/auth.service';
import { AdvisorVisitResponseDto } from '../../../models/visit.model';
import { format } from 'date-fns';

interface VisitVM extends AdvisorVisitResponseDto {
  dateFormatted: string;
}

@Component({
  selector: 'app-visits-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './visits-page.component.html',
  styleUrls: ['./visits-page.component.scss']
})
export class VisitsPageComponent implements OnInit {
  isAdvisor = false; // We can check this based on logged-in role
  visits: VisitVM[] = [];
  internshipID = 1;

  constructor(
    private visitsService: AdvisorVisitsService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.authService.getMe().subscribe(res => {
      if (res.success && res.data) {
        this.isAdvisor = res.data.role === 'advisor';
      }
    });
    this.loadVisits();
  }

  loadVisits() {
    this.visitsService.getVisits(this.internshipID).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.visits = res.data.map((v: AdvisorVisitResponseDto) => ({
            ...v,
            studentName: v.studentName || 'นักศึกษา',
            advisorName: v.advisorName || 'อาจารย์นิเทศ',
            dateFormatted: v.visitDate ? format(new Date(v.visitDate), 'dd MMM yyyy') : '-'
          }));
        }
      },
      error: (err) => console.error('Failed to load visits', err)
    });
  }

  getFormatClass(format: string): string {
    return format === 'Onsite' ? 'chip-info' : 'chip-warning';
  }

  addVisit() {
    alert('เปิดหน้าต่างบันทึกการนิเทศ (สำหรับอาจารย์)');
  }

  viewDetails(visit: VisitVM) {
    alert(`เปิดรายละเอียดการนิเทศครั้งที่ ${visit.visitNo}`);
  }
}
