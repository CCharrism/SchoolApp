import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ClassroomService, DashboardData, Classroom, Announcement } from '../../services/classroom.service';
import { ThemeService, SchoolSettings } from '../../services/theme.service';
import { User } from '../../models/auth.models';
import { ChangeDetectorRef } from '@angular/core';

declare var bootstrap: any;

@Component({
  selector: 'app-new-student-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './new-student-dashboard.component.html',
  styleUrl: './new-student-dashboard.component.css'
})
export class NewStudentDashboardComponent implements OnInit {
  currentUser: User | null = null;
  activeMenu = 'dashboard';
  dashboardData: DashboardData | null = null;
  announcements: Announcement[] = [];
  joinClassForm: FormGroup;
  isJoiningClass = false;
  schoolSettings: SchoolSettings | null = null;

  constructor(
    private authService: AuthService,
    private classroomService: ClassroomService,
    private themeService: ThemeService,
    private router: Router,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.joinClassForm = this.fb.group({
      classCode: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (!this.currentUser) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadSchoolTheme();
    this.loadDashboardData();
    this.loadAnnouncements();
  }

  loadSchoolTheme(): void {
    this.themeService.loadSchoolSettings().subscribe({
      next: (settings) => {
        console.log('Student Dashboard - Received theme settings:', settings);
        this.schoolSettings = settings;
        this.themeService.applyTheme(settings);
      },
      error: (error) => {
        console.error('Student Dashboard - Error loading theme settings:', error);
      }
    });
  }

  loadDashboardData(): void {
    console.log('Loading student dashboard data...');
    this.classroomService.getStudentDashboard().subscribe({
      next: (data) => {
        // Map backend response to frontend format
        this.dashboardData = {
          classrooms: data.enrolledClassrooms || data.classrooms || [],
          totalStudents: data.enrolledClasses || 0,
          totalAssignments: data.recentAssignments?.length || 0,
          pendingAssignments: data.pendingAssignments || 0,
          recentClassrooms: data.enrolledClassrooms || data.classrooms || [],
          recentAssignments: data.recentAssignments || [],
          recentAnnouncements: data.recentAnnouncements || [],
          enrolledClasses: data.enrolledClasses,
          upcomingTests: data.upcomingTests,
          enrolledClassrooms: data.enrolledClassrooms
        };
         this.cdr.detectChanges();
        console.log('Student dashboard data loaded:', this.dashboardData);
      },
      error: (error) => {
        console.error('Error loading student dashboard:', error);
        
        // Don't redirect on dashboard load error - user is already authenticated
        // Just log the error and continue with empty dashboard
        if (error.status === 400 || error.status === 401) {
          console.warn('Dashboard load failed, but user authentication is valid. Continuing with empty dashboard.');
          this.dashboardData = {
            classrooms: [],
            totalStudents: 0,
            totalAssignments: 0,
            pendingAssignments: 0,
            recentClassrooms: [],
            recentAssignments: [],
            recentAnnouncements: []
          };
        }
      }
    });
  }

  loadAnnouncements(): void {
    this.classroomService.getAnnouncements().subscribe({
      next: (announcements: Announcement[]) => {
        this.announcements = announcements;
      },
      error: (error: any) => {
        console.error('Error loading announcements:', error);
      }
    });
  }

  setActiveMenu(menu: string, event?: Event): void {
    console.log('Setting active menu to:', menu);
    // Prevent any potential page navigation
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    
    this.activeMenu = menu;
    if (menu === 'announcements') {
      this.loadAnnouncements();
    }
    if (menu === 'classes') {
      console.log('Classes menu selected, current dashboard data:', this.dashboardData);
    }
  }

  openJoinClassModal(): void {
    const modal = new bootstrap.Modal(document.getElementById('joinClassModal'));
    modal.show();
  }

  joinClass(): void {
    if (this.joinClassForm.valid && !this.isJoiningClass) {
      this.isJoiningClass = true;
      
      this.classroomService.joinClassroom(this.joinClassForm.value.classCode).subscribe({
        next: (result) => {
          console.log('Joined classroom:', result);
          
          // Show success message based on server response
          if (result.alreadyEnrolled) {
            alert(`You are already enrolled in ${result.classroomName}`);
          } else {
            alert(`Successfully joined ${result.classroomName || 'classroom'}!`);
          }
          
          this.joinClassForm.reset();
          this.isJoiningClass = false;
          
          // Close modal
          const modal = bootstrap.Modal.getInstance(document.getElementById('joinClassModal'));
          modal?.hide();
          
          // Reload dashboard data to show new classroom
          this.loadDashboardData();
        },
        error: (error) => {
          console.error('Error joining classroom:', error);
          this.isJoiningClass = false;
          
          // Show specific error message
          if (error.status === 400) {
            alert(error.error?.message || error.error || 'Invalid class code or unable to join classroom');
          } else {
            alert('An error occurred while joining the classroom. Please try again.');
          }
        }
      });
    }
  }

  viewClassroomDetails(classroom: Classroom, event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    
    console.log('Navigating to classroom details:', classroom);
    // Navigate to the new classroom detail page
    this.router.navigate(['/student/classroom', classroom.id]);
  }

  getRandomGradient(): string {
    const gradients = [
      'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
      'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
      'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
      'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
      'linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)',
      'linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%)'
    ];
    return gradients[Math.floor(Math.random() * gradients.length)];
  }

  logout(): void {
    console.log('Logging out...');
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good Morning';
    if (hour < 18) return 'Good Afternoon';
    return 'Good Evening';
  }
}
