
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";

@Injectable({providedIn: 'root'})
export class AuthService {
  constructor(private router: Router,private http: HttpClient) {
  }
  tokenKey='in-ui-tk';
  accesstoken:string="";

  getaccessToken(){
    let value =  this.accesstoken;
    if (value === null || value === undefined || value === '') {
      return localStorage.getItem(this.tokenKey);
    }
    return value;
  }

  login(username:string, password: string){
    let data = {'username':username, 'password':password}
    this.http.post<LoginAuth>("/api/login", data).subscribe((resp)=>{
        this.accesstoken = resp.accessToken;
        localStorage.setItem(this.tokenKey, resp.accessToken)
        this.router.navigate(['bill']);
    });
  }
}
export interface LoginAuth{
    accessToken:string
}