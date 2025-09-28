import { Component, HostListener, inject, ViewChild } from '@angular/core';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, RowSelectionOptions, GridApi, GridReadyEvent } from 'ag-grid-community';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Item } from '../contracts/item.model';
import { InventoryService } from '../common/InventoryService/InventoryService';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { CommonModule } from '@angular/common';
import { TranslateDirective, TranslatePipe } from "@ngx-translate/core";
import { PaginatedTableComponent, TableCol } from "../common/paginated-table/paginated-table.component";
import { History } from '../models/hisotry';
import { DashboardComponent } from "../dashboard/dashboard.component";
@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatProgressBarModule,
    MatDividerModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatTabsModule,
    MatCardModule, FormsModule,
    TranslateDirective, TranslatePipe,
    PaginatedTableComponent,
    DashboardComponent
],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.scss'
})
export class InventoryComponent {
  @ViewChild(PaginatedTableComponent) table!: PaginatedTableComponent;
  
  scanMode = false;
  scannedItems: Item[] =[];
scanToUpdateInventory() {
  // Implement scanning logic here
  this.scanMode = true;
  const input = document.createElement('input');
  input.id = 'barcodeInput';
  input.type = 'text';
  input.style.position = 'fixed';
  input.style.top = '0';
  input.style.display = 'hidden';
  input.style.width = '1px';
  document.body.appendChild(input);
  input.focus();
  //add overlay to the body
  const overlay = document.createElement('div');
  overlay.id = 'barcode-overlay';
  overlay.style.position = 'fixed';
  overlay.style.top = '0';
  overlay.style.left = '0';
  overlay.style.width = '100%';
  overlay.style.height = '100%';
  overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.5)';
  overlay.style.zIndex = '1000';
  document.body.appendChild(overlay);
  document.getElementById('scanmodal')?.classList.add('show');
  document.getElementById('scanmodal')?.setAttribute('style', 'display: block;');
  input.focus();
  // Listen for Enter key to capture barcode input

  input.addEventListener('keydown', (event) => {
    if (event.key === 'Enter') {
      let barcode = input.value.trim();
      let scannedItem = this.scannedItems?.find(x => x.barcode === barcode) 
      if(scannedItem) {
        scannedItem.quantity += 1;
      }
      else {
        let itemsScanned  = this.items.find(x => x.barcode === barcode);
        if (!itemsScanned) {
          this._snackBar.open('Item not found in inventory', 'Close', { duration: 3000 });
          return;
        }
        let newItem =new Item(itemsScanned.name,
          itemsScanned?.category,
          itemsScanned.car,1, itemsScanned.description, itemsScanned.price, itemsScanned.id
        )
        newItem.barcode = barcode ;
        this.scannedItems.push(newItem);
      }
      input.value = ''; // Clear input after scanning
      input.focus(); // Refocus input for next scan
    
    }});
}
exitScanner() {
  const input = document.querySelector('input#barcode-input') as HTMLInputElement;
  if (input) {
    document.body.removeChild(input);
  }
  const overlay = document.getElementById('barcode-overlay');
  if (overlay) {
    document.body.removeChild(overlay);
  }
  const modal = document.getElementById('scanmodal');
  if (modal) {
    modal.classList.remove('show');
    modal.setAttribute('style', 'display: none;');
  }
  this.scannedItems = [];
  this.scanMode = false;
}
@HostListener('document:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      this.exitScanner();
    }
  }
uploadInventory() {
//show file upload dialog
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = '.csv';
  input.onchange = (event: any) => {  
    const file = event.target.files[0];
    if (!file) {
      this._snackBar.open('No file selected', 'Close', { duration: 3000 });
      return;
    }
    if(file.type !== 'text/csv') {
      this._snackBar.open('Please upload a valid CSV file', 'Close', { duration: 3000 });
      return;
    }
    if (file) {
      this.inventoryService.uploadInventory(file).subscribe((resp: any) => {
        this._snackBar.open(`Inventory updated - Added - ${resp.Added} Updated - ${resp.Updated}`, 'Close', { duration: 3000 });
        this.getItems(); // Refresh the inventory after upload
      }, (error) => {
        this._snackBar.open('Error uploading file', 'Close', { duration: 3000 });
        console.error('Error uploading file:', error) ;
    });
    }
  };
  input.click();
}

