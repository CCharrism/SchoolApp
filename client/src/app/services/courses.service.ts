import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';

export interface Course {
  id: number;
  courseName: string;
  description: string;
  thumbnailImageUrl?: string;
  branchId: number;
  branchName: string;
  createdBy: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  lessonCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class CoursesService {
  private coursesSubject = new BehaviorSubject<Course[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private loadedSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string>('');

  courses$ = this.coursesSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  loaded$ = this.loadedSubject.asObservable();
  error$ = this.errorSubject.asObservable();

  private apiUrl = 'http://localhost:5021/api/Course';
  private isCurrentlyLoading = false;

  constructor(private http: HttpClient) {
    console.log('CoursesService: Constructor called');
  }

  loadCourses(forceReload: boolean = false): void {
    console.log('CoursesService: loadCourses called', { 
      forceReload, 
      isCurrentlyLoading: this.isCurrentlyLoading, 
      loaded: this.loadedSubject.value 
    });

    // Prevent multiple simultaneous requests
    if (this.isCurrentlyLoading) {
      console.log('CoursesService: Already loading, skipping request');
      return;
    }

    // If already loaded and not forcing reload, skip
    if (this.loadedSubject.value && !forceReload) {
      console.log('CoursesService: Already loaded, skipping request');
      return;
    }

    const token = sessionStorage.getItem('token');
    if (!token) {
      console.error('CoursesService: No authentication token found');
      this.errorSubject.next('No authentication token found');
      return;
    }

    this.isCurrentlyLoading = true;
    this.loadingSubject.next(true);
    this.errorSubject.next('');
    
    const headers = { 'Authorization': `Bearer ${token}` };

    console.log('CoursesService: Making HTTP request');
    this.http.get<Course[]>(this.apiUrl, { headers }).pipe(
      tap(courses => {
        console.log('CoursesService: HTTP request successful', courses.length, 'courses');
        this.coursesSubject.next(courses);
        this.loadedSubject.next(true);
        this.loadingSubject.next(false);
        this.isCurrentlyLoading = false;
      }),
      catchError(error => {
        console.error('CoursesService: HTTP request failed', error);
        this.errorSubject.next('Failed to load courses');
        this.loadingSubject.next(false);
        this.isCurrentlyLoading = false;
        return of([]);
      })
    ).subscribe();
  }

  getCourses(): Course[] {
    return this.coursesSubject.value;
  }

  addCourse(course: Course): void {
    const currentCourses = this.coursesSubject.value;
    this.coursesSubject.next([...currentCourses, course]);
  }

  clearCache(): void {
    this.coursesSubject.next([]);
    this.loadedSubject.next(false);
    this.loadingSubject.next(false);
    this.isCurrentlyLoading = false;
  }
}
