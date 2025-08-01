import { Component, inject } from '@angular/core';
import { ExpenseService } from '../common/ExpenseService';
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


@Component({
  selector: 'app-expense',
  standalone: true,
  imports: [
    CommonModule,
    DecimalPipe,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
    FormsModule,
    AgGridAngular
  ],
  templateUrl: './expense.component.html',
  styleUrl: './expense.component.scss'
})
export class ExpenseComponent {
  rowSelection: RowSelectionOptions | "single" | "multiple" = {
    mode: "singleRow",
  };
  onRowSelected($event: RowSelectedEvent<any,any>) {
    let selectedRows = this.gridApi.getSelectedRows();
    if(selectedRows.length==0){
      this.clear();
    }else{
      this.selectedExpense = selectedRows[0];
    }
  }
  clear() {
    this.selectedExpense = { id: "0", description: '', user: 'admin', amount: "0", date: formatDate((new Date()), "yyyy-MM-ddThh:mm:ss", "en-US") }
  }
  getDateString(arg0: string) {
    return formatDate(arg0, "dd MMM yyy hh:ss", "en-US");
  }
  expenses: Expense[] = [];
  private _snackBar = inject(MatSnackBar);

  selectedExpense: Expense = { id: "0", description: '', user: 'admin', amount: "0", date: formatDate((new Date()), "yyyy-MM-ddThh:mm:ss", "en-US") }
  constructor(private expenseService: ExpenseService) { }

  loading = false;
  totalExpenses = 0;

  defaultColDef = {
    sortable: true,
    filter: true,
    resizable: true
  };

  colDefs: ColDef[] = [
    { 
      field: "description",
      headerName: "Description",
      flex: 2,
      minWidth: 200
    },
    { 
      field: "amount",
      headerName: "Amount",
      type: 'numericColumn',
      flex: 1,
      minWidth: 120,
      valueFormatter: (params) => {
        return params.value ? '₹' + Number(params.value).toLocaleString() : '';
      }
    },
    { 
      field: "date",
      headerName: "Date",
      flex: 1,
      minWidth: 180,
      valueFormatter: (params) => {
        return this.getDateString(params.value);
      },
      sortable: true,
      sort: 'desc' as const,
      sortIndex: 0
    }
  ];

  private gridApi!: GridApi;
  onGridReady(params: GridReadyEvent) {
    this.gridApi = params.api;
  }

  ngOnInit() {
    this.getExpenses();
  }
  getExpenses() {
    this.loading = true;
    this.expenseService.getExpenses().subscribe({
      next: (expenses) => {
        this.expenses = expenses;
        this.totalExpenses = expenses.reduce((sum, exp) => sum + Number(exp.amount), 0);
      },
      error: (error) => {
        this.openSnackBar('Error loading expenses');
        console.error('Error loading expenses:', error);
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
  addExpense() {
    if (this.selectedExpense.description?.trim() === '') {
      this.openSnackBar('Please add expense details');
      return;
    }

    this.loading = true;
    const action = this.selectedExpense.id === "0" ?
      this.expenseService.addExpense(this.selectedExpense) :
      this.expenseService.updateExpense(this.selectedExpense);

    action.subscribe({
      next: () => {
        this.openSnackBar(this.selectedExpense.id === "0" ? 'Expense added' : 'Expense updated');
        this.clear();
        this.getExpenses();
      },
      error: (error) => {
        this.openSnackBar('Error saving expense');
        console.error('Error saving expense:', error);
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
  openSnackBar(message: string) {
    this._snackBar.open(message, 'Close', { duration: 4000 });
  }

  selectExpense(index: number) {
    this.selectedExpense = this.expenses[index];
  }

  onFilterTextBoxChanged() {
    const filterValue = (document.getElementById('filter-text-box') as HTMLInputElement).value.toLowerCase();
    this.gridApi.setGridOption('quickFilterText', filterValue);
  }
}

export interface Expense {
  id: string,
  description: string
  user: string,
  amount: string
  date: string
}
