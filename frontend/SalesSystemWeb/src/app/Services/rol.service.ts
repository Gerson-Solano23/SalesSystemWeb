import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
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
    return this.http.get<ResponseApi>(`${this.urlAPI}List`);
  }
}
