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
  selector: 'app-new-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './new-teacher-dashboard.component.html',
  styleUrl: './new-teacher-dashboard.component.css'
})
export class NewTeacherDashboardComponent implements OnInit {
  currentUser: User | null = null;
  activeMenu = 'dashboard';
  dashboardData: DashboardData | null = null;
  createAnnouncementForm: FormGroup;
  isCreatingAnnouncement = false;
  announcements: Announcement[] = [];
  schoolSettings: SchoolSettings | null = null;

  constructor(
    private authService: AuthService,
    private classroomService: ClassroomService,
    private themeService: ThemeService,
    private router: Router,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.createAnnouncementForm = this.fb.group({
      classroomId: ['', [Validators.required]],
      title: ['', [Validators.required, Validators.minLength(3)]],
      content: ['', [Validators.required, Validators.minLength(10)]]
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
    console.log('Teacher Dashboard - Loading theme settings...');
    const token = sessionStorage.getItem('token');
    console.log('Teacher Dashboard - Token available:', !!token);
    
    this.themeService.loadSchoolSettings().subscribe({
      next: (settings) => {
        console.log('Teacher Dashboard - Received theme settings:', settings);
        this.schoolSettings = settings;
        this.themeService.applyTheme(settings);
        this.cdr.detectChanges();

      },
      error: (error) => {
        console.error('Teacher Dashboard - Error loading theme settings:', error);
        console.error('Teacher Dashboard - Error details:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          url: error.url
        });
      }
    });
  }

  loadDashboardData(): void {
    console.log('Teacher Dashboard - Loading dashboard data...');
    const token = sessionStorage.getItem('token');
    console.log('Teacher Dashboard - Token for dashboard available:', !!token);
    
    this.classroomService.getTeacherDashboard().subscribe({
      next: (data) => {
        console.log('Teacher Dashboard - Received dashboard data:', data);
        this.dashboardData = data;
        console.log('Teacher dashboard data loaded:', data);
        this.cdr.detectChanges();

      },
      error: (error) => {
        console.error('Error loading teacher dashboard:', error);
        console.error('Dashboard Error details:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          url: error.url
        });
      }
    });
  }

  loadAnnouncements(): void {
    this.classroomService.getAnnouncements().subscribe({
      next: (announcements: Announcement[]) => {
        this.announcements = announcements;
        this.cdr.detectChanges();

      },
      error: (error: any) => {
        console.error('Error loading announcements:', error);
      }
    });
  }

  setActiveMenu(menu: string, event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    
    this.activeMenu = menu;
    if (menu === 'announcements') {
      this.loadAnnouncements();
    }
  }

  openCreateAnnouncementModal(): void {
    const modal = new bootstrap.Modal(document.getElementById('createAnnouncementModal'));
    modal.show();
  }

  createAnnouncement(): void {
    if (this.createAnnouncementForm.valid && !this.isCreatingAnnouncement) {
      this.isCreatingAnnouncement = true;
      const formData = this.createAnnouncementForm.value;

      this.classroomService.createClassroomAnnouncement(
        formData.classroomId,
        {
          title: formData.title,
          content: formData.content
        }
      ).subscribe({
        next: (announcement) => {
          console.log('Announcement created successfully:', announcement);
          this.loadAnnouncements(); // Refresh announcements
          this.loadDashboardData(); // Refresh dashboard data
          this.createAnnouncementForm.reset();
          this.isCreatingAnnouncement = false;
        },
        error: (error) => {
          console.error('Error creating announcement:', error);
          this.isCreatingAnnouncement = false;
        }
      });
    }
  }

  deleteAnnouncement(announcement: Announcement): void {
    if (confirm(`Are you sure you want to delete the announcement "${announcement.title}"?`)) {
      this.classroomService.deleteAnnouncement(announcement.id).subscribe({
        next: () => {
          console.log('Announcement deleted successfully');
          this.loadAnnouncements(); // Refresh announcements
          this.loadDashboardData(); // Refresh dashboard data
        },
        error: (error) => {
          console.error('Error deleting announcement:', error);
        }
      });
    }
  }

  openClassroom(classroom: Classroom, event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.router.navigate(['/teacher/classroom', classroom.id]);
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
