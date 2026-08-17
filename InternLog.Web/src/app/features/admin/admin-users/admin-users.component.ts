import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { UserRecord } from '../../../models/admin.model';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.scss']
})
export class AdminUsersComponent implements OnInit {
  users: UserRecord[] = [];

  constructor(private adminService: AdminService) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.adminService.getUsers().subscribe({
      next: (res: any) => {
        const data = res.data;
        if (data) {
          this.users = data;
        }
      },
      error: (err: any) => console.error('Failed to load users', err)
    });
  }

  getRoleClass(role: string): string {
    const r = (role || '').toLowerCase();
    switch(r) {
      case 'student': return 'chip-info';
      case 'advisor': return 'chip-success';
      case 'supervisor': return 'chip-warning';
      case 'admin': return 'chip-danger';
      default: return '';
    }
  }

  toggleStatus(user: UserRecord) {
    const updatedStatus = !user.isActive;
    this.adminService.updateUser(user.userID.toString(), { isActive: updatedStatus }).subscribe({
      next: () => {
        user.isActive = updatedStatus;
      },
      error: (err: any) => console.error('Failed to update user status', err)
    });
  }
}
