import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/auth.models';
import { SettingsComponent } from '../settings/settings.component';
import { BranchesComponent } from '../branches/branches.component';

@Component({
  selector: 'app-owner-dashboard',
  standalone: true,
  imports: [CommonModule, SettingsComponent, BranchesComponent],
  templateUrl: './owner-dashboard.component.html',
  styleUrl: './owner-dashboard.component.css'
})
export class OwnerDashboardComponent implements OnInit {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);
  
  currentUser: User | null = null;
  currentTime = new Date();
  activeMenu = 'dashboard';
  schoolName = ''; // Will be set from user data
  schoolSettings: any = null;
  private isLoadingSettings = false;

  // Dashboard stats for school owner
  stats = {
    totalStudents: 245,
    totalTeachers: 18,
    totalClasses: 12,
    activeEvents: 3
  };

  // Recent activities for school
  recentActivities = [
    { id: 1, type: 'student', message: 'New student Emily Johnson enrolled in Grade 5', time: '2 hours ago', icon: 'fa-user-plus' },
    { id: 2, type: 'teacher', message: 'Teacher Mark Wilson updated Math curriculum', time: '4 hours ago', icon: 'fa-chalkboard-teacher' },
    { id: 3, type: 'event', message: 'Science Fair scheduled for next week', time: '6 hours ago', icon: 'fa-calendar' },
    { id: 4, type: 'system', message: 'Monthly attendance report generated', time: '1 day ago', icon: 'fa-chart-bar' }
  ];

  // Quick stats for cards
  quickStats = [
    { title: 'Total Students', value: '245', icon: 'fa-users', color: 'success', change: '+8 new' },
    { title: 'Active Teachers', value: '18', icon: 'fa-chalkboard-teacher', color: 'primary', change: '100%' },
    { title: 'Classes Running', value: '12', icon: 'fa-book-open', color: 'info', change: '+2 new' },
    { title: 'Upcoming Events', value: '3', icon: 'fa-calendar-alt', color: 'warning', change: 'This week' }
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (this.currentUser) {
      this.schoolName = this.currentUser.schoolName || 'Your School';
    }

    // Load school settings
    this.loadSchoolSettings();

    // Update time every second
    setInterval(() => {
      this.currentTime = new Date();
    }, 1000);
  }

  // Add method to refresh settings when returning from settings page
  refreshSettings(): void {
    console.log('🔄 refreshSettings called - reloading and applying settings immediately');
    this.loadSchoolSettings();
  }

  // Add method to handle immediate settings update without API reload
  onSettingsUpdated(newSettings: any): void {
    console.log('🎨 onSettingsUpdated called with:', newSettings);
    // Update local settings immediately
    this.schoolSettings = { ...this.schoolSettings, ...newSettings };
    
    // Apply changes immediately to the UI
    this.applySettings();
    
    // Force change detection to update the view
    this.cdr.detectChanges();
    
    // Also update with a small delay to ensure DOM is ready
    setTimeout(() => {
      this.applySettings();
      this.cdr.detectChanges();
    }, 100);
  }

  // Update the activeMenu setter to refresh settings when returning to dashboard
  setActiveMenu(menu: string): void {
    console.log('🔄 setActiveMenu called:', menu, 'from:', this.activeMenu);
    const previousMenu = this.activeMenu;
    this.activeMenu = menu;
    
    // If returning from settings to dashboard, refresh settings to show changes
    if (previousMenu === 'settings' && menu === 'dashboard') {
      setTimeout(() => {
        this.refreshSettings();
      }, 100);
    }
  }

  loadSchoolSettings(): void {
    console.log('🔄 loadSchoolSettings called, isLoadingSettings:', this.isLoadingSettings);
    
    if (this.isLoadingSettings) {
      console.log('⚠️ Already loading settings, skipping...');
      return;
    }
    
    const token = sessionStorage.getItem('token');
    if (!token) {
      console.log('❌ No token found');
      return;
    }

    this.isLoadingSettings = true;

    this.http.get('http://localhost:5021/api/SchoolSettings', {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: (settings: any) => {
        console.log('✅ School settings loaded:', settings);
        this.schoolSettings = settings;
        this.applySettings();
        this.isLoadingSettings = false;
        // Remove the detectChanges call to prevent change detection loops
        // this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('❌ Error loading settings:', error);
        this.isLoadingSettings = false;
        // Use default settings if loading fails
        this.schoolSettings = {
          schoolDisplayName: this.schoolName,
          navigationType: 'sidebar',
          themeColor: 'DD4470',
          logoImageUrl: ''
        };
        this.applySettings();
      }
    });
  }

  applySettings(): void {
    console.log('🎨 applySettings called with:', this.schoolSettings);
    if (!this.schoolSettings) return;

    // Update school name display
    if (this.schoolSettings.schoolDisplayName) {
      this.schoolName = this.schoolSettings.schoolDisplayName;
    }

    // Apply theme color to sidebar and other elements
    const themeColor = `#${this.schoolSettings.themeColor}`;
    console.log('🎨 Applying theme color:', themeColor);
    this.updateThemeColor(themeColor);
    
    // Force change detection after applying settings
    this.cdr.detectChanges();
  }

  updateThemeColor(color: string): void {
    console.log('🎨 updateThemeColor called with:', color);
    
    // Update CSS custom properties for dynamic theming
    document.documentElement.style.setProperty('--primary-color', color);
    
    // Force immediate update using both setTimeout and immediate application
    const applyStyles = () => {
      console.log('🎨 Applying styles with color:', color);
      
      const sidebar = document.querySelector('.sidebar') as HTMLElement;
      if (sidebar) {
        sidebar.style.backgroundColor = color;
        sidebar.style.setProperty('background-color', color, 'important');
        console.log('✅ Sidebar color updated');
      }

      const navbar = document.querySelector('.navbar') as HTMLElement;
      if (navbar) {
        navbar.style.backgroundColor = color;
        navbar.style.setProperty('background-color', color, 'important');
        console.log('✅ Navbar color updated');
      }

      // Update buttons and other themed elements
      const buttons = document.querySelectorAll('.btn-success, .quick-action-btn');
      buttons.forEach((btn) => {
        (btn as HTMLElement).style.backgroundColor = color;
        (btn as HTMLElement).style.borderColor = color;
        (btn as HTMLElement).style.setProperty('background-color', color, 'important');
        (btn as HTMLElement).style.setProperty('border-color', color, 'important');
      });

      // Update badges
      const badges = document.querySelectorAll('.school-badge');
      badges.forEach((badge) => {
        (badge as HTMLElement).style.backgroundColor = color;
        (badge as HTMLElement).style.setProperty('background-color', color, 'important');
      });

      // Update activity icons
      const activityIcons = document.querySelectorAll('.activity-icon');
      activityIcons.forEach((icon) => {
        (icon as HTMLElement).style.backgroundColor = color;
        (icon as HTMLElement).style.setProperty('background-color', color, 'important');
      });

      // Update stat cards
      const statCards = document.querySelectorAll('.stat-card');
      statCards.forEach((card) => {
        (card as HTMLElement).style.borderLeftColor = color;
        (card as HTMLElement).style.setProperty('border-left-color', color, 'important');
      });

      // Update logo elements
      const logoImgs = document.querySelectorAll('.school-logo-img');
      logoImgs.forEach((img) => {
        (img as HTMLElement).style.borderColor = color;
        (img as HTMLElement).style.setProperty('border-color', color, 'important');
      });

      const logoPlaceholders = document.querySelectorAll('.school-logo-placeholder');
      logoPlaceholders.forEach((placeholder) => {
        (placeholder as HTMLElement).style.backgroundColor = color;
        (placeholder as HTMLElement).style.setProperty('background-color', color, 'important');
      });
      
      console.log('🎨 All styles applied successfully');
    };

    // Apply immediately
    applyStyles();
    
    // Also apply after short delays to catch any dynamically loaded elements
    setTimeout(applyStyles, 50);
    setTimeout(applyStyles, 200);
    setTimeout(applyStyles, 500);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good Morning';
    if (hour < 17) return 'Good Afternoon';
    return 'Good Evening';
  }

  loadDashboardData() {
    console.log('🔄 loadDashboardData called');
    // Refresh dashboard data when branches are updated
    this.loadSchoolSettings();
    // Remove the duplicate refreshSettings call to prevent loops
  }

  getActivityIconClass(type: string): string {
    switch (type) {
      case 'student': return 'text-success';
      case 'teacher': return 'text-primary';
      case 'event': return 'text-info';
      case 'system': return 'text-secondary';
      default: return 'text-muted';
    }
  }
}
