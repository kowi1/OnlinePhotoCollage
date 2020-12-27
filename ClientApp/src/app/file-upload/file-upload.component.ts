import { Component,Inject } from '@angular/core';
import { FormBuilder, FormGroup,FormControl } from "@angular/forms";
import { HttpClient ,HttpHeaders} from '@angular/common/http';


@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html'
})

export class FileUploadComponent  {
  form: FormGroup;
  public baseUrls:string;
  size:number;
  

  constructor(
    public fb: FormBuilder,
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.form = this.fb.group({
      border: 1,
      colorRed: 2,
      colorGreen: 2,
      colorBlue: 3,
      orientation: ['']
    })
    this.baseUrls=baseUrl
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
    
    formData.append("border", this.form.get('border').value);
    formData.append("colorRed", this.form.get('colorRed').value);
    formData.append("colorGreen", this.form.get('colorGreen').value);
    formData.append("colorBlue", this.form.get('colorBlue').value);
    formData.append("orientation", this.form.get('orientation').value);
   // formData.append("coursefile", this.form.get('coursefile').value);
    for (let i = 0; i < this.size; i++) {
      formData.append('newControl'+String(i), this.form.get('newControl'+String(i)).value);
    }
    


    this.http.post(this.baseUrls + 'ImageUpload/uploads', formData).subscribe(
      (response) => console.log(response),
      (error) => console.log(error)
    )
  }

}
