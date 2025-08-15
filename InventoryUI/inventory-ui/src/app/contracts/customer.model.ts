export interface CustomerModel {
  name: string;
  mobile: string;
}

export interface CustomerAccountModel {
  customerId: number;
  customerName: string;
  bills: CustomerBillModel[];
}

export interface CustomerBillModel {
  id: number;
  billNumber: string;
  billDate: Date;
  totalAmount: number;
  status: string;
}

export interface BillItemModel {
  id: number;
  itemName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}
