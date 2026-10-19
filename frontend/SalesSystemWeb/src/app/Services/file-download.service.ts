import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ResponseApi } from '../Interfaces/response-api';
import { Report } from '../Interfaces/report';
@Injectable({
  providedIn: 'root'
})
export class FileDownloadService {

  private urlAPI: string = environment.apiURL + 'FileDownload/';

  constructor(private http:HttpClient
  ) { }


  // The API builds the Excel file for the date range in the same request (dates as DD/MM/YYYY).
  downloadExcel(startDate:string, endDate:string):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}DownloadExcel?Startdate=${startDate}&endDate=${endDate}`);
  }
  
}
