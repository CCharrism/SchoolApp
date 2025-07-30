import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ClassroomService, DashboardData, Classroom, Announcement } from '../../services/classroom.service';
import { User } from '../../models/auth.models';

declare var bootstrap: any;

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrl: './teacher-dashboard.component.css'
})
export class TeacherDashboardComponent implements OnInit {
  currentUser: User | null = null;
  activeMenu = 'dashboard';
  dashboardData: DashboardData | null = null;
  createAnnouncementForm: FormGroup;
  isCreatingAnnouncement = false;
  isLoading = false;
  announcements: Announcement[] = [];

  private gradients = [
    'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
    'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
    'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
    'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
    'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
    'linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)',
    'linear-gradient(135deg, #a18cd1 0%, #fbc2eb 100%)'
  ];

  constructor(
    private authService: AuthService,
    private classroomService: ClassroomService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.createAnnouncementForm = this.fb.group({
      classroomId: ['', [Validators.required]],
      title: ['', [Validators.required, Validators.minLength(3)]],
      content: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadDashboardData();
    this.loadAnnouncements();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    
    this.classroomService.getTeacherDashboard().subscribe({
      next: (data) => {
        this.dashboardData = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
        this.isLoading = false;
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

  setActiveMenu(menu: string): void {
    this.activeMenu = menu;
    if (menu === 'announcements') {
      this.loadAnnouncements();
    }
  }

  getRandomGradient(): string {
    return this.gradients[Math.floor(Math.random() * this.gradients.length)];
  }

  openCreateAnnouncementModal(): void {
    const modal = new bootstrap.Modal(document.getElementById('createAnnouncementModal'));
    modal.show();
  }

  createAnnouncement(): void {
    if (this.createAnnouncementForm.valid && !this.isCreatingAnnouncement) {
      this.isCreatingAnnouncement = true;
      const formData = this.createAnnouncementForm.value;

      this.classroomService.createAnnouncement(formData).subscribe({
        next: (announcement) => {
          console.log('Announcement created successfully:', announcement);
          this.loadAnnouncements(); // Refresh announcements
          this.loadDashboardData(); // Refresh dashboard data
          this.createAnnouncementForm.reset();
          this.isCreatingAnnouncement = false;
          
          // Close modal
          const modal = bootstrap.Modal.getInstance(document.getElementById('createAnnouncementModal'));
          modal?.hide();
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

  openClassroom(classroom: Classroom): void {
    console.log('Opening classroom:', classroom);
    // Navigate to classroom details or implement classroom view
  }

  viewClassroomDetails(classroom: Classroom): void {
    console.log('Viewing classroom details:', classroom);
    // Implement classroom details view
  }

  manageStudents(classroom: Classroom): void {
    console.log('Managing students for classroom:', classroom);
    // Implement student management
  }

  openCreateAssignmentModal(): void {
    console.log('Opening create assignment modal');
    // Implement assignment creation
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
