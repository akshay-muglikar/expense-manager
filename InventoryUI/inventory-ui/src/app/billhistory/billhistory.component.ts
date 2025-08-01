import { Component } from '@angular/core';
import { BillService } from '../services/bill.service';
import { FormsModule } from '@angular/forms';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, RowSelectionOptions, GridApi, GridReadyEvent } from 'ag-grid-community';

@Component({
  selector: 'app-billhistory',
  standalone: true,
  imports: [
    FormsModule,
    MatProgressBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    AgGridAngular
  ],
  templateUrl: './billhistory.component.html',
  styleUrls: ['./billhistory.component.scss']
})
export class BillhistoryComponent {
  loading = false;
  bills: any[] = [];
  billsList: any[] = [];
  
  defaultColDef = {
    flex: 1,
    minWidth: 100,
    sortable: true,
    filter: true,
    resizable: true
  };
  
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
    this.loadBills();
  }

  loadBills() {
    this.loading = true;
    this.billService.getAllBills().subscribe({
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
