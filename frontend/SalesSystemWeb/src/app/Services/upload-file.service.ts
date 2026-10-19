import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { ResponseApi } from '../Interfaces/response-api';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UploadFileService {
  
  private urlAPI: string = environment.apiURL + 'UploadFile/';

  constructor(private http:HttpClient) { }

  uploadFile(fileData:any):Observable<ResponseApi>{
    return this.http.post<ResponseApi>(`${this.urlAPI}UploadFile`, fileData);
  }

  // deleteFile(fileName:string):Observable<ResponseApi>{
  //   return this.http.delete<ResponseApi>(`${this.urlAPI}DeleteFile?fileName=${fileName}`);
  // }

  DownloadFile(fileName:string):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}DownloadFile?fileName=${fileName}`);
  }

  getListPerMonth(month:string, year:string):Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}getReportsPerMonth?month=${month}&year=${year}`);
  }

 
}
