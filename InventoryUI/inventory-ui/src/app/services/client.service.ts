import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClientDetails } from '../models/client-Details';

@Injectable({
  providedIn: 'root'
})
export class ClientService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) {}

  getClientDetails(): Observable<ClientDetails> {
    return this.http.get<ClientDetails>(`${this.baseUrl}/login/client-details`);
  }
   updateClientDetails(clientDetails: ClientDetails) {
    return this.http.post<ClientDetails>(`${this.baseUrl}/login/client-details`,clientDetails);
  }

  uploadLogo(logo:any){
    return this.http.post(`${this.baseUrl}/login/upload-logo`, logo)
  }
  preview(){
     return this.http.get('/api/bill/preview', {
                  headers: new HttpHeaders({
                      'Accept': 'application/pdf'
                  }),
                  responseType: 'blob'
              });
  }
}
