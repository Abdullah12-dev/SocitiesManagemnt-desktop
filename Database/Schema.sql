-- ============================================================
-- FAST Societies Management System - Database Schema
-- ============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SocietiesManagementSystem')
BEGIN
    CREATE DATABASE SocietiesManagementSystem;
END
GO

USE SocietiesManagementSystem;
GO

-- ============================================================
-- USERS (base identity table for all roles)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserID      INT           PRIMARY KEY IDENTITY(1,1),
        Email       VARCHAR(100)  NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        Role        VARCHAR(20)   NOT NULL CHECK (Role IN ('Student','SocietyHead','Admin')),
        IsActive    BIT           NOT NULL DEFAULT 1,
        CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO

-- ============================================================
-- ADMINS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Admins')
BEGIN
    CREATE TABLE Admins (
        AdminID   INT          PRIMARY KEY IDENTITY(1,1),
        UserID    INT          NOT NULL UNIQUE REFERENCES Users(UserID) ON DELETE CASCADE,
        FirstName NVARCHAR(50) NOT NULL,
        LastName  NVARCHAR(50) NOT NULL,
        Phone     VARCHAR(20)  NULL
    );
END
GO

-- ============================================================
-- STUDENTS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    CREATE TABLE Students (
        StudentID          INT          PRIMARY KEY IDENTITY(1,1),
        UserID             INT          NOT NULL UNIQUE REFERENCES Users(UserID) ON DELETE CASCADE,
        FirstName          NVARCHAR(50) NOT NULL,
        LastName           NVARCHAR(50) NOT NULL,
        RegistrationNumber VARCHAR(20)  NOT NULL UNIQUE,
        Department         NVARCHAR(60) NULL,
        Semester           INT          NULL CHECK (Semester BETWEEN 1 AND 8),
        Phone              VARCHAR(20)  NULL
    );
END
GO

-- ============================================================
-- SOCIETIES
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Societies')
BEGIN
    CREATE TABLE Societies (
        SocietyID        INT           PRIMARY KEY IDENTITY(1,1),
        Name             NVARCHAR(100) NOT NULL UNIQUE,
        Description      NVARCHAR(MAX) NULL,
        Category         NVARCHAR(50)  NULL,
        HeadStudentID    INT           NULL REFERENCES Students(StudentID),
        Status           VARCHAR(20)   NOT NULL DEFAULT 'Pending'
                             CHECK (Status IN ('Pending','Active','Suspended','Deleted')),
        CreatedAt        DATETIME      NOT NULL DEFAULT GETDATE(),
        ApprovedByAdminID INT          NULL REFERENCES Admins(AdminID)
    );
END
GO

-- ============================================================
-- MEMBERSHIP REQUESTS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MembershipRequests')
BEGIN
    CREATE TABLE MembershipRequests (
        RequestID         INT          PRIMARY KEY IDENTITY(1,1),
        StudentID         INT          NOT NULL REFERENCES Students(StudentID),
        SocietyID         INT          NOT NULL REFERENCES Societies(SocietyID),
        Status            VARCHAR(20)  NOT NULL DEFAULT 'Pending'
                              CHECK (Status IN ('Pending','Approved','Rejected')),
        RequestedAt       DATETIME     NOT NULL DEFAULT GETDATE(),
        ProcessedAt       DATETIME     NULL,
        ProcessedByHeadID INT          NULL REFERENCES Students(StudentID),
        RejectionReason   NVARCHAR(255) NULL,
        CONSTRAINT UQ_MembershipRequest UNIQUE (StudentID, SocietyID)
    );
END
GO

-- ============================================================
-- SOCIETY MEMBERS (approved memberships)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SocietyMembers')
BEGIN
    CREATE TABLE SocietyMembers (
        MemberID  INT          PRIMARY KEY IDENTITY(1,1),
        StudentID INT          NOT NULL REFERENCES Students(StudentID),
        SocietyID INT          NOT NULL REFERENCES Societies(SocietyID),
        Role      NVARCHAR(50) NOT NULL DEFAULT 'Member',
        JoinedAt  DATETIME     NOT NULL DEFAULT GETDATE(),
        IsActive  BIT          NOT NULL DEFAULT 1,
        CONSTRAINT UQ_SocietyMember UNIQUE (StudentID, SocietyID)
    );
