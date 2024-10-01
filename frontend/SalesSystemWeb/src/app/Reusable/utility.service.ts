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
  private controlDowloadExcel:boolean = false;

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
    this.logout();
    this.deleteIdRol(); 
  }
  
  isDownloadExcelSelected():boolean{
    return this.controlDowloadExcel;
  }

  setToken(token: string): void {
    localStorage.setItem('authToken', token);
  }

  // Método para obtener el token JWT
  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  logout(): void {
    localStorage.removeItem('authToken');
  }
  setIdRol(idRol: string): void {
    localStorage.setItem('idRol', idRol);
  }

  // Método para obtener el token JWT
  getIdRol(): string | null {
    return localStorage.getItem('idRol');
  }

  deleteIdRol(): void {
    localStorage.removeItem('idRol');
  }

  onlyRolOne(){
    if (this.getIdRol() != 'Admin') {
      if (this.getIdRol() === 'Supervisor') {
        this.router.navigate(['pages/products']);
        
      }else if (this.getIdRol() === 'Employee') {
        this.router.navigate(['pages/sale']);
      }
    }
  }

  onlyRolOneAndTwo(){
    if (this.getIdRol() != 'Supervisor' && this.getIdRol() != 'Admin') {
      this.router.navigate(['pages/sale']);
    }
  }

  checkTokendExpired(){
    if (this.checkTokenExpiration()){
      this.ShowAlert("Session expired", "Opps!");
      this.deleteUserSession();
      this.router.navigate(['/login']);
    }

    
  }

  checkTokenExpiration(): boolean {
    const ARRAY_TOKEN = this.getToken()?.split('.');
    if (ARRAY_TOKEN == null || ARRAY_TOKEN == undefined) {
      console.log('Token not found');
      return true;
      
    }else{
      const TOKEN_PAYLOAD = JSON.parse(atob(ARRAY_TOKEN[1])); 
      return Math.floor(new Date().getTime() / 1000) >= TOKEN_PAYLOAD.exp;
    }

  }
  

}
