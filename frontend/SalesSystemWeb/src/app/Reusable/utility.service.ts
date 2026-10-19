import { Injectable } from '@angular/core';

import { MatSnackBar } from '@angular/material/snack-bar';
import { Session } from '../Interfaces/session';
import { Router } from '@angular/router';
@Injectable({
  providedIn: 'root'
})
export class UtilityService {

  constructor(private _SackBar: MatSnackBar,
    private router: Router
  ) { }

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

  getUserSession(): Session | null {
    try {
      const data = localStorage.getItem("userSession");
      return data ? JSON.parse(data) as Session : null;
    } catch {
      return null;
    }
  }

  deleteUserSession() {
    localStorage.removeItem("userSession");
    this.logout();
    this.deleteIdRol();
  }

  setToken(token: string): void {
    localStorage.setItem('authToken', token);
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  logout(): void {
    localStorage.removeItem('authToken');
  }

  setIdRol(idRol: string): void {
    localStorage.setItem('idRol', idRol);
  }

  getIdRol(): string | null {
    return localStorage.getItem('idRol');
  }

  deleteIdRol(): void {
    localStorage.removeItem('idRol');
  }

  checkTokendExpired(){
    if (this.checkTokenExpiration()){
      this.ShowAlert("Session expired", "Opps!");
      this.deleteUserSession();
      this.router.navigate(['/login']);
    }
  }

  /** True when there is no token, it cannot be read, or its exp claim is in the past. */
  checkTokenExpiration(): boolean {
    const parts = this.getToken()?.split('.');
    if (parts == null || parts.length !== 3) {
      return true;
    }
    try {
      const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
      return Math.floor(Date.now() / 1000) >= payload.exp;
    } catch {
      return true;
    }
  }
}
