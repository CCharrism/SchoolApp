CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Username" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL
);

CREATE TABLE "Schools" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Schools" PRIMARY KEY AUTOINCREMENT,
    "SchoolName" TEXT NOT NULL,
    "OwnerName" TEXT NOT NULL,
    "OwnerUsername" TEXT NOT NULL,
    "OwnerPasswordHash" TEXT NOT NULL,
    "Address" TEXT NOT NULL,
    "Phone" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedByAdminId" INTEGER NOT NULL,
    CONSTRAINT "FK_Schools_Users_CreatedByAdminId" FOREIGN KEY ("CreatedByAdminId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Schools_CreatedByAdminId" ON "Schools" ("CreatedByAdminId");

CREATE UNIQUE INDEX "IX_Schools_OwnerUsername" ON "Schools" ("OwnerUsername");

CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250723071234_AddSchoolsTable', '9.0.0');

CREATE TABLE "SchoolSettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SchoolSettings" PRIMARY KEY AUTOINCREMENT,
    "SchoolId" INTEGER NOT NULL,
    "SchoolDisplayName" TEXT NOT NULL,
    "LogoImageUrl" TEXT NOT NULL,
    "NavigationType" TEXT NOT NULL,
    "ThemeColor" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_SchoolSettings_Schools_SchoolId" FOREIGN KEY ("SchoolId") REFERENCES "Schools" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_SchoolSettings_SchoolId" ON "SchoolSettings" ("SchoolId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250723075106_AddSchoolSettings', '9.0.0');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250723081435_UpdateSchoolSettingsConstraints', '9.0.0');

CREATE TABLE "Branches" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Branches" PRIMARY KEY AUTOINCREMENT,
    "BranchName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Location" TEXT NOT NULL,
    "SchoolId" INTEGER NOT NULL,
    "SchoolHeadUsername" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_Branches_Schools_SchoolId" FOREIGN KEY ("SchoolId") REFERENCES "Schools" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Courses" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Courses" PRIMARY KEY AUTOINCREMENT,
    "CourseName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "ThumbnailImageUrl" TEXT NULL,
    "BranchId" INTEGER NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    CONSTRAINT "FK_Courses_Branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "Branches" ("Id") ON DELETE CASCADE
);

CREATE TABLE "CourseLessons" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CourseLessons" PRIMARY KEY AUTOINCREMENT,
    "LessonTitle" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "YouTubeUrl" TEXT NOT NULL,
    "CourseId" INTEGER NOT NULL,
    "SortOrder" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_CourseLessons_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Branches_SchoolId" ON "Branches" ("SchoolId");

CREATE INDEX "IX_CourseLessons_CourseId" ON "CourseLessons" ("CourseId");

CREATE INDEX "IX_Courses_BranchId" ON "Courses" ("BranchId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250723095215_AddBranchesAndCoursesSimplified', '9.0.0');

ALTER TABLE "Users" ADD "IsActive" INTEGER NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250725051751_AddUserIsActiveField', '9.0.0');

COMMIT;

