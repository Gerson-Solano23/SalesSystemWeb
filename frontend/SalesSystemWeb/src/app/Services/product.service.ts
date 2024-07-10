import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../enviroments/enviroments';
import { ResponseApi } from '../Interfaces/response-api';
import { Product } from '../Interfaces/product';
@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private urlAPI: string = environment.apiURL + 'Product/';

  constructor(private http:HttpClient) { }

  listProducts():Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}List`);
  }

  createProduct(product:Product):Observable<ResponseApi>{
    return this.http.post<ResponseApi>(`${this.urlAPI}Create`,product);
  }

  updateProduct(product:Product):Observable<ResponseApi>{
    return this.http.put<ResponseApi>(`${this.urlAPI}}Update`,product);
  }

  deleteproduct(Idproduct:number):Observable<ResponseApi>{
    return this.http.delete<ResponseApi>(`${this.urlAPI}Delete/${Idproduct}`);
  }
}
