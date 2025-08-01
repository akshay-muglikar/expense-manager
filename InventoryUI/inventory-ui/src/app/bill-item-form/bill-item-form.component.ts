import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BillItemModel } from '../models/bill-item.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-bill-item-form',
  imports: [FormsModule, CommonModule],
  templateUrl: './bill-item-form.component.html',
  styleUrl: './bill-item-form.component.scss'
})
export class BillItemFormComponent {
  @Output() itemAdded = new EventEmitter<BillItemModel>();

  item: BillItemModel = {
    quantity: 1,
  };

  submit() {
    if (this.item.itemId && this.item.quantity > 0 && this.item.amount! >= 0) {
      this.itemAdded.emit({ ...this.item });
      this.item = { quantity: 1, amount: 0 };
    }
  }
}
