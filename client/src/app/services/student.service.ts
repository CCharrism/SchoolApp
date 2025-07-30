import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Assignment, 
  AssignmentSubmission, 
  Classroom,
  SubmitAssignmentRequest
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private apiUrl = 'http://localhost:5021/api';

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  // Classroom Management (Student's perspective)
  getMyClassrooms(): Observable<Classroom[]> {
    return this.http.get<Classroom[]>(`${this.apiUrl}/student/classrooms`, {
      headers: this.getAuthHeaders()
    });
  }

  // Assignment Management
  getMyAssignments(): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.apiUrl}/student/assignments`, {
      headers: this.getAuthHeaders()
    });
  }

  getAssignmentsByType(type: 'assignment' | 'quiz'): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.apiUrl}/student/assignments?type=${type}`, {
      headers: this.getAuthHeaders()
    });
  }

  getAssignmentById(id: number): Observable<Assignment> {
    return this.http.get<Assignment>(`${this.apiUrl}/student/assignments/${id}`, {
      headers: this.getAuthHeaders()
    });
  }

  // Submission Management
  submitAssignment(submission: SubmitAssignmentRequest): Observable<AssignmentSubmission> {
    return this.http.post<AssignmentSubmission>(`${this.apiUrl}/student/submissions`, submission, {
      headers: this.getAuthHeaders()
    });
  }

  updateSubmission(submissionId: number, content: string, attachmentUrl?: string): Observable<AssignmentSubmission> {
    return this.http.put<AssignmentSubmission>(`${this.apiUrl}/student/submissions/${submissionId}`, {
      content,
      attachmentUrl
    }, {
      headers: this.getAuthHeaders()
    });
  }

  getMySubmissions(): Observable<AssignmentSubmission[]> {
    return this.http.get<AssignmentSubmission[]>(`${this.apiUrl}/student/submissions`, {
      headers: this.getAuthHeaders()
    });
  }

  getSubmissionByAssignmentId(assignmentId: number): Observable<AssignmentSubmission> {
    return this.http.get<AssignmentSubmission>(`${this.apiUrl}/student/assignments/${assignmentId}/submission`, {
      headers: this.getAuthHeaders()
    });
  }

  // Dashboard stats
  getDashboardStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/student/dashboard-stats`, {
      headers: this.getAuthHeaders()
    });
  }

  // Quick actions
  getPendingAssignments(): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.apiUrl}/student/pending-assignments`, {
      headers: this.getAuthHeaders()
    });
  }

  getRecentGrades(): Observable<AssignmentSubmission[]> {
    return this.http.get<AssignmentSubmission[]>(`${this.apiUrl}/student/recent-grades`, {
      headers: this.getAuthHeaders()
    });
  }

  getUpcomingDeadlines(): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.apiUrl}/student/upcoming-deadlines`, {
      headers: this.getAuthHeaders()
    });
  }
}
