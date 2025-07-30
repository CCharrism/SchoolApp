import { Component, OnInit, Output, EventEmitter, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface Branch {
  id: number;
  branchName: string;
  description: string;
  location: string;
  schoolId: number;
  schoolName: string;
  schoolHeadUsername: string;
  isActive: boolean;
  createdAt: string;
  courseCount: number;
}

interface CreateBranchRequest {
  branchName: string;
  description: string;
  location: string;
  schoolHeadUsername: string;
  schoolHeadPassword: string;
}

@Component({
  selector: 'app-branches',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './branches.component.html',
  styleUrls: ['./branches.component.css']
})
export class BranchesComponent implements OnInit, OnDestroy {
  @Output() branchCreated = new EventEmitter<void>();

  branches: Branch[] = [];
  showCreateForm = false;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  isDestroyed = false;

  newBranch: CreateBranchRequest = {
    branchName: '',
    description: '',
    location: '',
    schoolHeadUsername: '',
    schoolHeadPassword: ''
  };

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    console.log('🚀 BranchesComponent ngOnInit called');
    console.log('📊 Initial state - isLoading:', this.isLoading, 'isDestroyed:', this.isDestroyed);
    this.loadBranches();
    
    // Debug: Check state periodically
    const debugInterval = setInterval(() => {
      if (this.isDestroyed) {
        clearInterval(debugInterval);
        return;
      }
      console.log('⏰ Periodic check - isLoading:', this.isLoading, 'branches.length:', this.branches.length);
    }, 2000);
  }

  ngOnDestroy() {
    this.isDestroyed = true;
  }

  loadBranches() {
    console.log('🔄 loadBranches called');
    console.log('📊 Current state before check - isLoading:', this.isLoading, 'isDestroyed:', this.isDestroyed);
    
    // Prevent multiple simultaneous calls
    if (this.isLoading || this.isDestroyed) {
      console.log('⚠️ Skipping loadBranches - already loading or destroyed');
      return;
    }
    
    console.log('✅ Proceeding with loadBranches');
    this.isLoading = true;
    this.errorMessage = '';
    console.log('📊 Set isLoading to true, starting API call');

    // Check if user is authenticated
    if (!this.authService.isAuthenticated()) {
      console.log('❌ User not authenticated');
      this.errorMessage = 'You are not authenticated. Please login again.';
      this.router.navigate(['/login']);
      this.isLoading = false;
      return;
    }

    console.log('✅ User authenticated, making API call');
    this.http.get<Branch[]>('http://localhost:5021/api/Branch').subscribe({
      next: (branches) => {
        if (!this.isDestroyed) {
          console.log('✅ Branches loaded successfully:', branches);
          console.log('🔄 Setting isLoading to false');
          this.branches = branches;
          this.isLoading = false;
          console.log('✅ After setting: isLoading =', this.isLoading, 'branches.length =', this.branches.length);
          
          // Force change detection
          this.cdr.detectChanges();
          console.log('🔄 Change detection triggered');
        } else {
          console.log('⚠️ Component destroyed, skipping state update');
        }
      },
      error: (error: HttpErrorResponse) => {
        if (!this.isDestroyed) {
          console.error('❌ Error loading branches:', error);
          console.log('🔄 Setting isLoading to false due to error');
          
          if (error.status === 401) {
            this.errorMessage = 'Authentication failed. Please login again.';
            this.authService.logout();
            this.router.navigate(['/login']);
          } else {
            this.errorMessage = 'Failed to load branches. Please try again.';
          }
          this.isLoading = false;
          console.log('✅ After error: isLoading =', this.isLoading);
          
          // Force change detection
          this.cdr.detectChanges();
          console.log('🔄 Change detection triggered after error');
        } else {
          console.log('⚠️ Component destroyed, skipping error handling');
        }
      }
    });
  }

  toggleCreateForm() {
    this.showCreateForm = !this.showCreateForm;
    if (!this.showCreateForm) {
      this.resetForm();
    }
  }

  resetForm() {
    this.newBranch = {
      branchName: '',
      description: '',
      location: '',
      schoolHeadUsername: '',
      schoolHeadPassword: ''
    };
    this.errorMessage = '';
    this.successMessage = '';
  }

createBranch() {
  if (!this.validateForm()) {
    return;
  }

  // Check authentication before creating branch
  if (!this.authService.isAuthenticated()) {
    this.errorMessage = 'You are not authenticated. Please login again.';
    this.router.navigate(['/login']);
    return;
  }

  this.isLoading = true;
  this.errorMessage = '';
  this.successMessage = '';
  this.cdr.detectChanges();  // Trigger UI update immediately after setting loading

  this.http.post<Branch>('http://localhost:5021/api/Branch', this.newBranch).subscribe({
    next: (branch) => {
      this.successMessage = 'Branch created successfully!';
      this.branches.push(branch);
      this.branchCreated.emit();

      setTimeout(() => {
        this.toggleCreateForm();
      }, 2000);

      this.isLoading = false;
      this.cdr.detectChanges();  // Update UI after success
    },
    error: (error: HttpErrorResponse) => {
      console.error('Error creating branch:', error);

      if (error.status === 401) {
        this.errorMessage = 'Authentication failed. Please login again.';
        this.authService.logout();
        this.router.navigate(['/login']);
      } else if (error.error && typeof error.error === 'string') {
        this.errorMessage = error.error;
      } else if (error.error && error.error.message) {
        this.errorMessage = error.error.message;
      } else {
        this.errorMessage = 'Failed to create branch. Please try again.';
      }

      this.isLoading = false;
      this.cdr.detectChanges();  // Update UI after error
    }
  });
}


  validateForm(): boolean {
    if (!this.newBranch.branchName.trim()) {
      this.errorMessage = 'Branch name is required.';
      return false;
    }

    if (!this.newBranch.schoolHeadUsername.trim()) {
      this.errorMessage = 'School head username is required.';
      return false;
    }

    if (!this.newBranch.schoolHeadPassword.trim()) {
      this.errorMessage = 'School head password is required.';
      return false;
    }

    if (this.newBranch.schoolHeadPassword.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters long.';
      return false;
    }

    return true;
  }

  toggleBranchStatus(branch: Branch) {
    const updatedBranch = {
      branchName: branch.branchName,
      description: branch.description,
      location: branch.location,
      isActive: !branch.isActive
    };

    this.http.put(`http://localhost:5021/api/Branch/${branch.id}`, updatedBranch).subscribe({
      next: () => {
        branch.isActive = !branch.isActive;
        this.successMessage = `Branch ${branch.isActive ? 'activated' : 'deactivated'} successfully!`;
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error updating branch:', error);
        this.errorMessage = 'Failed to update branch status. Please try again.';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  deleteBranch(branch: Branch) {
    if (!confirm(`Are you sure you want to delete the branch "${branch.branchName}"? This action cannot be undone.`)) {
      return;
    }

    this.http.delete(`http://localhost:5021/api/Branch/${branch.id}`).subscribe({
      next: () => {
        this.branches = this.branches.filter(b => b.id !== branch.id);
        this.successMessage = 'Branch deleted successfully!';
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error deleting branch:', error);
        if (error.error && typeof error.error === 'string') {
          this.errorMessage = error.error;
        } else {
          this.errorMessage = 'Failed to delete branch. Please try again.';
        }
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }
}
