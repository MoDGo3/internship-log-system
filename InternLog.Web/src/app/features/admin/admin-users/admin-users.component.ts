import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService, AdminUser } from '../services/admin.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.scss']
})
export class AdminUsersComponent implements OnInit {
  private adminService = inject(AdminService);

  users: AdminUser[] = [];
  isLoading = false;

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading = true;
    this.adminService.getUsers().subscribe({
      next: (data: AdminUser[]) => {
        this.users = data;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Failed to load users', err);
        this.isLoading = false;
      }
    });
  }

  getRoleClass(role: string): string {
    const r = (role || '').toLowerCase();
    switch(r) {
      case 'student': return 'chip-info';
      case 'advisor': return 'chip-success';
      case 'admin': return 'chip-danger';
      default: return '';
    }
  }

  toggleStatus(user: AdminUser) {
    const updatedStatus = user.status === 'active' ? 'suspended' : 'active';
    this.adminService.updateUser(user.id, { isActive: updatedStatus === 'active' }).subscribe({
      next: () => {
        user.status = updatedStatus as 'active' | 'suspended';
      },
      error: (err: any) => console.error('Failed to update user status', err)
    });
  }
}
