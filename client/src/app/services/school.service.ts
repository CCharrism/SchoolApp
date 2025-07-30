import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateSchoolRequest, School } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class SchoolService {
  private apiUrl = 'http://localhost:5021/api';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const token = sessionStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  createSchool(schoolData: CreateSchoolRequest): Observable<School> {
    return this.http.post<School>(`${this.apiUrl}/school`, schoolData, {
      headers: this.getHeaders()
    });
  }

  getAllSchools(): Observable<School[]> {
    return this.http.get<School[]>(`${this.apiUrl}/school`, {
      headers: this.getHeaders()
    });
  }

  getSchoolById(id: number): Observable<School> {
    return this.http.get<School>(`${this.apiUrl}/school/${id}`, {
      headers: this.getHeaders()
    });
  }

  updateSchool(id: number, schoolData: Partial<CreateSchoolRequest>): Observable<School> {
    return this.http.put<School>(`${this.apiUrl}/school/${id}`, schoolData, {
      headers: this.getHeaders()
    });
  }

  deleteSchool(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/school/${id}`, {
      headers: this.getHeaders()
    });
  }

  toggleSchoolStatus(id: number): Observable<School> {
    return this.http.patch<School>(`${this.apiUrl}/school/${id}/toggle-status`, {}, {
      headers: this.getHeaders()
    });
  }
}
