import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, of, catchError } from 'rxjs';
import { LoginRequest, LoginResponse, User } from '../models/auth.models';

export interface Course {
  id: number;
  courseName: string;
  description: string;
  teacherId: number;
  teacherName: string;
  subject: string;
  grade: string;
  students: StudentInCourse[];
  isActive: boolean;
  createdAt: string;
}

export interface StudentInCourse {
  id: number;
  fullName: string;
  username: string;
  email: string;
  grade: string;
  rollNumber: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5021/api';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadUserFromStorage();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => {
          if (response.token) {
            this.setUserSession(response);
          }
        }),
        catchError(error => {
          console.log('Login failed:', error.error);
          console.error('Login error:', error);
          throw error;
        })
      );
  }

  private getMockLogin(username: string): Observable<LoginResponse> {
    const mockUsers = [
      {
        id: 1,
        username: 'admin',
        fullName: 'School Administrator',
        email: 'admin@school.edu',
        role: 'SchoolHead' as const,
        schoolId: 1
      },
      {
        id: 2,
        username: 'john.smith',
        fullName: 'John Smith',
        email: 'john.smith@school.edu',
        role: 'Teacher' as const,
        schoolId: 1,
        subject: 'Mathematics'
      },
      {
        id: 3,
        username: 'alice.johnson',
        fullName: 'Alice Johnson',
        email: 'alice.johnson@student.edu',
        role: 'Student' as const,
        schoolId: 1,
        grade: 'Grade 10-A'
      }
    ];

    const user = mockUsers.find(u => u.username === username) || mockUsers[0];
    const response: LoginResponse = {
      token: 'mock-token-' + Date.now(),
      username: user.username,
      role: user.role,
      expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
      user: user
    };

    this.setUserSession(response);
    return of(response);
  }

  logout(): void {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
    sessionStorage.removeItem('tokenExpiry');
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return sessionStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    const expiry = sessionStorage.getItem('tokenExpiry');
    
    if (!token || !expiry) {
      return false;
    }

    const expiryDate = new Date(expiry);
    const now = new Date();
    
    if (now >= expiryDate) {
      this.logout();
      return false;
    }

    return true;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  getDashboardRoute(): string {
    const user = this.getCurrentUser();
    if (!user) return '/login';
    
    switch (user.role.toLowerCase()) {
      case 'admin':
        return '/admin/dashboard';
      case 'schoolowner':
        return '/owner/dashboard';
      case 'schoolhead':
        return '/school-head/dashboard';
      case 'teacher':
        return '/teacher/dashboard';
      case 'student':
        return '/student/dashboard';
      default:
        return '/login';
    }
  }

  private setUserSession(response: LoginResponse): void {
    sessionStorage.setItem('token', response.token);
    sessionStorage.setItem('tokenExpiry', response.expiresAt);
    
    if (response.user) {
      sessionStorage.setItem('user', JSON.stringify(response.user));
      this.currentUserSubject.next(response.user);
    }
  }

  private loadUserFromStorage(): void {
    if (this.isAuthenticated()) {
      const userStr = sessionStorage.getItem('user');
      if (userStr) {
        const user: User = JSON.parse(userStr);
        this.currentUserSubject.next(user);
      }
    }
  }

  // Test methods for debugging JWT
  testAuthEndpoint(): Observable<any> {
    return this.http.get(`${this.apiUrl}/jwttest/test-auth`);
  }

  decodeToken(): Observable<any> {
    const token = this.getToken();
    if (!token) {
      return of({ error: 'No token found' });
    }
    
    return this.http.post(`${this.apiUrl}/jwttest/decode`, { token });
  }
  
  testHeaders(): Observable<any> {
    return this.http.get(`${this.apiUrl}/jwttest/headers`);
  }
}
