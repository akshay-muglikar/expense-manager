import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustomerModel, CustomerAccountModel } from '../contracts/customer.model';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) {}

  getAllCustomers(): Observable<CustomerModel[]> {
    return this.http.get<CustomerModel[]>(`${this.baseUrl}/customer`);
  }

  getCustomerAccount(name: string, mobile: string): Observable<CustomerAccountModel> {
    return this.http.get<CustomerAccountModel>(`${this.baseUrl}/customer/bills`, {
      params: { name, mobile }
    });
  }

  getCustomerById(customerId: number): Observable<CustomerModel> {
    return this.http.get<CustomerModel>(`${this.baseUrl}/customers/${customerId}`);
  }
}
