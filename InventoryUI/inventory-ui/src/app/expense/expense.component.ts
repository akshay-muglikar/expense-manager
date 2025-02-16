import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ExpenseService } from '../common/ExpenseService';
import { formatDate } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { AgGridAngular, ICellRendererAngularComp } from 'ag-grid-angular';
import { ColDef, GridApi, GridReadyEvent, ICellRendererParams, RowSelectedEvent, RowSelectionOptions } from 'ag-grid-community';


@Component({
  selector: 'app-expense',
  imports: [MatIconModule, FormsModule, AgGridAngular],
  templateUrl: './expense.component.html',
  styleUrl: './expense.component.scss'
})
export class ExpenseComponent {
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

  ngOnInit() {
    this.getExpenses();
  }
  getExpenses() {
    this.expenseService.getExpenses().subscribe((response) => {
      this.expenses = response;
    });
  }
  rowSelection: RowSelectionOptions | "single" | "multiple" = {
      mode: "singleRow",
    };
  colDefs: ColDef[] = [
    { field: "description", width: 200 },
    { field: "amount", width: 120 },
    { field: "date", width: 180 },
  ];
  private gridApi!: GridApi;
  onGridReady(params: GridReadyEvent) {
    this.gridApi = params.api;
  }

  addExpense() {
    if (this.selectedExpense.id == "0") {
      if(this.selectedExpense.description==null || this.selectedExpense.description.trim()==''){
        this.openSnackBar('Please add expense details')
        return;
      }
      this.expenseService.addExpense(this.selectedExpense).subscribe((resp) => {
        this.openSnackBar('Added')
        this.selectedExpense = { id: "0", description: '', user: 'admin', amount: "0", date: formatDate((new Date()), "yyyy-MM-ddThh:mm:ss", "en-US") };
        this.getExpenses();
      });
      return;
    }
    this.expenseService.updateExpense(this.selectedExpense).subscribe((resp) => {
      this.openSnackBar('Updated')
      this.selectedExpense = { id: "0", description: '', user: 'admin', amount: "0", date: formatDate((new Date()), "yyyy-MM-ddThh:mm:ss", "en-US") };
      this.getExpenses();
    });
    return;
  }
  openSnackBar(message: string) {
    this._snackBar.open(message, 'Close', { duration: 4000 });
  }

  selectExpense(index: number) {
    this.selectedExpense = this.expenses[index];
  }


}

export interface Expense {
  id: string,
  description: string
  user: string,
  amount: string
  date: string
}
