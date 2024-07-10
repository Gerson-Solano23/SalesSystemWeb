import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../enviroments/enviroments';
import { ResponseApi } from '../Interfaces/response-api';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  private urlAPI: string = environment.apiURL + 'Menu/';

  constructor(private http:HttpClient) { }

  listMenus(IdUser:number):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}List/${IdUser}`);
  } 

  

}
