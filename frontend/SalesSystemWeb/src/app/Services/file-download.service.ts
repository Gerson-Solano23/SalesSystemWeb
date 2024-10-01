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


  downloadExcel(idUser:number):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}DownloadExcel?idUser=${idUser}`, {headers: this.getDateToken()});
  }

  freeStorage(idUser:number):Observable<ResponseApi>{
    return this.http.delete<ResponseApi>(`${this.urlAPI}DeleteFileTask?idUser=${idUser}`, {headers: this.getDateToken()});
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
