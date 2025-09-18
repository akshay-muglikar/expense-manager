import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CustomerService } from '../services/customer.service';
import { CustomerModel, CustomerAccountModel } from '../contracts/customer.model';
import { BillService } from '../services/bill.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { TranslateDirective, TranslatePipe } from "@ngx-translate/core";
import { PaginatedTableComponent, TableAction, TableCol } from "../common/paginated-table/paginated-table.component";

@Component({
  selector: 'app-customer',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTabsModule,
    MatCardModule,
    MatIconModule, TranslatePipe,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressBarModule,
    TranslateDirective,
    PaginatedTableComponent
],
  templateUrl: './customer.component.html',
  styleUrl: './customer.component.scss'
})
export class CustomerComponent implements OnInit {
  customers: CustomerModel[] = [];
  filteredCustomers: CustomerModel[] = [];
  selectedCustomer: CustomerModel | null = null;
  customerAccount: CustomerAccountModel | null = null;
  isLoading = false;
  searchTerm = '';
  selectedTabIndex = 0;
  rowActions:TableAction[]=[]
  // Grid column definitions for bills
  rowAction(data:any){
    if(data.action=='edit'){
      this.editBill(data.index);
    }else if(data.action=='print'){
      this.download(data.index)
    }

  }
  actions : TableAction[]= [
    {name:'Edit', action:'edit'},{name:'Print', action:'print'}
  ]
  billsColumnDefs: TableCol[] = [
    { key: 'id', name: 'Bill Number',width: 150 },
    { 
      key: 'billDate', 
      name: 'Bill Date', 
      width: 120,
    },
    { 
      key: 'totalAmount', 
      name: 'Amount', 
      currency: "₹", 
      width: 120,
    }
  ];
totalBills: any;
totalAmount: any;
lastBillDate: any;

  constructor(private customerService: CustomerService,
                private billService: BillService, // Assuming you have a BillService for bill-related operations
                private snackBar: MatSnackBar,private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading = true;
    this.customerService.getAllCustomers().subscribe({
      next: (customers) => {
        this.customers = customers;
        this.filteredCustomers = [...customers];
        this.isLoading = false;
        this.rowActions=[...this.actions]
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        this.isLoading = false;
      }
    });
  }

  onSearchChange(): void {
    if (!this.searchTerm.trim()) {
      this.filteredCustomers = [...this.customers];
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredCustomers = this.customers.filter(customer =>
        customer.name.toLowerCase().includes(term) ||
        customer.mobile.includes(term) 
      );
    }
  }

  selectCustomer(customer: CustomerModel): void {
    this.selectedCustomer = customer;
    this.selectedTabIndex = 1;
    this.loadCustomerAccount(customer.name, customer.mobile);
  }

  loadCustomerAccount(customerName: string, customerMobile: string): void {
    this.isLoading = true;
    this.customerService.getCustomerAccount(customerName, customerMobile).subscribe({
      next: (account) => {
        this.customerAccount = account;
        this.isLoading = false;
        this.calculateSummary()
      },
      error: (error) => {
        console.error('Error loading customer account:', error);
        this.isLoading = false;
      }
    });
  }
    calculateSummary() {
        this.totalBills = this.customerAccount?.bills.length || 0;
        this.totalAmount = this.customerAccount?.bills.reduce((sum, bill) => sum + bill.totalAmount, 0) || 0;
        this.lastBillDate = this.customerAccount?.bills.reduce((latest, bill) => {
          return new Date(bill.billDate) > new Date(latest) ? bill.billDate : latest;
        }, new Date(0));    
    }

  goBackToCustomers(): void {
    this.selectedTabIndex = 0;
    this.selectedCustomer = null;
    this.customerAccount = null;
  }

  getCustomerInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString();
  }
  actionCellRenderer(params: any) {
    const container = document.createElement('div');
    container.className = 'action-cell';

    const editButton = document.createElement('button');
    editButton.className = 'btn btn-primary';
    editButton.innerHTML = '<mat-icon>edit</mat-icon>';
    editButton.onclick = () => this.editBill(params.data.id);
    
    const printButton = document.createElement('button');
    printButton.className = 'btn btn-secondary';
    printButton.innerHTML = '<mat-icon>download</mat-icon>';
    printButton.onclick = () => this.download(params.data.id);
    
    container.appendChild(editButton);
    container.appendChild(printButton);
    
    return container;
  }
  editBill(id: number) {
    // Implement the logic to edit the bill

    this.router.navigateByUrl('bill?billid=' + this.customerAccount?.bills[id].id);
  }
  createNewBill(){
    this.router.navigateByUrl('bill?customerId=' + this.selectedCustomer?.name + '&customerMobile=' + this.selectedCustomer?.mobile);
  }
  download(id : number) {
   let billId=this.customerAccount?.bills[id].id
    this.billService.download(billId!.toString()).subscribe((resp: any) => {
      const fileURL = URL.createObjectURL(resp);
      const a = document.createElement('a');
      a.href = fileURL;
      a.download = 'bill.pdf'; // Specify the file name
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    });
  }
}
