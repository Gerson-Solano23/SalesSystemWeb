import { Injectable } from '@angular/core';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../enviroments/enviroments';
import { ResponseApi } from '../Interfaces/response-api';
import { Sale } from '../Interfaces/sale';

@Injectable({
  providedIn: 'root'
})
export class SaleService {

  private urlAPI: string = environment.apiURL + 'Sale/';

  constructor(private http:HttpClient) { }

  create(sale:Sale):Observable<ResponseApi>{
    return this.http.post<ResponseApi>(`${this.urlAPI}Create`,sale, {headers: this.getDateToken()});

  }

  history(searchBy:string, saleNumber?:string, Startdate?:string, endDate?:string):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}History?searchBy=${searchBy}&saleNumber=${saleNumber}&Startdate=${Startdate}&endDate=${endDate}`, {headers: this.getDateToken()});
  }

  Report(Startdate?:string, endDate?:string, idUser?:number):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}Report?Startdate=${Startdate}&endDate=${endDate}&idUser=${idUser}`, {headers: this.getDateToken()});
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
