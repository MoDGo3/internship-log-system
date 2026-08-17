-- ============================================================
-- InternLogDB.sql — ฐานข้อมูลระบบจดบันทึกการฝึกงาน
-- 20 ตาราง | Version: 2.1 (รวมมติทุกส่วน)
-- ใช้งานกับ: Microsoft SQL Server (SSMS)
-- สร้างโดย: Senior Dev | วันที่: 14 สิงหาคม 2569
-- ============================================================

USE master;
GO

-- สร้าง Database ใหม่ (ถ้ามีอยู่แล้วให้ลบก่อน)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'InternLogDB')
BEGIN
    ALTER DATABASE InternLogDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE InternLogDB;
END
GO

CREATE DATABASE InternLogDB
    COLLATE Thai_CI_AS;  -- รองรับภาษาไทย
GO

USE InternLogDB;
GO

-- ============================================================
-- PHASE 1: ตารางที่ไม่มี FK (Independent Tables)
-- ============================================================

-- ─── ตารางที่ 1: Roles ───────────────────────────────────
CREATE TABLE Roles (
    RoleID      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(20)  NOT NULL,   -- student|advisor|supervisor|admin
    Description NVARCHAR(200) NULL,
    CONSTRAINT UQ_Roles_Name UNIQUE (RoleName)
);
GO

