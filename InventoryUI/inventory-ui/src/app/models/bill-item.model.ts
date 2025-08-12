export interface BillItemModel {
  itemId?: number; // Optional, used when updating an existing item
  quantity: number; // Quantity of the item
  amount?: number; // Total amount for the item (calculated as quantity * unitPrice)
  itemName?: string; // Name of the item
  itemTotal?: number; // Price per unit of the item
}
