import { Injectable } from '@angular/core';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../enviroments/enviroments';
import { ResponseApi } from '../Interfaces/response-api';


@Injectable({
  providedIn: 'root'
})
export class RolService {
  private urlAPI: string = environment.apiURL + 'Rol/';
  constructor(private http:HttpClient) { }

  listRoles():Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}List`, {headers: this.getDateToken()});
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
