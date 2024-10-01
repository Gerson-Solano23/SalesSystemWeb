import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
@Injectable({
  providedIn: 'root'
})
export class LoadingService {

  public loadingSubject = new BehaviorSubject<boolean>(false);
  public controlExcelButton = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();
  controlExcel$ = this.controlExcelButton.asObservable();

  show(){
    this.loadingSubject.next(true);
  }
  hide(){
    this.loadingSubject.next(false);
  }

  active(){
    this.controlExcelButton.next(true);
  }
  inactive(){
    this.controlExcelButton.next(false);
  }
  getControl(){
    return this.controlExcel$;
  }
}
