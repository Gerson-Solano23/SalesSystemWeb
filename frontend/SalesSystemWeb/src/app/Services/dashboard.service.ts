import { Injectable } from '@angular/core';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../enviroments/enviroments';
import { ResponseApi } from '../Interfaces/response-api';
import { UtilityService } from '../Reusable/utility.service';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private urlAPI: string = environment.apiURL + 'Dashboard/';

  constructor(private http:HttpClient,
    private utilityService:UtilityService
  ) { }

  summary():Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}Summary`, {headers: this.getDateToken()});
  }
  MonthSummary(month:number, year:number):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}MonthSummary?month=${month}&year=${year}`, {headers: this.getDateToken()});
  }
  YearSummary(year:number):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}YearSummary?year=${year}`, {headers: this.getDateToken()});
  }

  RangeDatesSummary(startDate:string, endDate:string):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}RangeDatesSummary?startDate=${startDate}&endDate=${endDate}`, {headers: this.getDateToken()});
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
