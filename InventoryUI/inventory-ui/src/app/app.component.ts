import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community'; 
import {MatToolbarModule} from '@angular/material/toolbar';
import { AuthService } from './common/AuthService';
import { HeaderComponent } from './header/header.component';

ModuleRegistry.registerModules([AllCommunityModule]);

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MatIconModule, MatToolbarModule, HeaderComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  logout() {
    this.authService.logout();
  }
  clientName : string | undefined = "";
  getClient() {
    this.clientName =  this.authService.clientModel?.name
  }
  title = 'inventory-ui';
  selected=0;
  routes :RoutDetails[] = [
    new RoutDetails('bill','Billing', 'receipt'),
    new RoutDetails('dashboard','Dashboard', 'dashboard'),
    new RoutDetails('expense','Expense', 'money_off'),
    new RoutDetails('inventory','Inventory', 'inventory'),
  ]

  isLoggedIn(): boolean {
    return !!this.authService.getaccessToken();
  }
  constructor(private router: Router, private authService : AuthService){}
  
  navigateToLogin() {
    this.router.navigate(['login']);
  }

  onclickRoute(index:number){
    this.selected = index;
    this.router.navigate([this.routes[this.selected].path])
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
