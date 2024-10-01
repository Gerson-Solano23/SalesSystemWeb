import { Component } from '@angular/core';

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Login } from '../../Interfaces/login';
import { UserService } from '../../Services/user.service';
import { UtilityService } from '../../Reusable/utility.service';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css', '../../../styles.scss']
})
export class LoginComponent {
  formLogin: FormGroup;
  hidePassword: boolean = true;
  showLoading: boolean = false;

  constructor(private _FormBuilder: FormBuilder, private _UserService: UserService, private _UtilityService: UtilityService, private _Router: Router) {
    this.formLogin = this._FormBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  login(){
    this.showLoading = true;

    const request:Login={
      Email: this.formLogin.get('email')?.value,
      Password: this.formLogin.get('password')?.value
    }
    this._UserService.getToken(request.Email, request.Password).subscribe({
      next: (responseToken) => {
        if (responseToken.status) {
          this._UtilityService.setToken(responseToken.data);
        }
      }
    });
    this._UserService.login(request).subscribe({
      next: (response) => {
        if (response.status) {
          
          this._UtilityService.saveUserSession(response.data);
          console.log('sata in login', response.data);
          this._UtilityService.setIdRol(response.data.rolDescription);
          if (response.data.rolDescription == 'Admin') {
            this._Router.navigate(['/pages/dashboard']);    
          }else if (response.data.rolDescription == 'Supervisor') {
            this._Router.navigate(['/pages/products']);
            
          }else if (response.data.rolDescription == 'Employee') {
          this._Router.navigate(['/pages/sale']);
          }
        }else{
          this._UtilityService.ShowAlert("User not found", 'Opps!');
        }
      },
      complete: () => {
        this.showLoading = false;
      },
      error: (error) => {
        this._UtilityService.ShowAlert("An error has occurred", 'Opps!');
      }
    });
  }
  
  
}