downloadInventory() {
  this.inventoryService.downloadInventory().subscribe((response) => {
    const blob = new Blob([response], { type: 'application/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'inventory.csv';
    a.click();
    window.URL.revokeObjectURL(url);
  });
}
  private _snackBar = inject(MatSnackBar);

  showFormLoading = false; 
  loading = false;
  defaultColDef = {
    sortable: true,
    filter: true,
    resizable: true
  };
  
  // Column Definitions: Defines the columns to be displayed.
  colDefs: TableCol[] = [
    
    { 
      key: "name",
      name: "Name",
      width: 150
    },
    { 
      key: "car",
      name: "Car Model",
      width: 120
    },
    { 
      key: "price",
      name: "Price",

      width: 100,

    },
    { 
      key: "quantity",
      name: "Quantity",

      width: 100
    }
  ];
  rowSelection: RowSelectionOptions | "single" | "multiple" = {
    mode: "singleRow",
  };
  
  itemForm!: FormGroup;
  items: Item[] = [];
  showGrid = this.items.length > 0;
  constructor(private fb: FormBuilder, private inventoryService:InventoryService) {}
  uniqueCarCount = 0;
  commanCount=0;
  selectedTabIndex = 0;
  totalInventoryValue = 0;
  inventorybelowThreshold = 0;
  itembelowThreshold: Item[] = [];
  listenToAddInventory() {
    window.addEventListener('add-inventory', (event: any) => {
      this.getItems();
    });
  }
  
  ngOnInit(): void {

   this.getItems();
    this.itemForm = this.fb.group({
      Name: ['', Validators.required],
      Quantity: ['', [Validators.required, Validators.min(1)]],
      Car: [''],
      Category: [''],
      Type: [''],
      Description: [''],
      Price: ['0', Validators.required],
      Barcode: [''] // Add barcode field to the form
    });
    this.listenToAddInventory();
  }
  selectedRowId :number=0;
  onRowSelected(id:any){
    let node= this.items[id];
    if(id==-1){
      this.selectedRowId=0; 
      this.itemForm.reset();
      this.table.clearSelection()
      return;
    }
    this.selectedRowId = node.id;
    let itemselected = this.items.find(x=>x.id == this.selectedRowId);
    if(itemselected!==undefined){
      this.itemForm.controls['Car'].setValue(itemselected.car) 
      this.itemForm.controls['Name'].setValue(itemselected.name) 
      this.itemForm.controls['Quantity'].setValue(itemselected.quantity) 
      this.itemForm.controls['Description'].setValue(itemselected.car) 
      this.itemForm.controls['Price'].setValue(itemselected.price??0) 
      this.itemForm.controls['Barcode'].setValue(itemselected.barcode ?? ''); // Set barcode if available
      this.selectedTabIndex =1;
    }else{
      this.selectedRowId=0;
    }
  }

  resetForm(): void {
    this.selectedRowId = 0;
    this.itemForm.reset();
    this.table.clearSelection();
  }

  // Update getItems to handle loading state
  getItems(): void {
    this.loading = true;
    this.showGrid = false;
    this.inventoryService.getInventory().subscribe({
      next: (items) => {
        this.items = items;
        this.showGrid = true;
        this.SetStats();
      },
      error: (error: Error) => {
        this._snackBar.open('Error loading items', 'Close', { duration: 3000 });
        console.error('Error loading items:', error);
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.itemForm.valid) {
      this.showFormLoading = true;
      const itemData = this.itemForm.value as Item;
      
      const action = this.selectedRowId === 0 ? 
        this.inventoryService.addToInventory(itemData) :
        this.inventoryService.updateInventory({ ...itemData, id: this.selectedRowId });

      action.subscribe({
        next: () => {
          this._snackBar.open(
            this.selectedRowId === 0 ? 'Item added successfully' : 'Item updated successfully',
            'Close',
            { duration: 3000 }
          );
          this.resetForm();
          this.getItems();
        },
        error: (error: Error) => {
          this._snackBar.open('Error saving item', 'Close', { duration: 3000 });
          console.error('Error saving item:', error);
        },
        complete: () => {
          this.showFormLoading = false;
        }
      });
    }
  }

  openSnackBar(message: string) {
    this._snackBar.open(message, 'Close', {duration: 4000});
  }

  onTabChange(index: number) {
    this.selectedTabIndex = index;
    // Reset form and selection when switching away from add/edit tab
    if (index !== 1 && this.selectedRowId !== 0) {
      this.resetForm();
    }
    if(this.selectedTabIndex==2){
      this.getHistory()
    }
  }

  SetStats() {
    this.uniqueCarCount = new Set(this.items.filter(item=>item.car!='').map(item => item.car)).size;  
    this.commanCount = this.items.length -this.uniqueCarCount;
    this.totalInventoryValue = this.items.reduce((total, item) => total + (item.price || 0) * item.quantity, 0);
    this.inventorybelowThreshold = this.items.filter(item => item.quantity < 2).length;
    this.itembelowThreshold = this.items.filter(item => item.quantity < 2);
  }
  historyList : History[] = [];
  historyColDef : TableCol[] = [{
    name:"Id",
    key:'id',
    width:50
  },
  {
    name: "Name",
    key:'name',
    width:100
  },
   {
    name: "Qty",
    key:'quantityUpdated',
    width:100
  },
  {
    name: "Action From",
    key:'type',
    width:100
  },
  {
    name: "Bill",
    key:'billId',
    width:100
  },
  {
    name: "Date",
    key:'date',
    width:100,
    isDate :true
  },
  {
    name: "User",
    key:'user',
    width:100
  }]
  getHistory(){
    let newHistory:History[]=[];
    let billItemsHistory:any[] =[]
    this.inventoryService.getHistory().subscribe((res)=>{
      this.historyList = [...res]
    })
  }
}


