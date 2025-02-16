
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Bill } from "../billing/billing.component";

@Injectable({providedIn: 'root'})
export class BillingService {
  constructor(private http: HttpClient) {
  }

    getBills() {
        return this.http.get<Bill[]>('/api/bill');
    }
    getBillById(id:string) {
        return this.http.get<Bill>('/api/bill/'+id);
    }

    addBill(bill: Bill) {
        return this.http.post<Bill>('/api/bill', bill);
    }
    updateBill(bill: Bill) {
        return this.http.put<Bill>('/api/bill/'+bill.id, bill);
    }
    getBillsbyDate(start:string, end: string) {
        return this.http.get<Bill[]>('/api/bill?start='+start+'&end='+end);
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

}