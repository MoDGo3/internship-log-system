import { Component, inject, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsService, ReportSummary, ReportDetailRow } from '../services/reports.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-layout">
      
      <!-- Command Bar -->
      <div class="command-bar">
        <div class="cmd-left">
          <h2>รายงานสถิติ (Reports)</h2>
        </div>
        
        <div class="cmd-center">
          <!-- Optional filter -->
          <select class="form-control-sm">
            <option>สิงหาคม 2026</option>
            <option>กรกฎาคม 2026</option>
            <option>ทั้งหมด</option>
          </select>
        </div>

        <div class="cmd-right">
          <button class="btn-primary" [disabled]="isExporting()" (click)="exportToPdf()">
            <span class="icon">📄</span> {{ isExporting() ? 'กำลังสร้างเอกสาร...' : 'Export PDF' }}
          </button>
        </div>
      </div>

      <!-- Main Content -->
      <div class="page-content">
        
        <div *ngIf="isLoading()" class="loading-overlay">
          <div class="spinner"></div>
          <span>กำลังโหลดข้อมูลสถิติ...</span>
        </div>

        <ng-container *ngIf="!isLoading()">
          
          <!-- Summary Cards -->
          <div class="summary-cards" *ngIf="summary() as sum">
            <div class="card card-primary">
              <div class="card-icon">💼</div>
              <div class="card-info">
                <span class="card-label">วันทำงานทั้งหมด</span>
                <span class="card-value">{{ sum.totalDays }} <small>วัน</small></span>
              </div>
            </div>
            
            <div class="card card-success">
              <div class="card-icon">✅</div>
              <div class="card-info">
                <span class="card-label">มาตรงเวลา</span>
                <span class="card-value">{{ sum.onTimeDays }} <small>วัน</small></span>
              </div>
            </div>
            
            <div class="card card-warning">
              <div class="card-icon">⏰</div>
              <div class="card-info">
                <span class="card-label">มาสาย</span>
                <span class="card-value">{{ sum.lateDays }} <small>วัน</small></span>
              </div>
            </div>
            
            <div class="card card-danger">
              <div class="card-icon">🏖️</div>
              <div class="card-info">
                <span class="card-label">วันลา</span>
                <span class="card-value">{{ sum.leaveDays }} <small>วัน</small></span>
              </div>
            </div>
          </div>

          <!-- Charts Area (Mock UI with CSS) -->
          <div class="charts-row">
            
            <div class="chart-card shadow-card">
              <h3 class="chart-title">สัดส่วนการเข้างาน</h3>
              <div class="donut-chart-mock">
                <div class="donut-ring"></div>
                <div class="donut-center">
                  <span class="donut-value">88%</span>
                  <span class="donut-label">ตรงเวลา</span>
                </div>
              </div>
              <div class="chart-legend">
                <div class="legend-item"><span class="dot bg-success"></span> ตรงเวลา ({{ summary()?.onTimeDays }})</div>
                <div class="legend-item"><span class="dot bg-warning"></span> สาย ({{ summary()?.lateDays }})</div>
                <div class="legend-item"><span class="dot bg-danger"></span> วันลา ({{ summary()?.leaveDays }})</div>
              </div>
            </div>

            <div class="chart-card shadow-card">
              <h3 class="chart-title">สถิติการทำงาน 4 สัปดาห์ล่าสุด</h3>
              <div class="bar-chart-mock">
                <!-- W1 -->
                <div class="bar-col">
                  <div class="bar-track">
                    <div class="bar-fill bg-success" style="height: 100%;"></div>
                  </div>
                  <span class="bar-label">W1</span>
                </div>
                <!-- W2 -->
                <div class="bar-col">
                  <div class="bar-track">
                    <div class="bar-fill bg-success" style="height: 80%;"></div>
                    <div class="bar-fill bg-warning" style="height: 20%;"></div>
                  </div>
                  <span class="bar-label">W2</span>
                </div>
                <!-- W3 -->
                <div class="bar-col">
                  <div class="bar-track">
                    <div class="bar-fill bg-success" style="height: 60%;"></div>
                    <div class="bar-fill bg-danger" style="height: 40%;"></div>
                  </div>
                  <span class="bar-label">W3</span>
                </div>
                <!-- W4 -->
                <div class="bar-col">
                  <div class="bar-track">
                    <div class="bar-fill bg-success" style="height: 100%;"></div>
                  </div>
                  <span class="bar-label">W4</span>
                </div>
              </div>
            </div>

          </div>

          <!-- Details Table -->
          <div class="table-container shadow-card mt-4">
            <div class="table-header">
              <h3 class="m-0">รายละเอียดประวัติ (Recent Logs)</h3>
            </div>
            <table class="data-table">
              <thead>
                <tr>
                  <th>วันที่</th>
                  <th>ประเภท</th>
                  <th>สถานะ</th>
                  <th>หมายเหตุ</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let row of details()">
                  <td>{{ row.date | date:'d MMM yyyy' }}</td>
                  <td>{{ row.type }}</td>
                  <td>
                    <span class="badge" 
                          [class.badge-success]="row.status === 'ตรงเวลา' || row.status === 'อนุมัติ'"
                          [class.badge-warning]="row.status === 'สาย'"
                          [class.badge-info]="row.status === 'รออนุมัติ'">
                      {{ row.status }}
                    </span>
                  </td>
                  <td class="text-muted">{{ row.remark }}</td>
                </tr>
              </tbody>
            </table>
          </div>

        </ng-container>
      </div>
    </div>
  `,
  styles: [`
    /* Layout */
    .page-layout { display: flex; flex-direction: column; height: 100vh; background: #F5F6FA; font-family: 'Sarabun', sans-serif; overflow: hidden; }
    h2, h3 { font-family: 'Prompt', sans-serif; color: #1F2328; margin: 0; }
    .m-0 { margin: 0; }
    .mt-4 { margin-top: 24px; }
    
    /* Command Bar */
    .command-bar { display: flex; justify-content: space-between; align-items: center; padding: 12px 24px; background: #FFFFFF; border-bottom: 1px solid #E5E7EB; flex-shrink: 0; }
    .cmd-left h2 { font-size: 20px; font-weight: 600; }
    
    .form-control-sm { padding: 6px 12px; border: 1px solid #D1D5DB; border-radius: 6px; font-family: 'Sarabun'; font-size: 14px; outline: none; background: #F9FAFB; cursor: pointer; }
    
    .btn-primary { background: #5B5FC7; color: white; border: none; padding: 8px 16px; border-radius: 6px; font-weight: 600; font-family: 'Prompt'; cursor: pointer; transition: 0.2s; display: flex; align-items: center; gap: 8px; }
    .btn-primary:hover:not([disabled]) { background: #464AB9; }
    .btn-primary[disabled] { background: #9CA3AF; cursor: not-allowed; }

    /* Content Area */
    .page-content { flex: 1; overflow-y: auto; padding: 24px; position: relative; }
    
    .loading-overlay { position: absolute; inset: 0; background: rgba(255,255,255,0.8); z-index: 10; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #5B5FC7; font-weight: 600; }
    .spinner { width: 32px; height: 32px; border: 3px solid #EEF0FF; border-top-color: #5B5FC7; border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 12px; }
    @keyframes spin { to { transform: rotate(360deg); } }

    /* Summary Cards */
    .summary-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .card { background: #FFFFFF; border-radius: 12px; padding: 20px; display: flex; align-items: center; gap: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; border-left-width: 4px; }
    
    .card-primary { border-left-color: #5B5FC7; }
    .card-success { border-left-color: #16A34A; }
    .card-warning { border-left-color: #D97706; }
    .card-danger { border-left-color: #9333EA; }
    
    .card-icon { width: 48px; height: 48px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 24px; background: #F9FAFB; }
    .card-info { display: flex; flex-direction: column; }
    .card-label { font-size: 13px; color: #6B7280; font-weight: 600; margin-bottom: 4px; }
    .card-value { font-family: 'Prompt'; font-size: 24px; font-weight: 700; color: #1F2328; }
    .card-value small { font-size: 14px; font-weight: 500; color: #6B7280; }

    /* Charts Area */
    .charts-row { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 24px; }
    .chart-card { background: #FFFFFF; border-radius: 12px; padding: 24px; display: flex; flex-direction: column; align-items: center; }
    .chart-title { font-size: 16px; margin-bottom: 24px; align-self: flex-start; }
    .shadow-card { box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid #E5E7EB; }

    /* CSS Donut Mock */
    .donut-chart-mock { position: relative; width: 160px; height: 160px; border-radius: 50%; background: conic-gradient(#16A34A 0% 88%, #D97706 88% 95%, #9333EA 95% 100%); display: flex; align-items: center; justify-content: center; margin-bottom: 24px; }
    .donut-center { width: 110px; height: 110px; border-radius: 50%; background: #FFFFFF; display: flex; flex-direction: column; align-items: center; justify-content: center; }
    .donut-value { font-family: 'Prompt'; font-size: 24px; font-weight: 700; color: #1F2328; }
    .donut-label { font-size: 12px; color: #6B7280; }
    
    .chart-legend { display: flex; gap: 16px; font-size: 13px; color: #4B5563; }
    .legend-item { display: flex; align-items: center; gap: 6px; }
    .dot { width: 10px; height: 10px; border-radius: 50%; }
    .bg-success { background: #16A34A; }
    .bg-warning { background: #D97706; }
    .bg-danger { background: #9333EA; }

    /* CSS Bar Chart Mock */
    .bar-chart-mock { display: flex; gap: 32px; height: 160px; align-items: flex-end; padding-bottom: 30px; margin-top: 10px; }
    .bar-col { display: flex; flex-direction: column; align-items: center; gap: 12px; height: 100%; justify-content: flex-end; }
    .bar-track { width: 40px; height: 120px; background: #F3F4F6; border-radius: 6px; display: flex; flex-direction: column; justify-content: flex-end; overflow: hidden; }
    .bar-fill { width: 100%; transition: height 0.5s ease-out; }
    .bar-label { font-size: 13px; color: #6B7280; font-family: 'Prompt'; }

    /* Table */
    .table-container { background: #FFFFFF; border-radius: 12px; overflow: hidden; }
    .table-header { padding: 16px 24px; border-bottom: 1px solid #E5E7EB; background: #F9FAFB; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 14px; }
    .data-table th { background: #FFFFFF; padding: 12px 24px; text-align: left; font-family: 'Prompt'; font-weight: 600; color: #374151; border-bottom: 1px solid #E5E7EB; }
    .data-table td { padding: 16px 24px; border-bottom: 1px solid #E5E7EB; color: #1F2328; }
    .data-table tbody tr:hover { background: #F9FAFB; }
    
    .badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 600; }
    .badge-success { background: #DCFCE7; color: #16A34A; }
    .badge-warning { background: #FEF3C7; color: #D97706; }
    .badge-info { background: #E0F2FE; color: #0284C7; }
    
    .text-muted { color: #6B7280; }
  `]
})
export class ReportsComponent implements OnInit {
  private reportsService = inject(ReportsService);

  isLoading = signal(true);
  isExporting = signal(false);
  
  summary = signal<ReportSummary | null>(null);
  details = signal<ReportDetailRow[]>([]);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading.set(true);
    
    // Simulate parallel loading
    let done = 0;
    
    this.reportsService.getSummary().subscribe(data => {
      this.summary.set(data);
      done++;
      if (done === 2) this.isLoading.set(false);
    });

    this.reportsService.getDetails().subscribe(data => {
      this.details.set(data);
      done++;
      if (done === 2) this.isLoading.set(false);
    });
  }

  exportToPdf() {
    this.isExporting.set(true);
    this.reportsService.exportPdf().pipe(
      finalize(() => {
        this.isExporting.set(false);
        alert('ส่งออก PDF สำเร็จ! (ไฟล์จะถูกดาวน์โหลดไปยังเครื่องของคุณ)');
      })
    ).subscribe();
  }
}
