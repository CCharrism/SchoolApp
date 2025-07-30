import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { CourseService, Course, Assignment, Activity } from '../services/course.service';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h1>Teacher Dashboard</h1>
        <p>Welcome back! Manage your classes and students.</p>
      </div>

      <div class="dashboard-grid">
        <!-- Quick Stats -->
        <div class="stats-section">
          <div class="stat-card">
            <h3>My Classes</h3>
            <div class="stat-number">{{ classesCount }}</div>
          </div>
          <div class="stat-card">
            <h3>Total Students</h3>
            <div class="stat-number">{{ studentsCount }}</div>
          </div>
          <div class="stat-card">
            <h3>Assignments Due</h3>
            <div class="stat-number">{{ assignmentsDue }}</div>
          </div>
        </div>

        <!-- My Classrooms -->
        <div class="classrooms-section">
          <h2>My Classrooms</h2>
          <div class="classroom-grid">
            <div class="classroom-card" *ngFor="let classroom of classrooms">
              <div class="classroom-header">
                <h3>{{ classroom.courseName }}</h3>
                <span class="subject-badge">{{ classroom.subject }}</span>
              </div>
              <div class="classroom-info">
                <p>{{ classroom.studentsCount }} students</p>
                <p>Room: {{ classroom.roomNumber }}</p>
                <p>{{ classroom.grade }}</p>
              </div>
              <div class="classroom-actions">
                <button class="btn-primary" (click)="enterClassroom(classroom)">Enter Classroom</button>
                <button class="btn-secondary" (click)="manageClassroom(classroom)">Manage</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Recent Activity -->
        <div class="activity-section">
          <h2>Recent Activity</h2>
          <div class="activity-list">
            <div class="activity-item" *ngFor="let activity of recentActivity">
              <div class="activity-icon">📝</div>
              <div class="activity-content">
                <p>{{ activity.description }}</p>
                <span class="activity-time">{{ activity.time }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .dashboard-header {
      margin-bottom: 30px;
    }

    .dashboard-header h1 {
      color: #333;
      margin: 0;
    }

    .dashboard-header p {
      color: #666;
      margin: 5px 0 0 0;
    }

    .dashboard-grid {
      display: grid;
      gap: 30px;
    }

    .stats-section {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 20px;
    }

    .stat-card {
      background: white;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      text-align: center;
    }

    .stat-card h3 {
      margin: 0 0 10px 0;
      color: #666;
      font-size: 14px;
      text-transform: uppercase;
    }

    .stat-number {
      font-size: 36px;
      font-weight: bold;
      color: #2196F3;
    }

    .classroom-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }

    .classroom-card {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      padding: 20px;
    }

    .classroom-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 15px;
    }

    .classroom-header h3 {
      margin: 0;
      color: #333;
    }

    .subject-badge {
      background: #2196F3;
      color: white;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
    }

    .classroom-info p {
      margin: 5px 0;
      color: #666;
    }

    .classroom-actions {
      display: flex;
      gap: 10px;
      margin-top: 15px;
    }

    .btn-primary, .btn-secondary {
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
    }

    .btn-primary {
      background: #2196F3;
      color: white;
    }

    .btn-secondary {
      background: #f5f5f5;
      color: #333;
    }

    .activity-list {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .activity-item {
      display: flex;
      align-items: center;
      padding: 15px 20px;
      border-bottom: 1px solid #eee;
    }

    .activity-item:last-child {
      border-bottom: none;
    }

    .activity-icon {
      margin-right: 15px;
      font-size: 20px;
    }

    .activity-content p {
      margin: 0;
      color: #333;
    }

    .activity-time {
      font-size: 12px;
      color: #999;
    }
  `]
})
export class TeacherDashboardComponent implements OnInit {
  classesCount = 0;
  studentsCount = 0;
  assignmentsDue = 0;
  teacherName = '';

  classrooms: Course[] = [];
  recentActivity: Activity[] = [];

  constructor(
    private authService: AuthService,
    private courseService: CourseService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser && currentUser.role === 'Teacher') {
      this.teacherName = currentUser.fullName || currentUser.username;
      
      // Load teacher's courses
      this.courseService.getTeacherCourses(currentUser.id).subscribe(courses => {
        this.classrooms = courses;
        this.classesCount = courses.length;
        this.studentsCount = courses.reduce((total, course) => total + course.studentsCount, 0);
      });

      // Load recent activity
      this.courseService.getRecentActivity(currentUser.id, 'Teacher').subscribe(activity => {
        this.recentActivity = activity;
      });

      // Load assignments due count
      this.courseService.getTeacherAssignments(currentUser.id).subscribe(assignments => {
        this.assignmentsDue = assignments.length;
      });
    } else {
      // Fallback to mock data if no user or wrong role
      this.setMockData();
    }
  }

  private setMockData(): void {
    this.classrooms = [
      {
        id: 1,
        courseName: 'Mathematics 10A',
        description: 'Advanced Mathematics for Grade 10',
        teacherId: 2,
        teacherName: 'John Smith',
        subject: 'Mathematics',
        grade: 'Grade 10-A',
        studentsCount: 25,
        roomNumber: 'Room 101',
        isActive: true,
        createdAt: '2025-01-15T10:30:00Z'
      },
      {
        id: 2,
        courseName: 'Advanced Algebra',
        description: 'Advanced Algebra concepts',
        teacherId: 2,
        teacherName: 'John Smith',
        subject: 'Mathematics',
        grade: 'Grade 11-A',
        studentsCount: 18,
        roomNumber: 'Room 102',
        isActive: true,
        createdAt: '2025-01-16T11:45:00Z'
      }
    ];

    this.recentActivity = [
      {
        id: 1,
        description: 'New assignment submitted by Alice Johnson',
        time: '2 hours ago',
        type: 'submission'
      },
      {
        id: 2,
        description: 'Quiz scheduled for Mathematics 10A',
        time: '5 hours ago',
        type: 'quiz'
      },
      {
        id: 3,
        description: 'Parent meeting request from John Smith',
        time: '1 day ago',
        type: 'meeting'
      }
    ];

    this.classesCount = this.classrooms.length;
    this.studentsCount = this.classrooms.reduce((total, classroom) => total + classroom.studentsCount, 0);
    this.assignmentsDue = 3;
  }

  enterClassroom(course: Course): void {
    // Navigate to classroom view
    console.log('Entering classroom:', course.courseName);
  }

  manageClassroom(course: Course): void {
    // Navigate to classroom management
    console.log('Managing classroom:', course.courseName);
  }
}
