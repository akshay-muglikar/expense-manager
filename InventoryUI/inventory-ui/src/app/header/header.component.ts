import { CommonModule, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, UserModel } from '../common/AuthService';
import { Subscription } from 'rxjs';
import { MatIcon, MatIconModule } from "@angular/material/icon";
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateDirective } from "@ngx-translate/core";
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-header',
  imports: [MatIconModule, NgIf, TranslateDirective, FormsModule, CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {

openPanel(panel:string){
  if(panel=='notification'){
    this.showNotification = !this.showNotification;
    this.showSettings= false;
    this.showShareOptions = false
  }
  else if(panel=='user'){
    this.showNotification = false;
    this.showSettings= !this.showSettings;
    this.showShareOptions = false
  }else {
    this.showNotification = false;
    this.showSettings= false;
    this.showShareOptions = !this.showShareOptions
  }
}
copyInput() {
  navigator.clipboard.writeText(this.url).then(() => {
    this.snakbar.open("Link Copied...",'Close', { duration: 3000 })
  });
}
getInitials(){
  return localStorage.getItem('username')?.charAt(0).toUpperCase()??"U"
}
  isLoggedIn : boolean = false;
  private sub!: Subscription;
  showNotification = false;
  showSettings = false;
  showShareOptions = false
  url = window.location.origin
  user : UserModel | null = null
  constructor(private authService: AuthService, private snakbar : MatSnackBar,
    private router: Router
  ){
    
  }
  navigateToLogin() {
    this.router.navigate(['login']);
  }
  ngOnInit(){
      this.isLoggedIn = this.authService.isLoggedIn()
      if(this.isLoggedIn){
        this.getUser();
      }
      window.addEventListener('login-success',(event: any) => {
        this.isLoggedIn = this.authService.isLoggedIn();
        this.getUser()
      })
      window.addEventListener('logout', (event : any)=>{
        this.isLoggedIn =false;
      })
      

  }
  getUser(){
     this.authService.getUser().subscribe((res)=>{
      this.user = res;
    })
  }
  settings(){
    this.showSettings=false;
    this.router.navigate(['config']);
  }
  logout() {
    this.showSettings=false;
    this.authService.logout();
  }
  ngOnDestroy(): void {
    if (this.sub) {
      this.sub.unsubscribe();
    }
  }
notifications: Notification[] = [
    {
      id: 1,
      type: 'settings',
      title: 'New Configuration Settings',
      message: 'Configure your organization settings, user permissions and more in the new settings page.',
      time: new Date()
    },
    {
      id: 2,
      type: 'info',
      title: 'Welcome to Shrivo',
      message: 'Get started by exploring our features and setting up your inventory.',
      time: new Date()
    }
  ];
  clickNotification(id:number){
    if(id==1){
      this.showNotification = false;
      this.router.navigate(['config'])
    }
    if(id==2){
      this.showNotification = false;
      this.router.navigate(['dashboard'])
    }
  }
}
interface Notification {
  id: number;
  type: 'info' | 'settings';
  title: string;
  message: string;
  time: Date;
}
