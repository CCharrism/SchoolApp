import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CoursesService } from '../../services/courses.service';

interface CreateCourseRequest {
  courseName: string;
  description: string;
  thumbnailImageUrl?: string;
}

interface Course {
  id: number;
  title: string;
  description: string;
  price: number;
  category: string;
  difficulty: string;
  estimatedDuration: number;
  imageUrl: string;
  isActive: boolean;
  createdAt: string;
  instructorUsername: string;
  branchId: number;
  totalLessons: number;
  enrolledStudents: number;
}

@Component({
  selector: 'app-course-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './course-create.component.html',
  styleUrls: ['./course-create.component.css']
})
export class CourseCreateComponent implements OnInit {
  @Output() courseCreated = new EventEmitter<void>();

  isLoading = false;
  errorMessage = '';
  successMessage = '';

  newCourse: CreateCourseRequest = {
    courseName: '',
    description: '',
    thumbnailImageUrl: ''
  };

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private router: Router,
    private coursesService: CoursesService
  ) {}

  ngOnInit() {
    // Check authentication on init
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    
    const user = this.authService.getCurrentUser();
  }

  createCourse() {
    if (!this.validateForm()) {
      return;
    }

    if (!this.authService.isAuthenticated()) {
      this.errorMessage = 'You are not authenticated. Please login again.';
      this.router.navigate(['/login']);
      return;
    }

    const token = sessionStorage.getItem('token');
    if (!token) {
      this.errorMessage = 'No authentication token found. Please login again.';
      this.router.navigate(['/login']);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const headers = { 'Authorization': `Bearer ${token}` };

    this.http.post<Course>('http://localhost:5021/api/Course', this.newCourse, { headers }).subscribe({
      next: (course) => {
        this.successMessage = 'Course created successfully!';
        this.courseCreated.emit();
        // Reload courses to include the new one
        this.coursesService.loadCourses(true);
        setTimeout(() => {
          this.resetForm();
          // Navigate back to course list
          this.router.navigate(['/school-head/courses']);
        }, 1500);
        this.isLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        console.error('Error creating course:', error);
        console.error('Response status:', error.status);
        console.error('Response body:', error.error);
        
        if (error.status === 401) {
          this.errorMessage = 'Authentication failed. Please login again.';
          this.authService.logout();
          this.router.navigate(['/login']);
        } else if (error.error && typeof error.error === 'string') {
          this.errorMessage = error.error;
        } else if (error.error && error.error.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage = 'Failed to create course. Please try again.';
        }
        this.isLoading = false;
      }
    });
  }

  validateForm(): boolean {
    if (!this.newCourse.courseName.trim()) {
      this.errorMessage = 'Course name is required.';
      return false;
    }

    if (!this.newCourse.description.trim()) {
      this.errorMessage = 'Course description is required.';
      return false;
    }

    return true;
  }

  resetForm() {
    this.newCourse = {
      courseName: '',
      description: '',
      thumbnailImageUrl: ''
    };
    this.errorMessage = '';
    this.successMessage = '';
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
