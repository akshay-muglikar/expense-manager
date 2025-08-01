import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BillItemFormComponent } from './bill-item-form.component';

describe('BillItemFormComponent', () => {
  let component: BillItemFormComponent;
  let fixture: ComponentFixture<BillItemFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BillItemFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BillItemFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
