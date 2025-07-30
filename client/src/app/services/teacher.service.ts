import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Assignment, 
  AssignmentSubmission, 
  Classroom, 
  Student,
  CreateAssignmentRequest,
  CreateClassroomRequest
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class TeacherService {
  private apiUrl = 'http://localhost:5021/api';

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  // Classroom Management (Teacher's perspective)
  getMyClassrooms(): Observable<Classroom[]> {
    return this.http.get<Classroom[]>(`${this.apiUrl}/teacher/classrooms`, {
      headers: this.getAuthHeaders()
    });
  }

  getClassroomStudents(classroomId: number): Observable<Student[]> {
    return this.http.get<Student[]>(`${this.apiUrl}/teacher/classrooms/${classroomId}/students`, {
      headers: this.getAuthHeaders()
    });
  }

  // Assignment Management
  getMyAssignments(): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.apiUrl}/teacher/assignments`, {
      headers: this.getAuthHeaders()
    });
  }

  getAssignmentsByType(type: 'assignment' | 'quiz'): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.apiUrl}/teacher/assignments?type=${type}`, {
      headers: this.getAuthHeaders()
    });
  }

  createAssignment(assignment: CreateAssignmentRequest): Observable<Assignment> {
    return this.http.post<Assignment>(`${this.apiUrl}/teacher/assignments`, assignment, {
      headers: this.getAuthHeaders()
    });
  }

  updateAssignment(id: number, assignment: Partial<CreateAssignmentRequest>): Observable<Assignment> {
    return this.http.put<Assignment>(`${this.apiUrl}/teacher/assignments/${id}`, assignment, {
      headers: this.getAuthHeaders()
    });
  }

  deleteAssignment(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/teacher/assignments/${id}`, {
      headers: this.getAuthHeaders()
    });
  }

  // Submission Management
  getAssignmentSubmissions(assignmentId: number): Observable<AssignmentSubmission[]> {
    return this.http.get<AssignmentSubmission[]>(`${this.apiUrl}/teacher/assignments/${assignmentId}/submissions`, {
      headers: this.getAuthHeaders()
    });
  }

  gradeSubmission(submissionId: number, marks: number, feedback?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/teacher/submissions/${submissionId}/grade`, {
      marks,
      feedback
    }, {
      headers: this.getAuthHeaders()
    });
  }

  // Dashboard stats
  getDashboardStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/teacher/dashboard-stats`, {
      headers: this.getAuthHeaders()
    });
  }

  // Quick actions
  getRecentSubmissions(): Observable<AssignmentSubmission[]> {
    return this.http.get<AssignmentSubmission[]>(`${this.apiUrl}/teacher/recent-submissions`, {
      headers: this.getAuthHeaders()
    });
  }

  getUpcomingDeadlines(): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.apiUrl}/teacher/upcoming-deadlines`, {
      headers: this.getAuthHeaders()
    });
  }

  // Additional Classroom Management
  createClassroom(classroomData: CreateClassroomRequest): Observable<Classroom> {
    return this.http.post<Classroom>(`${this.apiUrl}/teacher/classrooms`, classroomData, {
      headers: this.getAuthHeaders()
    });
  }

  deleteClassroom(classroomId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/teacher/classrooms/${classroomId}`, {
      headers: this.getAuthHeaders()
    });
  }
}
