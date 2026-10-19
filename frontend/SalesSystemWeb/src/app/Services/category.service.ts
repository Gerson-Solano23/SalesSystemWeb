import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ResponseApi } from '../Interfaces/response-api';
import { UtilityService } from '../Reusable/utility.service';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private urlAPI: string = environment.apiURL + 'Category/';

  constructor(private http:HttpClient,
    private utilityService:UtilityService
  ) { }

  listCategories():Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}List`);
  }
}
