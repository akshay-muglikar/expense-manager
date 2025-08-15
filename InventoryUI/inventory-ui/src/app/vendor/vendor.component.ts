import { Component, inject, ViewChild } from '@angular/core';
import { VendorService } from '../services/vendor.service';
import { CommonModule, DecimalPipe, formatDate } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { FormsModule } from '@angular/forms';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, GridApi, GridReadyEvent, RowSelectedEvent, RowSelectionOptions } from 'ag-grid-community';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule, MatTabGroup } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { VendorModel, VendorAccountModel } from '../contracts/vendor.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-vendor',
  standalone: true,
  imports: [
    AgGridAngular,
    CommonModule,
    MatProgressBarModule,
    MatSelectModule,
    MatIconModule,
    MatCardModule,
    MatButtonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatTabsModule,
    MatTooltipModule,
    MatDatepickerModule
  ],
  templateUrl: './vendor.component.html',
  styleUrl: './vendor.component.scss',
  providers: [provideNativeDateAdapter()]
})
export class VendorComponent {
  @ViewChild('tabGroup') tabGroup!: MatTabGroup;
  private _snackBar = inject(MatSnackBar);
  
  loading = false;
  vendors: VendorModel[] = [];
  filteredVendors: VendorModel[] = [];
  vendorAccounts: VendorAccountModel[] = [];
  selectedVendor: VendorModel = { name: '', mobile: '' };
  selectedVendorForAccount: VendorModel | null = null;
  searchTerm = '';
  newExpense: Omit<VendorAccountModel, 'id' | 'vendorId'> = {
    description: '',
    amount: 0,
    date: new Date(),
    expenseType: 'DEBIT'
  };
  
  rowSelection: RowSelectionOptions | "single" | "multiple" = {
    mode: "singleRow",
  };

  private accountGridApi!: GridApi;

  // Column definitions for vendor account grid
  accountColDefs: ColDef[] = [
    { 
      field: 'description', 
      headerName: 'Description',
      flex: 1,
      sortable: true,
      filter: true
    },
    { 
      field: 'amount', 
      headerName: 'Amount',
      width: 120,
      sortable: true,
      valueFormatter: (params) => '₹' + params.value?.toFixed(2),
      cellStyle: (params) => {
        const row = params.data;
        if (row?.expenseType === 'CREDIT') {
          return { color: 'green', fontWeight: 'bold' };
        } else if (row?.expenseType === 'DEBIT') {
          return { color: 'red', fontWeight: 'bold' };
        }
        return null;
      }
    },
    { 
      field: 'type', 
      headerName: 'Type',
      width: 120,
      sortable: true,
      valueFormatter: (params) => {
        return params.data.expenseType === 'CREDIT' ? 'Payment From Vendor' : 'Payment To Vendor';
      },
      cellStyle: (params) => {
        if (params.data.expenseType === 'CREDIT') {
          return { color: 'green', fontWeight: 'bold' };
        } else if (params.data.expenseType === 'DEBIT') {
          return { color: 'red', fontWeight: 'bold' };
        }
        return null;
      }
    },
    { 
      field: 'date', 
      headerName: 'Date',
      width: 120,
      sortable: true,
      valueFormatter: (params) => {
        if (params.value) {
          return formatDate(params.value, 'dd/MM/yyyy', 'en-US');
        }
        return '';
      }
    }
  ];

  defaultColDef: ColDef = {
    resizable: true,
    sortable: true,
    filter: false,
  };

  constructor(private vendorService: VendorService) {}

  ngOnInit() {
    this.getVendors();
  }

  // Card view methods
  filterVendors() {
    if (!this.searchTerm.trim()) {
      this.filteredVendors = [...this.vendors];
    } else {
      const searchLower = this.searchTerm.toLowerCase();
      this.filteredVendors = this.vendors.filter(vendor => 
        vendor.name.toLowerCase().includes(searchLower) ||
        vendor.mobile.includes(searchLower)
      );
    }
  }

  selectVendor(vendor: VendorModel) {
    this.selectedVendor = { ...vendor };
    if (this.selectedVendor.id) {
      this.viewVendorAccount(this.selectedVendor.id);
    }
  }

  editVendor(vendor: VendorModel) {
    this.selectedVendor = { ...vendor };
  }

  formatDate(date: Date): string {
    return formatDate(date, 'dd/MM/yyyy', 'en-US');
  }

  onAccountGridReady(params: GridReadyEvent) {
    this.accountGridApi = params.api;
  }

  onAccountFilterTextBoxChanged() {
    const filterTextBox = document.getElementById('account-filter-text-box') as HTMLInputElement;
    this.accountGridApi.setGridOption('quickFilterText', filterTextBox.value);
  }