END
GO

-- ============================================================
-- EVENTS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Events')
BEGIN
    CREATE TABLE Events (
        EventID           INT           PRIMARY KEY IDENTITY(1,1),
        SocietyID         INT           NOT NULL REFERENCES Societies(SocietyID),
        Title             NVARCHAR(100) NOT NULL,
        Description       NVARCHAR(MAX) NULL,
        EventDate         DATETIME      NOT NULL,
        EndDate           DATETIME      NULL,
        Venue             NVARCHAR(100) NULL,
        MaxParticipants   INT           NULL,
        EventType         NVARCHAR(50)  NULL,
        Status            VARCHAR(20)   NOT NULL DEFAULT 'Pending'
                              CHECK (Status IN ('Pending','Approved','Cancelled','Completed')),
        CreatedAt         DATETIME      NOT NULL DEFAULT GETDATE(),
        ApprovedByAdminID INT           NULL REFERENCES Admins(AdminID)
    );
END
GO

-- ============================================================
-- EVENT REGISTRATIONS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventRegistrations')
BEGIN
    CREATE TABLE EventRegistrations (
        RegistrationID    INT          PRIMARY KEY IDENTITY(1,1),
        EventID           INT          NOT NULL REFERENCES Events(EventID),
        StudentID         INT          NOT NULL REFERENCES Students(StudentID),
        RegisteredAt      DATETIME     NOT NULL DEFAULT GETDATE(),
        TicketCode        VARCHAR(50)  NOT NULL UNIQUE,
        AttendanceStatus  VARCHAR(20)  NOT NULL DEFAULT 'Registered'
                              CHECK (AttendanceStatus IN ('Registered','Attended','Absent')),
        CONSTRAINT UQ_EventRegistration UNIQUE (EventID, StudentID)
    );
END
GO

-- ============================================================
-- TASKS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tasks')
BEGIN
    CREATE TABLE Tasks (
        TaskID              INT           PRIMARY KEY IDENTITY(1,1),
        SocietyID           INT           NOT NULL REFERENCES Societies(SocietyID),
        Title               NVARCHAR(100) NOT NULL,
        Description         NVARCHAR(MAX) NULL,
        AssignedToStudentID INT           NOT NULL REFERENCES Students(StudentID),
        AssignedByStudentID INT           NOT NULL REFERENCES Students(StudentID),
        DueDate             DATETIME      NULL,
        Status              VARCHAR(20)   NOT NULL DEFAULT 'Pending'
                                CHECK (Status IN ('Pending','InProgress','Completed','Cancelled')),
        CreatedAt           DATETIME      NOT NULL DEFAULT GETDATE(),
        CompletedAt         DATETIME      NULL
    );
END
GO

-- ============================================================
-- ANNOUNCEMENTS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Announcements')
BEGIN
    CREATE TABLE Announcements (
        AnnouncementID    INT           PRIMARY KEY IDENTITY(1,1),
        SocietyID         INT           NOT NULL REFERENCES Societies(SocietyID),
        Title             NVARCHAR(100) NOT NULL,
        Content           NVARCHAR(MAX) NOT NULL,
        CreatedByStudentID INT          NOT NULL REFERENCES Students(StudentID),
        Priority          VARCHAR(10)   NOT NULL DEFAULT 'Normal'
                              CHECK (Priority IN ('Low','Normal','High')),
        IsActive          BIT           NOT NULL DEFAULT 1,
        CreatedAt         DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO

-- ============================================================
-- SEED: Default Admin Account  (password: Admin@123)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@fast.edu.pk')
BEGIN
    DECLARE @AdminUserID INT;
    INSERT INTO Users (Email, PasswordHash, Role)
    VALUES ('admin@fast.edu.pk',
            '$2a$11$c2eO5wLXlkq9HBL0JCRxy.27XrmYEY3BT5S5pZtK1MlqNfF3Jp6Pa',
            'Admin');
    SET @AdminUserID = SCOPE_IDENTITY();
    INSERT INTO Admins (UserID, FirstName, LastName, Phone)
    VALUES (@AdminUserID, 'System', 'Admin', '0300-0000000');
END
GO

PRINT 'Schema applied successfully.';
GO
