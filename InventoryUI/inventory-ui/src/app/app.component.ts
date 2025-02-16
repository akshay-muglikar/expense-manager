import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community'; 
import {MatToolbarModule} from '@angular/material/toolbar';

ModuleRegistry.registerModules([AllCommunityModule]);

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, MatIconModule, MatToolbarModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'inventory-ui';
  selected=0;
  routes :RoutDetails[] = [
    new RoutDetails('','Dashboard', 'home'),
    new RoutDetails('bill','Billing', 'receipt'),
    new RoutDetails('expense','Expense', 'money_off'),
    new RoutDetails('inventory','Inventory', 'inventory'),

  ]
  onclickRoute(index:number){
    this.selected = index;
  }
}

export class RoutDetails {
  path:string ='';
  label:string ='';
  icon:string ='';  
  constructor(path: string, label: string, icon: string = '') {
    this.path = path;
    this.label = label;
    this.icon = icon;
  }
  onMenuClick(){

  }
}
