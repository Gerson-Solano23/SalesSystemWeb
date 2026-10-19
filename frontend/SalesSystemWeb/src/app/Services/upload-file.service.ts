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

  // Sent as multipart/form-data; the API reads the "file" field.
  uploadFile(file:File):Observable<ResponseApi>{
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<ResponseApi>(`${this.urlAPI}UploadFile`, formData);
  }

  // Years that have stored files, plus the current year, newest first.
  getYears():Observable<ResponseApi>{
    return this.http.get<ResponseApi>(`${this.urlAPI}getReportYears`);
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
