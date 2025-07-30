import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Teacher, 
  Student, 
  Classroom, 
  Assignment, 
  CreateTeacherRequest, 
  CreateStudentRequest, 
  CreateClassroomRequest,
  CreateSchoolHeadClassroomRequest,
  CreateAssignmentRequest
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class SchoolHeadService {
  private apiUrl = 'http://localhost:5021/api';

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  // Teacher Management
  getTeachers(): Observable<Teacher[]> {
    return this.http.get<Teacher[]>(`${this.apiUrl}/schoolhead/teachers`, {
      headers: this.getAuthHeaders()
    });
  }

  createTeacher(teacher: CreateTeacherRequest): Observable<Teacher> {
    return this.http.post<Teacher>(`${this.apiUrl}/schoolhead/teachers`, teacher, {
      headers: this.getAuthHeaders()
    });
  }

  updateTeacher(id: number, teacher: Partial<CreateTeacherRequest>): Observable<Teacher> {
    return this.http.put<Teacher>(`${this.apiUrl}/schoolhead/teachers/${id}`, teacher, {
      headers: this.getAuthHeaders()
    });
  }

  deleteTeacher(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/schoolhead/teachers/${id}`, {
      headers: this.getAuthHeaders()
    });
  }

  toggleTeacherStatus(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/schoolhead/teachers/${id}/toggle-status`, {}, {
      headers: this.getAuthHeaders()
    });
  }

  // Student Management
  getStudents(): Observable<Student[]> {
    return this.http.get<Student[]>(`${this.apiUrl}/schoolhead/students`, {
      headers: this.getAuthHeaders()
    });
  }

  createStudent(student: CreateStudentRequest): Observable<Student> {
    return this.http.post<Student>(`${this.apiUrl}/schoolhead/students`, student, {
      headers: this.getAuthHeaders()
    });
  }

  updateStudent(id: number, student: Partial<CreateStudentRequest>): Observable<Student> {
    return this.http.put<Student>(`${this.apiUrl}/schoolhead/students/${id}`, student, {
      headers: this.getAuthHeaders()
    });
  }

  deleteStudent(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/schoolhead/students/${id}`, {
      headers: this.getAuthHeaders()
    });
  }

  toggleStudentStatus(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/schoolhead/students/${id}/toggle-status`, {}, {
      headers: this.getAuthHeaders()
    });
  }

  // Classroom Management
  getClassrooms(): Observable<Classroom[]> {
    return this.http.get<Classroom[]>(`${this.apiUrl}/schoolhead/classrooms`, {
      headers: this.getAuthHeaders()
    });
  }

  createClassroom(classroom: CreateSchoolHeadClassroomRequest): Observable<Classroom> {
    return this.http.post<Classroom>(`${this.apiUrl}/schoolhead/classrooms`, classroom, {
      headers: this.getAuthHeaders()
    });
  }

  updateClassroom(id: number, classroom: Partial<CreateSchoolHeadClassroomRequest>): Observable<Classroom> {
    return this.http.put<Classroom>(`${this.apiUrl}/schoolhead/classrooms/${id}`, classroom, {
      headers: this.getAuthHeaders()
    });
  }

  deleteClassroom(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/schoolhead/classrooms/${id}`, {
      headers: this.getAuthHeaders()
    });
  }

  assignTeacherToClassroom(classroomId: number, teacherId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/schoolhead/classrooms/${classroomId}/teacher`, { teacherId }, {
      headers: this.getAuthHeaders()
    });
  }

  assignStudentToClassroom(classroomId: number, studentId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/schoolhead/classrooms/${classroomId}/students/${studentId}`, {}, {
      headers: this.getAuthHeaders()
    });
  }

  removeStudentFromClassroom(classroomId: number, studentId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/schoolhead/classrooms/${classroomId}/students/${studentId}`, {
      headers: this.getAuthHeaders()
    });
  }

  getClassroomStudents(classroomId: number): Observable<Student[]> {
    return this.http.get<Student[]>(`${this.apiUrl}/schoolhead/classrooms/${classroomId}/students`, {
      headers: this.getAuthHeaders()
    });
  }

  // Dashboard stats
  getDashboardStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/schoolhead/dashboard-stats`, {
      headers: this.getAuthHeaders()
    });
  }
}
