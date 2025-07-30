import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../services/auth.service';
import { LoginRequest } from '../models/auth.models';
import { YourComponent } from "./background/background";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, YourComponent],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  credentials: LoginRequest = {
    username: '',
    password: ''
  };

  isLoading = false;

  constructor(
    public authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    // Check authentication status after component initialization
    setTimeout(() => {
      if (this.authService.isAuthenticated()) {
        const dashboardRoute = this.authService.getDashboardRoute();
        this.router.navigate([dashboardRoute]);
      }
    }, 0);
  }

  onSubmit(): void {
    if (!this.credentials.username || !this.credentials.password) {
      this.showError('Please enter both username and password');
      return;
    }

    // Use setTimeout to avoid change detection issues
    setTimeout(() => {
      this.isLoading = true;
    }, 0);

    this.authService.login(this.credentials).subscribe({
      next: () => {
        setTimeout(() => {
          this.isLoading = false;
          const dashboardRoute = this.authService.getDashboardRoute();
          this.router.navigate([dashboardRoute]);
        }, 0);
      },
      error: (error) => {
        setTimeout(() => {
          this.isLoading = false;
          console.log('Login error:', error);
          this.showError(error.error || 'Invalid username or password');
        }, 0)
      }
    });
  }

  // JWT Debug methods
  testJwtAuth(): void {
    console.log('🔍 Testing JWT authentication...');
    this.authService.testAuthEndpoint().subscribe({
      next: (response) => {
        console.log('✅ JWT Test successful:', response);
        this.toastr.success('JWT Authentication test passed', 'Success');
      },
      error: (error) => {
        console.log('❌ JWT Test failed:', error);
        this.toastr.error(`JWT Test failed: ${error.status} - ${error.error?.title || error.message}`, 'JWT Error');
      }
    });
  }

  decodeJwtToken(): void {
    console.log('🔍 Decoding JWT token...');
    this.authService.decodeToken().subscribe({
      next: (response) => {
        console.log('✅ JWT Token decoded:', response);
        this.toastr.success('Token decoded successfully', 'Success');
      },
      error: (error) => {
        console.log('❌ JWT Decode failed:', error);
        this.toastr.error(`Token decode failed: ${error.message}`, 'Decode Error');
      }
    });
  }

  testHeaders(): void {
    console.log('🔍 Testing request headers...');
    this.authService.testHeaders().subscribe({
      next: (response) => {
        console.log('✅ Headers test response:', response);
        this.toastr.success('Headers test completed', 'Success');
      },
      error: (error) => {
        console.log('❌ Headers test failed:', error);
        this.toastr.error(`Headers test failed: ${error.status}`, 'Headers Error');
      }
    });
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

  // Debug method to check sessionStorage
  checkSessionStorage(): void {
    console.log('🔍 Checking sessionStorage contents...');
    console.log('🔑 Token:', sessionStorage.getItem('token'));
    console.log('⏰ Token Expiry:', sessionStorage.getItem('tokenExpiry'));
    console.log('👤 User:', sessionStorage.getItem('user'));
    console.log('✅ Is Authenticated:', this.authService.isAuthenticated());
    
    this.toastr.info('Check console for sessionStorage contents', 'Debug');
  }
}
