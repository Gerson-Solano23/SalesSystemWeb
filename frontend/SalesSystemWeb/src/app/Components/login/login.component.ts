import { Component } from '@angular/core';

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Login } from '../../Interfaces/login';
import { LoginResponse } from '../../Interfaces/login-response';
import { HOME_BY_ROLE, RoleName } from '../../Reusable/auth.guard';
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
    // One request returns both the token and the session, so the token is stored
    // before navigating and the first API calls of the layout are already authenticated.
    this._UserService.login(request).subscribe({
      next: (response) => {
        if (response.status) {
          const loginData = response.data as LoginResponse;
          this._UtilityService.setToken(loginData.token);
          this._UtilityService.saveUserSession(loginData.session);
          this._UtilityService.setIdRol(loginData.session.rolDescription);
          const home = HOME_BY_ROLE[loginData.session.rolDescription as RoleName];
          this._Router.navigateByUrl(home ?? '/login');
        }else{
          this._UtilityService.ShowAlert("User not found", 'Opps!');
        }
      },
      complete: () => {
        this.showLoading = false;
      },
      error: (error) => {
        this.showLoading = false;
        const message = error.status === 401 ? 'Invalid email or password'
          : error.error?.message ?? 'An error has occurred';
        this._UtilityService.ShowAlert(message, 'Opps!');
      }
    });
  }
  
  
}
