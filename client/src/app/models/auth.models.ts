export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
  expiresAt: string;
  schoolName?: string; // Optional field for school owners
  user?: User; // Add user field for detailed user info
}

export interface User {
  id: number;
  username: string;
  role: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  schoolName?: string; // Optional field for school owners
}

export interface CreateSchoolRequest {
  schoolName: string;
  ownerName: string;
  ownerUsername: string;
  ownerPassword: string;
  address?: string;
  phone?: string;
  email?: string;
}

export interface School {
  id: number;
  schoolName: string;
  ownerName: string;
  ownerUsername: string;
  address: string;
  phone: string;
  email: string;
  isActive: boolean;
  createdAt: string;
  createdByAdmin: string;
}

// New interfaces for school management system
export interface Teacher {
  id: number;
  fullName: string;
  username: string;
  email: string;
  phone: string;
  subject: string;
  qualification: string;
  schoolId: number;
  isActive: boolean;
  createdAt: string;
}

export interface Student {
  id: number;
  fullName: string;
  username: string;
  email: string;
  phone: string;
  rollNumber: string;
  grade: string;
  parentName: string;
  parentPhone: string;
  schoolId: number;
  isActive: boolean;
  createdAt: string;
}

export interface Classroom {
  id: number;
  name: string;
  description: string;
  grade: string;
  subject: string;
  schoolId: number;
  teacherId: number;
  teacherName: string;
  studentCount: number;
  isActive: boolean;
  createdAt: string;
  classCode?: string;
}

export interface CreateSchoolHeadClassroomRequest {
  name: string;
  description: string;
  subject: string;
  section: string;
  teacherId: number;
}

export interface Assignment {
  id: number;
  title: string;
  description: string;
  type: 'assignment' | 'quiz' | 'exam';
  classroomId: number;
  classroomName?: string;
  teacherId: number;
  dueDate: string;
  totalMarks: number;
  status: 'draft' | 'active' | 'completed' | 'graded';
  submissionCount?: number;
  totalStudents?: number;
  isActive: boolean;
  createdAt: string;
  submissions?: AssignmentSubmission[];
}

export interface AssignmentSubmission {
  id: number;
  assignmentId: number;
  assignmentTitle?: string;
  studentId: number;
  studentName: string;
  classroomName?: string;
  content: string;
  attachmentUrl?: string;
  submittedAt: string;
  marks?: number;
  score?: number;
  feedback?: string;
  isGraded: boolean;
  gradedAt?: string;
}

export interface CreateTeacherRequest {
  fullName: string;
  username: string;
  password: string;
  email: string;
  phone: string;
  subject: string;
  qualification: string;
}

export interface CreateStudentRequest {
  fullName: string;
  username: string;
  password: string;
  email: string;
  phone: string;
  rollNumber: string;
  grade: string;
  parentName: string;
  parentPhone: string;
}

export interface CreateClassroomRequest {
  name: string;
  description: string;
  grade: string;
  subject: string;
  teacherId: number;
}

export interface CreateAssignmentRequest {
  title: string;
  description: string;
  type: 'assignment' | 'quiz' | 'exam';
  classroomId: number;
  dueDate: string;
  totalMarks: number;
  instructions?: string;
}

export interface SubmitAssignmentRequest {
  assignmentId: number;
  content: string;
  attachmentUrl?: string;
}
