import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet, NavigationEnd } from '@angular/router';
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community'; 
import {MatToolbarModule} from '@angular/material/toolbar';
import { AuthService } from './common/AuthService';
import { HeaderComponent } from './header/header.component';
import { ShrivoChatComponent } from './shrivo-chat/shrivo-chat.component';
import { filter } from 'rxjs/operators';
import {
    TranslateService,
    TranslateDirective
} from "@ngx-translate/core";
ModuleRegistry.registerModules([AllCommunityModule]);

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MatIconModule, 
    MatToolbarModule, HeaderComponent, ShrivoChatComponent,
     TranslateDirective],
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
  clentName :string = '';
  sidebarExpanded = false; // Start collapsed
  routes :RoutDetails[] = [
    new RoutDetails('bill','Billing', 'receipt'),
    new RoutDetails('dashboard','Dashboard', 'speedometer'),
    new RoutDetails('expense','Expense', 'shopping_bag'),
    new RoutDetails('inventory','Inventory', 'inventory'),
    new RoutDetails('vendor','Vendors', 'people'),
    new RoutDetails('customer','Customers', 'person'),
  ]
  
  isLoggedIn(): boolean {
    return !!this.authService.getaccessToken();
  }
  constructor(private router: Router, 
    private authService : AuthService,
    private active: ActivatedRoute,
    private translate: TranslateService){

    translate.addLangs(['en', 'mr']);

    const savedLang = localStorage.getItem('lang');
      translate.use(savedLang || 'en');
    }
  
  changeLanguage(selectedLang:string) {
    const lang = selectedLang
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
  }
  onclickRoute(index:number){
    this.selected = index;
    this.sidebarExpanded = false; // Collapse sidebar on route change
    this.router.navigate([this.routes[this.selected].path])
  }

  toggleSidebar() {
    this.sidebarExpanded = !this.sidebarExpanded;
  }

  onSidebarMouseEnter() {
    if (!this.sidebarExpanded) {
      this.sidebarExpanded = true;
    }
  }

  onSidebarMouseLeave() {
    // Small delay to prevent flickering when moving between sidebar elements
    setTimeout(() => {
      this.sidebarExpanded = false;
    }, 200);
  }
  getClientDetails(){
    this.authService.getClientDetails().subscribe((resp: any)=>{
      this.clentName = resp.name;
    });
  }
  ngOnInit(){
    this.getClientDetails();
    this.setSelectedIndexFromRoute();
    
    // Subscribe to router events to update selected index on route changes
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.setSelectedIndexFromRoute();
      });
  }

  setSelectedIndexFromRoute() {
    const currentPath = this.router.url.split('?')[0].split('/')[1]; // Get the first path segment
    
    // Handle empty path (root route)
    if (!currentPath || currentPath === '') {
      this.selected = 1; // Default to dashboard or whichever is your default
      return;
    }
    
    const routeIndex = this.routes.findIndex(route => route.path === currentPath);
    this.selected = routeIndex >= 0 ? routeIndex : 1; // Default to dashboard if not found
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
