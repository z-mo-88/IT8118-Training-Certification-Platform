CREATE TABLE Role (
    RoleId INTEGER PRIMARY KEY IDENTITY(1,1),
    RoleName VARCHAR(50) NOT NULL
);

CREATE TABLE [User] (
    UserId INTEGER PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(50) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(20),
    RoleId INTEGER NOT NULL,
    IsActive BIT NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Role(RoleId)
);
CREATE TABLE InstructorProfile (
    UserId INTEGER PRIMARY KEY,
    Bio VARCHAR(1000) NOT NULL,
    Notes VARCHAR(1000) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES [User](UserId)
);

CREATE TABLE InstructorAvailability (
    AvailabilityId INTEGER PRIMARY KEY IDENTITY(1,1),
    DayOfWeek VARCHAR(50) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsAvailable BIT NOT NULL,
    UserId INTEGER NOT NULL,
    FOREIGN KEY (UserId) REFERENCES [User](UserId)
);
CREATE TABLE ExpertiseArea (
    ExpertiseId INTEGER PRIMARY KEY IDENTITY(1,1),
    ExpertiseName VARCHAR(50) NOT NULL
);

CREATE TABLE InstructorExpertise (
    InstructorExpertiseId INTEGER PRIMARY KEY IDENTITY(1,1),
    ExpertiseId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    FOREIGN KEY (ExpertiseId) REFERENCES ExpertiseArea(ExpertiseId),
    FOREIGN KEY (UserId) REFERENCES [User](UserId)
);
CREATE TABLE Equipment (
    EquipmentId INTEGER PRIMARY KEY IDENTITY(1,1),
    EquipmentName VARCHAR(50) NOT NULL,
    Description VARCHAR(255) NOT NULL
);

CREATE TABLE Room (
    RoomId INTEGER PRIMARY KEY IDENTITY(1,1),
    RoomName VARCHAR(50) NOT NULL,
    Capacity INTEGER NOT NULL,
    Location VARCHAR(100) NOT NULL
);

CREATE TABLE RoomEquipment (
    RoomEquipmentId INTEGER PRIMARY KEY IDENTITY(1,1),
    Quantity INTEGER NOT NULL,
    RoomId INTEGER NOT NULL,
    EquipmentId INTEGER NOT NULL,
    FOREIGN KEY (RoomId) REFERENCES Room(RoomId),
    FOREIGN KEY (EquipmentId) REFERENCES Equipment(EquipmentId)
);
CREATE TABLE SubjectCategory (
    CategoryId INTEGER PRIMARY KEY IDENTITY(1,1),
    CategoryName VARCHAR(50) NOT NULL,
    Description VARCHAR(255) NOT NULL
);

CREATE TABLE Course (
    CourseId INTEGER PRIMARY KEY IDENTITY(1,1),
    Title VARCHAR(50) NOT NULL,
    Description VARCHAR(1000) NOT NULL,
    DurationHours INTEGER NOT NULL,
    DefaultCapacity INTEGER NOT NULL,
    EnrollmentFee DECIMAL(10,2) NOT NULL,
    CategoryId INTEGER NOT NULL,
    PrerequisiteCourseId INTEGER,
    FOREIGN KEY (CategoryId) REFERENCES SubjectCategory(CategoryId),
    FOREIGN KEY (PrerequisiteCourseId) REFERENCES Course(CourseId)
);
CREATE TABLE CourseEquipmentRequirement (
    CourseEquipmentRequirementId INTEGER PRIMARY KEY IDENTITY(1,1),
    QuantityRequired INTEGER NOT NULL,
    CourseId INTEGER NOT NULL,
    EquipmentId INTEGER NOT NULL,
    FOREIGN KEY (CourseId) REFERENCES Course(CourseId),
    FOREIGN KEY (EquipmentId) REFERENCES Equipment(EquipmentId)
);
CREATE TABLE CourseSession (
    SessionId INTEGER PRIMARY KEY IDENTITY(1,1),
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Status VARCHAR(50) NOT NULL,
    SessionCapacity INTEGER NOT NULL,
    SessionDate DATE NOT NULL,
    AvailableSeats INTEGER NOT NULL,
    CourseId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    RoomId INTEGER NOT NULL,
    FOREIGN KEY (CourseId) REFERENCES Course(CourseId),
    FOREIGN KEY (UserId) REFERENCES [User](UserId),
    FOREIGN KEY (RoomId) REFERENCES Room(RoomId)
);
CREATE TABLE Enrollment (
    EnrollmentId INTEGER PRIMARY KEY IDENTITY(1,1),
    Status VARCHAR(50) NOT NULL,
    EnrollmentDate DATE NOT NULL,
    OutstandingBalance DECIMAL(10,2) NOT NULL,
    IsOverdue BIT NOT NULL,
    UserId INTEGER NOT NULL,
    SessionId INTEGER NOT NULL,
    FOREIGN KEY (UserId) REFERENCES [User](UserId),
    FOREIGN KEY (SessionId) REFERENCES CourseSession(SessionId)
);
CREATE TABLE AssessmentResult (
    ResultId INTEGER PRIMARY KEY IDENTITY(1,1),
    Remarks VARCHAR(50) NOT NULL,
    RecordTime TIME NOT NULL,
    RecordDate DATE NOT NULL,
    IsPassed BIT NOT NULL,
    EnrollmentId INTEGER NOT NULL,
    FOREIGN KEY (EnrollmentId) REFERENCES Enrollment(EnrollmentId)
);
CREATE TABLE CertificationTrack (
    CertificationTrackId INTEGER PRIMARY KEY IDENTITY(1,1),
    TrackName VARCHAR(50) NOT NULL,
    Description VARCHAR(1000) NOT NULL
);

CREATE TABLE CertificationTrackCourse (
    TrackCourseId INTEGER PRIMARY KEY IDENTITY(1,1),
    IsRequired BIT NOT NULL,
    CertificationTrackId INTEGER NOT NULL,
    CourseId INTEGER NOT NULL,
    FOREIGN KEY (CertificationTrackId) REFERENCES CertificationTrack(CertificationTrackId),
    FOREIGN KEY (CourseId) REFERENCES Course(CourseId)
);
CREATE TABLE TraineeCertificationProgress (
    ProgressId INTEGER PRIMARY KEY IDENTITY(1,1),
    Status VARCHAR(50) NOT NULL,
    ProgressPercent INTEGER NOT NULL,
    EligibleDate DATE NOT NULL,
    UserId INTEGER NOT NULL,
    CertificationTrackId INTEGER NOT NULL,
    FOREIGN KEY (UserId) REFERENCES [User](UserId),
    FOREIGN KEY (CertificationTrackId) REFERENCES CertificationTrack(CertificationTrackId)
);
CREATE TABLE Certificate (
    CertificateId INTEGER PRIMARY KEY IDENTITY(1,1),
    IssuedDate DATE NOT NULL,
    CertificateReferenceNumber VARCHAR(50) NOT NULL,
    CertificateStatus VARCHAR(50) NOT NULL,
    UserId INTEGER NOT NULL,
    CertificationTrackId INTEGER NOT NULL,
    FOREIGN KEY (UserId) REFERENCES [User](UserId),
    FOREIGN KEY (CertificationTrackId) REFERENCES CertificationTrack(CertificationTrackId)
);
