import { Component, HostListener, inject } from '@angular/core';
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

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [
    CommonModule,
    AgGridAngular,
    ReactiveFormsModule,
    MatProgressBarModule,
    MatDividerModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatTabsModule,
    MatCardModule,FormsModule
  ],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.scss'
})
export class InventoryComponent {
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
  private gridApi!: GridApi;
  private _snackBar = inject(MatSnackBar);

  showFormLoading = false; 
  loading = false;
  defaultColDef = {
    sortable: true,
    filter: true,
    resizable: true
  };
  
  // Column Definitions: Defines the columns to be displayed.
  colDefs: ColDef[] = [
    { 
      field: "updatedDate",
      headerName: "Last Updated",
      flex: 1,
      minWidth: 150,
      valueFormatter: (params) => {
        return params.value ? new Date(params.value).toLocaleDateString() : '';
      },
      sortable: true,
      sort: 'desc' as const,
      sortIndex: 0
    },
    { 
      field: "name",
      headerName: "Name",
      flex: 2,
      minWidth: 150
    },
    { 
      field: "car",
      headerName: "Car Model",
      flex: 1,
      minWidth: 120
    },
    { 
      field: "price",
      headerName: "Price",
      type: 'numericColumn',
      flex: 1,
      minWidth: 100,
      valueFormatter: (params) => {
        return params.value ? '₹' + params.value.toLocaleString() : '';
      }
    },
    { 
      field: "quantity",
      headerName: "Quantity",
      type: 'numericColumn',
      flex: 1,
      minWidth: 100
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
  onFilterTextBoxChanged() {
    this.gridApi.setGridOption(
      "quickFilterText",
      (document.getElementById("filter-text-box") as HTMLInputElement).value,
    );
  }
    onGridReady(params: GridReadyEvent) {
      this.gridApi = params.api;
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
  onRowSelected(event:any){
    let nodes= this.gridApi.getSelectedRows();
    if(nodes.length==0){
      this.selectedRowId=0; 
      this.itemForm.reset();
      return;
    }
    this.selectedRowId = nodes[0].id;
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
    this.gridApi?.deselectAll();
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
  }

  SetStats() {
    this.uniqueCarCount = new Set(this.items.filter(item=>item.car!='').map(item => item.car)).size;  
    this.commanCount = this.items.length -this.uniqueCarCount;
    this.totalInventoryValue = this.items.reduce((total, item) => total + (item.price || 0) * item.quantity, 0);
    this.inventorybelowThreshold = this.items.filter(item => item.quantity < 2).length;
    this.itembelowThreshold = this.items.filter(item => item.quantity < 2);
  }
}


