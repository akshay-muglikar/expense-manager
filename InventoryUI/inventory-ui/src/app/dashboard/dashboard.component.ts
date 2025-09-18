import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatLabel, MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from "@angular/material/form-field";
import { FormsModule } from '@angular/forms';
import { ExpenseService } from '../common/ExpenseService';
import { CommonModule, DecimalPipe, formatDate } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import type { ColDef, RowSelectionOptions } from 'ag-grid-community'; // Column Definition Type Interface

import { AgGridAngular } from 'ag-grid-angular';
import { Expense } from '../expense/expense.component';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { BillModel, GetAllBillModel, GetBillModel } from '../models/bill.model';
import { BillService } from '../services/bill.service';
import { TranslateDirective, TranslatePipe, TranslateService } from '@ngx-translate/core';
import { InventoryService } from '../common/InventoryService/InventoryService';
import { ItemSummary } from '../models/item-summary.model';
import { SalesSummary } from '../models/sales-summary';
import { AuthService, ClientModel } from '../common/AuthService';
import { PaginatedTableComponent ,TableCol} from "../common/paginated-table/paginated-table.component";
@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    DecimalPipe,
    MatProgressBarModule,
    MatSelectModule,
    MatIconModule,
    MatCardModule,
    FormsModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule, TranslateDirective, TranslatePipe,
    PaginatedTableComponent
],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  providers: [provideNativeDateAdapter()],

})
export class DashboardComponent {


  tableColDefs: TableCol[]=[]
  
  setColDefs(){
  this.tableColDefs  = [{
      name : this.translate.instant('Type'),
      width: 150, 
      key: 'type'
    },
    {
      name : this.translate.instant("Description"),
      width: 250, 
      key: 'name'
    },
    
    {
      name : this.translate.instant("Amount"),
      width: 80, 
      key: 'price',
      currency:"₹"
    },
    {
      name : this.translate.instant("Date"),
      width: 150, 
      key:'date'
    },
    {
      name : this.translate.instant("Payment Mode"),
      width: 150, 
      key:'paymentMode'
    },
    {
      name : this.translate.instant("Manager"),
      width: 80, 
      key:'paymentUser'
    },
  ]
}
  tableData = [];
  calculateExpense(): number {
    let sum = 0;
    this.expenses.forEach(x => {
      if(x.expenseType === 'CREDIT') {
        sum -= Number(x.amount);
      } else {
        sum += Number(x.amount);
      }
    });

    return sum;}
  calculate() {
    let sum = this.bills.reduce((n, { totalAmount }) => n + (totalAmount || 0), 0);
    //let expense = this.calculateExpense()
    return sum;
  }
  isHighlighted: any;
  hideCustomDate: boolean = true;
  filter: string = "0";
  start: Date = new Date();
  end: Date = new Date();

  startDateText:string='';
  endDateText:string ='';
  bills: GetAllBillModel[] = []
  expenses: Expense[] = [];
  allItems: DashboardItems[] = [];
  dashboardStats:DashboardStats = {billCount:0,revenue:0,expenses:0}
  hideFormLoading = true;
  colDefs: ColDef[] =[]
  
  clientModel : ClientModel | undefined =undefined;
  

  constructor(private billService: BillService, private expenseService: ExpenseService
    ,private translate : TranslateService, private inventoryService : InventoryService
    ,private authService : AuthService
  ) {
    const savedLang = localStorage.getItem('lang');
    translate.use(savedLang || 'en');
    this.setColDefs();

    this.onSelectChange();
    this.getSummary();
    this.getSalesSummary();
    
    
  }
  ngOnInit() {
    this.authService.getClientDetails().subscribe((res)=>{
      this.clientModel =res;
    })
  }
  inventorySummary: ItemSummary | null = null;
  salesSummary : SalesSummary | null = null;
  options : any= {};
  getSummary(){
    this.inventoryService.summary().subscribe((resp)=>{
      this.inventorySummary = resp
      this.inventorySummary.lowStockItems = this.inventorySummary.lowStockItems.slice(0, 3); 
    });
  }
   getSalesSummary(){
    const formatedDate = formatDate(this.start, 'yyyy-MM-ddTHH:mm:ss', 'en-US');
    const formatedStartDate = formatDate(this.end, 'yyyy-MM-ddTHH:mm:ss', 'en-US');
    this.billService.summary(formatedStartDate, formatedDate).subscribe((resp)=>{
      this.salesSummary = resp
      this.salesSummary.topProducts = this.salesSummary.topProducts.slice(0, 3); 
    });
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
    let allItems :DashboardItems[] =[]
    this.getSalesSummary()
    this.billService.getBillsbyDate(formatedDate, formatedStartDate).subscribe((response) => {
      console.log(response);
      this.bills = response;
      this.expenseService.getExpensesbyDate(formatedDate, formatedStartDate).subscribe((resonse : any[]) => {
        this.expenses = resonse;
        resonse.forEach(x => {
          if (x.supplierId) {
            allItems.push({
              type:  'Payment' + (x.expenseType === 'CREDIT' ? ' from vendor' : ' to vendor'),
              name: x.description,
              price: x.amount?.toString() ?? "0",
              date: formatDate(x.date ?? new Date(), 'dd MMM yyyy HH:mm', 'en-US'),
              paymentMode: x.paymentMode ,
              paymentUser: x.user ?? ''
            });
          }else{
            allItems.push({
              type: 'Expense',
              name: x.description,
              price: x.amount?.toString() ?? "0",
              date: formatDate(x.date ?? new Date(), 'dd MMM yyyy HH:mm', 'en-US'),
              paymentMode: x.paymentMode ,
              paymentUser: x.user ?? ''
            });
          }
        });
        this.bills.forEach(x => {
          allItems.push({
            type: 'Bill',
            name: x.name + ' - ' + x.mobile,
            price: (x.totalAmount||0 ).toString(),//x.calculatedBillAmount?.toString() ?? "0",
            date: formatDate(x.billDate ?? new Date(), 'dd MMM yyyy HH:mm', 'en-US'),
            paymentMode: paymentMode[x.paymentMode].toString(),
            paymentUser: x.paymentUser?? ''
          }) 
        });
        this.allItems = [...allItems]
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
  paymentMode?: string,
  paymentUser?: string
}

export interface DashboardStats {
  billCount: number,
  revenue: number,
  expenses: number
}
enum paymentMode {
CARD, UPI,CASH
}