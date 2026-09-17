import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CompaniesService } from '../services/companies.service';
import { Company } from '../models/companies.model';
import { AuthService } from '../../auth/services/auth.service';

@Component({
  selector: 'app-companies',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6 max-w-7xl mx-auto">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold">Companies</h1>
        <button *ngIf="canManage()" (click)="openForm()" class="bg-blue-600 text-white px-4 py-2 rounded shadow">
          Add Company
        </button>
      </div>

      <div *ngIf="loading" class="text-gray-500">Loading companies...</div>
      
      <div *ngIf="!loading && companies.length === 0" class="text-gray-500">
        No companies found.
      </div>

      <div class="bg-white rounded shadow overflow-hidden" *ngIf="companies.length > 0">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Address</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Phone</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Working Hours</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr *ngFor="let c of companies">
              <td class="px-6 py-4 whitespace-nowrap">{{ c.companyName }}</td>
              <td class="px-6 py-4 whitespace-nowrap">{{ c.address }}</td>
              <td class="px-6 py-4 whitespace-nowrap">{{ c.phone }}</td>
              <td class="px-6 py-4 whitespace-nowrap">{{ c.workStartTime }} - {{ c.workEndTime }}</td>
              <td class="px-6 py-4 whitespace-nowrap">
                <button *ngIf="canEdit(c)" (click)="openForm(c)" class="text-blue-600 hover:text-blue-900 mr-2">Edit</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Modal -->
      <div *ngIf="showModal" class="fixed z-10 inset-0 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
        <div class="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
          <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" aria-hidden="true"></div>
          <span class="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>
          <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
            <form [formGroup]="companyForm" (ngSubmit)="saveCompany()">
              <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">{{ editingId ? 'Edit' : 'Add' }} Company</h3>
                <div class="mb-4">
                  <label class="block text-sm font-medium text-gray-700">Company Name</label>
                  <input type="text" formControlName="companyName" class="mt-1 p-2 block w-full border border-gray-300 rounded-md">
                </div>
                <div class="mb-4">
                  <label class="block text-sm font-medium text-gray-700">Address</label>
                  <input type="text" formControlName="address" class="mt-1 p-2 block w-full border border-gray-300 rounded-md">
                </div>
                <div class="mb-4">
                  <label class="block text-sm font-medium text-gray-700">Phone</label>
                  <input type="text" formControlName="phone" class="mt-1 p-2 block w-full border border-gray-300 rounded-md">
                </div>
                <div class="grid grid-cols-2 gap-4 mb-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Start Time</label>
                    <input type="time" formControlName="workStartTime" class="mt-1 p-2 block w-full border border-gray-300 rounded-md">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">End Time</label>
                    <input type="time" formControlName="workEndTime" class="mt-1 p-2 block w-full border border-gray-300 rounded-md">
                  </div>
                </div>
              </div>
              <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                <button type="submit" [disabled]="companyForm.invalid" class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50">Save</button>
                <button type="button" (click)="closeForm()" class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm">Cancel</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  `
})
export class CompaniesComponent implements OnInit {
  companies: Company[] = [];
  loading = false;
  showModal = false;
  editingId: number | null = null;
  companyForm: FormGroup;

  private companiesService = inject(CompaniesService);
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);

  constructor() {
    this.companyForm = this.fb.group({
      companyName: ['', Validators.required],
      address: ['', Validators.required],
      phone: ['', Validators.required],
      workStartTime: ['', Validators.required],
      workEndTime: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.loadCompanies();
  }

  loadCompanies() {
    this.loading = true;
    this.companiesService.getCompanies().subscribe({
      next: (res) => {
        if (res.success) {
          this.companies = res.data || [];
        }
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  canManage(): boolean {
    const role = this.authService.currentUser()?.roleName;
    return role === 'advisor' || role === 'admin';
  }

  canEdit(company: Company): boolean {
    const user = this.authService.currentUser();
    if (user?.roleName === 'admin') return true;
    if (user?.roleName === 'advisor') return company.advisorID === user.userID;
    return false;
  }

  openForm(company?: Company) {
    if (company) {
      this.editingId = company.companyID;
      this.companyForm.patchValue(company);
    } else {
      this.editingId = null;
      this.companyForm.reset();
    }
    this.showModal = true;
  }

  closeForm() {
    this.showModal = false;
    this.companyForm.reset();
    this.editingId = null;
  }

  saveCompany() {
    if (this.companyForm.invalid) return;

    const data = this.companyForm.value;
    const request = this.editingId 
      ? this.companiesService.updateCompany(this.editingId, data)
      : this.companiesService.createCompany(data);

    request.subscribe(res => {
      if (res.success) {
        this.loadCompanies();
        this.closeForm();
      }
    });
  }
}
