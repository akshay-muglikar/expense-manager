import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../common/AuthService';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-header',
  imports: [NgIf],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  clientName : string | null = "";
  isLoggedIn : boolean = false;
  private sub!: Subscription;

  constructor(private authService: AuthService){
    
  }
  ngOnInit(){
    
      this.sub = this.authService.clientName$.subscribe(name => {
        this.clientName = name ?? '';
        this.isLoggedIn = !!name;
      });

  }
  ngOnDestroy(): void {
    if (this.sub) {
      this.sub.unsubscribe();
    }
  }

}
