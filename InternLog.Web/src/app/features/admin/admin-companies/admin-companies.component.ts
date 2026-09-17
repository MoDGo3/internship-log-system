import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';

interface Company {
  companyID: number;
  companyName: string;
  address?: string;
  contactPerson?: string;
  phone?: string;
  studentCount?: number; // Might not be returned by backend currently
}

@Component({
  selector: 'app-admin-companies',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-companies.component.html',
  styleUrls: ['./admin-companies.component.scss']
})
export class AdminCompaniesComponent implements OnInit {
  companies: Company[] = [];

  constructor(private adminService: AdminService) {}

  ngOnInit() {
    this.loadCompanies();
  }

  loadCompanies() {
    this.adminService.getCompanies().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.companies = res.data;
        }
      },
      error: (err) => console.error('Failed to load companies', err)
    });
  }
}
