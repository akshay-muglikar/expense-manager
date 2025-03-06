import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../common/AuthService';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username:string="";
  password:string="";
  constructor(private router: Router,private authService:AuthService){}

  login(){
    this.authService.login(this.username, this.password);
  }
}
