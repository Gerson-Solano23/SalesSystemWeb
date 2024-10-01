import { Injectable } from '@angular/core';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../enviroments/enviroments';
import { ResponseApi } from '../Interfaces/response-api';
import { Product } from '../Interfaces/product';
@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private urlAPI: string = environment.apiURL + 'Product/';

  constructor(private http:HttpClient) { }

  listProducts():Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}List`, {headers: this.getDateToken()});
  }

  createProduct(product:Product):Observable<ResponseApi>{
    return this.http.post<ResponseApi>(`${this.urlAPI}Create`,product, {headers: this.getDateToken()});
  }

  updateProduct(product:Product):Observable<ResponseApi>{
    return this.http.put<ResponseApi>(`${this.urlAPI}Update`,product, {headers: this.getDateToken()});
  }

  deleteproduct(Idproduct:number):Observable<ResponseApi>{
    return this.http.delete<ResponseApi>(`${this.urlAPI}Delete?id=${Idproduct}`, {headers: this.getDateToken()});
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
