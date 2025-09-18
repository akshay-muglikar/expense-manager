import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BillModel } from '../models/bill.model';
import { BillItemModel } from '../models/bill-item.model';
import { Observable } from 'rxjs';
import { AddBillModel } from '../contracts/bill.model';
import { SalesSummary } from '../models/sales-summary';
@Injectable({
  providedIn: 'root'
})
export class BillService {
  getBillsbyDate(formatedDate: string, formatedStartDate: string) {
    return this.http.get<any[]>(`/api/bill?start=${formatedDate}&end=${formatedStartDate}`);
  }
  updateBillWithItems(id: string | undefined, illWithItems: any) {
    return this.http.post<BillModel>('/api/bill/update-with-items/'+id, illWithItems);
  }
  private apiUrl = 'api/bill'; // Update this

  constructor(private http: HttpClient) {}

  addBill(bill: BillModel): Observable<BillModel> {
    return this.http.post<BillModel>(this.apiUrl, bill);
  }

  updateBill(id: number, bill: BillModel): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, bill);
  }

  addBillItem(billId: number, item: BillItemModel): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${billId}/add`, item);
  }

  removeBillItem(billId: number, billItemId: number): Observable<void> {
    const params = new HttpParams().set('billItemId', billItemId.toString());
    return this.http.put<void>(`${this.apiUrl}/${billId}/remove`, {}, { params });
  }
  addBillWithItems(bill: AddBillModel) {
      return this.http.post<AddBillModel>('/api/bill/add-with-items', bill);
  }
  download(id:string) {
          const httpOptions = {
              headers: new HttpHeaders(
                {
                  'Accept': 'application/json',
                  'responseType':'blob'
                },
              )
            };
          return this.http.get('/api/bill/'+id+'/download', {
              headers: new HttpHeaders({
                  'Accept': 'application/pdf'
              }),
              responseType: 'blob' as 'json'
          });
      }
  getAllBills() {
    return this.http.get<any[]>('/api/bill');
  }
  getBillById(billId: number) {
    return this.http.get<any>(`/api/bill/${billId}`);
  }
  summary(formatedDate: string, formatedStartDate: string){
    return this.http.get<SalesSummary>(`/api/analytics/sales-summary?start=${formatedDate}&end=${formatedStartDate}`)
  }
}
