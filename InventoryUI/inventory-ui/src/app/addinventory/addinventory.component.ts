import { CommonModule, NgIf } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Item } from '../contracts/item.model';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { InventoryService } from '../common/InventoryService/InventoryService';

@Component({
  selector: 'app-addinventory',
  imports: [MatIconModule, FormsModule, ReactiveFormsModule, NgIf, CommonModule,
    MatProgressBarModule,
  ],
  templateUrl: './addinventory.component.html',
  styleUrl: './addinventory.component.scss'
})
export class AddinventoryComponent {
  @Input() selectedItem: Item | null  = null;

  showFormLoading = false;

  itemForm!: FormGroup;
  constructor(private fb: FormBuilder, private inventoryService:InventoryService) {}
  private _snackBar = inject(MatSnackBar);

  ngOnInit() {
    if (this.selectedItem) {
      this.itemForm.controls['Car'].setValue(this.selectedItem.car) 
      this.itemForm.controls['Name'].setValue(this.selectedItem.name) 
      this.itemForm.controls['Quantity'].setValue(this.selectedItem.quantity) 
      this.itemForm.controls['Description'].setValue(this.selectedItem.description) 
      this.itemForm.controls['Price'].setValue(this.selectedItem.price??0) 
    }else {
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
  }

   onSubmit(): void {
    if (this.itemForm.valid) {
      this.showFormLoading = true;
      const itemData = this.itemForm.value as Item;
      
      const action = this.selectedItem?.id == null? 
        this.inventoryService.addToInventory(itemData) :
        this.inventoryService.updateInventory({ ...itemData, id: this.selectedItem?.id! });

      action.subscribe({
        next: () => {
          this._snackBar.open(
            this.selectedItem?.id === null ? 'Item added successfully' : 'Item updated successfully',
            'Close',
            { duration: 3000 }
          );
          this.itemForm.reset();
        },
        error: (error: Error) => {
          this._snackBar.open('Error saving item', 'Close', { duration: 3000 });
          console.error('Error saving item:', error);
        },
        complete: () => {
          this.showFormLoading = false;
          window.dispatchEvent(new CustomEvent('add-inventory'));
        }
      });
    }
  }
}
