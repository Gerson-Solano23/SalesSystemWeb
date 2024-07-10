import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../enviroments/enviroments';
import { ResponseApi } from '../Interfaces/response-api';
import { User } from '../Interfaces/user';
import { Login } from '../Interfaces/login';
@Injectable({
  providedIn: 'root'
})
export class UserService {
  private urlAPI: string = environment.apiURL + 'Usuario/';

  constructor(private http:HttpClient) { }

  login(login:Login):Observable<ResponseApi>{
    return this.http.post<ResponseApi>(`${this.urlAPI}Login`,login);
  }

  listUsers():Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}List`);
  }

  createUser(user:User):Observable<ResponseApi>{
    return this.http.post<ResponseApi>(`${this.urlAPI}Create`,user);
  }

  updateUser(user:User):Observable<ResponseApi>{
    return this.http.put<ResponseApi>(`${this.urlAPI}}Update`,user);
  }

  deleteUser(Id:number):Observable<ResponseApi>{
    return this.http.delete<ResponseApi>(`${this.urlAPI}Delete/${Id}`);
  }
}
