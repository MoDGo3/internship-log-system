import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  userRole = computed(() => this.authService.currentUser()?.role ?? null);
  isStudent = computed(() => this.userRole() === 'student');
  isAdvisor = computed(() => this.userRole() === 'advisor');
  isAdmin = computed(() => this.userRole() === 'admin');

  constructor(private authService: AuthService) {}

  logout() {
    this.authService.logout();
  }
}
