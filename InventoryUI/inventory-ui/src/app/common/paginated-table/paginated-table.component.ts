import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { FormsModule } from "@angular/forms";
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-paginated-table',
  imports: [MatPaginatorModule,MatIconModule, NgIf, NgFor, FormsModule, CommonModule],
  templateUrl: './paginated-table.component.html',
  styleUrl: './paginated-table.component.scss'
})
export class PaginatedTableComponent implements OnChanges {
rowAction(index: number,action: TableAction) {
   const actualIndex = this.currentPage * this.pageSize + index;
   this.action.emit({index:actualIndex, action : action.action});
}
  @Input() data: any[] = [];
  @Input() colDefs: any[] = [];
  @Input() rowSelection: boolean = false;
  @Input() rowActions? : TableAction[]
  
  dataToDisplay: any[] = [];
  searchText: string = '';
  pageSize: number = 10;
  currentPage: number = 0;
  total: number = 0;
  
  @Output() rowSelectionChange = new EventEmitter<number>();
  @Output() action = new EventEmitter<any>();

  constructor(private cdr: ChangeDetectorRef) {}

  // This is the key fix - implement OnChanges to detect when data input changes
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && changes['data'].currentValue) {
      this.updateDisplayData();
    }
  }

  // Update the display data whenever data changes
  private updateDisplayData(): void {
    if (!this.data) {
      this.dataToDisplay = [];
      this.total = 0;
      return;
    }

    // Apply search filter if searchText exists
    let filteredData = this.data;
    if (this.searchText && this.searchText.trim()) {
      filteredData = this.data.filter(row => 
        this.colDefs.some(col => {
          const value = row[col.key ?? col.name];
          return value && value.toString().toLowerCase().includes(this.searchText.toLowerCase());
        })
      );
    }

    this.total = filteredData.length;
    
    // Apply pagination
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.dataToDisplay = filteredData.slice(startIndex, endIndex);
    
    // Manually trigger change detection
    this.cdr.detectChanges();
  }

  search(): void {
    this.currentPage = 0; // Reset to first page when searching
    this.updateDisplayData();
  }

  pageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updateDisplayData();
  }

  onRowSelection(index: number, event: any): void {
    // Handle row selection logic
    const target = event.target as HTMLInputElement;
    let isChecked = target.checked; 
    if(isChecked){
      const checkboxes = document.querySelectorAll<HTMLInputElement>('.checkbox');
      checkboxes.forEach(cb => {
        if(cb!=target)
          cb.checked = false
      });
      const actualIndex = this.currentPage * this.pageSize + index;
      // Emit selection change
      this.rowSelectionChange.emit(actualIndex);
    }
  }
  clearSelection(){
    const checkboxes = document.querySelectorAll<HTMLInputElement>('.checkbox');
      checkboxes.forEach(cb => {
          cb.checked = false
      });
  }
}
export interface TableCol{
  name :string,
  width:number,
  key? : string|null
  currency?:string|null,
  actions?:string[]|null
  isDate?:boolean
}
export interface TableAction {
  name:string,
  action: string
  icon?:string
}