import { Injectable } from '@angular/core';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../enviroments/enviroments';
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
    return this.http.get<ResponseApi>(`${this.urlAPI}DownloadExcel?Startdate=${startDate}&endDate=${endDate}`, {headers: this.getDateToken()});
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
