import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ClassroomService, Classroom, Announcement, Assignment } from '../../services/classroom.service';


@Component({
  selector: 'app-classroom-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './classroom-detail.component.html',
  styleUrls: ['./classroom-detail.component.css']
})
export class ClassroomDetailComponent implements OnInit {
  classroom: Classroom | null = null;
  announcements: Announcement[] = [];
  assignments: Assignment[] = [];
  activeTab: string = 'stream';
  error: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private classroomService: ClassroomService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const classroomId = +params['id'];
      if (classroomId) {
        this.loadClassroomData(classroomId);
      }
    });
  }

  loadClassroomData(classroomId: number): void {
    console.log('Loading classroom data for ID:', classroomId);
    
    // Load classroom details
    this.classroomService.getClassroom(classroomId).subscribe({
      next: (classroom) => {
        console.log('Received classroom data:', classroom);
        console.log('Classroom type:', typeof classroom);
        console.log('Classroom truthy?', !!classroom);
        console.log('Classroom keys:', Object.keys(classroom));
        
        // Force assignment and trigger change detection
        this.classroom = { ...classroom }; // Create new object to ensure reference change
        console.log('Component classroom set:', this.classroom);
        console.log('Classroom name:', this.classroom?.name);
        
        // Force change detection
        this.cdr.detectChanges();
        
        this.loadAnnouncements(classroomId);
        this.loadAssignments(classroomId);
      },
      error: (error) => {
        console.error('Error loading classroom:', error);
        console.error('Error details:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          url: error.url
        });
        this.error = 'Failed to load classroom details';
      }
    });
  }

  loadAnnouncements(classroomId: number): void {
    console.log('Loading announcements for classroom:', classroomId);
    this.classroomService.getClassroomAnnouncements(classroomId).subscribe({
      next: (announcements) => {
        console.log('Received announcements:', announcements);
        this.announcements = announcements;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading announcements:', error);
        console.error('Announcements error details:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          url: error.url
        });
      }
    });
  }

  loadAssignments(classroomId: number): void {
    console.log('Loading assignments for classroom:', classroomId);
    this.classroomService.getClassroomAssignments(classroomId).subscribe({
      next: (assignments) => {
        console.log('Received assignments:', assignments);
        this.assignments = assignments;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading assignments:', error);
        console.error('Assignments error details:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          url: error.url
        });
      }
    });
  }

  setActiveTab(tab: string, event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.activeTab = tab;
  }

  goBack(): void {
    this.router.navigate(['/student/dashboard']);
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase();
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' at ' + date.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
  }

  getClassroomThemeColor(): string {
    // Generate a consistent color based on classroom name
    const colors = ['#1976d2', '#388e3c', '#f57c00', '#7b1fa2', '#d32f2f', '#0288d1'];
    const hash = this.classroom?.name.split('').reduce((a, b) => {
      a = ((a << 5) - a) + b.charCodeAt(0);
      return a & a;
    }, 0) || 0;
    return colors[Math.abs(hash) % colors.length];
  }
}
