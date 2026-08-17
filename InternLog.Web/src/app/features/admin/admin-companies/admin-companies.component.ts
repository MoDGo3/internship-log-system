import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-companies',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-companies.component.html',
  styleUrls: ['./admin-companies.component.scss']
})
export class AdminCompaniesComponent {
  companies = [
    { id: 1, name: 'บริษัท เอบีซี เทคโนโลยี จำกัด', address: '123 ถ.สุขุมวิท กทม.', phone: '02-123-4567', time: '09:00 - 18:00', studentCount: 5 },
    { id: 2, name: 'บริษัท ซอฟต์แวร์ เฮ้าส์ จำกัด', address: '456 ถ.สีลม กทม.', phone: '02-987-6543', time: '08:30 - 17:30', studentCount: 2 }
  ];
}
