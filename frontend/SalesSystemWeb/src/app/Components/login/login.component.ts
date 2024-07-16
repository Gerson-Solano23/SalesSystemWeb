import { Component } from '@angular/core';

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Login } from '../../Interfaces/login';
import { UserService } from '../../Services/user.service';
import { UtilityService } from '../../Reusable/utility.service';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
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

    this._UserService.login(request).subscribe({
      next: (response) => {
        if (response.status) {
          this._UtilityService.saveUserSession(response.data);
          this._Router.navigate(['/pages']);
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
