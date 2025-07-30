import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { LoadingGuardService } from '../../services/loading-guard.service';

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
  styleUrls: ['./course-list.component.css']
})
export class CourseListComponent implements OnInit, OnDestroy {
  courses: Course[] = [];
  isLoading = false;
  errorMessage = '';
  
  private readonly REQUEST_KEY = 'load-courses';

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient
  ) {
    console.log('CourseListComponent: Constructor called');
  }

  ngOnInit() {
    console.log('CourseListComponent: ngOnInit called');
    
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadCourses();
  }

  ngOnDestroy() {
    console.log('CourseListComponent: ngOnDestroy called');
    LoadingGuardService.endRequest(this.REQUEST_KEY);
  }

  loadCourses() {
    console.log('CourseListComponent: loadCourses called');
    
    // Check if request is already active
    if (LoadingGuardService.isRequestActive(this.REQUEST_KEY)) {
      console.log('CourseListComponent: Request already active, skipping');
      return;
    }

    // Start the request
    if (!LoadingGuardService.startRequest(this.REQUEST_KEY)) {
      console.log('CourseListComponent: Could not start request, already active');
      return;
    }

    const token = sessionStorage.getItem('token');
    if (!token) {
      console.error('CourseListComponent: No authentication token found');
      LoadingGuardService.endRequest(this.REQUEST_KEY);
      this.router.navigate(['/login']);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    
    const headers = { 'Authorization': `Bearer ${token}` };

    console.log('CourseListComponent: Making HTTP request');
    this.http.get<Course[]>('http://localhost:5021/api/Course', { headers }).subscribe({
      next: (courses) => {
        console.log('CourseListComponent: HTTP request successful', courses.length, 'courses');
        this.courses = courses;
        this.isLoading = false;
        LoadingGuardService.endRequest(this.REQUEST_KEY);
      },
      error: (error) => {
        console.error('CourseListComponent: HTTP request failed', error);
        this.isLoading = false;
        LoadingGuardService.endRequest(this.REQUEST_KEY);
        
        if (error.status === 401) {
          this.errorMessage = 'Authentication failed. Please login again.';
          this.authService.logout();
          this.router.navigate(['/login']);
        } else {
          this.errorMessage = 'Failed to load courses. Please try again.';
        }
      }
    });
  }

  refreshCourses() {
    console.log('CourseListComponent: Manual refresh triggered');
    LoadingGuardService.endRequest(this.REQUEST_KEY); // Clear any existing request
    this.loadCourses();
  }

  toggleCourseStatus(course: Course) {
    course.isActive = !course.isActive;
  }

  deleteCourse(course: Course) {
    if (!confirm(`Are you sure you want to delete the course "${course.courseName}"?`)) {
      return;
    }
    this.courses = this.courses.filter(c => c.id !== course.id);
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
