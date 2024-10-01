import { Injectable } from '@angular/core';
import { environment } from '../enviroments/enviroments';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ResponseApi } from '../Interfaces/response-api';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UploadFileService {
  
  private urlAPI: string = environment.apiURL + 'UploadFile/';

  constructor(private http:HttpClient) { }

  uploadFile(fileData:any):Observable<ResponseApi>{
    return this.http.post<ResponseApi>(`${this.urlAPI}UploadFile`, fileData, {headers: this.getDateToken()});
  }

  // deleteFile(fileName:string):Observable<ResponseApi>{
  //   return this.http.delete<ResponseApi>(`${this.urlAPI}DeleteFile?fileName=${fileName}`, {headers: this.getDateToken()});
  // }

  DownloadFile(fileName:string):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}DownloadFile?fileName=${fileName}`, {headers: this.getDateToken()});
  }

  getListPerMonth(month:string, year:string):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}getReportsPerMonth?month=${month}&year=${year}`, {headers: this.getDateToken()});
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
