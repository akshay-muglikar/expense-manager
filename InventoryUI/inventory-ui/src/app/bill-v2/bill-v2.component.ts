import { Component, inject } from '@angular/core';
import { BillModel } from '../models/bill.model';
import { BillService } from '../services/bill.service';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BillItemModel } from '../models/bill-item.model';
import { CommonModule, formatDate } from '@angular/common';
import { InventoryService } from '../common/InventoryService/InventoryService';
import { Item } from '../contracts/item.model';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { map, Observable, finalize } from 'rxjs';
import { AddBillModel } from '../contracts/bill.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BillhistoryComponent } from '../billhistory/billhistory.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { AddinventoryComponent } from "../addinventory/addinventory.component";

@Component({
  selector: 'app-bill-v2',
  imports: [
    FormsModule,
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatAutocompleteModule,
    MatProgressBarModule,
    NgSelectModule,
    ReactiveFormsModule,
    BillhistoryComponent,
    AddinventoryComponent
],
  templateUrl: './bill-v2.component.html',
  styleUrl: './bill-v2.component.scss'
})
export class BillV2Component {


  loading = false;
  bill: BillModel = {
    name: '',
    mobile: '',
    paymentMode: 'UPI',
    billDate: new Date(),
  };
  billItems: BillItemModel[] = [];
  items: Item[] = [];
  total: number = 0;
  filteredOptions: Observable<Item[]>;
  myControl = new FormControl('');
  activeTab = 'bill';
  selectedItem: BillItemModel = {
    quantity: 1,

  };
  private _snackBar = inject(MatSnackBar);
  displayFn(shop: number): string {
    if (!shop) return '';
    let item = this.items.find(i => i.id === shop);
    if (!item) return '';
    return item?.name + ' - ' + (item?.car || '');
  }
  addIteminBill() {
    this.billItems.push({
      quantity: 1,
      amount: 0,
      itemName: ''
    });
  }
  //listen to print event
  listenToPrint() {
    window.addEventListener('print-bill', (event: any) => {
      const billId = event.detail.billId;
      this.print(billId);
    });
  }
  listenToAddInventory() {
    window.addEventListener('add-inventory', (event: any) => {
      this.getItems();
      //close modal if open
      const modal = document.querySelector('.modal');
      if (modal) {
        modal.classList.remove('show');
        modal.setAttribute('aria-hidden', 'true');
        (modal as HTMLElement).style.display = 'none';
      }
      const backdrop = document.querySelector('.modal-backdrop');
      if (backdrop) {
        backdrop.remove();
      }
    });
  }
  listenToEdit() {
    window.addEventListener('edit-bill', (event: any) => {
      const billId = event.detail.billId;
       this.billService.getBillById(billId).subscribe((bill: any) => {
        this.bill = bill;
        this.billItems = bill.billItems || [];
        this.activeTab = 'bill';
     });
    });
  }
  sendToWhatsapp() {
    if (!this.bill.id) {
      this.openSnackBar('Save the bill before sending to WhatsApp');
      return;
    }
    this.calculateTotal();
    //send download link to whatsapp
    const message = `Hi ${this.bill.name},
Thank you for your purchase! Here are the details of your bill:
    
Invoice ID: ${this.bill.id}
Bill Date: ${formatDate(this.bill.billDate ?? new Date(), 'dd/MM/yyyy', 'en-US')}

Total Item(s): ${this.billItems.length}
    
Total Amount: ₹${this.total.toFixed(2)}
Payment Mode: ${this.bill.paymentMode}

If you have any questions, feel free to contact us.`;
    // Send the message to WhatsApp
    const whatsappUrl = `https://api.whatsapp.com/send?phone=91${this.bill.mobile}&text=${encodeURIComponent(message)}`;
    window.open(whatsappUrl, '_blank');
  }
  constructor(private billService: BillService, private itemService: InventoryService) {
    this.filteredOptions = this.myControl.valueChanges.pipe(
      map(value => this._filter(value || '')),
    );
  }
  private _filter(value: string): Item[] {
    if (!value) {
      return this.items;
    }
    const filterValue = value.toLowerCase();

    return this.items.filter(option => option.name.toLowerCase().includes(filterValue));
  }

