import { Component, inject } from '@angular/core';
import { AgGridAngular } from 'ag-grid-angular'; // Angular Data Grid Component
import type { ColDef, RowSelectionOptions } from 'ag-grid-community'; // Column Definition Type Interface
import {
  GridApi,GridReadyEvent
} from "ag-grid-community";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Item } from '../contracts/item.model';
import {InventoryService} from '../common/InventoryService/InventoryService';
import {MatProgressBarModule} from '@angular/material/progress-bar';
import {MatSnackBar} from '@angular/material/snack-bar';
import {MatDividerModule} from '@angular/material/divider';

@Component({
  selector: 'app-inventory',
  imports: [AgGridAngular, ReactiveFormsModule, MatProgressBarModule,MatDividerModule],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.scss'
})
export class InventoryComponent {
  private gridApi!: GridApi;
  private _snackBar = inject(MatSnackBar);

  showFormLoading = true; 
  // Column Definitions: Defines the columns to be displayed.
  colDefs: ColDef[] = [
      { field: "name", width: 150 },
      { field: "car",width: 120 },
      { field: "price", width: 80 },
      { field: "quantity", width: 70 },
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
    });
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
      this.itemForm.controls['Category'].setValue(itemselected.car) 
      this.itemForm.controls['Type'].setValue(itemselected.car) 
      this.itemForm.controls['Description'].setValue(itemselected.car) 
      this.itemForm.controls['Price'].setValue(itemselected.price??0) 

    }else{
      this.selectedRowId=0;
    }
  }

  onSubmit(): void {
    if (this.itemForm.valid) {
      this.showFormLoading = false;
      const itemData = this.itemForm.value as Item;
      if(this.selectedRowId==0){
        this.inventoryService.addToInventory(itemData).subscribe(() => {
          this.items.push(itemData);
          this.showFormLoading = true;
          this.itemForm.reset();
          this.getItems();
          this.openSnackBar('Item added successfully');
        });
      }
      else{
        itemData.id = this.selectedRowId;
        this.inventoryService.updateInventory(itemData).subscribe(() => {
          this.showFormLoading = true;
          this.itemForm.reset();
          this.getItems();
          this.openSnackBar('Item updated successfully');
        });
      }
    }
  }
  getItems() {
    this.inventoryService.getInventory().subscribe((items) => {
      this.items = items;
      this.SetStats();
    });
  }

  openSnackBar(message: string) {
    this._snackBar.open(message, 'Close', {duration: 4000});
  }

  SetStats() {
    this.uniqueCarCount = new Set(this.items.filter(item=>item.car!='').map(item => item.car)).size;  
    this.commanCount = this.items.length -this.uniqueCarCount ;  

  }
}


