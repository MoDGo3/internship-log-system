import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, TopbarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="app-layout">
      <app-sidebar></app-sidebar>
      <div class="main-container">
        <app-topbar></app-topbar>
        <main class="content-area">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `,
  styles: [`
    .app-layout { display: flex; height: 100vh; overflow: hidden; background-color: #F5F6FA; }
    .main-container { flex: 1; display: flex; flex-direction: column; min-width: 0; }
    .content-area { flex: 1; overflow-y: auto; overflow-x: hidden; position: relative; }
  `]
})
export class MainLayoutComponent {}
