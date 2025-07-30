import { Component, OnInit, inject, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface SchoolSettings {
  id: number;
  schoolId: number;
  schoolDisplayName: string;
  logoImageUrl: string;
  navigationType: string;
  themeColor: string;
  updatedAt: string;
}

@Component({
  selector: 'app-settings',
  imports: [CommonModule, FormsModule],
  template: `
    <div class="settings-container">
      <div class="settings-header mb-4">
        <h2 class="fw-bold" style="color: #A7226F;">School Customization</h2>
        <p class="text-muted">Customize your school's appearance and branding</p>
      </div>

      <div class="row g-4">
        <!-- Settings Panel -->
        <div class="col-lg-6">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body p-4">
              <!-- School Display Name -->
              <div class="mb-4">
                <label class="form-label fw-semibold">School Display Name</label>
                <input 
                  type="text" 
                  class="form-control" 
                  [(ngModel)]="settings.schoolDisplayName"
                  placeholder="Enter school display name"
                  style="border-color: #DD4470;"
                  maxlength="50">
              </div>

              <!-- Navigation Type -->
              <div class="mb-4">
                <label class="form-label fw-semibold">Navigation Style</label>
                <div class="row g-3">
                  <div class="col-6">
                    <div class="nav-option p-3 text-center border rounded" 
                         [class.selected]="settings.navigationType === 'sidebar'"
                         (click)="settings.navigationType = 'sidebar'"
                         style="cursor: pointer;">
                      <i class="fas fa-bars mb-2 d-block" style="font-size: 2rem; color: #A7226F;"></i>
                      <div class="fw-semibold">Sidebar</div>
                      <small class="text-muted">Side navigation</small>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="nav-option p-3 text-center border rounded" 
                         [class.selected]="settings.navigationType === 'topbar'"
                         (click)="settings.navigationType = 'topbar'"
                         style="cursor: pointer;">
                      <i class="fas fa-minus mb-2 d-block" style="font-size: 2rem; color: #A7226F;"></i>
                      <div class="fw-semibold">Top Bar</div>
                      <small class="text-muted">Top navigation</small>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Theme Color -->
              <div class="mb-4">
                <label class="form-label fw-semibold">Theme Color</label>
                <div class="color-palette">
                  <div class="row g-2">
                    <div class="col-3" *ngFor="let color of colorPalette">
                      <div class="color-circle" 
                           [style.background-color]="'#' + color"
                           [class.selected]="settings.themeColor === color"
                           (click)="settings.themeColor = color"
                           [title]="'#' + color">
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Logo Upload -->
              <div class="mb-4">
                <label class="form-label fw-semibold">School Logo</label>
                <div class="logo-upload-section">
                  <input type="file" 
                         class="form-control mb-3" 
                         accept="image/*"
                         (change)="onLogoUpload($event)"
                         #logoInput>
                  <div class="current-logo text-center" *ngIf="settings.logoImageUrl">
                    <img [src]="settings.logoImageUrl" 
                         alt="School Logo" 
                         class="img-thumbnail"
                         style="max-width: 120px; max-height: 120px;">
                    <div class="mt-2">
                      <button class="btn btn-sm btn-outline-danger" 
                              (click)="removeLogo()">
                        <i class="fas fa-trash"></i> Remove Logo
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Save Button -->
              <div class="d-grid">
                <button class="btn btn-lg fw-semibold" 
                        [style.background-color]="'#' + settings.themeColor"
                        style="color: white; border: none;"
                        (click)="saveSettings()"
                        [disabled]="isSaving">
                  <i class="fas fa-save me-2"></i>
                  {{ isSaving ? 'Saving...' : 'Save Settings' }}
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Preview Panel -->
        <div class="col-lg-6">
          <div class="card border-0 shadow-sm">
            <div class="card-body p-4">
              <h5 class="fw-bold mb-3" style="color: #A7226F;">Live Preview</h5>
              
              <!-- Fixed height preview container -->
              <div class="preview-wrapper">
                <!-- Sidebar Preview -->
                <div class="preview-container" *ngIf="settings.navigationType === 'sidebar'">
                  <div class="preview-sidebar" [style.background-color]="'#' + settings.themeColor">
                    <div class="preview-logo mb-3">
                      <img *ngIf="settings.logoImageUrl" 
                           [src]="settings.logoImageUrl" 
                           alt="Logo"
                           class="preview-logo-img">
                      <div *ngIf="!settings.logoImageUrl" class="preview-logo-placeholder">
                        <i class="fas fa-image"></i>
                      </div>
                    </div>
                    <div class="preview-school-name" [title]="settings.schoolDisplayName || 'School Name'">
                      {{ (settings.schoolDisplayName || 'School Name') | slice:0:15 }}{{ (settings.schoolDisplayName || 'School Name').length > 15 ? '...' : '' }}
                    </div>
                    <div class="preview-nav-items">
                      <div class="preview-nav-item active">Dashboard</div>
                      <div class="preview-nav-item">Students</div>
                      <div class="preview-nav-item">Teachers</div>
                      <div class="preview-nav-item">Settings</div>
                    </div>
                  </div>
                  <div class="preview-content">
                    <div class="preview-main-content">
                      <div class="preview-header">Dashboard Content</div>
                      <div class="preview-cards">
                        <div class="preview-card"></div>
                        <div class="preview-card"></div>
                        <div class="preview-card"></div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Topbar Preview -->
                <div class="preview-container" *ngIf="settings.navigationType === 'topbar'">
                  <div class="preview-topbar" [style.background-color]="'#' + settings.themeColor">
                    <div class="preview-topbar-content">
                      <div class="preview-logo-section">
                        <img *ngIf="settings.logoImageUrl" 
                             [src]="settings.logoImageUrl" 
                             alt="Logo"
                             class="preview-topbar-logo">
                        <div *ngIf="!settings.logoImageUrl" class="preview-topbar-logo-placeholder">
                          <i class="fas fa-image"></i>
                        </div>
                        <span class="preview-school-name-top" [title]="settings.schoolDisplayName || 'School Name'">
                          {{ (settings.schoolDisplayName || 'School Name') | slice:0:20 }}{{ (settings.schoolDisplayName || 'School Name').length > 20 ? '...' : '' }}
                        </span>
                      </div>
                      <div class="preview-nav-horizontal">
                        <span class="preview-nav-item-top active">Dashboard</span>
                        <span class="preview-nav-item-top">Students</span>
                        <span class="preview-nav-item-top">Teachers</span>
                        <span class="preview-nav-item-top">Settings</span>
                      </div>
                    </div>
                  </div>
                  <div class="preview-content-full">
                    <div class="preview-main-content">
                      <div class="preview-header">Dashboard Content</div>
                      <div class="preview-cards">
                        <div class="preview-card"></div>
                        <div class="preview-card"></div>
                        <div class="preview-card"></div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .settings-container {
      padding: 2rem;
      background: linear-gradient(135deg, #f8f9ff 0%, #fef7f7 100%);
      min-height: 100vh;
    }

    .nav-option {
      transition: all 0.3s ease;
      background: white;
    }

    .nav-option:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(221, 68, 112, 0.15);
    }

    .nav-option.selected {
      border-color: #DD4470 !important;
      background: linear-gradient(135deg, #DD4470, #A7226F);
      color: white;
    }

    .nav-option.selected i {
      color: white !important;
    }

    .nav-option.selected .text-muted {
      color: rgba(255, 255, 255, 0.8) !important;
    }

    .color-circle {
      width: 50px;
      height: 50px;
      border-radius: 50%;
      cursor: pointer;
      border: 3px solid transparent;
      transition: all 0.3s ease;
      margin: 0 auto;
    }

    .color-circle:hover {
      transform: scale(1.1);
      box-shadow: 0 4px 12px rgba(0,0,0,0.2);
    }

    .color-circle.selected {
      border-color: #333;
      transform: scale(1.2);
      box-shadow: 0 0 0 3px rgba(221, 68, 112, 0.3);
    }

    /* Preview Styles */
    .preview-wrapper {
      height: 450px;
      overflow: hidden;
    }

    .preview-container {
      border: 2px solid #e9ecef;
      border-radius: 8px;
      overflow: hidden;
      background: white;
      height: 100%;
      display: flex;
    }

    .preview-sidebar {
      width: 180px;
      padding: 1rem;
      color: white;
      display: flex;
      flex-direction: column;
      flex-shrink: 0;
    }

    .preview-logo {
      text-align: center;
      padding: 0.5rem;
    }

    .preview-logo-img {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      object-fit: cover;
    }

    .preview-logo-placeholder {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: rgba(255,255,255,0.2);
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto;
    }

    .preview-school-name {
      font-size: 0.85rem;
      font-weight: bold;
      text-align: center;
      margin-bottom: 1rem;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      padding: 0 0.25rem;
      line-height: 1.2;
      max-height: 2.4em;
    }

    .preview-nav-item {
      padding: 0.5rem;
      font-size: 0.8rem;
      border-radius: 4px;
      margin-bottom: 0.25rem;
      opacity: 0.7;
    }

    .preview-nav-item.active {
      background: rgba(255,255,255,0.2);
      opacity: 1;
    }

    .preview-content {
      flex: 1;
      padding: 1rem;
      background: #f8f9fa;
      overflow: hidden;
    }

    .preview-content-full {
      width: 100%;
      padding: 1rem;
      background: #f8f9fa;
      overflow: hidden;
    }

    .preview-topbar {
      height: 60px;
      width: 100%;
      color: white;
      padding: 0 1rem;
      flex-shrink: 0;
    }

    .preview-topbar-content {
      display: flex;
      align-items: center;
      justify-content: space-between;
      height: 100%;
    }

    .preview-logo-section {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      flex: 1;
      min-width: 0;
    }

    .preview-topbar-logo {
      width: 35px;
      height: 35px;
      border-radius: 50%;
      object-fit: cover;
      flex-shrink: 0;
    }

    .preview-topbar-logo-placeholder {
      width: 35px;
      height: 35px;
      border-radius: 50%;
      background: rgba(255,255,255,0.2);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .preview-school-name-top {
      font-weight: bold;
      font-size: 0.9rem;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      flex: 1;
      min-width: 0;
    }

    .preview-nav-horizontal {
      display: flex;
      gap: 1rem;
      flex-shrink: 0;
    }

    .preview-nav-item-top {
      font-size: 0.8rem;
      opacity: 0.7;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      white-space: nowrap;
    }

    .preview-nav-item-top.active {
      background: rgba(255,255,255,0.2);
      opacity: 1;
    }

    .preview-header {
      font-weight: bold;
      margin-bottom: 1rem;
      color: #333;
      font-size: 0.9rem;
    }

    .preview-cards {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
    }

    .preview-card {
      height: 60px;
      background: white;
      border-radius: 4px;
      flex: 1;
      min-width: 80px;
      border: 1px solid #e9ecef;
    }

    @media (max-width: 768px) {
      .preview-wrapper {
        height: 350px;
      }
      
      .preview-sidebar {
        width: 140px;
      }
      
      .preview-school-name {
        font-size: 0.75rem;
      }
      
      .preview-nav-item {
        font-size: 0.7rem;
        padding: 0.4rem;
      }
      
      .preview-cards {
        flex-direction: column;
        gap: 0.25rem;
      }
      
      .preview-card {
        height: 40px;
      }
    }
  `]
})
export class SettingsComponent implements OnInit {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);

  @Output() settingsUpdated = new EventEmitter<any>();

  settings: SchoolSettings = {
    id: 0,
    schoolId: 0,
    schoolDisplayName: '',
    logoImageUrl: '',
    navigationType: 'sidebar',
    themeColor: 'DD4470',
    updatedAt: ''
  };

  colorPalette = [
    'DD4470', 'FFC872', 'FCE4BF', 'A7226F',
    'F46C3F', '97DBAE', 'C5211C', 'B7EDFF',
    'F3533A', 'F2EAE3', '874356', '524582'
  ];

  isSaving = false;

  ngOnInit() {
    this.loadSettings();
  }

  loadSettings() {
    const token = sessionStorage.getItem('token');
    if (!token) return;

    this.http.get<SchoolSettings>('http://localhost:5021/api/SchoolSettings', {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: (settings) => {
        this.settings = settings;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading settings:', error);
      }
    });
  }

  onLogoUpload(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.settings.logoImageUrl = e.target.result;
        this.cdr.detectChanges();
      };
      reader.readAsDataURL(file);
    }
  }

  removeLogo() {
    this.settings.logoImageUrl = '';
    this.cdr.detectChanges();
  }

  saveSettings() {
    const token = sessionStorage.getItem('token');
    if (!token) {
      alert('Authentication required. Please login again.');
      return;
    }

    if (!this.settings.schoolDisplayName?.trim()) {
      alert('Please enter a school display name.');
      return;
    }

    this.isSaving = true;

    const request = {
      schoolDisplayName: this.settings.schoolDisplayName.trim(),
      logoImageUrl: this.settings.logoImageUrl || '',
      navigationType: this.settings.navigationType,
      themeColor: this.settings.themeColor
    };

    console.log('Saving settings:', request);

    this.http.put('http://localhost:5021/api/SchoolSettings', request, {
      headers: { 
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }).subscribe({
      next: (response) => {
        this.isSaving = false;
        console.log('Settings saved successfully:', response);
        alert('Settings saved successfully!');
        
        // Update local settings with the saved values
        const savedSettings = {
          schoolDisplayName: this.settings.schoolDisplayName.trim(),
          logoImageUrl: this.settings.logoImageUrl || '',
          navigationType: this.settings.navigationType,
          themeColor: this.settings.themeColor
        };
        
        console.log('Emitting saved settings:', savedSettings);
        this.settingsUpdated.emit(savedSettings); // Emit the actual settings data
        
        this.loadSettings(); // Reload to get updated data from server
      },
      error: (error) => {
        this.isSaving = false;
        console.error('Error saving settings:', error);
        
        let errorMessage = 'Failed to save settings. ';
        if (error.status === 401) {
          errorMessage += 'Authentication failed. Please login again.';
        } else if (error.status === 403) {
          errorMessage += 'You do not have permission to modify settings.';
        } else if (error.status === 400) {
          errorMessage += 'Invalid data provided.';
        } else if (error.status === 500) {
          errorMessage += 'Server error occurred.';
        } else {
          errorMessage += 'Please check your connection and try again.';
        }
        
        alert(errorMessage);
      }
    });
  }
}
