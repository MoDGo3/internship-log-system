import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvisorVisitsService } from '../../../core/services/advisor-visits.service';
import { AuthService } from '../../../core/auth/auth.service';
import { VisitRecord } from '../../../models/visit.model';
import { format } from 'date-fns';

@Component({
  selector: 'app-visits-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './visits-page.component.html',
  styleUrls: ['./visits-page.component.scss']
})
export class VisitsPageComponent implements OnInit {
  isAdvisor = false; // We can check this based on logged-in role
  visits: any[] = [];

  constructor(
    private visitsService: AdvisorVisitsService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.isAdvisor = this.authService.currentUser()?.role === 'advisor';
    this.loadVisits();
  }

  loadVisits() {
    this.visitsService.getVisits().subscribe({
      next: (res) => {
        const data = res.data;
        if (data) {
          this.visits = data.map((v: VisitRecord) => ({
            ...v,
            studentName: 'นักศึกษา', // The backend might return student object or ID
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
    // In a real implementation this would open a dialog or navigate to a form
    alert('เปิดหน้าต่างบันทึกการนิเทศ (สำหรับอาจารย์)');
  }

  viewDetails(visit: any) {
    alert(`เปิดรายละเอียดการนิเทศครั้งที่ ${visit.visitNo}`);
  }
}
