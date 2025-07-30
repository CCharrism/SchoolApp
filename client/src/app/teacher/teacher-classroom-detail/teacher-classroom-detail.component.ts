import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ClassroomService, Classroom, Announcement, Assignment } from '../../services/classroom.service';
@Component({
  selector: 'app-teacher-classroom-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './teacher-classroom-detail.component.html',
  styleUrls: ['./teacher-classroom-detail.component.css']
})
export class TeacherClassroomDetailComponent implements OnInit {
  classroom: Classroom | null = null;
  announcements: Announcement[] = [];
  assignments: Assignment[] = [];
  activeTab: string = 'stream';
  error: string = '';
  
  // Announcement creation
  showAnnouncementForm: boolean = false;
  newAnnouncement = {
    title: '',
    content: ''
  };
  creatingAnnouncement: boolean = false;

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
    // Load classroom details
    this.classroomService.getClassroom(classroomId).subscribe({
      next: (classroom) => {
        console.log('Teacher - Received classroom data:', classroom);
        // Force assignment and trigger change detection
        this.classroom = { ...classroom }; // Create new object to ensure reference change
        console.log('Teacher - Component classroom set:', this.classroom);
        
        // Force change detection
        this.cdr.detectChanges();
        
        this.loadAnnouncements(classroomId);
        this.loadAssignments(classroomId);
      },
      error: (error) => {
        console.error('Error loading classroom:', error);
        this.error = 'Failed to load classroom details';
      }
    });
  }

  loadAnnouncements(classroomId: number): void {
    this.classroomService.getClassroomAnnouncements(classroomId).subscribe({
      next: (announcements) => {
        this.announcements = announcements;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading announcements:', error);
      }
    });
  }

  loadAssignments(classroomId: number): void {
    this.classroomService.getClassroomAssignments(classroomId).subscribe({
      next: (assignments) => {
        this.assignments = assignments;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading assignments:', error);
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
    this.router.navigate(['/teacher/dashboard']);
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

  // Announcement methods
  toggleAnnouncementForm(event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
      
    }
    this.showAnnouncementForm = !this.showAnnouncementForm;
    if (!this.showAnnouncementForm) {
      this.resetAnnouncementForm();
    }
   
  }

  resetAnnouncementForm(): void {
    this.newAnnouncement = {
      title: '',
      content: ''
    };
  }

  createAnnouncement(): void {
  if (!this.classroom || !this.newAnnouncement.title.trim() || !this.newAnnouncement.content.trim()) {
    return;
  }

  this.creatingAnnouncement = true;

  this.classroomService.createClassroomAnnouncement(this.classroom.id, {
    title: this.newAnnouncement.title.trim(),
    content: this.newAnnouncement.content.trim()
  }).subscribe({
    next: (announcement) => {
      // Instead of just unshift, reload announcements from backend:
      this.loadAnnouncements(this.classroom!.id);

      this.resetAnnouncementForm();
      this.showAnnouncementForm = false;
      this.creatingAnnouncement = false;
    },
    error: (error) => {
      console.error('Error creating announcement:', error);
      this.creatingAnnouncement = false;
      alert('Failed to create announcement. Please try again.');
    }
  });
}


  cancelAnnouncement(): void {
    this.resetAnnouncementForm();
    this.showAnnouncementForm = false;
  }
}
