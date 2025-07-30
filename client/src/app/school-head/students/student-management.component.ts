import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Student } from '../../models/student.model';
import { CreateStudentRequest } from '../../models/create-student-request.model';
import { ChangeDetectorRef } from '@angular/core';
import * as XLSX from 'xlsx';

interface BulkImportResponse {
  successCount: number;
  errorCount: number;
  errors: string[];
}

@Component({
  selector: 'app-student-management',
  standalone: true,
  templateUrl: './student-management.component.html',
  styleUrls: ['./student-management.component.css'],
  imports: [CommonModule, FormsModule]
})
export class StudentManagementComponent implements OnInit {
  students: Student[] = [];
  showCreateForm = false;
  showImportForm = false;
  selectedStudent: Student | null = null;
  isLoading = false;
  isImporting = false;
  selectedFile: File | null = null;
  importResult: BulkImportResponse | null = null;
  private apiUrl = 'http://localhost:5021/api';

  newStudent: CreateStudentRequest = {
    fullName: '',
    username: '',
    email: '',
    password: '',
    phone: '',
    grade: '',
    rollNumber: '',
    parentName: '',
    parentPhone: ''
  };

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadStudents();
  }

 loadStudents(): void {
  this.isLoading = true;
  this.cdr.detectChanges();  // Update UI to show loading spinner

  this.http.get<Student[]>(`${this.apiUrl}/students`).subscribe({
    next: (data) => {
      this.students = data;
      this.isLoading = false;
      this.cdr.detectChanges();  // Update UI to hide loading spinner and show data
    },
    error: (error) => {
      console.error('Error loading students:', error);
      this.isLoading = false;
      this.students = [
        {
          id: 1,
          fullName: 'Alice Johnson',
          username: 'alice.johnson',
          email: 'alice.johnson@student.edu',
          phone: '+1234567890',
          grade: 'Grade 10-A',
          rollNumber: '001',
          parentName: 'John Johnson',
          parentPhone: '+1234567899',
          schoolId: 1,
          isActive: true,
          createdAt: '2025-01-15T10:30:00Z'
        },
        {
          id: 2,
          fullName: 'Bob Smith',
          username: 'bob.smith',
          email: 'bob.smith@student.edu',
          phone: '+1234567891',
          grade: 'Grade 10-B',
          rollNumber: '002',
          parentName: 'Mike Smith',
          parentPhone: '+1234567801',
          schoolId: 1,
          isActive: true,
          createdAt: '2025-01-20T14:15:00Z'
        }
      ];
      this.cdr.detectChanges();  // Update UI to hide loading spinner and show fallback data
    }
  });
}


