import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { InventoryComponent } from './inventory/inventory.component';
import { ExpenseComponent } from './expense/expense.component';
import { LoginComponent } from './login/login.component';
import { BillV2Component } from './bill-v2/bill-v2.component';
import { LandingComponent } from './landing/landing.component';
import { BookDemoComponent } from './book-demo/book-demo.component';

export const routes: Routes = [
    {path : '' , component : LandingComponent},
    {path : 'dashboard' , component : DashboardComponent},
    {path : 'inventory', component: InventoryComponent},
    {path:'expense', component: ExpenseComponent},
    {path:'bill', component:BillV2Component},
    {path:'login', component:LoginComponent},
    {path:'book-demo', component: BookDemoComponent},
];
