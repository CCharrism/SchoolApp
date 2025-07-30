import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Classroom, CreateSchoolHeadClassroomRequest, Teacher, Student } from '../../models/auth.models';
import { SchoolHeadService } from '../../services/school-head.service';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-classroom-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './classroom-management.component.html',
  styleUrl: './classroom-management.component.css'
})
export class ClassroomManagementComponent implements OnInit {
  classrooms: Classroom[] = [];
  teachers: Teacher[] = [];
  students: Student[] = [];
  showCreateForm = false;
  isLoading = false;
  selectedClassroom: Classroom | null = null;
  showStudentsModal = false;
  classroomStudents: Student[] = [];
  
  newClassroom: CreateSchoolHeadClassroomRequest = {
    name: '',
    description: '',
    subject: '',
    section: '',
    teacherId: 0
  };

  private apiUrl = 'http://localhost:5021/api';

  constructor(
    private http: HttpClient,
    private schoolHeadService: SchoolHeadService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadClassrooms();
    this.loadTeachers();
    this.loadStudents();
  }

loadClassrooms(): void {
  this.isLoading = true;
  this.cdr.detectChanges(); // Show loading spinner immediately

  this.schoolHeadService.getClassrooms().subscribe({
    next: (classrooms) => {
      this.classrooms = classrooms;
      this.isLoading = false;
      this.cdr.detectChanges(); // Update UI after data loaded
    },
    error: (error) => {
      console.error('Error loading classrooms:', error);
      this.isLoading = false;

      // Mock data fallback
      this.classrooms = [
        {
          id: 1,
          name: 'Mathematics 10-A',
          description: 'Advanced Mathematics for Grade 10',
          grade: 'Grade 10',
          subject: 'Mathematics',
          schoolId: 1,
          teacherId: 1,
          teacherName: 'John Smith',
          studentCount: 25,
          isActive: true,
          createdAt: '2025-01-15T10:30:00Z'
        },
        {
          id: 2,
          name: 'English Literature 10-B',
          description: 'English Literature and Composition',
          grade: 'Grade 10',
          subject: 'English',
          schoolId: 1,
          teacherId: 2,
          teacherName: 'Sarah Johnson',
          studentCount: 28,
          isActive: true,
          createdAt: '2025-01-20T14:15:00Z'
        },
        {
          id: 3,
          name: 'Chemistry Lab 9-A',
          description: 'Practical Chemistry Laboratory',
          grade: 'Grade 9',
          subject: 'Chemistry',
          schoolId: 1,
          teacherId: 3,
          teacherName: 'Michael Brown',
          studentCount: 20,
          isActive: false,
          createdAt: '2025-01-10T09:00:00Z'
        }
      ];

      this.cdr.detectChanges(); // Update UI after setting fallback data
    }
  });
}


  loadTeachers(): void {
    this.schoolHeadService.getTeachers().subscribe({
      next: (teachers) => {
        this.teachers = teachers;
      },
      error: (error) => {
        console.error('Error loading teachers:', error);
        // Mock data
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
            subject: 'English',
            qualification: 'M.A English Literature',
            schoolId: 1,
            isActive: true,
            createdAt: '2025-01-20T14:15:00Z'
          }
        ];
      }
    });
  }

  loadStudents(): void {
    this.schoolHeadService.getStudents().subscribe({
      next: (students) => {
        this.students = students;
      },
      error: (error) => {
        console.error('Error loading students:', error);
        // Mock data
        this.students = [
          {
            id: 1,
            fullName: 'Alice Johnson',
            username: 'alice.johnson',
            email: 'alice.johnson@student.edu',
            phone: '+1234567890',
            grade: 'Grade 10',
            rollNumber: '001',
            parentName: 'John Johnson',
            parentPhone: '+1234567800',
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
            grade: 'Grade 10',
            rollNumber: '002',
            parentName: 'Mike Smith',
            parentPhone: '+1234567801',
            schoolId: 1,
            isActive: true,
            createdAt: '2025-01-20T14:15:00Z'
          }
        ];
      }
    });
  }

