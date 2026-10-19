import { Injectable } from '@angular/core';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../enviroments/enviroments';
import { ResponseApi } from '../Interfaces/response-api';
import { User } from '../Interfaces/user';
import { Login } from '../Interfaces/login';
@Injectable({
  providedIn: 'root'
})
export class UserService {
  private urlAPI: string = environment.apiURL + 'Usuario/';
  private urlAPIAuth: string = environment.apiURL + 'Authenticator/';

  constructor(private http:HttpClient) { }

  // Returns the JWT and the user session in a single request: { token, session }.
  login(login:Login):Observable<ResponseApi>{
    return this.http.post<ResponseApi>(`${this.urlAPIAuth}Login`, login);
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