  ngOnInit() {
    this.getItems();
    this.addIteminBill();
     this.listenToPrint();
    this.listenToEdit();
    this.listenToAddInventory();
  }

  addBill() {
    this.billService.addBill(this.bill).subscribe(result => {
      this.bill = result;
    });
  }

  updateBill() {
    if (!this.bill.id) return;
    this.billService.updateBill(this.bill.id, this.bill).subscribe();
  }
  openItemForm() {
  }
  getItems() {
    this.loading = true;
    this.itemService.getInventory().pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: (items) => {
        this.items = items;
      },
      error: (error) => {
        this._snackBar.open('Error loading items', 'Close', { duration: 3000 });
      }
    });
  }
  addItem(item: BillItemModel) {
    if (!this.bill.id) return;
    this.billService.addBillItem(this.bill.id, item).subscribe(() => {
      this.billItems.push(item);
      this.billItems = [...this.billItems]; // Trigger change detection
      this.calculateTotal();
    });
  }
  calculateTotal() {
    this.total = this.billItems.reduce((sum, item) => sum + (item.quantity * item.amount!), 0);
    if (this.bill.discount) {
      this.total -= this.bill.discount;
    }
    if (this.bill.advance) {
      this.total -= this.bill.advance;
    }
    return this.total;
  }
  removeItem(index: number) {
    this.billItems.splice(index, 1);
  }


  addSelectedItem() {
    if (this.selectedItem) {
      let item = this.items.find(item => item.id == this.selectedItem.itemId)
      const newItem: BillItemModel = {
        itemId: this.selectedItem.itemId,
        quantity: this.selectedItem.quantity, // Default quantity, can be adjusted
        amount: this.selectedItem.amount, // Assuming Item has a unitPrice property
        itemName: item?.name + '-' + item?.car, // Assuming Item has a name property
      };
      this.addItem(newItem);
      this.selectedItem = { quantity: 1 };
    }
  }
  updateBillWithItems() {
    if (!this.bill.id) {
      this.openSnackBar('Save the bill before updating');
      return;
    }
    let addBill: AddBillModel = {
      name: this.bill.name,
      mobile: this.bill.mobile,
      paymentMode: this.bill.paymentMode,
      billDate: this.bill.billDate,
      BillItems: this.billItems,
      discount: this.bill.discount,
      advance: this.bill.advance,
    };
    this.loading = true;
    this.billService.updateBillWithItems(this.bill.id.toString(), addBill).pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: () => {
        this._snackBar.open('Bill updated successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        this._snackBar.open('Error updating bill', 'Close', { duration: 3000 });
      }
    });
  }
  saveBillWithItems() {
    this.loading = true;
    let addBill: AddBillModel = {
      name: this.bill.name,
      mobile: this.bill.mobile,
      paymentMode: this.bill.paymentMode,
      billDate: this.bill.billDate,
      BillItems: this.billItems,
      discount: this.bill.discount,
      advance: this.bill.advance,
    };
    this.billService.addBillWithItems(addBill).pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: (result) => {
        this.bill = result;
        this._snackBar.open('Bill saved successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        this._snackBar.open('Error saving bill', 'Close', { duration: 3000 });
      }
    });
  }
  openSnackBar(message: string) {
    this._snackBar.open(message, 'Close', { duration: 4000 });
  }
  print(id:any){
    this.billService.download(id.toString()).subscribe((resp: any) => {

      const fileURL = URL.createObjectURL(resp);

      const printWindow = window.open(fileURL, '_blank');

      // Optional: Trigger print dialog when file is loaded
      if (printWindow) {
        printWindow.onload = () => {
          printWindow.focus();
          printWindow.print();
        };
      }
    });
  }
  printBill() {
    if (!this.bill.id) {
      this.openSnackBar('Save the bill before printing');
      return;
    }
    this.print(this.bill.id);
   
  }
  Close() {
    this.bill = {
      name: '',
      mobile: '',
      paymentMode: 'UPI',
      billDate: new Date(),
    };
    this.billItems = [];
    this.total = 0;
    this.selectedItem = { quantity: 1 };
    this.myControl.setValue('');
    this.addIteminBill();
    this.openSnackBar('Bill closed and reset');
  }
}
