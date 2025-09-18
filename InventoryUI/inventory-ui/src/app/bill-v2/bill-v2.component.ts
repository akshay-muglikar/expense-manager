import { Component, HostListener, inject } from '@angular/core';
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
import { MatCard, MatCardModule } from "@angular/material/card";
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateDirective, TranslatePipe } from "@ngx-translate/core";
import { MatProgressSpinner } from "@angular/material/progress-spinner";

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
    AddinventoryComponent,
    MatCardModule,
    TranslateDirective, TranslatePipe,
    MatProgressSpinner
],
  templateUrl: './bill-v2.component.html',
  styleUrl: './bill-v2.component.scss'
})
export class BillV2Component {
  scannedCode = '';
  ScanItem() {
    if (!this.scannedCode) {
      this._snackBar.open('Please enter a valid barcode', 'Close', { duration: 3000 });
      return;
    }
    const item = this.items.find(item => item.barcode === this.scannedCode);
    if (item) {
      this.selectItem(item);
      this.scannedCode = ''; // Clear input after scanning
      // Refocus input for next scan
      const input = document.getElementById('barcodeinput');
      if (input) {
        (input as HTMLInputElement).value = ''; // Clear the input field
        input.focus(); // Refocus for next scan
      }
    } else {
      this._snackBar.open('Item not found for the scanned barcode', 'Close', { duration: 3000 });
    }
  }
updateQty(_t51: number) {
    if (this.billItems[_t51]) {
      this.billItems[_t51].itemTotal = this.billItems[_t51].quantity * (this.billItems[_t51].amount ?? 0);
    }
    this.calculateTotal();
}
selectItemFromScanner($event: KeyboardEvent) {
if($event.key === 'Enter') {
    let barcode = ($event.target as HTMLInputElement).value.trim();
    console.log('Barcode scanned:', barcode);
      const item = this.items.find(item => item.barcode === barcode);
      if (item) {
        console.log('Item found:', item);
        this.selectItem(item);
      }
    }
}

  loading = false;
  bill: BillModel = {
    name: '',
    mobile: '',
    paymentMode: 'UPI',
    billDate: new Date(),
  };
  billItems: BillItemModel[] = [];
  items: Item[] = [];
  searchTerm = '';
  searchedItems: Item[] = [];
  
  filterItems(){
    this.searchedItems = this.items.filter(item => 
      item.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      item.car?.toLowerCase().includes(this.searchTerm.toLowerCase())
    ).splice(0, 10); // Limit to 10 results
  }

  isItemSelected(item: Item): boolean {
    return this.billItems.some(billItem => billItem.itemId === item.id);
  }
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
      this.gotoEditBill(event.detail.billId);
    });
  }
  gotoEditBill(id:number) {
    this._snackBar.open(`Bill# ${id} selected for editing`, 'Close', { duration: 3000 })
     const billId = id;
       this.billService.getBillById(billId).subscribe((bill: any) => {
        this.bill = bill;
        this.billItems = bill.billItems || [];
        this.activeTab = 'bill';
        this.calculateTotal();
     });
  }
  sendToWhatsapp() {
    if (!this.bill.id) {
      this.openSnackBar('Save the bill before sending to WhatsApp');
      return;
    }
    this.loading = true;
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
    this.loading = false;
  }
  constructor(private billService: BillService, private itemService: InventoryService,
    private route: ActivatedRoute, private router: Router
  ) {
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
    //this.addIteminBill();
    this.readParams();
     this.listenToPrint();
    this.listenToEdit();
    this.listenToAddInventory();
  }
  readParams() {
//read router params
this.route.queryParams.subscribe(params => {
    const billId = params['billid'];
    if(billId) {
      this.gotoEditBill(Number(billId));
      //clear the bill id from params
      this.router.navigate([], {
        queryParams: { billid: null },
        queryParamsHandling: 'merge', // remove billid from the URL
      });

      return;
    }
    const customerId = params['customerId'];
    const customerMobile = params['customerMobile'];
    if (customerId && customerMobile) {
      this.bill.name = customerId;
      this.bill.mobile = customerMobile;
      this._snackBar.open(`New bill for ${customerId} - ${customerMobile}`,'Close', { duration: 3000 })
       this.router.navigate([], {
        queryParams: { customerId: null, customerMobile: null },
        queryParamsHandling: 'merge', // remove billid from the URL
      });
    }
  });
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
    ).subscribe((items: Item[]) => {
      this.items = items;
      this.searchedItems = items.slice(0, 10); // Limit to 10 results
    }, (error) => {
        this._snackBar.open('Error loading items', 'Close', { duration: 3000 });
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
    this.total =0;
    this.total = this.billItems.reduce((sum, item) => sum + (item.quantity * item.amount!), 0);

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
      BillItems: this.billItems.filter(item => item.itemId && item.quantity),
      discount: this.bill.discount || 0,
      advance: this.bill.advance || 0,
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
      BillItems: this.billItems.filter(item => item.itemId && item.quantity),
      discount: this.bill.discount|| 0,
      advance: this.bill.advance|| 0,
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
  download() {
    if (!this.bill.id) {
      this.openSnackBar('Save the bill before downloading');
      return;
    }
    this.downloadBill(this.bill.id);
  }
  downloadBill(id:number){

    this.loading =true;

    this.billService.download(id.toString()).subscribe((resp: any) => {
      const fileURL = URL.createObjectURL(resp);
      const a = document.createElement('a');
      a.href = fileURL;
      a.download = 'bill.pdf'; // Specify the file name
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      this.loading= false
    });
  }
  print(id:any){
    this.loading = true;
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
      this.loading = false;
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
  setItemProps(id: number) {
    let billItem = this.items.find(item => item.id === this.billItems[id].itemId);
    this.billItems[id].amount = billItem?.price || 0; // Set amount based on item price
    this.billItems[id].itemName = billItem?.name + ' - ' + (billItem?.car || ''); // Set item name with car info
    this.billItems[id].itemTotal = this.billItems[id].quantity * (billItem?.price ?? 0); // Set item total based on item price and quantity
    this.billItems[id].quantity = 1;
  } 
  selectItem(item: Item) {
    if (this.isItemSelected(item)) {
      // If item is already in the bill, increase its quantity
      const existingItem = this.billItems.find(billItem => billItem.itemId === item.id);
      if (existingItem) {
        existingItem.quantity += 1; // Increase quantity by 1
        existingItem.amount = item.price||0 * existingItem.quantity || 0; // Update amount if needed
      }
     
    } else {
      // Add new item to the bill
      this.billItems.push({
        itemId: item.id,
        itemName: item.name,
        quantity: 1,
        itemTotal: 1 * item.price || 0,
        amount: item.price || 0,
      });
    }
    this.calculateTotal();
  }
}