-- ─── ตารางที่ 3: Companies ───────────────────────────────
CREATE TABLE Companies (
    CompanyID      INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName    NVARCHAR(200) NOT NULL,
    Address        NVARCHAR(500) NULL,
    Phone          NVARCHAR(20)  NULL,
    WorkStartTime  TIME(0)       NOT NULL DEFAULT '09:00:00',
    WorkEndTime    TIME(0)       NOT NULL DEFAULT '18:00:00',
    CreatedAt      DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ─── ตารางที่ 4: TrainingPeriods ─────────────────────────
CREATE TABLE TrainingPeriods (
    PeriodID   INT IDENTITY(1,1) PRIMARY KEY,
    PeriodName NVARCHAR(100) NOT NULL,
    StartDate  DATE          NOT NULL,
    EndDate    DATE          NOT NULL,
    IsActive   BIT           NOT NULL DEFAULT 1
);
GO

-- ─── ตารางที่ 7: LeaveTypes ──────────────────────────────
CREATE TABLE LeaveTypes (
    LeaveTypeID INT IDENTITY(1,1) PRIMARY KEY,
    TypeName    NVARCHAR(50) NOT NULL,
    MaxDays     INT          NOT NULL
);
GO

-- ─── ตารางที่ 9: WorkCategories ──────────────────────────
CREATE TABLE WorkCategories (
    CategoryID   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL
);
GO

-- ============================================================
-- PHASE 2: Users (ต้องมี Roles ก่อน)
-- ============================================================

-- ─── ตารางที่ 2: Users ───────────────────────────────────
CREATE TABLE Users (
    UserID       INT IDENTITY(1,1) PRIMARY KEY,
    RoleID       INT           NOT NULL REFERENCES Roles(RoleID),
    Email        NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName    NVARCHAR(100) NOT NULL,
    LastName     NVARCHAR(100) NOT NULL,
    StudentID    NVARCHAR(20)  NULL,
    Phone        NVARCHAR(20)  NULL,
    ProfileImage NVARCHAR(500) NULL,
    IsActive     BIT           NOT NULL DEFAULT 1,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

-- Index: Email (ใช้บ่อยใน WHERE clause)
CREATE INDEX IX_Users_Email  ON Users(Email);
CREATE INDEX IX_Users_RoleID ON Users(RoleID);
GO

-- ============================================================
-- PHASE 3: Internships (ต้องมี Users + Companies + Periods ก่อน)
-- ============================================================

-- ─── ตารางที่ 5: Internships (ตารางแกนกลาง) ─────────────
CREATE TABLE Internships (
    InternshipID  INT IDENTITY(1,1) PRIMARY KEY,
    StudentID     INT  NOT NULL REFERENCES Users(UserID),
    CompanyID     INT  NOT NULL REFERENCES Companies(CompanyID),
    AdvisorID     INT  NOT NULL REFERENCES Users(UserID),
    SupervisorID  INT  NULL     REFERENCES Users(UserID),
    PeriodID      INT  NOT NULL REFERENCES TrainingPeriods(PeriodID),
    StartDate     DATE NOT NULL,
    EndDate       DATE NOT NULL,
    RequiredDays  INT  NOT NULL DEFAULT 60,
    RequiredHours INT  NOT NULL DEFAULT 240,
    Status        NVARCHAR(20) NOT NULL DEFAULT 'active'  -- active|completed
        CONSTRAINT CHK_Internship_Status CHECK (Status IN ('active', 'completed'))
);
GO

CREATE INDEX IX_Internships_Student  ON Internships(StudentID);
CREATE INDEX IX_Internships_Advisor  ON Internships(AdvisorID);
CREATE INDEX IX_Internships_Period   ON Internships(PeriodID);
GO

-- ============================================================
-- PHASE 4: Feature Tables (ต้องมี Internships + Users ก่อน)
-- ============================================================

-- ─── ตารางที่ 6: Attendances ─────────────────────────────
CREATE TABLE Attendances (
    AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID    INT           NOT NULL REFERENCES Users(UserID),
    AttDate      DATE          NOT NULL,
    CheckInTime  TIME(0)       NULL,
    CheckOutTime TIME(0)       NULL,
    RequestedCheckInTime TIME(0) NULL,
    RequestedCheckOutTime TIME(0) NULL,
    TotalHours   DECIMAL(5,2)  NULL,
    Status       NVARCHAR(20)  NOT NULL DEFAULT 'ontime'
        CONSTRAINT CHK_Attendance_Status CHECK (Status IN ('ontime','late','leave','absent','pending_edit')),
    Note         NVARCHAR(500) NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    -- UNIQUE: 1 นักศึกษา ลงได้แค่ 1 ครั้ง/วัน
    CONSTRAINT UQ_Attendance_Student_Date UNIQUE (StudentID, AttDate)
);
GO

CREATE INDEX IX_Attendances_Student ON Attendances(StudentID);
CREATE INDEX IX_Attendances_Date    ON Attendances(AttDate);
GO

-- ─── ตารางที่ 8: Leaves ──────────────────────────────────
CREATE TABLE Leaves (
    LeaveID        INT IDENTITY(1,1) PRIMARY KEY,
    StudentID      INT           NOT NULL REFERENCES Users(UserID),
    LeaveTypeID    INT           NOT NULL REFERENCES LeaveTypes(LeaveTypeID),
    InternshipID   INT           NOT NULL REFERENCES Internships(InternshipID),
    StartDate      DATE          NOT NULL,
    EndDate        DATE          NOT NULL,
    DayCount       DECIMAL(4,1)  NOT NULL,
    Reason         NVARCHAR(1000) NOT NULL,
    AttachmentPath NVARCHAR(500) NULL,
    Status         NVARCHAR(20)  NOT NULL DEFAULT 'pending'
        CONSTRAINT CHK_Leave_Status CHECK (Status IN ('pending','approved','rejected')),
    ReviewNote     NVARCHAR(500) NULL,
    CreatedAt      DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Leaves_Student     ON Leaves(StudentID);
CREATE INDEX IX_Leaves_Internship  ON Leaves(InternshipID);
CREATE INDEX IX_Leaves_Status      ON Leaves(Status);
GO

-- ─── ตารางที่ 10: WorkLogs ───────────────────────────────
CREATE TABLE WorkLogs (
    LogID      INT IDENTITY(1,1) PRIMARY KEY,
    StudentID  INT           NOT NULL REFERENCES Users(UserID),
    CategoryID INT           NOT NULL REFERENCES WorkCategories(CategoryID),
    LogDate    DATE          NOT NULL,
    Hours      DECIMAL(5,2)  NULL,       -- Backend computed จาก Attendances (มติ #2)
    Title      NVARCHAR(200) NOT NULL,
    Detail     NVARCHAR(MAX) NULL,
    Problem    NVARCHAR(MAX) NULL,
    Solution   NVARCHAR(MAX) NULL,
    Status     NVARCHAR(20)  NOT NULL DEFAULT 'draft'
        CONSTRAINT CHK_WorkLog_Status CHECK (Status IN ('draft','submitted','approved','returned')),
    CreatedAt  DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedAt  DATETIME      NULL
);
GO

CREATE INDEX IX_WorkLogs_Student  ON WorkLogs(StudentID);
CREATE INDEX IX_WorkLogs_Date     ON WorkLogs(LogDate);
CREATE INDEX IX_WorkLogs_Status   ON WorkLogs(Status);
GO

-- ─── ตารางที่ 11: Attachments ────────────────────────────
CREATE TABLE Attachments (
    AttachmentID INT IDENTITY(1,1) PRIMARY KEY,
    LogID        INT           NOT NULL REFERENCES WorkLogs(LogID) ON DELETE CASCADE,
    FileName     NVARCHAR(255) NOT NULL,
    FilePath     NVARCHAR(500) NOT NULL,
    FileSize     BIGINT        NOT NULL,
    UploadedAt   DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ─── ตารางที่ 12: LogComments ────────────────────────────
CREATE TABLE LogComments (
    CommentID INT IDENTITY(1,1) PRIMARY KEY,
    LogID     INT          NOT NULL REFERENCES WorkLogs(LogID) ON DELETE CASCADE,
    UserID    INT          NOT NULL REFERENCES Users(UserID),
    Comment   NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME     NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_LogComments_LogID ON LogComments(LogID);
GO

-- ─── ตารางที่ 13: Tasks (InternshipID FK — มติ #5) ───────
CREATE TABLE Tasks (
    TaskID       INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID INT           NOT NULL REFERENCES Internships(InternshipID),  -- FK เปลี่ยนจาก InternID
    AssignedBy   INT           NOT NULL REFERENCES Users(UserID),
    Title        NVARCHAR(200) NOT NULL,
    Detail       NVARCHAR(MAX) NULL,
    DueDate      DATE          NULL,
    Priority     NVARCHAR(10)  NOT NULL DEFAULT 'medium'
        CONSTRAINT CHK_Task_Priority CHECK (Priority IN ('low','medium','high')),
    Status       NVARCHAR(20)  NOT NULL DEFAULT 'todo'
        CONSTRAINT CHK_Task_Status CHECK (Status IN ('todo','doing','done')),
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Tasks_Internship ON Tasks(InternshipID);
CREATE INDEX IX_Tasks_Status     ON Tasks(Status);
GO

-- ─── ตารางที่ 14: AdvisorVisits ──────────────────────────
CREATE TABLE AdvisorVisits (
    VisitID      INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID INT           NOT NULL REFERENCES Internships(InternshipID),
    PersonNo     INT           NOT NULL,    -- ลำดับนศ.ในกลุ่มนิเทศ
    VisitNo      INT           NOT NULL,    -- ครั้งที่นิเทศ
    VisitDate    DATE          NOT NULL,
    AdvisorID    INT           NOT NULL REFERENCES Users(UserID),
    VisitType    NVARCHAR(20)  NOT NULL DEFAULT 'Onsite'
        CONSTRAINT CHK_Visit_Type CHECK (VisitType IN ('Onsite','Online')),
    Summary      NVARCHAR(MAX) NULL,
    Score        INT           NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    -- UNIQUE: 1 internship → VisitNo ซ้ำไม่ได้ (มติ #3)
    CONSTRAINT UQ_Visit_No UNIQUE (InternshipID, VisitNo)
);
GO

-- ─── ตารางที่ 15: Messages ───────────────────────────────
CREATE TABLE Messages (
    MessageID  INT IDENTITY(1,1) PRIMARY KEY,
    SenderID   INT          NOT NULL REFERENCES Users(UserID),
    ReceiverID INT          NOT NULL REFERENCES Users(UserID),
    Content    NVARCHAR(MAX) NOT NULL,
    IsRead     BIT          NOT NULL DEFAULT 0,
    CreatedAt  DATETIME     NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Messages_Sender   ON Messages(SenderID);
CREATE INDEX IX_Messages_Receiver ON Messages(ReceiverID);
CREATE INDEX IX_Messages_Pair     ON Messages(SenderID, ReceiverID);
GO

-- ─── ตารางที่ 16: Notifications ──────────────────────────
CREATE TABLE Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID         INT           NOT NULL REFERENCES Users(UserID) ON DELETE CASCADE,
    Title          NVARCHAR(200) NOT NULL,
    Message        NVARCHAR(MAX) NOT NULL,
    Link           NVARCHAR(300) NULL,
    IsRead         BIT           NOT NULL DEFAULT 0,
    CreatedAt      DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Notifications_User   ON Notifications(UserID);
CREATE INDEX IX_Notifications_IsRead ON Notifications(UserID, IsRead);
GO

-- ─── ตารางที่ 17: Reports ────────────────────────────────
CREATE TABLE Reports (
    ReportID     INT IDENTITY(1,1) PRIMARY KEY,
    StudentID    INT           NOT NULL REFERENCES Users(UserID),
    InternshipID INT           NOT NULL REFERENCES Internships(InternshipID),
    PeriodLabel  NVARCHAR(100) NOT NULL,
    StartDate    DATE          NOT NULL,
    EndDate      DATE          NOT NULL,
    Status       NVARCHAR(20)  NOT NULL DEFAULT 'draft'
        CONSTRAINT CHK_Report_Status CHECK (Status IN ('draft','submitted','approved','returned')),
    ReviewNote   NVARCHAR(500) NULL,
    FilePath     NVARCHAR(500) NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ─── ตารางที่ 18: Evaluations ────────────────────────────
CREATE TABLE Evaluations (
    EvaluationID  INT IDENTITY(1,1) PRIMARY KEY,
    InternshipID  INT           NOT NULL REFERENCES Internships(InternshipID),
    EvaluatorRole NVARCHAR(20)  NOT NULL    -- supervisor|student
        CONSTRAINT CHK_Eval_Role CHECK (EvaluatorRole IN ('supervisor','student')),
    CriteriaJson  NVARCHAR(MAX) NULL,       -- JSON array ของ criteria
    TotalScore    DECIMAL(5,2)  NULL,
    Grade         NVARCHAR(2)   NULL
        CONSTRAINT CHK_Eval_Grade CHECK (Grade IN ('A','B','C','D','F')),
    Comment       NVARCHAR(MAX) NULL,
    CreatedAt     DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- PHASE 5: Auth Security Tables
-- ============================================================

-- ─── ตารางที่ 19: RefreshTokens (มติ #4) ─────────────────
CREATE TABLE RefreshTokens (
    RefreshTokenID   INT IDENTITY(1,1) PRIMARY KEY,
    UserID           INT           NOT NULL REFERENCES Users(UserID) ON DELETE CASCADE,
    TokenHash        NVARCHAR(255) NOT NULL,  -- BCrypt hash (ไม่เก็บตัวดิบ)
    ExpiresAt        DATETIME      NOT NULL,
    RevokedAt        DATETIME      NULL,
    ReplacedByTokenID INT          NULL,
    CreatedAt        DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_RefreshTokens_User ON RefreshTokens(UserID);
GO

-- ─── ตารางที่ 20: PasswordResetTokens (มติ #7) ──────────
CREATE TABLE PasswordResetTokens (
    TokenID   INT IDENTITY(1,1) PRIMARY KEY,
    UserID    INT           NOT NULL REFERENCES Users(UserID) ON DELETE CASCADE,
    TokenHash NVARCHAR(255) NOT NULL,  -- BCrypt hash
    ExpiresAt DATETIME      NOT NULL,
    IsUsed    BIT           NOT NULL DEFAULT 0,
    CreatedAt DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_PasswordResetTokens_User ON PasswordResetTokens(UserID);
CREATE INDEX IX_PasswordResetTokens_Hash ON PasswordResetTokens(TokenHash);
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Roles (4 แถว)
INSERT INTO Roles (RoleName, Description) VALUES
    ('student',    N'นักศึกษาฝึกงาน'),
    ('advisor',    N'อาจารย์นิเทศก์'),
    ('supervisor', N'พี่เลี้ยงบริษัท'),
    ('admin',      N'ผู้ดูแลระบบ');
GO

-- LeaveTypes (3 แถว)
INSERT INTO LeaveTypes (TypeName, MaxDays) VALUES
    (N'ลาป่วย',    30),
    (N'ลากิจ',     3),
    (N'ลาพักร้อน', 6);
GO

-- WorkCategories (5 แถว)
INSERT INTO WorkCategories (CategoryName) VALUES
    ('Development'),
    ('Meeting'),
    ('Documentation'),
    ('Testing'),
    (N'อื่นๆ');
GO

-- Admin user (BCrypt hash ของ "Admin@1234!" — work factor 12)
-- Hash นี้ generate จาก: BCrypt.Net.BCrypt.HashPassword("Admin@1234!", 12)
INSERT INTO Users (RoleID, Email, PasswordHash, FirstName, LastName, IsActive)
VALUES (
    4,
    'admin@internlog.dev',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewxKi6ZXJ3cOKy6m',  -- Admin@1234!
    'System',
    'Admin',
    1
);
GO

-- ============================================================
-- VERIFY
-- ============================================================
SELECT
    t.name AS TableName,
    COUNT(c.column_id) AS ColumnCount
FROM sys.tables t
JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.type = 'U'
GROUP BY t.name
ORDER BY t.name;
GO

PRINT N'✅ InternLogDB สร้างสำเร็จ — 20 ตาราง พร้อมใช้งาน';
GO
