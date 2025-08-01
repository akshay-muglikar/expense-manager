import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatLabel, MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from "@angular/material/form-field";
import { FormsModule } from '@angular/forms';
import { ExpenseService } from '../common/ExpenseService';
import { formatDate } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import type { ColDef, RowSelectionOptions } from 'ag-grid-community'; // Column Definition Type Interface
import {
  GridApi, GridReadyEvent
} from "ag-grid-community";
import { AgGridAngular } from 'ag-grid-angular';
import { Expense } from '../expense/expense.component';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { BillModel } from '../models/bill.model';
import { BillService } from '../services/bill.service';
@Component({
  selector: 'app-dashboard',
  imports: [AgGridAngular, MatProgressBarModule, MatSelectModule, MatIconModule, FormsModule, MatDatepickerModule, MatFormFieldModule, MatInputModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  providers: [],

})
export class DashboardComponent {
  calculateExpense(): number {
    return this.expenses.reduce((n, { amount }) => n + Number(amount), 0);
  }
  calculate() {
    //let sum = this.bills.reduce((n, { calculatedBillAmount }) => n + calculatedBillAmount, 0);
    let expense = this.calculateExpense()
    return  expense;
  }
  private gridApi!: GridApi;
  isHighlighted: any;
  hideCustomDate: boolean = true;
  filter: string = "0";
  start: Date = new Date();
  end: Date = new Date();

  startDateText:string='';
  endDateText:string ='';
  bills: BillModel[] = []
  expenses: Expense[] = [];
  allItems: DashboardItems[] = [];
  dashboardStats:DashboardStats = {billCount:0,revenue:0,expenses:0}
  hideFormLoading = true;
  colDefs: ColDef[] = [
    { field: "type", width: 120 },
    { field: "name", width: 400 },
    { field: "price", width: 80 },
    { field: "date", width: 200 }
  ];
  constructor(private billService: BillService, private expenseService: ExpenseService) {
    this.onSelectChange();
  }
  ngOnInit() {
  }

  onGridReady(params: GridReadyEvent) {
    this.gridApi = params.api;
  }

  onSelectChange() {
    if (this.filter != "-1") {
      this.end = new Date();
      this.start = this.subtractDays(Number(this.filter))
      this.getBills()
      this.hideCustomDate = true;
      
    }
    else {
      this.start = this.subtractDays(0);
      this.end = this.subtractDays(0);
      this.hideCustomDate = false;
    }
  }
  onCustomDateSet() {
    this.start = new Date(this.startDateText)
    this.end = this.subtractDays(-1, new Date(this.endDateText))
    this.getBills();
  }

  subtractDays(days: number, date: Date = new Date()): Date {
    if (days == 0) {
      date.setHours(0, 0, 0, 0)
      return date;
    }
    date.setDate(date.getDate() - days);
    return date;
  }

  getBills() {
    this.hideFormLoading = false;
    const formatedDate = formatDate(this.start, 'yyyy-MM-ddTHH:mm:ss', 'en-US');
    const formatedStartDate = formatDate(this.end, 'yyyy-MM-ddTHH:mm:ss', 'en-US');
    this.allItems = [];

    this.billService.getBillsbyDate(formatedDate, formatedStartDate).subscribe((response) => {
      console.log(response);
      this.bills = response;
      this.expenseService.getExpensesbyDate(formatedDate, formatedStartDate).subscribe((resonse) => {
        this.expenses = resonse;
        this.expenses.forEach(x => {
          this.allItems.push({
            type: 'Expense',
            name: x.description,
            price: x.amount?.toString() ?? "0",
            date: formatDate(x.date ?? new Date(), 'dd MMM yyyy HH:mm', 'en-US')
          })
        });
        this.bills.forEach(x => {
          this.allItems.push({
            type: 'Bill',
            name: x.name + ' - ' + x.mobile,
            price: '0',//x.calculatedBillAmount?.toString() ?? "0",
            date: formatDate(x.billDate ?? new Date(), 'dd MMM yyyy HH:mm', 'en-US')
          }) 
        });
        this.gridApi.setGridOption("rowData", this.allItems);
        this.hideFormLoading = true;
        this.calculateStats();
      });
    });
  }

  calculateStats(){
    this.dashboardStats.billCount = this.bills.length;
    this.dashboardStats.expenses = this.calculateExpense();
    this.dashboardStats.revenue = this.calculate();
  }
}


export interface DashboardItems {
  type: string,
  name: string,
  price: string,
  date: string
}

export interface DashboardStats {
  billCount: number,
  revenue: number,
  expenses: number
}