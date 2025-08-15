import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { VendorModel, VendorAccountModel } from '../contracts/vendor.model';

@Injectable({
  providedIn: 'root'
})
export class VendorService {
  private baseUrl = '/api/supplier';

  constructor(private http: HttpClient) { }

  // Get all vendors
  getAllVendors(): Observable<VendorModel[]> {
    return this.http.get<VendorModel[]>(this.baseUrl);
  }

  // Get vendor by ID
  getVendorById(id: number): Observable<VendorModel> {
    return this.http.get<VendorModel>(`${this.baseUrl}/${id}`);
  }

  // Get vendor account/expenses
  getVendorAccount(id: number): Observable<VendorAccountModel[]> {
    return this.http.get<VendorAccountModel[]>(`${this.baseUrl}/${id}`);
  }

  // Add new vendor
  addVendor(vendor: VendorModel): Observable<VendorModel> {
    return this.http.post<VendorModel>(this.baseUrl, vendor);
  }

  // Update vendor
  updateVendor(id: number, vendor: VendorModel): Observable<VendorModel> {
    return this.http.put<VendorModel>(`${this.baseUrl}/${id}`, vendor);
  }

  // Delete vendor
  deleteVendor(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Add vendor expense/transaction
  addVendorExpense(data: VendorAccountModel): Observable<VendorAccountModel> {
    return this.http.post<VendorAccountModel>(`api/expense`, data);
  }
}
