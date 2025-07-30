import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SchoolHierarchy, AdminStatistics } from '../models/admin.models';

interface SchoolSettings {
  id: number;
  schoolId: number;
  schoolDisplayName: string;
  logoImageUrl: string;
  navigationType: string;
  themeColor: string;
  updatedAt: string;
}

interface SchoolSettingsRequest {
  schoolDisplayName: string;
  logoImageUrl: string;
  navigationType: string;
  themeColor: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = 'http://localhost:5021/api/Admin';
  private settingsApiUrl = 'http://localhost:5021/api/SchoolSettings';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const token = sessionStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  getSchoolHierarchy(): Observable<SchoolHierarchy[]> {
    return this.http.get<SchoolHierarchy[]>(`${this.apiUrl}/school-hierarchy`, {
      headers: this.getHeaders()
    });
  }

  getSchoolHierarchyPaginated(page: number = 1, pageSize: number = 5, search: string = ''): Observable<any> {
    const params: any = { page: page.toString(), pageSize: pageSize.toString() };
    if (search) {
      params.search = search;
    }
    
    return this.http.get<any>(`${this.apiUrl}/school-hierarchy-paginated`, {
      headers: this.getHeaders(),
      params: params
    });
  }

  toggleSchoolOwnerStatus(ownerId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/toggle-school-owner-status/${ownerId}`, {}, {
      headers: this.getHeaders()
    });
  }

  toggleSchoolHeadStatus(headId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/toggle-school-head-status/${headId}`, {}, {
      headers: this.getHeaders()
    });
  }

  getAdminStatistics(): Observable<AdminStatistics> {
    return this.http.get<AdminStatistics>(`${this.apiUrl}/statistics`, {
      headers: this.getHeaders()
    });
  }

  // School Settings methods for Admin
  getSchoolSettings(schoolId: number): Observable<SchoolSettings> {
    return this.http.get<SchoolSettings>(`${this.settingsApiUrl}?schoolId=${schoolId}`, {
      headers: this.getHeaders()
    });
  }

  updateSchoolSettings(schoolId: number, settings: SchoolSettingsRequest): Observable<SchoolSettings> {
    return this.http.put<SchoolSettings>(`${this.settingsApiUrl}?schoolId=${schoolId}`, settings, {
      headers: this.getHeaders()
    });
  }
}
