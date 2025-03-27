import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DynamicModel } from '../models/dynamic-model';
import { ValidationResult } from '../models/validation-result';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DynamicModelService {
  private apiUrl = `${environment.apiUrl}/api/DynamicModel`;

  constructor(private http: HttpClient) { }

  getModels(): Observable<DynamicModel[]> {
    return this.http.get<DynamicModel[]>(this.apiUrl);
  }

  getModelById(id: number): Observable<DynamicModel> {
    return this.http.get<DynamicModel>(`${this.apiUrl}/${id}`);
  }

  getModelByName(name: string): Observable<DynamicModel> {
    return this.http.get<DynamicModel>(`${this.apiUrl}/byname/${name}`);
  }

  createModel(model: DynamicModel): Observable<any> {
    return this.http.post(this.apiUrl, model);
  }

  updateModel(id: number, model: DynamicModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, model);
  }

  deleteModel(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  validateData(modelName: string, data: any): Observable<ValidationResult> {
    return this.http.post<ValidationResult>(`${this.apiUrl}/validate/${modelName}`, data);
  }

  autoDetectAndValidate(data: any): Observable<ValidationResult> {
    return this.http.post<ValidationResult>(`${this.apiUrl}/validate`, data);
  }
}