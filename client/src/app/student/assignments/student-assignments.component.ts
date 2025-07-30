import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentService } from '../../services/student.service';
import { Assignment, AssignmentSubmission, SubmitAssignmentRequest } from '../../models/auth.models';

@Component({
  selector: 'app-student-assignments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './student-assignments.component.html',
  styleUrl: './student-assignments.component.css'
})
export class StudentAssignmentsComponent implements OnInit {
  assignments: Assignment[] = [];
  mySubmissions: AssignmentSubmission[] = [];
  selectedAssignment: Assignment | null = null;
  showSubmissionModal = false;
  isLoading = false;
  activeTab = 'pending';
  
  submissionData: SubmitAssignmentRequest = {
    assignmentId: 0,
    content: '',
    attachmentUrl: ''
  };

  constructor(private studentService: StudentService) {}

  ngOnInit(): void {
    this.loadAssignments();
    this.loadMySubmissions();
  }

  loadAssignments(): void {
    this.isLoading = true;
    this.studentService.getPendingAssignments().subscribe({
      next: (assignments) => {
        this.assignments = assignments;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading assignments:', error);
        this.isLoading = false;
      }
    });
  }

  loadMySubmissions(): void {
    this.studentService.getMySubmissions().subscribe({
      next: (submissions) => {
        this.mySubmissions = submissions;
      },
      error: (error) => {
        console.error('Error loading submissions:', error);
      }
    });
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  getFilteredAssignments(): Assignment[] {
    switch (this.activeTab) {
      case 'pending':
        return this.assignments.filter(a => !this.isSubmitted(a.id));
      case 'submitted':
        return this.assignments.filter(a => this.isSubmitted(a.id));
      case 'graded':
        return this.assignments.filter(a => this.isGraded(a.id));
      case 'overdue':
        return this.assignments.filter(a => this.isOverdue(a.dueDate) && !this.isSubmitted(a.id));
      default:
        return this.assignments;
    }
  }

  isSubmitted(assignmentId: number): boolean {
    return this.mySubmissions.some(s => s.assignmentId === assignmentId);
  }

  isGraded(assignmentId: number): boolean {
    const submission = this.mySubmissions.find(s => s.assignmentId === assignmentId);
    return submission ? submission.isGraded : false;
  }

  getSubmission(assignmentId: number): AssignmentSubmission | undefined {
    return this.mySubmissions.find(s => s.assignmentId === assignmentId);
  }

  openSubmissionModal(assignment: Assignment): void {
    this.selectedAssignment = assignment;
    this.showSubmissionModal = true;
    this.submissionData.assignmentId = assignment.id;
  }

  closeSubmissionModal(): void {
    this.showSubmissionModal = false;
    this.selectedAssignment = null;
    this.resetSubmissionForm();
  }

  resetSubmissionForm(): void {
    this.submissionData = {
      assignmentId: 0,
      content: '',
      attachmentUrl: ''
    };
  }

  submitAssignment(): void {
    if (this.validateSubmission()) {
      this.studentService.submitAssignment(this.submissionData).subscribe({
        next: (submission) => {
          this.mySubmissions.push(submission);
          this.closeSubmissionModal();
          // Show success message
        },
        error: (error) => {
          console.error('Error submitting assignment:', error);
          // Show error message
        }
      });
    }
  }

  validateSubmission(): boolean {
    return !!(this.submissionData.content && this.submissionData.content.trim().length > 0);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  formatDateTime(dateString: string): string {
    return new Date(dateString).toLocaleString();
  }

  isOverdue(dueDateString: string): boolean {
    return new Date(dueDateString) < new Date();
  }

  getDaysUntilDue(dueDateString: string): number {
    const dueDate = new Date(dueDateString);
    const today = new Date();
    const diffTime = dueDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }

  getStatusBadgeClass(assignment: Assignment): string {
    if (this.isOverdue(assignment.dueDate) && !this.isSubmitted(assignment.id)) {
      return 'bg-danger';
    }
    if (this.isGraded(assignment.id)) {
      return 'bg-success';
    }
    if (this.isSubmitted(assignment.id)) {
      return 'bg-info';
    }
    return 'bg-warning';
  }

  getStatusText(assignment: Assignment): string {
    if (this.isOverdue(assignment.dueDate) && !this.isSubmitted(assignment.id)) {
      return 'Overdue';
    }
    if (this.isGraded(assignment.id)) {
      return 'Graded';
    }
    if (this.isSubmitted(assignment.id)) {
      return 'Submitted';
    }
    return 'Pending';
  }

  getTypeBadgeClass(type: string): string {
    switch (type) {
      case 'assignment': return 'bg-primary';
      case 'quiz': return 'bg-warning';
      case 'exam': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }
}
