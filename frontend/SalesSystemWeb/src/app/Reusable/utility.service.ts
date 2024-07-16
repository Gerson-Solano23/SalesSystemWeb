import { Injectable } from '@angular/core';

import { MatSnackBar } from '@angular/material/snack-bar';
import { Session } from '../Interfaces/session';
@Injectable({
  providedIn: 'root'
})
export class UtilityService {

  constructor(private _SackBar: MatSnackBar) { }


  ShowAlert(message: string, type: string) {
    this._SackBar.open(message, type, {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
    });
  }

  saveUserSession(session: Session) {
    localStorage.setItem("userSession", JSON.stringify(session));
  }

  getUserSession(): Session {
    const data = localStorage.getItem("userSession");

    const user = JSON.parse(data!);

    return user;
  }

  deleteUserSession() {
    localStorage.removeItem("userSession");
  }  

}
