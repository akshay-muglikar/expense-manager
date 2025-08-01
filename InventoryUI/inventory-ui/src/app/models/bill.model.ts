import { BillItemModel } from "./bill-item.model";

export interface BillModel {
  id?: number;
  name: string;
  mobile?: string;
  discount?: number;
  advance?: number;
  paymentMode?: string;
  billDate?: Date;
}
