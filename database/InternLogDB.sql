-- =============================================
-- Database: InternLogDB
-- Author: Assistant
-- Description: Complete 19 tables schema for Internship Log System
-- =============================================

USE master;
GO
IF DB_ID('InternLogDB') IS NOT NULL
BEGIN
    ALTER DATABASE InternLogDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE InternLogDB;
END
GO
CREATE DATABASE InternLogDB;
GO
USE InternLogDB;
GO

-- 1. Roles
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

-- 2. Companies
CREATE TABLE Companies (
    CompanyID INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName NVARCHAR(255) NOT NULL,
    Address NVARCHAR(MAX),
    Phone NVARCHAR(50),
    WorkStartTime TIME,
    WorkEndTime TIME,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 3. Users (Base Table)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    RoleID INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleID),
    Phone NVARCHAR(50),
    AvatarUrl NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);

-- 4. RefreshTokens (for JWT)
CREATE TABLE RefreshTokens (
    TokenID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    Token NVARCHAR(MAX) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 5. Students (Extended from Users)
CREATE TABLE Students (
    StudentID INT PRIMARY KEY FOREIGN KEY REFERENCES Users(UserID),
    StudentCode NVARCHAR(20) NOT NULL UNIQUE,
    Major NVARCHAR(100),
    Faculty NVARCHAR(100)
);

-- 6. Advisors (Extended from Users)
CREATE TABLE Advisors (
    AdvisorID INT PRIMARY KEY FOREIGN KEY REFERENCES Users(UserID),
    Department NVARCHAR(100)
);

-- 7. Mentors (Extended from Users)
CREATE TABLE Mentors (
    MentorID INT PRIMARY KEY FOREIGN KEY REFERENCES Users(UserID),
    CompanyID INT NOT NULL FOREIGN KEY REFERENCES Companies(CompanyID),
    Position NVARCHAR(100)
);

-- 8. Internships (Match)
CREATE TABLE Internships (
    InternshipID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT NOT NULL FOREIGN KEY REFERENCES Students(StudentID),
    CompanyID INT NOT NULL FOREIGN KEY REFERENCES Companies(CompanyID),
    AdvisorID INT NOT NULL FOREIGN KEY REFERENCES Advisors(AdvisorID),
    MentorID INT NOT NULL FOREIGN KEY REFERENCES Mentors(MentorID),
    Term NVARCHAR(20) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Active'
);

-- 9. Attendance
CREATE TABLE Attendance (
    AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID INT NOT NULL FOREIGN KEY REFERENCES Internships(InternshipID),
    LogDate DATE NOT NULL,
    CheckInTime TIME,
    CheckOutTime TIME,
    TotalHours DECIMAL(5,2),
    Status NVARCHAR(50) CHECK (Status IN ('Ontime', 'Late', 'Absent')),
    Location NVARCHAR(MAX)
);

-- 10. LeaveTypes
CREATE TABLE LeaveTypes (
    LeaveTypeID INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(100) NOT NULL UNIQUE,
    MaxDays INT NOT NULL
);

-- 11. Leaves
CREATE TABLE Leaves (
    LeaveID INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID INT NOT NULL FOREIGN KEY REFERENCES Internships(InternshipID),
    LeaveTypeID INT NOT NULL FOREIGN KEY REFERENCES LeaveTypes(LeaveTypeID),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Reason NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    MentorComment NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 12. WorkCategories
CREATE TABLE WorkCategories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE
);

-- 13. WorkLogs
CREATE TABLE WorkLogs (
    WorkLogID INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID INT NOT NULL FOREIGN KEY REFERENCES Internships(InternshipID),
    CategoryID INT NOT NULL FOREIGN KEY REFERENCES WorkCategories(CategoryID),
    LogDate DATE NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    HoursSpent DECIMAL(5,2) NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Submitted',
    MentorFeedback NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 14. WorkLogImages
CREATE TABLE WorkLogImages (
    ImageID INT IDENTITY(1,1) PRIMARY KEY,
    WorkLogID INT NOT NULL FOREIGN KEY REFERENCES WorkLogs(WorkLogID),
    ImageUrl NVARCHAR(MAX) NOT NULL
);

-- 15. Tasks
CREATE TABLE Tasks (
    TaskID INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID INT NOT NULL FOREIGN KEY REFERENCES Internships(InternshipID),
    AssignedByMentorID INT NOT NULL FOREIGN KEY REFERENCES Mentors(MentorID),
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    DueDate DATE,
    Status NVARCHAR(50) DEFAULT 'Todo' CHECK (Status IN ('Todo', 'In Progress', 'Review', 'Done')),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 16. Visits
CREATE TABLE Visits (
    VisitID INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID INT NOT NULL FOREIGN KEY REFERENCES Internships(InternshipID),
    AdvisorID INT NOT NULL FOREIGN KEY REFERENCES Advisors(AdvisorID),
    VisitDate DATE NOT NULL,
    Type NVARCHAR(50) CHECK (Type IN ('On-site', 'Online')),
    Status NVARCHAR(50) DEFAULT 'Scheduled',
    Feedback NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 17. Chats
CREATE TABLE Chats (
    ChatID INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID INT NOT NULL FOREIGN KEY REFERENCES Internships(InternshipID),
    SenderID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    Message NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME DEFAULT GETDATE(),
    IsRead BIT DEFAULT 0
);

-- 18. Notifications
CREATE TABLE Notifications (
    NotifID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    Title NVARCHAR(255) NOT NULL,
    Message NVARCHAR(MAX),
    Type NVARCHAR(50),
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 19. Evaluations
CREATE TABLE Evaluations (
    EvalID INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID INT NOT NULL FOREIGN KEY REFERENCES Internships(InternshipID),
    EvaluatorID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    EvalType NVARCHAR(50) CHECK (EvalType IN ('MentorToStudent', 'StudentToCompany')),
    TotalScore DECIMAL(5,2),
    Comments NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- SEED DATA --
INSERT INTO Roles (RoleName) VALUES ('student'), ('advisor'), ('supervisor'), ('admin');
INSERT INTO LeaveTypes (TypeName, MaxDays) VALUES ('ป่วย', 30), ('ลากิจ', 3), ('พักร้อน', 6);
INSERT INTO WorkCategories (CategoryName) VALUES ('Front-end'), ('Back-end'), ('Database'), ('Testing'), ('Other');
-- PasswordHash for 'admin@internlog.dev' = 'admin123' (BCrypt needs to be generated in code, but we provide dummy here for structure)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, RoleID) 
VALUES ('admin@internlog.dev', '$2a$11$sR/...', 'System', 'Admin', 4);
GO