  getVendors() {
    this.loading = true;
    this.vendorService.getAllVendors().pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: (vendors) => {
        this.vendors = vendors;
        this.filteredVendors = [...vendors];
      },
      error: (error) => {
        this.openSnackBar('Error loading vendors');
        console.error('Error loading vendors:', error);
      }
    });
  }

  addVendor() {
    if (!this.selectedVendor.name.trim() || !this.selectedVendor.mobile.trim()) {
      this.openSnackBar('Please fill in all required fields');
      return;
    }

    this.loading = true;
    const vendorToAdd = {
      name: this.selectedVendor.name.trim(),
      mobile: this.selectedVendor.mobile.trim()
    };

    this.vendorService.addVendor(vendorToAdd).pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: (newVendor) => {
        this.vendors.push(newVendor);
        this.vendors = [...this.vendors]; // Trigger change detection
        this.filteredVendors = [...this.vendors];
        this.clearVendorForm();
        this.openSnackBar('Vendor added successfully');
        this.getVendors(); // Refresh vendor list
      },
      error: (error) => {
        this.openSnackBar('Error adding vendor');
        console.error('Error adding vendor:', error);
      }
    });
  }

  updateVendor() {
    if (!this.selectedVendor.id || !this.selectedVendor.name.trim() || !this.selectedVendor.mobile.trim()) {
      this.openSnackBar('Please select a vendor and fill in all required fields');
      return;
    }

    this.loading = true;
    this.vendorService.updateVendor(this.selectedVendor.id, this.selectedVendor).pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: (updatedVendor) => {
        const index = this.vendors.findIndex(v => v.id === updatedVendor.id);
        if (index !== -1) {
          this.vendors[index] = updatedVendor;
          this.vendors = [...this.vendors]; // Trigger change detection
        }
        this.clearVendorForm();
        this.openSnackBar('Vendor updated successfully');
        this.getVendors(); // Refresh vendor list
      },
      error: (error) => {
        this.openSnackBar('Error updating vendor');
        console.error('Error updating vendor:', error);
      }
    });
  }

  deleteVendor() {
    if (!this.selectedVendor.id) {
      this.openSnackBar('Please select a vendor to delete');
      return;
    }

    if (!confirm('Are you sure you want to delete this vendor?')) {
      return;
    }

    this.loading = true;
    this.vendorService.deleteVendor(this.selectedVendor.id).pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: () => {
        this.vendors = this.vendors.filter(v => v.id !== this.selectedVendor.id);
        this.clearVendorForm();
        this.openSnackBar('Vendor deleted successfully');
      },
      error: (error) => {
        this.openSnackBar('Error deleting vendor');
        console.error('Error deleting vendor:', error);
      }
    });
  }

  clearVendorForm() {
    this.selectedVendor = { name: '', mobile: '' };
  }

  viewVendorAccount(id: number) {
    this.getVendorAccount(id);
    this.selectedVendorForAccount = this.vendors.find(v => v.id === id) || null;
    this.clearExpenseForm(); // Clear expense form when switching vendors
    
    // Switch to the vendor account tab
    setTimeout(() => {
      if (this.tabGroup) {
        this.tabGroup.selectedIndex = 1; // Switch to the second tab (Vendor Account)
      }
    }, 100);
  }

  getVendorAccount(vendorId: number) {
    this.loading = true;
    this.vendorService.getVendorAccount(vendorId).pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: (accounts) => {
        // Sort by date descending (newest first)
        this.vendorAccounts = accounts.sort((a, b) => 
          new Date(b.date).getTime() - new Date(a.date).getTime()
        );
      },
      error: (error) => {
        this.openSnackBar('Error loading vendor account');
        console.error('Error loading vendor account:', error);
      }
    });
  }

  getTotalBalance(): number {
    return this.vendorAccounts.reduce((total, account) => {
      return account.expenseType === 'CREDIT' ? total + account.amount : total - account.amount;
    }, 0);
  }

  getTotalCredits(): number {
    return this.vendorAccounts
      .filter(account => account.expenseType === 'CREDIT')
      .reduce((total, account) => total + account.amount, 0);
  }

  getTotalDebits(): number {
    return this.vendorAccounts
      .filter(account => account.expenseType === 'DEBIT')
      .reduce((total, account) => total + account.amount, 0);
  }

  // Expense management methods
  addVendorExpense() {
    if (!this.selectedVendorForAccount?.id || !this.isExpenseFormValid()) {
      this.openSnackBar('Please fill in all required fields');
      return;
    }

    this.loading = true;
    const expenseToAdd : VendorAccountModel = {
      description: this.newExpense.description.trim(),
      amount: Number(this.newExpense.amount),
      date: this.newExpense.date,
      expenseType: this.newExpense.expenseType,
      supplierId: this.selectedVendorForAccount.id
    };
    this.vendorService.addVendorExpense(expenseToAdd).pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: (newExpense) => {
       // this.vendorAccounts.unshift(newExpense); // Add to beginning of array
        // Re-sort to maintain date order
        this.vendorAccounts = this.vendorAccounts.sort((a, b) => 
          new Date(b.date).getTime() - new Date(a.date).getTime()
        );
        this.clearExpenseForm();
        this.openSnackBar('Transaction added successfully');
        this.getVendorAccount(this.selectedVendorForAccount?.id!); // Refresh account details
      },
      error: (error) => {
        this.openSnackBar('Error adding transaction');
        console.error('Error adding vendor expense:', error);
      }
    });
  }

  isExpenseFormValid(): boolean {
    return !!(
      this.newExpense.description.trim() &&
      this.newExpense.amount > 0 &&
      this.newExpense.expenseType &&
      this.newExpense.date
    );
  }

  clearExpenseForm() {
    this.newExpense = {
      description: '',
      amount: 0,
      date: new Date(),
      expenseType: 'DEBIT',
      paymentMode: 'CASH' // Reset payment mode
    };
  }

  openSnackBar(message: string) {
    this._snackBar.open(message, 'Close', {
      duration: 4000,
    });
  }
}
