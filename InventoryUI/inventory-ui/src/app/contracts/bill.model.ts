import { BillItemModel } from "./bill.item.model";

export interface AddBillModel {
    id?: number;
  name: string;
  mobile?: string;
  discount?: number;
  advance?: number;
  paymentMode?: string;
  billDate?: Date;
  BillItems?: BillItemModel[];
}