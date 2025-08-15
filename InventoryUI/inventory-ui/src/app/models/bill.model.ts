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

export interface GetBillModel {
  id?: number;
  name: string;
  mobile?: string;
  discount?: number;
  advance?: number;
  paymentMode: number;
  billDate?: Date;
  paymentUser?: string;
}

export interface GetAllBillModel {
  id?: number;
  name: string;
  mobile?: string;
  discount?: number;
  advance?: number;
  paymentMode: number;
  billDate?: Date;
  paymentUser?: string;
  totalAmount?: number;
}