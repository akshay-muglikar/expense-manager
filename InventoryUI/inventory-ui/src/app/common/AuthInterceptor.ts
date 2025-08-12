import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable } from 'rxjs';
import {tap} from 'rxjs/operators';
import {Router} from '@angular/router';
import { AuthService } from './AuthService';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private router: Router, private authService:AuthService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getaccessToken();
    if(request.body && request.body instanceof FormData) {
      request = request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    }else{
    request = request.clone({
          setHeaders: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`
          }
        });
    }
    

    return next.handle(request).pipe( tap(() => {},
      (err: any) => {
      if (err instanceof HttpErrorResponse) {
        if (err.status !== 401) {
         return;
        }
        this.router.navigate(['login']);
      }
    }));
  }
}