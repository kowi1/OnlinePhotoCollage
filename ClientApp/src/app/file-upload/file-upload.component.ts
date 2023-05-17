import { Component,Inject } from '@angular/core';
import { FormBuilder, FormGroup,FormControl } from "@angular/forms";
import { HttpClient ,HttpHeaders} from '@angular/common/http';
import { of } from "rxjs";
import { delay,concatMap } from "rxjs/operators";



@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html'
})

export class FileUploadComponent  {
  form: FormGroup;
  public baseUrls:string;
  size:number;
  uniqueid: string;
  selectedOption: string; // This variable will store the selected radio button value

  // Define an array of radio button options
  radioOptions = [
    { label: 'Horizontal', value: 'horizontal' },
    { label: 'Vertical', value: 'vertical' }
    
  ];
  

  constructor(
    public fb: FormBuilder,
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.form = this.fb.group({
      border: 1,
      bg_color: [''],
  
      orientation: ['']
    })
    this.baseUrls=baseUrl
  }

  // Function to handle radio button change
  onRadioChange(option: string) {
    this.selectedOption = option;
    console.log('Selected option:', this.selectedOption);
  }

  uploadFile(event) {
    this.size=(event.target as HTMLInputElement).files.length;
    for (let i = 0; i < (event.target as HTMLInputElement).files.length; i++) {
    const file = (event.target as HTMLInputElement).files[i];
    this.form.addControl('newControl'+String(i), new FormControl('', []));
   // this.form.patchValue({
     // : file
    //});
    this.form.controls['newControl'+String(i)].setValue(file);
    this.form.get('newControl'+String(i)).updateValueAndValidity()
    }
  }

  submitForm() {
    var formData: any = new FormData();
    var hex = this.form.get('bg_color').value;

    
  
    hex = hex.replace('#', '');

    // Convert the hex values to decimal
    const red = parseInt(hex.substr(0, 2), 16);
    const green = parseInt(hex.substr(2, 2), 16);
    const blue = parseInt(hex.substr(4, 2), 16);

   

    formData.append("border", this.form.get('border').value);
    formData.append("colorRed", red);
    formData.append("colorGreen", green);
    formData.append("colorBlue", blue);
    formData.append("orientation", this.selectedOption)
   
    for (let i = 0; i < this.size; i++) {
      formData.append('newControl'+String(i), this.form.get('newControl'+String(i)).value);
    }
    

    
    this.http.post(this.baseUrls + 'ImageUpload/uploads', formData)
    .pipe(concatMap(item => of(item).pipe(delay(8000))))
    .subscribe(
     (response) => {
       console.log(response);
       
       let resStr = JSON.stringify(response);
       let resJSON = JSON.parse(resStr);
       this.uniqueid="/ImageUpload/img?unique_id="+response.toString();
       console.log(resJSON.text);
      },
      
        (error) => {console.log(error);
          
      }
    )
  }

}
