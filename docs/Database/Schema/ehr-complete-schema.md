# EHR Database Complete Schema

## Core Tables

### Users Table
```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Role NVARCHAR(50), -- Admin, Doctor, Nurse, Patient
    Status NVARCHAR(20) DEFAULT 'Active',
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME,
    CreatedBy INT,
    UpdatedBy INT,
    IsDeleted BIT DEFAULT 0
);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_Status ON Users(Status);
```

### Patients Table
```sql
CREATE TABLE Patients (
    PatientId INT PRIMARY KEY IDENTITY(1,1),
    MRN NVARCHAR(50) UNIQUE NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DOB DATE NOT NULL,
    Gender NVARCHAR(20),
    BloodType NVARCHAR(5),
    Email NVARCHAR(255),
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(500),
    EmergencyContact NVARCHAR(255),
    EmergencyPhone NVARCHAR(20),
    Status NVARCHAR(20) DEFAULT 'Active',
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME,
    CreatedBy INT FOREIGN KEY REFERENCES Users(UserId),
    UpdatedBy INT FOREIGN KEY REFERENCES Users(UserId),
    IsDeleted BIT DEFAULT 0
);

CREATE INDEX IX_Patients_MRN ON Patients(MRN);
CREATE INDEX IX_Patients_Status ON Patients(Status);
```

### Appointments Table
```sql
CREATE TABLE Appointments (
    AppointmentId INT PRIMARY KEY IDENTITY(1,1),
    PatientId INT FOREIGN KEY REFERENCES Patients(PatientId),
    ProviderId INT FOREIGN KEY REFERENCES Users(UserId),
    AppointmentDate DATETIME NOT NULL,
    Duration INT, -- Minutes
    Type NVARCHAR(50), -- Office, Telehealth, etc.
    Status NVARCHAR(20) DEFAULT 'Scheduled',
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME,
    CreatedBy INT FOREIGN KEY REFERENCES Users(UserId)
);

CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);
CREATE INDEX IX_Appointments_ProviderId ON Appointments(ProviderId);
CREATE INDEX IX_Appointments_Date ON Appointments(AppointmentDate);
CREATE INDEX IX_Appointments_Status ON Appointments(Status);
```
