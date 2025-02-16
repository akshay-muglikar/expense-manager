
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Expense } from "../expense/expense.component";

@Injectable({providedIn: 'root'})
export class ExpenseService {
  constructor(private http: HttpClient) {
  }

    getExpenses() {
        return this.http.get<Expense[]>('/api/expense');
    }
    getExpenseById(id:string) {
        return this.http.get<Expense>('/api/expense/'+id);
    }

    addExpense(expense: Expense) {
        return this.http.post<Expense>('/api/expense', expense);
    }
    updateExpense(expense: Expense) {
        return this.http.put<Expense>('/api/expense/'+expense.id, expense);
    }
    getExpensesbyDate(start:string, end: string) {
        return this.http.get<Expense[]>('/api/expense?start='+start+'&end='+end);
    }
}