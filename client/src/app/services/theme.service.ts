import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';

export interface SchoolSettings {
  id: number;
  schoolId: number;
  schoolDisplayName: string;
  themeColor: string;
  logoImageUrl?: string;
  navigationType: string;
}

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private apiUrl = 'http://localhost:5021/api';
  private currentTheme = new BehaviorSubject<SchoolSettings | null>(null);
  
  public theme$ = this.currentTheme.asObservable();

  constructor(private http: HttpClient) {}

  loadSchoolSettings(): Observable<SchoolSettings> {
    const token = sessionStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    return this.http.get<SchoolSettings>(`${this.apiUrl}/SchoolSettings`, { headers });
  }

  applyTheme(settings: SchoolSettings): void {
    console.log('ThemeService - Applying theme:', settings);
    this.currentTheme.next(settings);

    if (settings.themeColor) {
      this.updateThemeColor(settings.themeColor);
    }
  }

  private updateThemeColor(color: string): void {
    console.log('ThemeService - updateThemeColor called with:', color);
    if (!color) {
      console.log('ThemeService - No color provided, skipping theme update');
      return;
    }
    
    // Ensure color has # prefix
    const formattedColor = color.startsWith('#') ? color : `#${color}`;
    console.log('ThemeService - Formatted color:', formattedColor);
    
    // Set CSS custom property with high priority
    document.documentElement.style.setProperty('--primary-color', formattedColor, 'important');
    console.log('ThemeService - Set CSS variable --primary-color to:', formattedColor);
    
    // Apply comprehensive theming
    this.applyComprehensiveStyles(formattedColor);
  }

  private applyComprehensiveStyles(color: string): void {
    console.log('ThemeService - Applying comprehensive styles with color:', color);
    
    // Remove any existing conflicting styles first
    const existingStyleElement = document.getElementById('dynamic-theme-override');
    if (existingStyleElement) {
      existingStyleElement.remove();
    }
    
    // Create a style element to force theme application
    const styleElement = document.createElement('style');
    styleElement.id = 'dynamic-theme-override';
    styleElement.innerHTML = `
      /* Dynamic Theme Override */
      :root {
        --primary-color: ${color} !important;
        --primary-dark: ${this.darkenColor(color, 10)} !important;
        --primary-light: ${this.lightenColor(color, 10)} !important;
        --navbar-bg: linear-gradient(135deg, ${color} 0%, ${this.darkenColor(color, 10)} 100%) !important;
        --sidebar-bg: linear-gradient(135deg, ${color} 0%, ${this.darkenColor(color, 10)} 100%) !important;
      }
      
      /* Navbar theming */
      .navbar {
        background: var(--navbar-bg) !important;
        border-bottom: 3px solid ${this.darkenColor(color, 10)} !important;
      }
      
      .navbar-brand, .navbar .nav-link {
        color: white !important;
      }
      
      /* Sidebar theming */
      .sidebar {
        background: var(--sidebar-bg) !important;
      }
      
      .sidebar .nav-link {
        color: rgba(255, 255, 255, 0.8) !important;
      }
      
      .sidebar .nav-link:hover {
        background-color: rgba(255, 255, 255, 0.1) !important;
        color: white !important;
      }
      
      .sidebar .nav-link.active {
        background-color: white !important;
        color: ${color} !important;
      }
      
      /* Button theming */
      .btn-primary {
        background-color: ${color} !important;
        border-color: ${color} !important;
      }
      
      .btn-primary:hover {
        background-color: ${this.darkenColor(color, 10)} !important;
        border-color: ${this.darkenColor(color, 10)} !important;
      }
      
      .btn-outline-primary {
        border-color: ${color} !important;
        color: ${color} !important;
      }
      
      .btn-outline-primary:hover {
        background-color: ${color} !important;
        border-color: ${color} !important;
      }
      
      /* Stats icons */
      .stats-icon.bg-primary {
        background-color: ${color} !important;
      }
      
      /* Form controls focus */
      .form-control:focus {
        border-color: ${color} !important;
        box-shadow: 0 0 0 0.2rem ${this.hexToRgba(color, 0.25)} !important;
      }
      
      /* Badge theming */
      .badge.bg-primary {
        background-color: ${color} !important;
      }
      
      /* User info card */
      .user-info-card {
        background: rgba(255, 255, 255, 0.1) !important;
        border: 1px solid rgba(255, 255, 255, 0.2) !important;
      }
      
      /* Classroom header gradients */
      .classroom-header {
        background: ${color} !important;
      }
      
      /* Modal header */
      .modal-header {
        background: linear-gradient(135deg, ${color} 0%, ${this.darkenColor(color, 10)} 100%) !important;
      }
    `;
    
    document.head.appendChild(styleElement);
    console.log('ThemeService - Injected comprehensive theme styles');
  }

  private darkenColor(hex: string, percent: number): string {
    const num = parseInt(hex.replace("#", ""), 16);
    const amt = Math.round(2.55 * percent);
    const R = (num >> 16) - amt;
    const G = (num >> 8 & 0x00FF) - amt;
    const B = (num & 0x0000FF) - amt;
    return "#" + (0x1000000 + (R < 255 ? R < 1 ? 0 : R : 255) * 0x10000 +
      (G < 255 ? G < 1 ? 0 : G : 255) * 0x100 +
      (B < 255 ? B < 1 ? 0 : B : 255)).toString(16).slice(1);
  }

  private lightenColor(hex: string, percent: number): string {
    const num = parseInt(hex.replace("#", ""), 16);
    const amt = Math.round(2.55 * percent);
    const R = (num >> 16) + amt;
    const G = (num >> 8 & 0x00FF) + amt;
    const B = (num & 0x0000FF) + amt;
    return "#" + (0x1000000 + (R < 255 ? R < 1 ? 0 : R : 255) * 0x10000 +
      (G < 255 ? G < 1 ? 0 : G : 255) * 0x100 +
      (B < 255 ? B < 1 ? 0 : B : 255)).toString(16).slice(1);
  }

  private hexToRgba(hex: string, alpha: number): string {
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    if (!result) return `rgba(146, 222, 139, ${alpha})`;
    
    const r = parseInt(result[1], 16);
    const g = parseInt(result[2], 16);
    const b = parseInt(result[3], 16);
    
    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
  }

  getCurrentTheme(): SchoolSettings | null {
    return this.currentTheme.value;
  }
}
