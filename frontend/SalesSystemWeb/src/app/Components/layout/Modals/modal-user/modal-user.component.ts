import { Component, OnInit, Inject } from '@angular/core';

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Rol } from '../../../../Interfaces/rol';
import { RolService } from '../../../../Services/rol.service';
import { User } from '../../../../Interfaces/user';
import { UserService } from '../../../../Services/user.service';
import { UtilityService } from '../../../../Reusable/utility.service';

@Component({
  selector: 'app-modal-user',
  templateUrl: './modal-user.component.html',
  styleUrl: './modal-user.component.css'
})
export class ModalUserComponent implements OnInit {
  formUser: FormGroup;
  hidePassword = true;
  actionTitle: string = 'Add';
  actionButton: string = 'Save';
  rolsList: Rol[] = [];
  constructor(
    private nowModal: MatDialogRef<ModalUserComponent>,
    @Inject(MAT_DIALOG_DATA) public userData: User,
    private form: FormBuilder,
    private rolService: RolService,
    private userService: UserService,
    private utilityService: UtilityService
  ) { 
    this.formUser = this.form.group({
      FullName: ['', Validators.required],
      Email: ['', Validators.required],
      IdRol: ['', Validators.required],
      Password: ['', Validators.required],
      Status: ['', Validators.required],
    });
    if (this.userData != null) {
      this.actionTitle = 'Edit';
      this.actionButton = 'Update';
    }
    
    this.rolService.listRoles().subscribe({
      next: (response) => {
        if (response.status) {
          this.rolsList = response.data;
          console.log('Constructor response', response.data[0].idRol);
        }
      },
      error: (error) => {}
    });  

  }

  ngOnInit(): void {
    if (this.userData != null) {
      this.formUser.patchValue({
        FullName: this.userData.fullName,
        Email: this.userData.email,
        IdRol: this.userData.idRol,
        Password: this.userData.password,
        Status: this.userData.status.toString()
      });
      
    }
  }

  createOrUpdateUser() {
    const user: User ={
      idUser: this.userData == null ? 0 : this.userData.idUser,
      fullName: this.formUser.value.FullName,
      email: this.formUser.value.Email,
      idRol: this.formUser.value.IdRol,
      rolDescription:"",
      password: this.formUser.value.Password,
      status: parseInt(this.formUser.value.Status)
    }
    if (this.userData == null) {
      this.userService.createUser(user).subscribe({
        next:(response)=>{
          if(response.status){
            this.utilityService.ShowAlert('User was successfully registered', 'success');
            this.nowModal.close("true");
          }else{
            this.utilityService.ShowAlert("User could not be registered", 'error');
          }
        },
        error:(error)=>{
          this.utilityService.ShowAlert("User could not be registered, try again", 'error');
        }
      });
    }else{
      this.userService.updateUser(user).subscribe({
        next:(response)=>{
          if(response.status){
            this.utilityService.ShowAlert('User was successfully updated', 'success');
            this.nowModal.close("true");
          }else{
            this.utilityService.ShowAlert("User could not be updated", 'error');
          }
        },
        error:(error)=>{
          this.utilityService.ShowAlert("User could not be updated, try again", 'error');
        }
      });
    }
  }

}
