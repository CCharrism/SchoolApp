import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface Course {
  id: number;
  courseName: string;
  description: string;
  teacherId: number;
  teacherName: string;
  subject: string;
  grade: string;
  studentsCount: number;
  roomNumber: string;
  isActive: boolean;
  createdAt: string;
}

export interface Assignment {
  id: number;
  title: string;
  description: string;
  dueDate: string;
  courseId: number;
  courseName: string;
  teacherId?: number;
  teacherName?: string;
  isSubmitted?: boolean;
  submittedAt?: string;
  submittedDate?: string | null;
  status: 'pending' | 'submitted' | 'completed' | 'graded';
  grade?: number;
  maxPoints?: number;
  earnedPoints?: number | null;
  instructions?: string;
  attachments?: string[];
  createdAt?: string;
}

export interface Activity {
  id: number;
  description: string;
  time: string;
  type: 'assignment' | 'submission' | 'meeting' | 'quiz';
}

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private apiUrl = 'http://localhost:5021/api';

  constructor(private http: HttpClient) {}

  getTeacherCourses(teacherId: number): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.apiUrl}/teachers/${teacherId}/courses`).pipe(
      catchError(error => {
        console.error('Error loading teacher courses:', error);
        return this.getMockTeacherCourses(teacherId);
      })
    );
  }

  getStudentCourses(studentId: number): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.apiUrl}/students/${studentId}/courses`).pipe(
      catchError(error => {
        console.error('Error loading student courses:', error);
        return this.getMockStudentCourses(studentId);
      })
    );
  }

  getTeacherAssignments(teacherId: number): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.apiUrl}/teachers/${teacherId}/assignments`).pipe(
      catchError(error => {
        console.error('Error loading teacher assignments:', error);
        return this.getMockTeacherAssignments();
      })
    );
  }

  getStudentAssignments(studentId: number): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${this.apiUrl}/students/${studentId}/assignments`).pipe(
      catchError(error => {
        console.error('Error loading student assignments:', error);
        return this.getMockStudentAssignments();
      })
    );
  }

  getRecentActivity(userId: number, role: string): Observable<Activity[]> {
    return this.http.get<Activity[]>(`${this.apiUrl}/${role.toLowerCase()}s/${userId}/activity`).pipe(
      catchError(error => {
        console.error('Error loading recent activity:', error);
        return this.getMockActivity(role);
      })
    );
  }

  private getMockTeacherCourses(teacherId: number): Observable<Course[]> {
    const courses: Course[] = [
      {
        id: 1,
        courseName: 'Mathematics 10A',
        description: 'Advanced Mathematics for Grade 10',
        teacherId: teacherId,
        teacherName: 'John Smith',
        subject: 'Mathematics',
        grade: 'Grade 10-A',
        studentsCount: 25,
        roomNumber: 'Room 101',
        isActive: true,
        createdAt: '2025-01-15T10:30:00Z'
      },
      {
        id: 2,
        courseName: 'Advanced Algebra',
        description: 'Advanced Algebra concepts and applications',
        teacherId: teacherId,
        teacherName: 'John Smith',
        subject: 'Mathematics',
        grade: 'Grade 11-A',
        studentsCount: 18,
        roomNumber: 'Room 102',
        isActive: true,
        createdAt: '2025-01-16T11:45:00Z'
      }
    ];
    return of(courses);
  }

  private getMockStudentCourses(studentId: number): Observable<Course[]> {
    const courses: Course[] = [
      {
        id: 1,
        courseName: 'Mathematics 10A',
        description: 'Advanced Mathematics for Grade 10',
        teacherId: 2,
        teacherName: 'John Smith',
        subject: 'Mathematics',
        grade: 'Grade 10-A',
        studentsCount: 25,
        roomNumber: 'Room 101',
        isActive: true,
        createdAt: '2025-01-15T10:30:00Z'
      },
      {
        id: 3,
        courseName: 'English Literature 10A',
        description: 'Literature and Language Arts',
        teacherId: 3,
        teacherName: 'Sarah Johnson',
        subject: 'English',
        grade: 'Grade 10-A',
        studentsCount: 25,
        roomNumber: 'Room 203',
        isActive: true,
        createdAt: '2025-01-17T09:00:00Z'
      },
      {
        id: 4,
        courseName: 'Physics 10A',
        description: 'Introduction to Physics',
        teacherId: 4,
        teacherName: 'Michael Brown',
        subject: 'Physics',
        grade: 'Grade 10-A',
        studentsCount: 25,
        roomNumber: 'Room 301',
        isActive: true,
        createdAt: '2025-01-18T14:30:00Z'
      }
    ];
    return of(courses);
  }

  private getMockTeacherAssignments(): Observable<Assignment[]> {
    const assignments: Assignment[] = [
      {
        id: 1,
        title: 'Quadratic Equations Homework',
        description: 'Solve problems 1-20 from Chapter 5',
        dueDate: '2025-07-30T23:59:00Z',
        courseId: 1,
        courseName: 'Mathematics 10A',
        status: 'pending'
      },
      {
        id: 2,
        title: 'Algebra Quiz Preparation',
        description: 'Study guide for upcoming quiz',
        dueDate: '2025-07-28T23:59:00Z',
        courseId: 2,
        courseName: 'Advanced Algebra',
        status: 'pending'
      }
    ];
    return of(assignments);
  }

  private getMockStudentAssignments(): Observable<Assignment[]> {
    const assignments: Assignment[] = [
      {
        id: 1,
        title: 'Quadratic Equations Homework',
        description: 'Solve problems 1-20 from Chapter 5',
        dueDate: '2025-07-30T23:59:00Z',
        courseId: 1,
        courseName: 'Mathematics 10A',
        isSubmitted: false,
        status: 'pending'
      },
      {
        id: 2,
        title: 'Literature Essay',
        description: 'Write a 500-word essay on Shakespeare',
        dueDate: '2025-07-29T23:59:00Z',
        courseId: 3,
        courseName: 'English Literature 10A',
        isSubmitted: true,
        submittedAt: '2025-07-26T15:30:00Z',
        grade: 85,
        status: 'graded'
      },
      {
        id: 3,
        title: 'Physics Lab Report',
        description: 'Submit lab report for pendulum experiment',
        dueDate: '2025-08-01T23:59:00Z',
        courseId: 4,
        courseName: 'Physics 10A',
        isSubmitted: false,
        status: 'pending'
      }
    ];
    return of(assignments);
  }

  private getMockActivity(role: string): Observable<Activity[]> {
    if (role === 'Teacher') {
      return of([
        {
          id: 1,
          description: 'New assignment submitted by Alice Johnson',
          time: '2 hours ago',
          type: 'submission'
        },
        {
          id: 2,
          description: 'Quiz scheduled for Mathematics 10A',
          time: '5 hours ago',
          type: 'quiz'
        },
        {
          id: 3,
          description: 'Parent meeting request from John Smith',
          time: '1 day ago',
          type: 'meeting'
        }
      ]);
    } else {
      return of([
        {
          id: 1,
          description: 'Assignment graded: Literature Essay (85/100)',
          time: '1 hour ago',
          type: 'assignment'
        },
        {
          id: 2,
          description: 'New assignment posted: Physics Lab Report',
          time: '3 hours ago',
          type: 'assignment'
        },
        {
          id: 3,
          description: 'Quiz reminder: Mathematics Quiz tomorrow',
          time: '6 hours ago',
          type: 'quiz'
        }
      ]);
    }
  }
}
