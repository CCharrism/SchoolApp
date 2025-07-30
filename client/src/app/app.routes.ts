import { Routes } from '@angular/router';
import { TestComponent } from './test/test.component';
import { SimpleLoginComponent } from './simple-login/simple-login.component';
import { LoginComponent } from './login/login.component';
import { DashboardComponent } from './admin/dashboard/dashboard.component';
import { ManageSchoolsComponent } from './admin/manage-schools/manage-schools.component';
import { OwnerDashboardComponent } from './owner/dashboard/owner-dashboard.component';
import { SchoolHeadDashboardComponent } from './school-head/dashboard/school-head-dashboard.component';
import { CourseListComponent } from './school-head/course-list/course-list.component';
import { CourseCreateComponent } from './school-head/course-create/course-create.component';
import { NewTeacherDashboardComponent } from './teacher/dashboard/new-teacher-dashboard.component';
import { NewStudentDashboardComponent } from './student/dashboard/new-student-dashboard.component';
import { ClassroomDetailComponent } from './student/classroom-detail/classroom-detail.component';
import { TeacherClassroomDetailComponent } from './teacher/teacher-classroom-detail/teacher-classroom-detail.component';
import { AuthGuard } from './admin/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'test', component: TestComponent },
  { path: 'simple-login', component: SimpleLoginComponent },
  { path: 'login', component: LoginComponent },
  { 
    path: 'admin/dashboard', 
    component: DashboardComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'admin/manage-schools', 
    component: ManageSchoolsComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'owner/dashboard', 
    component: OwnerDashboardComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'school-head/dashboard', 
    component: SchoolHeadDashboardComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'school-head/courses', 
    component: CourseListComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'school-head/create-course', 
    component: CourseCreateComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'teacher/dashboard', 
    component: NewTeacherDashboardComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'teacher/classroom/:id', 
    component: TeacherClassroomDetailComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'student/dashboard', 
    component: NewStudentDashboardComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'student/classroom/:id', 
    component: ClassroomDetailComponent,
    canActivate: [AuthGuard]
  },
  { path: '**', redirectTo: '/login' }
];
