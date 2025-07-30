import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { AdminService } from '../../services/admin.service';
import { SchoolHierarchy, AdminStatistics } from '../../models/admin.models';

@Component({
  selector: 'app-manage-schools',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-schools.component.html',
  styleUrls: ['./manage-schools.component.css']
})
export class ManageSchoolsComponent implements OnInit {
  schoolHierarchies: SchoolHierarchy[] = [];
  statistics: AdminStatistics | null = null;
  loading = true;
  error: string | null = null;
  
  // Pagination
  currentPage = 1;
  pageSize = 5;
  totalPages = 0;
  totalCount = 0;
  hasNextPage = false;
  hasPreviousPage = false;
  
  // Search
  searchTerm = '';
  
  // UI State
  processingAction = false;

  constructor(
    private adminService: AdminService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    console.log('ManageSchoolsComponent ngOnInit called');
    this.loadData();
  }

  loadData() {
    console.log('loadData called, setting loading to true');
    this.loading = true;
    this.error = null;
    
    console.log('Loading admin data...');
    console.log('Token from sessionStorage:', sessionStorage.getItem('token'));

    // Use forkJoin to handle both API calls together
    forkJoin({
      statistics: this.adminService.getAdminStatistics(),
      hierarchyData: this.adminService.getSchoolHierarchyPaginated(this.currentPage, this.pageSize, this.searchTerm)
    }).subscribe({
      next: (results) => {
        console.log('Both API calls completed successfully');
        console.log('Statistics:', results.statistics);
        console.log('Hierarchy Data:', results.hierarchyData);
        
        // Use NgZone to ensure we're in the Angular zone
        this.ngZone.run(() => {
          this.statistics = results.statistics;
          this.schoolHierarchies = results.hierarchyData.data;
          
          // Update pagination info
          const pagination = results.hierarchyData.pagination;
          this.currentPage = pagination.currentPage;
          this.totalPages = pagination.totalPages;
          this.totalCount = pagination.totalCount;
          this.hasNextPage = pagination.hasNextPage;
          this.hasPreviousPage = pagination.hasPreviousPage;
          
          this.loading = false;
          
          console.log('Loading set to false in NgZone, current value:', this.loading);
          
          // Force change detection
          this.cdr.detectChanges();
          console.log('Change detection triggered in NgZone');
        });
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.ngZone.run(() => {
          this.error = 'Failed to load school data. Please try again.';
          this.loading = false;
          console.log('Loading set to false due to error, current value:', this.loading);
          this.cdr.detectChanges();
        });
      }
    });
  }

  refreshData() {
    this.loadData();
  }

  onSearch() {
    this.currentPage = 1; // Reset to first page when searching
    this.loadData();
  }

  onPageChange(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadData();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const startPage = Math.max(1, this.currentPage - 2);
    const endPage = Math.min(this.totalPages, this.currentPage + 2);
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  toggleSchoolOwnerStatus(ownerId: number, ownerName: string, currentStatus: boolean) {
    if (this.processingAction) return;
    
    const action = currentStatus ? 'deactivate' : 'activate';
    const confirmMessage = `Are you sure you want to ${action} ${ownerName}? ${!currentStatus ? '' : 'This will also deactivate all their school heads.'}`;
    
    if (confirm(confirmMessage)) {
      this.processingAction = true;
      
      this.adminService.toggleSchoolOwnerStatus(ownerId).subscribe({
        next: (response) => {
          console.log('Status toggled successfully:', response);
          this.processingAction = false;
          this.loadData(); // Reload data to reflect changes
        },
        error: (error) => {
          console.error('Error toggling status:', error);
          this.processingAction = false;
          alert('Failed to update status. Please try again.');
        }
      });
    }
  }

  toggleSchoolHeadStatus(headId: number, headName: string, currentStatus: boolean) {
    if (this.processingAction) return;
    
    const action = currentStatus ? 'deactivate' : 'activate';
    const confirmMessage = `Are you sure you want to ${action} ${headName}?`;
    
    if (confirm(confirmMessage)) {
      this.processingAction = true;
      
      this.adminService.toggleSchoolHeadStatus(headId).subscribe({
        next: (response) => {
          console.log('Status toggled successfully:', response);
          this.processingAction = false;
          this.loadData(); // Reload data to reflect changes
        },
        error: (error) => {
          console.error('Error toggling status:', error);
          this.processingAction = false;
          alert('Failed to update status. Please try again.');
        }
      });
    }
  }

  // Debug method to manually stop loading
  stopLoading() {
    console.log('Manually stopping loading');
    this.ngZone.run(() => {
      this.loading = false;
      this.cdr.detectChanges();
      console.log('Loading is now:', this.loading);
    });
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }

  // New methods for the beautiful design
  toggleOwnerStatus(ownerId: number, currentStatus: boolean) {
    this.toggleSchoolOwnerStatus(ownerId, 'School Owner', currentStatus);
  }

  toggleHeadStatus(headId: number, currentStatus: boolean) {
    this.toggleSchoolHeadStatus(headId, 'School Head', currentStatus);
  }

  goToPage(page: number) {
    this.onPageChange(page);
  }

  clearSearch() {
    this.searchTerm = '';
    this.onSearch();
  }
}
