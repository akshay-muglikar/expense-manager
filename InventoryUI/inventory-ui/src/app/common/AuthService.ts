
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { BehaviorSubject } from "rxjs/internal/BehaviorSubject";

@Injectable({providedIn: 'root'})
export class AuthService {
  bookDemo(value: any) {
      return this.http.post('/api/login/contact', value);
  }
  logout() {
    localStorage.removeItem(this.tokenKey)
    this.router.navigate(['login']);
  }
  constructor(private router: Router,private http: HttpClient) {
  }
  tokenKey='in-ui-tk';
  accesstoken:string="";
  public clientModel:ClientModel | undefined;

  private clientNameSubject = new BehaviorSubject<string | null>(null);
  clientName$ = this.clientNameSubject.asObservable();

  
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
        this.getClient()
    });
  }

  getClient(){
    this.http.get<ClientModel>("/api/login/client").subscribe((resp)=>{
        this.clientModel = resp;
        console.log("---------"+ this.clientModel.name)
        localStorage.setItem('client_name', this.clientModel.name)
        this.clientNameSubject.next(this.clientModel.name);

    });
  }




}
export interface LoginAuth{
    accessToken:string
}

export interface ClientModel{
    id:string,
    name:string

}