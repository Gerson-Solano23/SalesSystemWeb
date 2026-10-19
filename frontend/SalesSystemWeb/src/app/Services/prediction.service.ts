import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ResponseApi } from '../Interfaces/response-api';

@Injectable({
  providedIn: 'root'
})
export class PredictionService {
  private urlAPI: string = environment.apiURL + 'Prediction/';

  constructor(private http:HttpClient) { }

  // Monthly revenue history plus the forecast for the current month and the next ones.
  monthlyRevenue(months:number):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}MonthlyRevenue?months=${months}`);
  }
}
