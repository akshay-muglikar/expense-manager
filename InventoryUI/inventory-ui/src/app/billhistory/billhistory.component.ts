import { Component } from '@angular/core';
import { BillService } from '../services/bill.service';
import { FormsModule } from '@angular/forms';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, RowSelectionOptions, GridApi, GridReadyEvent } from 'ag-grid-community';
import { CommonModule, DecimalPipe, formatDate } from '@angular/common';
import { MatOptionModule, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-billhistory',
  standalone: true,
  imports: [
    AgGridAngular,
        CommonModule,
        MatProgressBarModule,
        MatSelectModule,
        MatIconModule,
        MatCardModule,
        FormsModule,
        MatDatepickerModule,
        MatFormFieldModule,
        MatInputModule
  ],
  templateUrl: './billhistory.component.html',
  styleUrls: ['./billhistory.component.scss'],
  providers: [provideNativeDateAdapter()]
})
export class BillhistoryComponent {
  loading = false;
  bills: any[] = [];
  billsList: any[] = [];
   filter: string = "0";
  defaultColDef = {
    flex: 1,
    minWidth: 100,
    sortable: true,
    filter: true,
    resizable: true
  };
  start: Date = new Date();
  end: Date = new Date();
  hideCustomDate: boolean = true;
  startDateText:string='';
  endDateText:string ='';
  subtractDays(days: number, date: Date = new Date()): Date {
    if (days == 0) {
      date.setHours(0, 0, 0, 0)
      return date;
    }
    date.setDate(date.getDate() - days);
    return date;
  }
  onSelectChange() {
    if (this.filter != "-1") {
      this.end = new Date();
      this.start = this.subtractDays(Number(this.filter))
      this.loadBills()
      this.hideCustomDate = true;
      
    }
    else {
      this.start = this.subtractDays(0);
      this.end = this.subtractDays(0);
      this.hideCustomDate = false;
    }
  }
  colDefs: ColDef[] = [
    { 
      headerName: 'Bill No',
      field: 'id',
      width: 100,
      flex: 0
    },
    { 
      headerName: 'Date',
      field: 'billDate',
      valueFormatter: (params) => {
        return new Date(params.value).toLocaleDateString();
      },
      sortable: true,
      sort: 'desc' as const,
      sortIndex: 0
    },
    { 
      headerName: 'Customer',
      field: 'name'
    },
    { 
      headerName: 'Actions',
      field: 'action',
      cellRenderer: this.actionCellRenderer.bind(this),
      width: 100,
      flex: 0,
      sortable: false,
      filter: false
    }
  ];
    
    showGrid = false;

  constructor(private billService: BillService) {}

  ngOnInit() {
    this.onSelectChange();
  }
onCustomDateSet() {
    this.start = new Date(this.startDateText)
    this.end = this.subtractDays(-1, new Date(this.endDateText))
    this.loadBills();
  }
  loadBills() {
    this.loading = true;
    const formatedDate = formatDate(this.start, 'yyyy-MM-ddTHH:mm:ss', 'en-US');
    const formatedStartDate = formatDate(this.end, 'yyyy-MM-ddTHH:mm:ss', 'en-US');
    this.bills = [];
    this.billsList = [];
    this.billService.getBillsbyDate(formatedDate, formatedStartDate).subscribe({
      next: (bills) => {
        this.bills = bills;
        this.billsList = bills;
      },
      error: (error) => {
        console.error('Error loading bills:', error);
      },
      complete: () => {
        this.loading = false;
      }
     });
  }

  onFilterTextBoxChanged() {
    const filterValue = (document.getElementById('filter-text-box') as HTMLInputElement).value;
    this.bills = this.billsList.filter(bill =>
      bill.name?.toLowerCase().includes(filterValue.toLowerCase()) ||
      bill.id?.toString().includes(filterValue) ||
      bill.billDate?.toString().includes(filterValue)
    );
  }

  onRowSelected(event: any) {
    const selectedRow = event.api.getSelectedRows()[0];
    if (selectedRow) {
      this.editBill(selectedRow.id);
    }
  }

  onGridReady(event: GridReadyEvent) {
    event.api.sizeColumnsToFit();
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
    printButton.innerHTML = '<mat-icon>print</mat-icon>';
    printButton.onclick = () => this.printBill(params.data.id);
    
    container.appendChild(editButton);
    container.appendChild(printButton);
    
    return container;
  }

  editBill(billId: number) {
    window.dispatchEvent(new CustomEvent('edit-bill', { detail: { billId } }));
  }

  printBill(billId: number) {
    // TODO: Implement print functionality
    window.dispatchEvent(new CustomEvent('print-bill', { detail: { billId } }));
  }
}
