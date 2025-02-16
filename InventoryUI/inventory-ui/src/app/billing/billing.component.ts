import { Component, inject, model, signal } from '@angular/core';
import {FormControl, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatTabsModule} from '@angular/material/tabs';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatButtonModule} from '@angular/material/button';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatSelectModule} from '@angular/material/select';
import { Item } from '../contracts/item.model';
import { InventoryService } from '../common/InventoryService/InventoryService';
import {MatAutocompleteModule} from '@angular/material/autocomplete';
import { NgForOf } from '@angular/common';
import { BillingService } from '../common/BillingService';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { formatDate } from '@angular/common';
import { ColDef, GridApi, GridReadyEvent } from 'ag-grid-community';
import { AgGridAngular } from 'ag-grid-angular';

@Component({
  selector: 'app-billing',
  imports: [ MatFormFieldModule,
    MatInputModule,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatTabsModule,MatIconModule, MatAutocompleteModule, NgForOf],
  templateUrl: './billing.component.html',
  styleUrl: './billing.component.scss'
})
export class BillingComponent {

  private _snackBar = inject(MatSnackBar);
  
  readonly dialog = inject(MatDialog);
  readonly searchBillId = signal('');
  
  selectedBill!: Bill;
  tabs :Tab[]  = [new Tab('New Bill',)];


  openDialog(): void {
    const dialogRef = this.dialog.open(DialogOverviewExampleDialog, {
      data: {billid: ''},
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      console.log(result);
      if (result !== undefined) {
        
        this.getBillById(result);
      }
    });
  }
  getBillById(result: any) {
    this.billService.getBillById(result).subscribe((bill) => {
      console.log(bill);
      this.addTab(bill);
    });
  }
  searchOldBill() {
    
  }
filterOptions(itemname: string) {
  this.filtereditems = this.items.filter(item => item.name.toLowerCase().includes(itemname.toLowerCase())
    || item.car?.toLowerCase().includes(itemname.toLowerCase()));
}
calculateTotal() {
  let bill = this.tabs[this.selectedTab].bill;
  let total = 0;
  bill.billItems.forEach(item => {
  
    total += item.amount * item.quantity;
    
  });
  total = total - (bill.discount || 0) - (bill.Advance || 0);
  return total;
}
addBillItem() {
  let bill = this.tabs[this.selectedTab].bill;
  bill.billItems.push({item:"1", quantity:1, amount:0, mode:"edit"});

  
}
setTabName() {
  this.tabs[this.selectedTab].name =this.tabs[this.selectedTab].bill.name
}


  selectedTab =0;
  constructor(private inventoryService:InventoryService, private billService: BillingService) {
    
  }
  items: Item[] = [];
  filtereditems: Item[] = [];
  getTitle(itemId: number) {
    var item =this.filtereditems.find((item) => item.id == itemId);
    if(item==null){
      return "";
    }
    return item?.name ;
  }
  ngOnInit(): void {
    console.log(this.tabs);
    this.getItems();
    
  }
  getItems() {
    this.inventoryService.getInventory().subscribe((items) => {
      this.items = items;
    });
  }
  addTab(bill?: Bill) {
    if(bill == undefined || bill==null){
      this.tabs.push(new Tab('New Bill'));
      this.selectedTab = this.tabs.length-1;
    }else{
      this.tabs.push(new Tab(bill.name));
      this.tabs[this.tabs.length-1].bill = bill;
      this.selectedTab = this.tabs.length-1;
    }
    this.filtereditems = this.items
  }

  removeTab(index: number) {
    
    this.setSelectedTab(index==0?0:index-1);
    this.deleteFromTab(index)
    if(this.tabs.length==0){
      this.addTab();
    }
    console.log("removing "+ index);
    console.log("setting = " + (index==0?0:index-1))
    
  }
  deleteFromTab(index:number){
   
      this.tabs.splice(index, 1);
   
    
  }

  onSubmit(): void {
    
    let bill = this.tabs[this.selectedTab].bill;
    if(bill.name==null || bill.name.trim()==""){
      this.openSnackBar('Name is mandatory');
      return;
    }
    if(bill.mobile==null || bill.mobile.trim()==""){
      this.openSnackBar('Mobile is mandatory');
      return;
    }
    if(bill.billItems==null ||  bill.billItems.length==0){
      this.openSnackBar('Atleast one item is mandatory');
      return;
    }

    console.log(bill);
    if(bill.id ==undefined || bill.id==null){
      bill.billItems.forEach(item => item.item = item.item.toString());
      this.billService.addBill(bill).subscribe((response: Bill) => {
        this.openSnackBar('Bill added successfully');
        this.tabs[this.selectedTab].billStatus = "Saved";
        this.tabs[this.selectedTab].bill.id = response.id;
      });
    }else{
      this.billService.updateBill(bill).subscribe((response: Bill) => {
        this.openSnackBar('Bill updated successfully');
        
      });
    }

  }
  openSnackBar(message: string) {
    this._snackBar.open(message, 'Close', {duration: 4000});
  }
  setSelectedTab(_t11: number) {
    this.selectedTab = _t11;
  }

  downloadBill(){
    let id =this.tabs[this.selectedTab].bill.id??0;
    if(id==0)
    { 
      this.openSnackBar('Save the bill first!!'); 
      return;
    }
    this.billService.download(id.toString()).subscribe((resp:any)=>{
      console.log(resp);
      const a = document.createElement('a')
      const objectUrl = URL.createObjectURL(resp)
      a.href = objectUrl
      a.download = 'invoice-'+id+'.pdf';
      a.click();
      URL.revokeObjectURL(objectUrl);
  });
  }

}

export class Tab{
  name: string;
  bill: Bill;
  itemFormControl = new FormControl();
  billStatus = "New";
  constructor(name:string){
    this.name = name;
    this.bill = {name:"",mobile:"", billItems:[], Advance:0, billDate : new Date(), status:"New", calculatedBillAmount:0};
  }
}

export interface Bill {
  id?: number;
  name: string;
  mobile: string;
  billItems: BillItem[];
  discount?: number;
  billDate?: Date;
  status:string
  Advance : number
  calculatedBillAmount: number
}

export interface BillItem {
  item: string;
  quantity: number;
  amount: number;
  mode: string;
}



@Component({
  selector: 'dialog-overview-example-dialog',
  templateUrl: 'billingSearchDialog.html',
  imports: [
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatButtonModule,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    MatSelectModule,MatInputModule,
    MatAutocompleteModule,NgForOf
  ],
})
export class DialogOverviewExampleDialog {
  readonly dialogRef = inject(MatDialogRef<DialogOverviewExampleDialog>);
  readonly data = inject<DialogData>(MAT_DIALOG_DATA);
  readonly selectedBill = model(this.data.billid);
  bills: Bill[] = [];
  filtereditems: Bill[] = [];
  constructor(private billingService: BillingService){

  }
  ngOnInit(): void {
    this.getBills();
  }
  getBills() {
    this.billingService.getBills().subscribe((bills) => {
      console.log(bills);
      this.bills = bills;
    });
  }
  onNoClick(): void {
    this.dialogRef.close();
  }
  filterOptions(itemname: string) {
    this.filtereditems = this.bills.filter(item => item.name.toLowerCase().includes(itemname.toLowerCase())
      || item.mobile.toLowerCase().includes(itemname.toLowerCase()));
  }
  getTitle(itemId: number) {
    var item =this.filtereditems.find((item) => item.id == itemId);
    if(item==null){
      return "";
    }
    return item?.name + " - " + item?.mobile;
  }
}
export interface DialogData {
  billid: string;
}