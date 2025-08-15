export interface VendorModel {
  id?: number;
  name: string;
  mobile: string;
  createdDate?: Date;
  updatedDate?: Date;
}

export interface VendorAccountModel {
  id?: number;
  supplierId?: number;
  description: string;
  amount: number;
  expenseType: 'CREDIT' | 'DEBIT';
  paymentMode?: 'CASH' | 'CARD' | 'UPI';
  date: Date; // Optional date field for transactions
}
