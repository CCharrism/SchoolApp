import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Teacher, CreateTeacherRequest } from '../../models/auth.models';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-teacher-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './teacher-management.component.html',
  styleUrl: './teacher-management.component.css'
})
export class TeacherManagementComponent implements OnInit {
   @Input() active: boolean = false;
  teachers: Teacher[] = [];
  showCreateForm = false;
  isLoading = false;
  selectedTeacher: Teacher | null = null;
  
  newTeacher: CreateTeacherRequest = {
    fullName: '',
    username: '',
    email: '',
    password: '',
    phone: '',
    subject: '',
    qualification: ''
  };

  private apiUrl = 'http://localhost:5021/api';

  constructor(private http: HttpClient,private cdr: ChangeDetectorRef) {}
   ngOnChanges(changes: SimpleChanges) {
    if (changes['active'] && changes['active'].currentValue) {
      this.loadTeachers();
    }
  }

  ngOnInit(): void {
    this.loadTeachers();
  }


  loadTeachers(): void {
  console.log('Loading teachers...');
  this.isLoading = true;
  this.cdr.detectChanges(); // immediately update UI to show loading spinner
  
  this.http.get<Teacher[]>(`${this.apiUrl}/teachers`).subscribe({
    next: (data) => {
      console.log('Teachers loaded successfully:', data);
      this.teachers = data || [];
      this.isLoading = false;
      this.cdr.detectChanges(); // update UI to hide spinner and show data
      
      if (this.teachers.length === 0) {
        console.log('No teachers found from API, using mock data');
        this.setMockData();
        this.cdr.detectChanges(); // update UI after setting mock data
      }
    },
    error: (error) => {
      console.error('Error loading teachers:', error);
      this.isLoading = false;
      this.setMockData();
      this.cdr.detectChanges(); // update UI after error and mock data set
    }
  });
}


  private setMockData(): void {
    this.teachers = [
      {
        id: 1,
        fullName: 'John Smith',
        username: 'john.smith',
        email: 'john.smith@school.edu',
        phone: '+1234567890',
        subject: 'Mathematics',
        qualification: 'M.Sc Mathematics',
        schoolId: 1,
        isActive: true,
        createdAt: '2025-01-15T10:30:00Z'
      },
      {
        id: 2,
        fullName: 'Sarah Johnson',
        username: 'sarah.johnson',
        email: 'sarah.johnson@school.edu',
        phone: '+1234567891',
        subject: 'English Literature',
        qualification: 'M.A English',
        schoolId: 1,
        isActive: true,
        createdAt: '2025-01-16T11:45:00Z'
      },
      {
        id: 3,
        fullName: 'Michael Brown',
        username: 'michael.brown',
        email: 'michael.brown@school.edu',
        phone: '+1234567892',
        subject: 'Physics',
        qualification: 'M.Sc Physics',
        schoolId: 1,
        isActive: false,
        createdAt: '2025-01-14T09:15:00Z'
      }
    ];
  }

createTeacher(): void {
  console.log('Creating teacher:', this.newTeacher);

  if (!this.isFormValid()) {
    console.log('Form is not valid');
    return;
  }

  this.isLoading = true;
  this.cdr.detectChanges(); // <-- Force UI update here

  this.http.post<Teacher>(`${this.apiUrl}/teachers`, this.newTeacher).subscribe({
    next: (data) => {
      console.log('Teacher created successfully:', data);
      this.teachers.unshift(data);
      this.resetForm();
      this.isLoading = false;
      this.cdr.detectChanges(); // <-- And here after loading done
    },
    error: (error) => {
      console.error('Error creating teacher:', error);
      this.isLoading = false;
      this.cdr.detectChanges(); // <-- Also here
      // Add mock teacher on error (optional)
      const mockTeacher: Teacher = {
        id: Date.now(),
        fullName: this.newTeacher.fullName,
        username: this.newTeacher.username,
        email: this.newTeacher.email,
        phone: this.newTeacher.phone,
        subject: this.newTeacher.subject,
        qualification: this.newTeacher.qualification,
        schoolId: 1,
        isActive: true,
        createdAt: new Date().toISOString()
      };
      this.teachers.unshift(mockTeacher);
      this.resetForm();
    }
  });
}


  deleteTeacher(teacher: Teacher): void {
    if (confirm(`Are you sure you want to delete ${teacher.fullName}?`)) {
      console.log('Deleting teacher with id:', teacher.id);
      
      this.http.delete(`${this.apiUrl}/teachers/${teacher.id}`).subscribe({
        next: () => {
          console.log('Teacher deleted successfully');
          this.teachers = this.teachers.filter(t => t.id !== teacher.id);
        },
        error: (error) => {
          console.error('Error deleting teacher:', error);
          this.teachers = this.teachers.filter(t => t.id !== teacher.id);
        }
      });
    }
  }

  toggleTeacherStatus(teacher: Teacher): void {
    const updatedTeacher = { ...teacher, isActive: !teacher.isActive };
    
    this.http.put<Teacher>(`${this.apiUrl}/teachers/${teacher.id}`, updatedTeacher).subscribe({
      next: (updated) => {
        const index = this.teachers.findIndex(t => t.id === teacher.id);
        if (index !== -1) {
          this.teachers[index] = updated;
        }
      },
      error: (error) => {
        console.error('Error updating teacher status:', error);
        teacher.isActive = !teacher.isActive;
      }
    });
  }

  editTeacher(teacher: Teacher): void {
    this.selectedTeacher = { ...teacher };
  }

  saveTeacher(): void {
    if (!this.selectedTeacher) return;

    this.http.put<Teacher>(`${this.apiUrl}/teachers/${this.selectedTeacher.id}`, this.selectedTeacher).subscribe({
      next: (updatedTeacher) => {
        const index = this.teachers.findIndex(t => t.id === updatedTeacher.id);
        if (index !== -1) {
          this.teachers[index] = updatedTeacher;
        }
        this.selectedTeacher = null;
      },
      error: (error) => {
        console.error('Error updating teacher:', error);
        const index = this.teachers.findIndex(t => t.id === this.selectedTeacher!.id);
        if (index !== -1) {
          this.teachers[index] = this.selectedTeacher!;
        }
        this.selectedTeacher = null;
      }
    });
  }

  cancelEdit(): void {
    this.selectedTeacher = null;
  }

  showAddForm(): void {
    this.showCreateForm = true;
  }

  cancelForm(): void {
    this.resetForm();
    this.showCreateForm = false;
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'badge-success' : 'badge-secondary';
  }

  resetForm(): void {
    this.showCreateForm = false;
    this.newTeacher = {
      fullName: '',
      username: '',
      email: '',
      password: '',
      phone: '',
      subject: '',
      qualification: ''
    };
  }

  private isFormValid(): boolean {
    return !!(
      this.newTeacher.fullName &&
      this.newTeacher.username &&
      this.newTeacher.email &&
      this.newTeacher.password &&
      this.newTeacher.phone &&
      this.newTeacher.subject &&
      this.newTeacher.qualification
    );
  }
}