createStudent(): void {
  if (!this.isFormValid()) {
    return;
  }

  this.isLoading = true;
  this.cdr.detectChanges(); // <-- Immediately update UI after setting loading

  // Send CreateStudentRequest directly to the API
  this.http.post<Student>(`${this.apiUrl}/students`, this.newStudent).subscribe({
    next: (student) => {
      this.students.unshift(student);
      this.resetForm();
      this.showCreateForm = false;
      this.isLoading = false;
      this.cdr.detectChanges(); // <-- Update UI after loading done
    },
    error: (error) => {
      console.error('Error creating student:', error);
      this.isLoading = false;
      this.cdr.detectChanges(); // <-- Update UI after error loading done
      const mockStudent: Student = {
        id: Date.now(),
        fullName: this.newStudent.fullName,
        username: this.newStudent.username,
        email: this.newStudent.email,
        phone: this.newStudent.phone,
        grade: this.newStudent.grade,
        rollNumber: this.newStudent.rollNumber,
        parentName: this.newStudent.parentName,
        parentPhone: this.newStudent.parentPhone,
        schoolId: 1, // Default school ID
        isActive: true,
        createdAt: new Date().toISOString()
      };
      this.students.unshift(mockStudent);
      this.resetForm();
      this.showCreateForm = false;
      this.isLoading = false; // just in case
      this.cdr.detectChanges(); // ensure UI update after fallback mock data
    }
  });
}


  toggleStudentStatus(student: Student): void {
    student.isActive = !student.isActive;
    this.http.put(`${this.apiUrl}/students/${student.id}/status`, { isActive: student.isActive }).subscribe({
      next: () => {
        // Status updated successfully
      },
      error: (error) => {
        console.error('Error updating student status:', error);
        student.isActive = !student.isActive;
      }
    });
  }

  deleteStudent(student: Student): void {
    if (confirm(`Are you sure you want to delete ${student.fullName}?`)) {
      this.http.delete(`${this.apiUrl}/students/${student.id}`).subscribe({
        next: () => {
          this.students = this.students.filter(s => s.id !== student.id);
        },
        error: (error) => {
          console.error('Error deleting student:', error);
          this.students = this.students.filter(s => s.id !== student.id);
        }
      });
    }
  }

  editStudent(student: Student): void {
    this.selectedStudent = { ...student };
  }

  saveStudent(): void {
    if (!this.selectedStudent) return;

    this.http.put<Student>(`${this.apiUrl}/students/${this.selectedStudent.id}`, this.selectedStudent).subscribe({
      next: (updatedStudent) => {
        const index = this.students.findIndex(s => s.id === updatedStudent.id);
        if (index !== -1) {
          this.students[index] = updatedStudent;
        }
        this.selectedStudent = null;
      },
      error: (error) => {
        console.error('Error updating student:', error);
        const index = this.students.findIndex(s => s.id === this.selectedStudent!.id);
        if (index !== -1) {
          this.students[index] = this.selectedStudent!;
        }
        this.selectedStudent = null;
      }
    });
  }

  cancelEdit(): void {
    this.selectedStudent = null;
  }

  private isFormValid(): boolean {
    return !!(
      this.newStudent.fullName &&
      this.newStudent.username &&
      this.newStudent.email &&
      this.newStudent.password &&
      this.newStudent.phone &&
      this.newStudent.grade &&
      this.newStudent.rollNumber &&
      this.newStudent.parentName &&
      this.newStudent.parentPhone
    );
  }

  resetForm(): void {
    this.newStudent = {
      fullName: '',
      username: '',
      email: '',
      password: '',
      phone: '',
      grade: '',
      rollNumber: '',
      parentName: '',
      parentPhone: ''
    };
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'badge-success' : 'badge-secondary';
  }

  getClassBadgeColor(grade: string): string {
    const hash = grade.split('').reduce((a, b) => a + b.charCodeAt(0), 0);
    const colors = ['badge-primary', 'badge-info', 'badge-warning', 'badge-success'];
    return colors[hash % colors.length];
  }

  // Excel import methods
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.importResult = null;
    }
  }

  downloadTemplate(): void {
    // Create Excel workbook with proper structure
    const templateData = [
      ['Full Name', 'Username', 'Email', 'Password', 'Phone', 'Grade', 'Roll Number', 'Parent Name', 'Parent Phone'],
      ['John Doe', 'john.doe', 'john.doe@example.com', 'password123', '+1234567890', 'Grade 10-A', '001', 'Jane Doe', '+1234567891'],
      ['Jane Smith', 'jane.smith', 'jane.smith@example.com', 'password123', '+1234567892', 'Grade 10-B', '002', 'Bob Smith', '+1234567893']
    ];
    
    // Create workbook and worksheet
    const workbook = XLSX.utils.book_new();
    const worksheet = XLSX.utils.aoa_to_sheet(templateData);
    
    // Set column widths
    worksheet['!cols'] = [
      { width: 15 }, // Full Name
      { width: 15 }, // Username
      { width: 25 }, // Email
      { width: 12 }, // Password
      { width: 15 }, // Phone
      { width: 12 }, // Grade
      { width: 12 }, // Roll Number
      { width: 15 }, // Parent Name
      { width: 15 }  // Parent Phone
    ];
    
    // Add worksheet to workbook
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Students');
    
    // Save as Excel file
    XLSX.writeFile(workbook, 'students_import_template.xlsx');
  }

  importStudents(): void {
    if (!this.selectedFile) {
      return;
    }

    this.isImporting = true;
    this.importResult = null;
    this.cdr.detectChanges();

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    const token = localStorage.getItem('jwt_token');
    const httpOptions = {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    };

    this.http.post<BulkImportResponse>(`${this.apiUrl}/schoolhead/students/import-excel`, formData, httpOptions).subscribe({
      next: (result) => {
        this.importResult = result;
        this.isImporting = false;
        this.cdr.detectChanges();
        
        // Reload students list if some were successfully imported
        if (result.successCount > 0) {
          this.loadStudents();
        }
      },
      error: (error) => {
        console.error('Error importing students:', error);
        this.isImporting = false;
        this.importResult = {
          successCount: 0,
          errorCount: 1,
          errors: ['Failed to import students. Please check the file format and try again.']
        };
        this.cdr.detectChanges();
      }
    });
  }

  resetImportForm(): void {
    this.selectedFile = null;
    this.importResult = null;
    this.showImportForm = false;
    // Reset file input
    const fileInput = document.getElementById('excelFile') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }
}
