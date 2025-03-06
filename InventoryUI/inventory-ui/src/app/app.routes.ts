import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { InventoryComponent } from './inventory/inventory.component';
import { ExpenseComponent } from './expense/expense.component';
import { BillingComponent } from './billing/billing.component';
import { LoginComponent } from './login/login.component';

export const routes: Routes = [

    {path : '' , component : DashboardComponent},
    {path : 'inventory', component: InventoryComponent},
    {path:'expense', component: ExpenseComponent},
    {path:'bill', component:BillingComponent},
    {path:'login', component:LoginComponent}

];
