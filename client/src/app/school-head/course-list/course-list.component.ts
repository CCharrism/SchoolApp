import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

interface Course {
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

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './course-list.component.html',
  styleUrls: ['./course-list.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseListComponent implements OnInit, OnDestroy {
  courses: Course[] = [];
  isLoading = false;
  errorMessage = '';
  
  private static isLoadingCourses = false;
  private static coursesCache: Course[] | null = null;
  private static lastLoadTime = 0;
  private readonly CACHE_DURATION = 30000; // 30 seconds

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {
    console.log('CourseListComponent: Constructor called');
  }

  ngOnInit() {
    console.log('CourseListComponent: ngOnInit called');
    
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadCoursesWithCache();
  }

  ngOnDestroy() {
    console.log('CourseListComponent: ngOnDestroy called');
  }

  private loadCoursesWithCache() {
    console.log('CourseListComponent: loadCoursesWithCache called');
    
    // Check if we have valid cached data
    const now = Date.now();
    if (CourseListComponent.coursesCache && 
        (now - CourseListComponent.lastLoadTime) < this.CACHE_DURATION) {
      console.log('CourseListComponent: Using cached data');
      this.courses = CourseListComponent.coursesCache;
      this.cdr.detectChanges();
      return;
    }

    // Check if already loading
    if (CourseListComponent.isLoadingCourses) {
      console.log('CourseListComponent: Already loading, waiting...');
      this.waitForLoading();
      return;
    }

    this.loadCoursesFromAPI();
  }

  private waitForLoading() {
    // Wait for the ongoing load to complete
    const checkInterval = setInterval(() => {
      if (!CourseListComponent.isLoadingCourses && CourseListComponent.coursesCache) {
        console.log('CourseListComponent: Loading completed, using result');
        this.courses = CourseListComponent.coursesCache;
        this.isLoading = false;
        this.cdr.detectChanges();
        clearInterval(checkInterval);
      }
    }, 100);
    
    // Timeout after 10 seconds
    setTimeout(() => {
      clearInterval(checkInterval);
      if (CourseListComponent.isLoadingCourses) {
        console.log('CourseListComponent: Loading timeout, forcing reload');
        CourseListComponent.isLoadingCourses = false;
        this.loadCoursesFromAPI();
      }
    }, 10000);
  }

  private loadCoursesFromAPI() {
    console.log('CourseListComponent: loadCoursesFromAPI called');
    
    const token = sessionStorage.getItem('token');
    if (!token) {
      console.error('CourseListComponent: No authentication token found');
      this.router.navigate(['/login']);
      return;
    }

    CourseListComponent.isLoadingCourses = true;
    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();
    
    const headers = { 'Authorization': `Bearer ${token}` };

    console.log('CourseListComponent: Making HTTP request');
    this.http.get<Course[]>('http://localhost:5021/api/Course', { headers }).subscribe({
      next: (courses) => {
        console.log('CourseListComponent: HTTP request successful', courses.length, 'courses');
        
        // Update cache and state
        CourseListComponent.coursesCache = courses;
        CourseListComponent.lastLoadTime = Date.now();
        CourseListComponent.isLoadingCourses = false;
        
        this.courses = courses;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('CourseListComponent: HTTP request failed', error);
        
        CourseListComponent.isLoadingCourses = false;
        this.isLoading = false;
        
        if (error.status === 401) {
          this.errorMessage = 'Authentication failed. Please login again.';
          this.authService.logout();
          this.router.navigate(['/login']);
        } else {
          this.errorMessage = 'Failed to load courses. Please try again.';
        }
        this.cdr.detectChanges();
      }
    });
  }

  refreshCourses() {
    console.log('CourseListComponent: Manual refresh triggered');
    // Clear cache and force reload
    CourseListComponent.coursesCache = null;
    CourseListComponent.lastLoadTime = 0;
    CourseListComponent.isLoadingCourses = false;
    this.loadCoursesWithCache();
  }

  toggleCourseStatus(course: Course) {
    course.isActive = !course.isActive;
    this.cdr.detectChanges();
  }

  deleteCourse(course: Course) {
    if (!confirm(`Are you sure you want to delete the course "${course.courseName}"?`)) {
      return;
    }
    this.courses = this.courses.filter(c => c.id !== course.id);
    this.cdr.detectChanges();
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  getActiveCourses(): number {
    return this.courses.filter(course => course.isActive).length;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
