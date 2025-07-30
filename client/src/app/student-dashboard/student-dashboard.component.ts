import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { CourseService, Course, Assignment, Activity } from '../services/course.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h1>Student Dashboard</h1>
        <p>Welcome back! Stay up to date with your classes and assignments.</p>
      </div>

      <div class="dashboard-grid">
        <!-- Quick Stats -->
        <div class="stats-section">
          <div class="stat-card">
            <h3>Enrolled Classes</h3>
            <div class="stat-number">{{ enrolledClasses }}</div>
          </div>
          <div class="stat-card">
            <h3>Pending Assignments</h3>
            <div class="stat-number">{{ pendingAssignments }}</div>
          </div>
          <div class="stat-card">
            <h3>Upcoming Tests</h3>
            <div class="stat-number">{{ upcomingTests }}</div>
          </div>
        </div>

        <!-- My Classes -->
        <div class="classes-section">
          <h2>My Classes</h2>
          <div class="class-grid">
            <div class="class-card" *ngFor="let classItem of myClasses">
              <div class="class-header">
                <h3>{{ classItem.courseName }}</h3>
                <span class="teacher-name">{{ classItem.teacherName }}</span>
              </div>
              <div class="class-info">
                <p>Room: {{ classItem.roomNumber }}</p>
                <p>Grade: {{ classItem.grade }}</p>
                <p>Subject: {{ classItem.subject }}</p>
              </div>
              <div class="class-status">
                <span class="status-badge" [class]="classItem.isActive ? 'status-active' : 'status-inactive'">
                  {{ classItem.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Recent Assignments -->
        <div class="assignments-section">
          <h2>Recent Assignments</h2>
          <div class="assignment-list">
            <div class="assignment-item" *ngFor="let assignment of recentAssignments">
              <div class="assignment-header">
                <h4>{{ assignment.title }}</h4>
                <span class="due-date">Due: {{ assignment.dueDate | date:'short' }}</span>
              </div>
              <div class="assignment-info">
                <p>{{ assignment.courseName }}</p>
                <span class="status-badge" [class]="'status-' + assignment.status">
                  {{ assignment.status }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Announcements -->
        <div class="announcements-section">
          <h2>Announcements</h2>
          <div class="announcement-list">
            <div class="announcement-item" *ngFor="let announcement of announcements">
              <div class="announcement-icon">📢</div>
              <div class="announcement-content">
                <h4>{{ announcement.title }}</h4>
                <p>{{ announcement.message }}</p>
                <span class="announcement-time">{{ announcement.time }}</span>
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
      color: #4CAF50;
    }

    .class-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
    }

    .class-card {
      background: white;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      border-left: 4px solid #2196F3;
    }

    .class-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 10px;
    }

    .class-header h3 {
      margin: 0;
      color: #333;
    }

    .teacher-name {
      font-size: 14px;
      color: #666;
    }

    .class-info p {
      margin: 5px 0;
      color: #666;
    }

    .status-badge {
      display: inline-block;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: bold;
      text-transform: uppercase;
    }

    .status-active {
      background: #e8f5e8;
      color: #4caf50;
    }

    .status-inactive {
      background: #fff3e0;
      color: #ff9800;
    }

    .status-pending {
      background: #fff3e0;
      color: #ff9800;
    }

    .status-graded {
      background: #e8f5e8;
      color: #4caf50;
    }

    .assignment-list {
      display: grid;
      gap: 15px;
    }

    .assignment-item {
      background: white;
      padding: 15px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      border-left: 4px solid #FF9800;
    }

    .assignment-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 10px;
    }

    .assignment-header h4 {
      margin: 0;
      color: #333;
    }

    .due-date {
      font-size: 14px;
      color: #ff9800;
      font-weight: bold;
    }

    .assignment-info {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .assignment-info p {
      margin: 0;
      color: #666;
    }

    .announcement-list {
      display: grid;
      gap: 15px;
    }

    .announcement-item {
      background: white;
      padding: 15px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      display: flex;
      align-items: flex-start;
    }

    .announcement-icon {
      margin-right: 15px;
      font-size: 20px;
    }

    .announcement-content h4 {
      margin: 0 0 5px 0;
      color: #333;
    }

    .announcement-content p {
      margin: 0 0 5px 0;
      color: #666;
    }

    .announcement-time {
      font-size: 12px;
      color: #999;
    }
  `]
})
export class StudentDashboardComponent implements OnInit {
  enrolledClasses = 0;
  pendingAssignments = 0;
  upcomingTests = 0;
  studentName = '';

  myClasses: Course[] = [];
  recentAssignments: Assignment[] = [];
  recentActivity: Activity[] = [];

  announcements = [
    {
      title: 'Mid-term Examinations',
      message: 'Mid-term exams will be held from February 1-10. Please check your schedule.',
      time: '2 hours ago'
    },
    {
      title: 'Library Hours Extended',
      message: 'The library will now be open until 8 PM on weekdays.',
      time: '1 day ago'
    },
    {
      title: 'Science Fair Registration',
      message: 'Registration for the annual science fair is now open.',
      time: '3 days ago'
    }
  ];

  constructor(
    private authService: AuthService,
    private courseService: CourseService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser && currentUser.role === 'Student') {
      this.studentName = currentUser.fullName || currentUser.username;
      
      // Load student's courses
      this.courseService.getStudentCourses(currentUser.id).subscribe(courses => {
        this.myClasses = courses;
        this.enrolledClasses = courses.length;
      });

      // Load recent assignments
      this.courseService.getStudentAssignments(currentUser.id).subscribe(assignments => {
        this.recentAssignments = assignments;
        this.pendingAssignments = assignments.filter(a => a.status === 'pending').length;
      });

      // Load recent activity
      this.courseService.getRecentActivity(currentUser.id, 'Student').subscribe(activity => {
        this.recentActivity = activity;
      });

      this.upcomingTests = 2; // Mock data for now
    } else {
      // Fallback to mock data if no user or wrong role
      this.setMockData();
    }
  }

  private setMockData(): void {
    this.myClasses = [
      {
        id: 1,
        courseName: 'Mathematics 10A',
        description: 'Advanced Mathematics for Grade 10',
        teacherId: 2,
        teacherName: 'Mr. John Doe',
        subject: 'Mathematics',
        grade: 'Grade 10-A',
        studentsCount: 25,
        roomNumber: 'Room 101',
        isActive: true,
        createdAt: '2025-01-15T10:30:00Z'
      },
      {
        id: 2,
        courseName: 'English Literature',
        description: 'Advanced English Literature',
        teacherId: 3,
        teacherName: 'Ms. Jane Smith',
        subject: 'English',
        grade: 'Grade 10-A',
        studentsCount: 22,
        roomNumber: 'Room 205',
        isActive: true,
        createdAt: '2025-01-16T11:45:00Z'
      }
    ];

    this.recentAssignments = [
      {
        id: 1,
        title: 'Algebra Practice Set 5',
        description: 'Complete problems 1-20 from chapter 5',
        courseId: 1,
        courseName: 'Mathematics 10A',
        teacherId: 2,
        teacherName: 'Mr. John Doe',
        dueDate: '2025-01-25T23:59:00Z',
        submittedDate: null,
        status: 'pending',
        maxPoints: 100,
        earnedPoints: null,
        instructions: 'Show all work and submit as PDF',
        attachments: [],
        createdAt: '2025-01-20T10:00:00Z'
      },
      {
        id: 2,
        title: 'Essay on Shakespeare',
        description: 'Write a 500-word essay on Hamlet',
        courseId: 2,
        courseName: 'English Literature',
        teacherId: 3,
        teacherName: 'Ms. Jane Smith',
        dueDate: '2025-01-28T23:59:00Z',
        submittedDate: null,
        status: 'pending',
        maxPoints: 100,
        earnedPoints: null,
        instructions: 'Minimum 500 words, MLA format',
        attachments: [],
        createdAt: '2025-01-18T14:30:00Z'
      }
    ];

    this.enrolledClasses = this.myClasses.length;
    this.pendingAssignments = this.recentAssignments.filter(a => a.status === 'pending').length;
    this.upcomingTests = 2;
  }
}
