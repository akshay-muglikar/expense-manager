import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { NgIf } from '@angular/common';
import { AuthService } from '../common/AuthService';

@Component({
  selector: 'app-book-demo',
  templateUrl: './book-demo.component.html',
  styleUrls: ['./book-demo.component.scss'],
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatIconModule, NgIf
  ]
})
export class BookDemoComponent implements OnInit {
  demoForm: FormGroup;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private authService: AuthService // Assuming you have a login service to handle authentication
  ) {
    this.demoForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      phone: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      email: ['', [Validators.email]],
      city: [''],
      message: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  ngOnInit(): void {}

  onSubmit() {
    if (this.demoForm.valid) {
      this.isSubmitting = true;
      this.authService.bookDemo(this.demoForm.value).subscribe(()=>{
      // Simulate API call
      setTimeout(() => {
        this.snackBar.open('Thank you! We will contact you shortly.', 'Close', {
          duration: 5000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom'
        });
        this.demoForm.reset();
        this.isSubmitting = false;
      }, 1500);
    
    });
} else {
      this.markFormGroupTouched(this.demoForm);
    }
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  getErrorMessage(controlName: string): string {
    const control = this.demoForm.get(controlName);
    if (!control) return '';

    if (control.hasError('required')) {
      return 'This field is required';
    }
    if (control.hasError('email')) {
      return 'Please enter a valid email address';
    }
    if (control.hasError('pattern')) {
      return 'Please enter a valid 10-digit mobile number';
    }
    if (control.hasError('minlength')) {
      if (controlName === 'name') {
        return 'Name must be at least 3 characters long';
      }
      if (controlName === 'message') {
        return 'Message must be at least 10 characters long';
      }
    }
    return '';
  }


}
