import { Component } from '@angular/core';
import { ClientService } from '../services/client.service';
import { ClientDetails } from '../models/client-Details';
import { FormsModule } from "@angular/forms";
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MatIcon } from "@angular/material/icon";

@Component({
  selector: 'app-config',
  imports: [FormsModule, CommonModule, MatButtonModule],
  templateUrl: './config.component.html',
  styleUrl: './config.component.scss'
})
export class ConfigComponent {
  pdfUrl: SafeResourceUrl | null = null;

  constructor(private clientService:ClientService,
    private snakbar: MatSnackBar,
    private sanitizer: DomSanitizer
  ){}
  ngOnInit(){
    this.getClientDetails();
  }
  clientDetails:ClientDetails = {
    address:'',
    name:'',
    gstNumber:'',
    id:0,
    clientId:'',
    invoiceType:''
  };

  getClientDetails(){
    this.clientService.getClientDetails().subscribe((res)=>{
      this.clientDetails = res
    })
  }

  updateLogo(event: Event){
   const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      let file = input.files[0];
      if(!file.name.endsWith(".png") && !file.name.endsWith(".jpg")){
        this.snakbar.open('Please select jpg or png file', 'Close', {duration:3000})
      }
      const formData = new FormData();
      formData.append('file', file);
      this.clientService.uploadLogo(formData).subscribe(()=>{
        this.snakbar.open('Uploaded', 'Close', {duration:3000})
      })
    }
     
   
  }
  updateClientDetails(){
    this.clientService.updateClientDetails(this.clientDetails!!).subscribe(()=>{
      this.snakbar.open('Changes Saved', 'Close', {duration:3000})
    });
  }
  preview(){
    this.clientService.preview().subscribe((blob:Blob)=>{
       const fileUrl = URL.createObjectURL(blob);
        this.pdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(fileUrl);
    });
  }
}
