import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { TeacherManagementComponent } from '../teachers/teacher-management.component';
import { StudentManagementComponent } from '../students/student-management.component';
import { ClassroomManagementComponent } from '../classrooms/classroom-management.component';

interface User {
  username: string;
  role: string;
}

interface SchoolSettings {
  id: number;
  schoolId: number;
  schoolDisplayName: string;
  themeColor: string;
  logoImageUrl?: string;
  navigationType: string;
}

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
  selector: 'app-school-head-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, TeacherManagementComponent, StudentManagementComponent, ClassroomManagementComponent],
  templateUrl: './school-head-dashboard.component.html',
  styleUrls: ['./school-head-dashboard.component.css']
})
export class SchoolHeadDashboardComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  schoolSettings: SchoolSettings | null = null;
  schoolName: string = '';
  branchName: string = '';
  activeMenu: string = 'dashboard';
  currentTime: Date = new Date();
  courses: Course[] = [];
  selectedCourse: Course | null = null;
  private timeInterval: any;
  isLoadingSettings = false;
  isLoadingCourses = false;

  // Sample data for dashboard
  quickStats = [
    { title: 'Total Courses', value: '0', change: '+0%', icon: 'fa-book' },
    { title: 'Total Lessons', value: '0', change: '+0%', icon: 'fa-play-circle' },
    { title: 'Active Students', value: '0', change: '+0%', icon: 'fa-users' },
    { title: 'Course Views', value: '0', change: '+0%', icon: 'fa-eye' }
  ];

  recentActivities = [
    { message: 'Welcome to your School Head Dashboard', time: 'Just now', icon: 'fa-info-circle' },
    { message: 'Start by creating your first course', time: '1 minute ago', icon: 'fa-plus-circle' },
    { message: 'Upload course materials and videos', time: '2 minutes ago', icon: 'fa-upload' }
  ];

  currentRoute: string = '';

  constructor(
    private http: HttpClient,
    public router: Router
  ) {
    // Update time every second
    this.timeInterval = setInterval(() => this.currentTime = new Date(), 1000);
    
    // Detect current route
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.currentRoute = event.url;
    });
  }

  ngOnInit() {
    this.currentRoute = this.router.url; // Set initial route
    this.loadUserInfo();
    this.loadSchoolSettings();
    // Removed loadCourses() - individual components handle their own data
  }

  ngOnDestroy() {
    if (this.timeInterval) {
      clearInterval(this.timeInterval);
    }
  }

  loadUserInfo() {
    const userData = sessionStorage.getItem('user');
    if (userData) {
      this.currentUser = JSON.parse(userData);
    }
  }

  loadSchoolSettings() {
    this.isLoadingSettings = true;
    const token = sessionStorage.getItem('token');
    
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    this.http.get<SchoolSettings>('http://localhost:5021/api/SchoolSettings', { headers })
      .subscribe({
        next: (settings) => {
          console.log('School Head Dashboard - Received settings:', settings);
          this.schoolSettings = settings;
          this.schoolName = settings.schoolDisplayName;
          
          if (settings.themeColor) {
            console.log('School Head Dashboard - Applying theme color:', settings.themeColor);
            this.updateThemeColor(settings.themeColor);
          }
        },
        error: (error) => {
          console.error('School Head Dashboard - Error loading school settings:', error);
          this.schoolSettings = null;
        },
        complete: () => {
          this.isLoadingSettings = false;
        }
      });
  }

  updateQuickStats() {
    const totalCourses = this.courses.filter(c => c.isActive).length;
    const totalLessons = this.courses.reduce((sum, course) => sum + course.lessonCount, 0);
    
    this.quickStats[0].value = totalCourses.toString();
    this.quickStats[1].value = totalLessons.toString();
  }

  updateThemeColor(color: string) {
    console.log('School Head Dashboard - updateThemeColor called with:', color);
    if (!color) {
      console.log('School Head Dashboard - No color provided, skipping theme update');
      return;
    }
    
    // Ensure color has # prefix
    const formattedColor = color.startsWith('#') ? color : `#${color}`;
    console.log('School Head Dashboard - Formatted color:', formattedColor);
    
    // Set CSS custom property with high priority
    document.documentElement.style.setProperty('--primary-color', formattedColor, 'important');
    console.log('School Head Dashboard - Set CSS variable --primary-color to:', formattedColor);
    
    // Apply comprehensive theming with aggressive timing strategy
    const applyStyles = () => {
      console.log('School Head Dashboard - Applying comprehensive styles with color:', formattedColor);
      
      // Remove any existing conflicting styles first
      const existingStyleElement = document.getElementById('school-head-theme-override');
      if (existingStyleElement) {
        existingStyleElement.remove();
      }
      
      // Create a style element to force theme application
      const styleElement = document.createElement('style');
      styleElement.id = 'school-head-theme-override';
      styleElement.innerHTML = `
        /* School Head Dashboard Theme Override */
        :root {
          --primary-color: ${formattedColor} !important;
        }
        
        /* Navbar theming */
        .navbar, .navbar-brand, .navbar-light {
          background-color: ${formattedColor} !important;
          border-color: ${formattedColor} !important;
        }
        
        .navbar .navbar-brand, .navbar .nav-link {
          color: white !important;
        }
        
        /* Sidebar theming */
        .sidebar {
          background-color: ${formattedColor} !important;
        }
        
        .sidebar .nav-link {
          color: white !important;
        }
        
        .sidebar .nav-link.active {
          background-color: rgba(255, 255, 255, 0.2) !important;
        }
        
        /* Button theming */
        .btn-success, .btn-primary, .quick-action-btn {
          background-color: ${formattedColor} !important;
          border-color: ${formattedColor} !important;
        }
        
        /* Badge theming */
        .badge-success, .school-badge {
          background-color: ${formattedColor} !important;
        }
        
        /* Stat cards */
        .stat-card, .quick-stat-card {
          border-left: 4px solid ${formattedColor} !important;
        }
        
        /* Activity icons */
        .activity-icon {
          background-color: ${formattedColor} !important;
        }
        
        /* Logo border */
        .school-logo-img, .navbar-logo {
          border: 2px solid ${formattedColor} !important;
        }
      `;
      
      document.head.appendChild(styleElement);
      console.log('School Head Dashboard - Injected comprehensive theme styles');
      
      // Also apply direct DOM styling as backup
      const elementsToStyle = [
        { selector: '.navbar', property: 'backgroundColor' },
        { selector: '.sidebar', property: 'backgroundColor' },
        { selector: '.btn-success', property: 'backgroundColor' },
        { selector: '.btn-primary', property: 'backgroundColor' },
        { selector: '.quick-action-btn', property: 'backgroundColor' },
        { selector: '.badge-success', property: 'backgroundColor' },
        { selector: '.school-badge', property: 'backgroundColor' },
        { selector: '.activity-icon', property: 'backgroundColor' }
      ];
      
      elementsToStyle.forEach(({ selector, property }) => {
        const elements = document.querySelectorAll(selector);
        elements.forEach((element) => {
          (element as HTMLElement).style.setProperty(property, formattedColor, 'important');
          if (property === 'backgroundColor') {
            (element as HTMLElement).style.setProperty('border-color', formattedColor, 'important');
          }
        });
      });
      
      console.log('School Head Dashboard - Applied direct DOM styling to', elementsToStyle.length, 'element types');
    };

    // Apply styles with multiple timing strategies to ensure they stick
    applyStyles(); // Immediate
    setTimeout(applyStyles, 50);   // Quick follow-up
    setTimeout(applyStyles, 200);  // Medium delay for dynamic content
    setTimeout(applyStyles, 500);  // Long delay for any async loading
    setTimeout(applyStyles, 1000); // Final application
    
    console.log('School Head Dashboard - Scheduled theme application at multiple intervals');
  }

  setActiveMenu(menu: string) {
    this.activeMenu = menu;
  }

  getGreeting() {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good Morning';
    if (hour < 17) return 'Good Afternoon';
    return 'Good Evening';
  }

  logout() {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
    this.router.navigate(['/']);
  }

  viewCourse(course: Course) {
    this.selectedCourse = course;
    this.activeMenu = 'course-view';
  }

  backToCourses() {
    this.selectedCourse = null;
    this.activeMenu = 'courses';
  }

  getActiveCourseCount(): number {
    return this.courses.filter(c => c.isActive).length;
  }

  getTotalLessons(): number {
    return this.courses.reduce((sum, course) => sum + course.lessonCount, 0);
  }

  onCourseCreated() {
    // Navigate to courses view to show the new course
    this.activeMenu = 'course-list';
    // Course list component will handle its own data refresh
  }

  refreshData() {
    // Refresh dashboard-specific data only
    this.loadSchoolSettings();
    // Removed loadCourses() - let individual components handle their data
  }
}
