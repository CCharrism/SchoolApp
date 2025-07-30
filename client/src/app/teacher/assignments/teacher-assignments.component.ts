import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeacherService } from '../../services/teacher.service';
import { Assignment, CreateAssignmentRequest, Classroom } from '../../models/auth.models';

@Component({
  selector: 'app-teacher-assignments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './teacher-assignments.component.html',
  styleUrl: './teacher-assignments.component.css'
})
export class TeacherAssignmentsComponent implements OnInit {
  assignments: Assignment[] = [];
  classrooms: Classroom[] = [];
  isLoading = false;
  showCreateModal = false;
  activeTab = 'all';
  
  newAssignment: CreateAssignmentRequest = {
    title: '',
    description: '',
    type: 'assignment',
    classroomId: 0,
    dueDate: '',
    totalMarks: 100,
    instructions: ''
  };

  filteredAssignments: Assignment[] = [];

  constructor(private teacherService: TeacherService) {}

  ngOnInit(): void {
    this.loadAssignments();
    this.loadClassrooms();
  }

  loadAssignments(): void {
    this.isLoading = true;
    this.teacherService.getMyAssignments().subscribe({
      next: (assignments) => {
        this.assignments = assignments;
        this.filterAssignments();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading assignments:', error);
        this.isLoading = false;
      }
    });
  }

  loadClassrooms(): void {
    this.teacherService.getMyClassrooms().subscribe({
      next: (classrooms) => {
        this.classrooms = classrooms;
      },
      error: (error) => {
        console.error('Error loading classrooms:', error);
      }
    });
  }

  filterAssignments(): void {
    switch (this.activeTab) {
      case 'assignments':
        this.filteredAssignments = this.assignments.filter(a => a.type === 'assignment');
        break;
      case 'quizzes':
        this.filteredAssignments = this.assignments.filter(a => a.type === 'quiz');
        break;
      case 'graded':
        this.filteredAssignments = this.assignments.filter(a => a.status === 'graded');
        break;
      case 'pending':
        this.filteredAssignments = this.assignments.filter(a => a.status === 'active');
        break;
      default:
        this.filteredAssignments = [...this.assignments];
    }
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
    this.filterAssignments();
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
    this.newAssignment = {
      title: '',
      description: '',
      type: 'assignment',
      classroomId: 0,
      dueDate: '',
      totalMarks: 100,
      instructions: ''
    };
  }

  createAssignment(): void {
    if (this.validateForm()) {
      this.teacherService.createAssignment(this.newAssignment).subscribe({
        next: (assignment) => {
          this.assignments.unshift(assignment);
          this.filterAssignments();
          this.closeCreateModal();
          // Show success message
        },
        error: (error) => {
          console.error('Error creating assignment:', error);
          // Show error message
        }
      });
    }
  }

  validateForm(): boolean {
    return !!(
      this.newAssignment.title &&
      this.newAssignment.description &&
      this.newAssignment.classroomId &&
      this.newAssignment.dueDate &&
      this.newAssignment.totalMarks > 0
    );
  }

  deleteAssignment(id: number): void {
    if (confirm('Are you sure you want to delete this assignment?')) {
      this.teacherService.deleteAssignment(id).subscribe({
        next: () => {
          this.assignments = this.assignments.filter(a => a.id !== id);
          this.filterAssignments();
        },
        error: (error) => {
          console.error('Error deleting assignment:', error);
        }
      });
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  formatDateTime(dateString: string): string {
    return new Date(dateString).toLocaleString();
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'active': return 'bg-success';
      case 'completed': return 'bg-primary';
      case 'graded': return 'bg-info';
      case 'draft': return 'bg-secondary';
      default: return 'bg-secondary';
    }
  }

  getTypeBadgeClass(type: string): string {
    switch (type) {
      case 'assignment': return 'bg-primary';
      case 'quiz': return 'bg-warning';
      case 'exam': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }

  isOverdue(dueDateString: string): boolean {
    return new Date(dueDateString) < new Date();
  }

  getClassroomName(classroomId: number): string {
    const classroom = this.classrooms.find(c => c.id === classroomId);
    return classroom ? classroom.name : 'Unknown Classroom';
  }

  getSubmissionCount(assignmentId: number): number {
    // This would come from the assignment data
    // For now, return a placeholder
    return 0;
  }

  getTotalStudents(classroomId: number): number {
    const classroom = this.classrooms.find(c => c.id === classroomId);
    return classroom ? classroom.studentCount : 0;
  }

  getAssignmentCount(): number {
    return this.assignments.filter(a => a.type === 'assignment').length;
  }

  getQuizCount(): number {
    return this.assignments.filter(a => a.type === 'quiz').length;
  }
}
