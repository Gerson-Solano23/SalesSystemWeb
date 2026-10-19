import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
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
    return this.http.get<ResponseApi>(`${this.urlAPI}Summary`);
  }
  MonthSummary(month:number, year:number):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}MonthSummary?month=${month}&year=${year}`);
  }
  YearSummary(year:number):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}YearSummary?year=${year}`);
  }

  RangeDatesSummary(startDate:string, endDate:string):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}RangeDatesSummary?startDate=${startDate}&endDate=${endDate}`);
  }
}