createClassroom(): void {
  if (!this.isFormValid()) {
    return;
  }

  this.isLoading = true;
  this.cdr.detectChanges(); // Update UI to show loading immediately

  this.schoolHeadService.createClassroom(this.newClassroom).subscribe({
    next: (classroom) => {
      this.classrooms.unshift(classroom);
      this.resetForm();
      this.showCreateForm = false;
      this.isLoading = false;
      this.cdr.detectChanges(); // Update UI after success
      console.log('Classroom created successfully with code:', classroom.classCode);
    },
    error: (error) => {
      console.error('Error creating classroom:', error);
      this.isLoading = false;
      this.cdr.detectChanges(); // Update UI after error
    }
  });
}


  assignTeacher(classroom: Classroom, teacherId: number): void {
    const teacher = this.teachers.find(t => t.id === teacherId);
    if (!teacher) return;

    this.http.put(`${this.apiUrl}/classrooms/${classroom.id}/assign-teacher`, { teacherId }).subscribe({
      next: () => {
        classroom.teacherId = teacherId;
        classroom.teacherName = teacher.fullName;
      },
      error: (error) => {
        console.error('Error assigning teacher:', error);
        // Mock assignment for development
        classroom.teacherId = teacherId;
        classroom.teacherName = teacher.fullName;
      }
    });
  }

  onTeacherSelect(classroom: Classroom, event: Event): void {
    const select = event.target as HTMLSelectElement;
    const teacherId = parseInt(select.value);
    if (teacherId > 0) {
      this.assignTeacher(classroom, teacherId);
    }
  }

  toggleClassroomStatus(classroom: Classroom): void {
    classroom.isActive = !classroom.isActive;
    this.http.put(`${this.apiUrl}/classrooms/${classroom.id}/status`, { isActive: classroom.isActive }).subscribe({
      next: () => {
        console.log('Classroom status updated');
      },
      error: (error) => {
        console.error('Error updating classroom status:', error);
        // Revert on error
        classroom.isActive = !classroom.isActive;
      }
    });
  }

  deleteClassroom(classroom: Classroom): void {
    if (confirm(`Are you sure you want to delete ${classroom.name}?`)) {
      this.http.delete(`${this.apiUrl}/classrooms/${classroom.id}`).subscribe({
        next: () => {
          this.classrooms = this.classrooms.filter(c => c.id !== classroom.id);
        },
        error: (error) => {
          console.error('Error deleting classroom:', error);
          // Mock delete for development
          this.classrooms = this.classrooms.filter(c => c.id !== classroom.id);
        }
      });
    }
  }

  viewStudents(classroom: Classroom): void {
    this.selectedClassroom = classroom;
    this.loadClassroomStudents(classroom.id);
    this.showStudentsModal = true;
  }

  loadClassroomStudents(classroomId: number): void {
    this.http.get<Student[]>(`${this.apiUrl}/classrooms/${classroomId}/students`).subscribe({
      next: (students) => {
        this.classroomStudents = students;
      },
      error: (error) => {
        console.error('Error loading classroom students:', error);
        // Mock data - filter students by grade
        if (this.selectedClassroom) {
          this.classroomStudents = this.students.filter(s => 
            s.grade.includes(this.selectedClassroom!.grade.replace('Grade ', ''))
          );
        }
      }
    });
  }

  editClassroom(classroom: Classroom): void {
    this.selectedClassroom = { ...classroom };
  }

  saveClassroom(): void {
    if (!this.selectedClassroom) return;

    this.http.put<Classroom>(`${this.apiUrl}/classrooms/${this.selectedClassroom.id}`, this.selectedClassroom).subscribe({
      next: (updatedClassroom) => {
        const index = this.classrooms.findIndex(c => c.id === updatedClassroom.id);
        if (index !== -1) {
          this.classrooms[index] = updatedClassroom;
        }
        this.selectedClassroom = null;
      },
      error: (error) => {
        console.error('Error updating classroom:', error);
        // Mock update for development
        const index = this.classrooms.findIndex(c => c.id === this.selectedClassroom!.id);
        if (index !== -1) {
          this.classrooms[index] = this.selectedClassroom!;
        }
        this.selectedClassroom = null;
      }
    });
  }

  cancelEdit(): void {
    this.selectedClassroom = null;
  }

  closeStudentsModal(): void {
    this.showStudentsModal = false;
    this.selectedClassroom = null;
    this.classroomStudents = [];
  }

  private isFormValid(): boolean {
    return !!(this.newClassroom.name && 
              this.newClassroom.section && 
              this.newClassroom.subject &&
              this.newClassroom.teacherId > 0);
  }

  resetForm(): void {
    this.newClassroom = {
      name: '',
      description: '',
      section: '',
      subject: '',
      teacherId: 0
    };
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'bg-success' : 'bg-secondary';
  }

  getSubjectBadgeColor(subject: string): string {
    const colors = ['primary', 'success', 'info', 'warning', 'danger'];
    const hash = subject.split('').reduce((a, b) => a + b.charCodeAt(0), 0);
    return `bg-${colors[hash % colors.length]}`;
  }

  getAvailableTeachers(): Teacher[] {
    return this.teachers.filter(t => t.isActive);
  }
}
