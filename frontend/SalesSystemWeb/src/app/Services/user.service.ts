import { Injectable } from '@angular/core';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../enviroments/enviroments';
import { ResponseApi } from '../Interfaces/response-api';
import { User } from '../Interfaces/user';
import { Login } from '../Interfaces/login';
import { UtilityService } from '../Reusable/utility.service';
@Injectable({
  providedIn: 'root'
})
export class UserService {
  private urlAPI: string = environment.apiURL + 'Usuario/';
  private urlAPIToken: string = environment.apiURL + 'Authenticator/';

  constructor(private http:HttpClient,
              private utilityService:UtilityService
  ) { 
    
  }

  login(login:Login):Observable<ResponseApi>{
    var responseLogin = this.http.post<ResponseApi>(`${this.urlAPI}Login`,login);
    return responseLogin;

  }

  listUsers():Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}List`, {headers: this.getDateToken()});
  }

  createUser(user:User):Observable<ResponseApi>{
    return this.http.post<ResponseApi>(`${this.urlAPI}Create`,user, {headers: this.getDateToken()});
  }

  updateUser(user:User):Observable<ResponseApi>{
    return this.http.put<ResponseApi>(`${this.urlAPI}Update`,user, {headers: this.getDateToken()});
  }

  deleteUser(Id:number):Observable<ResponseApi>{
    return this.http.delete<ResponseApi>(`${this.urlAPI}Delete?id=${Id}`, {headers: this.getDateToken()});
  }

  getToken(email: string, password: string): Observable<ResponseApi> {
    return this.http.get<ResponseApi>(`${this.urlAPIToken}GetToken?email=${email}&password=${password}`);
  }
  

  getDateToken() {
    // Recuperar el token desde el localStorage
    const token = localStorage.getItem('authToken');

    // Crear las cabeceras incluyendo el token Bearer
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    return headers;
  }
}
