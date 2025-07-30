import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-simple-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-vh-100 d-flex align-items-center justify-content-center" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);">
      <div class="container">
        <div class="row justify-content-center">
          <div class="col-lg-5 col-md-7">
            <div class="card shadow-lg border-0" style="border-radius: 15px;">
              <div class="card-body p-5">
                <!-- School Logo and Title -->
                <div class="text-center mb-4">
                  <div class="mb-3">
                    <i class="fas fa-graduation-cap text-primary" style="font-size: 3rem;"></i>
                  </div>
                  <h2 class="fw-bold text-dark mb-2">School Management</h2>
                  <p class="text-muted">Admin Portal</p>
                </div>

                <!-- Login Form -->
                <form (ngSubmit)="onSubmit()" #loginForm="ngForm">
                  <div class="mb-4">
                    <label for="username" class="form-label fw-semibold text-dark">
                      <i class="fas fa-user me-2"></i>Username
                    </label>
                    <input 
                      type="text" 
                      class="form-control form-control-lg" 
                      id="username"
                      name="username"
                      [(ngModel)]="username"
                      placeholder="Enter your username"
                      required
                      style="border-radius: 10px; border: 2px solid #e9ecef;">
                  </div>

                  <div class="mb-4">
                    <label for="password" class="form-label fw-semibold text-dark">
                      <i class="fas fa-lock me-2"></i>Password
                    </label>
                    <input 
                      type="password" 
                      class="form-control form-control-lg" 
                      id="password"
                      name="password"
                      [(ngModel)]="password"
                      placeholder="Enter your password"
                      required
                      style="border-radius: 10px; border: 2px solid #e9ecef;">
                  </div>

                  <div class="d-grid mb-4">
                    <button 
                      type="submit" 
                      class="btn btn-primary btn-lg fw-semibold"
                      style="border-radius: 10px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border: none;">
                      <i class="fas fa-sign-in-alt me-2"></i>
                      Sign In
                    </button>
                  </div>
                </form>

                <!-- Demo Credentials Info -->
                <div class="text-center">
                  <small class="text-muted">
                    <strong>Demo Credentials:</strong><br>
                    Username: <code>admin</code> | Password: <code>admin123</code>
                  </small>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class SimpleLoginComponent {
  username = '';
  password = '';

  onSubmit(): void {
    console.log('Login attempted with:', this.username, this.password);
    alert('Login form submitted! Check console for details.');
  }
}
