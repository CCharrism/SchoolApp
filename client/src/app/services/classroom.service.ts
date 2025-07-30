import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Classroom {
  id: number;
  name: string;
  subject: string;
  section: string;
  description?: string;
  classCode: string;
  teacherId: number;
  teacherName?: string;
  studentCount?: number;
  assignmentCount?: number;
  isActive: boolean;
  createdAt: string;
}

export interface ClassroomStudent {
  id: number;
  classroomId: number;
  studentId: number;
  studentName: string;
  joinedAt: string;
}

export interface Assignment {
  id: number;
  classroomId: number;
  title: string;
  description: string;
  dueDate: string;
  totalPoints: number;
  points?: number;  // Add alias for API compatibility
  isPublished: boolean;
  submissionCount?: number;
  gradedCount?: number;
  createdAt: string;
}

export interface Announcement {
  id: number;
  classroomId: number;
  classroomName?: string;
  title: string;
  content: string;
  authorName?: string;
  createdAt: string;
}

export interface DashboardData {
  classrooms: Classroom[];
  totalStudents: number;
  totalAssignments: number;
  pendingAssignments: number;
  recentClassrooms: Classroom[];
  recentAssignments: Assignment[];
  recentAnnouncements: Announcement[];
  
  // Student-specific fields
  enrolledClasses?: number;
  upcomingTests?: number;
  enrolledClassrooms?: Classroom[];
}

@Injectable({
  providedIn: 'root'
})
export class ClassroomService {
  private baseUrl = 'http://localhost:5021/api';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = sessionStorage.getItem('token');
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  // Dashboard
  getTeacherDashboard(): Observable<DashboardData> {
    return this.http.get<any>(`${this.baseUrl}/dashboard/teacher`, {
      headers: this.getHeaders()
    }).pipe(
      map((response: any) => {
        // Transform the API response to match our interface
        return {
          classrooms: response.recentClassrooms || [],
          totalStudents: response.totalStudents || 0,
          totalAssignments: response.totalAssignments || 0,
          pendingAssignments: response.pendingAssignments || 0,
          recentClassrooms: response.recentClassrooms || [],
          recentAssignments: response.recentAssignments || [],
          recentAnnouncements: response.recentAnnouncements || []
        } as DashboardData;
      })
    );
  }

  getStudentDashboard(): Observable<DashboardData> {
    return this.http.get<any>(`${this.baseUrl}/dashboard/student`, {
      headers: this.getHeaders()
    }).pipe(
      map((response: any) => {
        // Transform the API response to match our interface
        return {
          classrooms: response.classrooms || response.enrolledClassrooms || [],
          totalStudents: response.totalStudents || 0,
          totalAssignments: response.totalAssignments || 0,
          pendingAssignments: response.pendingAssignments || 0,
          recentClassrooms: response.recentClassrooms || response.classrooms || [],
          recentAssignments: response.recentAssignments || [],
          recentAnnouncements: response.recentAnnouncements || [],
          enrolledClasses: response.enrolledClasses || 0,
          upcomingTests: response.upcomingTests || 0,
          enrolledClassrooms: response.enrolledClassrooms || response.classrooms || []
        } as DashboardData;
      })
    );
  }

  // Classrooms
  getClassrooms(): Observable<Classroom[]> {
    return this.http.get<Classroom[]>(`${this.baseUrl}/classrooms`, {
      headers: this.getHeaders()
    });
  }

  getClassroom(id: number): Observable<Classroom> {
    return this.http.get<Classroom>(`${this.baseUrl}/classrooms/${id}`, {
      headers: this.getHeaders()
    });
  }

  joinClassroom(classCode: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/classrooms/join`, { classCode }, {
      headers: this.getHeaders()
    });
  }

  getClassroomStudents(classroomId: number): Observable<ClassroomStudent[]> {
    return this.http.get<ClassroomStudent[]>(`${this.baseUrl}/classrooms/${classroomId}/students`, {
      headers: this.getHeaders()
    });
  }

  removeStudentFromClassroom(classroomId: number, studentId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/classrooms/${classroomId}/students/${studentId}`, {
      headers: this.getHeaders()
    });
  }

  // Assignments
  getClassroomAssignments(classroomId: number): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.baseUrl}/assignments/classroom/${classroomId}`, {
      headers: this.getHeaders()
    });
  }

  createAssignment(assignment: {
    classroomId: number;
    title: string;
    description: string;
    dueDate: string;
    totalPoints: number;
  }): Observable<Assignment> {
    return this.http.post<Assignment>(`${this.baseUrl}/assignments`, assignment, {
      headers: this.getHeaders()
    });
  }

  getAssignment(id: number): Observable<Assignment> {
    return this.http.get<Assignment>(`${this.baseUrl}/assignments/${id}`, {
      headers: this.getHeaders()
    });
  }

  // Announcements
  getAnnouncements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.baseUrl}/announcements`, {
      headers: this.getHeaders()
    });
  }

  getClassroomAnnouncements(classroomId: number): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.baseUrl}/classrooms/${classroomId}/announcements`, {
      headers: this.getHeaders()
    });
  }

  createAnnouncement(announcement: {
    classroomId: number;
    title: string;
    content: string;
  }): Observable<Announcement> {
    return this.http.post<Announcement>(`${this.baseUrl}/announcements`, announcement, {
      headers: this.getHeaders()
    });
  }

  createClassroomAnnouncement(classroomId: number, announcement: {
    title: string;
    content: string;
  }): Observable<Announcement> {
    return this.http.post<Announcement>(`${this.baseUrl}/classrooms/${classroomId}/announcements`, announcement, {
      headers: this.getHeaders()
    });
  }

  deleteAnnouncement(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/announcements/${id}`, {
      headers: this.getHeaders()
    });
  }
}
