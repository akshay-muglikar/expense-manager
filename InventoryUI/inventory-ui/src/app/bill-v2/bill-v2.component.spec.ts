import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BillV2Component } from './bill-v2.component';

describe('BillV2Component', () => {
  let component: BillV2Component;
  let fixture: ComponentFixture<BillV2Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BillV2Component]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BillV2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
