import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ResponseApi } from '../Interfaces/response-api';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  private urlAPI: string = environment.apiURL + 'Menu/';

  constructor(private http:HttpClient) { }

  // The API resolves the user from the JWT, so no user id is sent.
  listMenus():Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}List`);
  }


}
