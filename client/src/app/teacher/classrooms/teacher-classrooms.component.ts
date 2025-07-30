import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeacherService } from '../../services/teacher.service';
import { Classroom, CreateClassroomRequest, Student } from '../../models/auth.models';

@Component({
  selector: 'app-teacher-classrooms',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './teacher-classrooms.component.html',
  styleUrl: './teacher-classrooms.component.css'
})
export class TeacherClassroomsComponent implements OnInit {
  classrooms: Classroom[] = [];
  students: Student[] = [];
  selectedClassroom: Classroom | null = null;
  isLoading = false;
  showCreateModal = false;
  showStudentsModal = false;
  activeTab = 'all';
  
  newClassroom: CreateClassroomRequest = {
    name: '',
    description: '',
    grade: '',
    subject: '',
    teacherId: 0
  };

  constructor(private teacherService: TeacherService) {}

  ngOnInit(): void {
    this.loadClassrooms();
  }

  loadClassrooms(): void {
    this.isLoading = true;
    this.teacherService.getMyClassrooms().subscribe({
      next: (classrooms) => {
        this.classrooms = classrooms;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading classrooms:', error);
        this.isLoading = false;
      }
    });
  }

  openCreateModal(): void {
    this.showCreateModal = true;
    this.resetForm();
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.resetForm();
  }

  resetForm(): void {
    this.newClassroom = {
      name: '',
      description: '',
      grade: '',
      subject: '',
      teacherId: 0
    };
  }

  createClassroom(): void {
    if (this.validateForm()) {
      this.teacherService.createClassroom(this.newClassroom).subscribe({
        next: (classroom) => {
          this.classrooms.unshift(classroom);
          this.closeCreateModal();
        },
        error: (error) => {
          console.error('Error creating classroom:', error);
        }
      });
    }
  }

  validateForm(): boolean {
    return !!(
      this.newClassroom.name &&
      this.newClassroom.description &&
      this.newClassroom.grade &&
      this.newClassroom.subject
    );
  }

  viewStudents(classroom: Classroom): void {
    this.selectedClassroom = classroom;
    this.showStudentsModal = true;
    this.loadStudents(classroom.id);
  }

  loadStudents(classroomId: number): void {
    this.teacherService.getClassroomStudents(classroomId).subscribe({
      next: (students) => {
        this.students = students;
      },
      error: (error) => {
        console.error('Error loading students:', error);
      }
    });
  }

  closeStudentsModal(): void {
    this.showStudentsModal = false;
    this.selectedClassroom = null;
    this.students = [];
  }

  deleteClassroom(id: number): void {
    if (confirm('Are you sure you want to delete this classroom?')) {
      this.teacherService.deleteClassroom(id).subscribe({
        next: () => {
          this.classrooms = this.classrooms.filter(c => c.id !== id);
        },
        error: (error) => {
          console.error('Error deleting classroom:', error);
        }
      });
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  getGradeColor(grade: string): string {
    const gradeNum = parseInt(grade);
    if (gradeNum <= 5) return 'primary';
    if (gradeNum <= 8) return 'success';
    if (gradeNum <= 10) return 'warning';
    return 'info';
  }
}
