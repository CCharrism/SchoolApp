export interface SchoolHierarchy {
  schoolId: number;
  schoolName: string;
  schoolOwner: SchoolOwner;
  schoolHeads: SchoolHead[];
  totalBranches: number;
  totalCourses: number;
  createdAt: string;
  createdByAdmin: string;
}

export interface SchoolOwner {
  id: number;
  name: string;
  username: string;
  email: string;
  phone: string;
  address: string;
  isActive: boolean;
  createdAt: string;
}

export interface SchoolHead {
  id: number;
  username: string;
  branchId: number;
  branchName: string;
  branchLocation: string;
  courseCount: number;
  isActive: boolean;
  createdAt: string;
}

export interface AdminStatistics {
  totalSchools: number;
  totalOwners: number;
  totalSchoolHeads: number;
  totalBranches: number;
  totalCourses: number;
  recentSchools: RecentSchool[];
}

export interface RecentSchool {
  schoolName: string;
  ownerName: string;
  createdAt: string;
  createdByAdmin: string;
}
