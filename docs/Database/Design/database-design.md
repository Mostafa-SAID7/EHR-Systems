# Database Design Fundamentals

## Normalization

### 1NF - First Normal Form

```
❌ BAD - Repeating groups
┌──────┬──────────────┐
│ User │ Phone Numbers│
├──────┼──────────────┤
│Ahmed │ 123, 456, 789│
│ Ali  │ 222, 333     │
└──────┴──────────────┘

✅ GOOD - Separate rows
┌──────┬────────┐
│ User │ Phone  │
├──────┼────────┤
│Ahmed │ 123    │
│Ahmed │ 456    │
│Ahmed │ 789    │
│ Ali  │ 222    │
│ Ali  │ 333    │
└──────┴────────┘
```

### 2NF - Second Normal Form

```
❌ BAD - Partial dependency (StudentId, CourseId) → Professor
┌────────────┬──────────┬────────────┐
│StudentId   │CourseId  │Professor   │
├────────────┼──────────┼────────────┤
│1           │101       │Dr. Ahmed   │
│1           │102       │Dr. Ali     │
│2           │101       │Dr. Ahmed   │
└────────────┴──────────┴────────────┘

✅ GOOD - Separate tables
Courses Table:
┌────────┬────────────┐
│CourseId│Professor   │
├────────┼────────────┤
│101     │Dr. Ahmed   │
│102     │Dr. Ali     │
└────────┴────────────┘

Enrollments Table:
┌────────────┬──────────┐
│StudentId   │CourseId  │
├────────────┼──────────┤
│1           │101       │
│1           │102       │
│2           │101       │
└────────────┴──────────┘
```

### 3NF - Third Normal Form

```
❌ BAD - Transitive dependency
┌────────┬──────────┬──────────┐
│StudentId│CityId   │CityName  │
├────────┼──────────┼──────────┤
│1       │101       │Cairo     │
│2       │101       │Cairo     │
│3       │102       │Alex      │
└────────┴──────────┴──────────┘
CityName depends on CityId, not StudentId

✅ GOOD - Separate table
Students:
┌────────┬──────────┐
│StudentId│CityId   │
├────────┼──────────┤
│1       │101       │
│2       │101       │
│3       │102       │
└────────┴──────────┘

Cities:
┌──────────┬──────────┐
│CityId    │CityName  │
├──────────┼──────────┤
│101       │Cairo     │
│102       │Alex      │
└──────────┴──────────┘
```

---

## Entity Relationships

### One-to-Many

```
User (1) ──── (N) Orders
┌──────┐       ┌──────────┐
│UserId│◄──────│UserId FK │
│Name  │       │OrderId   │
│Email │       │Amount    │
└──────┘       └──────────┘

- User can have many orders
- Order belongs to one user
```

### Many-to-Many

```
Student (N) ──── (N) Course
┌──────┐   ┌──────────┐   ┌────────┐
│UserId│───│StudentId │───│CourseId│
│Name  │   │CourseId  │   │Name    │
│Email │   └──────────┘   │Credits │
└──────┘   Enrollment     └────────┘

- Student can take many courses
- Course can have many students
- Junction table: Enrollment
```

### One-to-One

```
User (1) ──── (1) UserProfile
┌──────┐       ┌────────────────┐
│UserId│◄──────│UserId FK       │
│Email │       │Biography       │
│Status│       │ProfileImageUrl │
└──────┘       └────────────────┘

- User has one profile
- Profile belongs to one user
```

---

## Denormalization Trade-offs

```
✅ NORMALIZED - 3NF
┌──────┐       ┌──────────┐       ┌────────┐
│User  │───►   │Orders    │───►   │Products│
└──────┘       └──────────┘       └────────┘

Query to get user with order product names:
SELECT u.Name, o.OrderId, p.ProductName
FROM Users u
JOIN Orders o ON u.UserId = o.UserId
JOIN OrderItems oi ON o.OrderId = oi.OrderId
JOIN Products p ON oi.ProductId = p.ProductId
(3 JOINs, slower)

❌ DENORMALIZED - Fewer JOINs
Orders Table (with denormalized product data):
┌──────────┬──────────────┬──────────────────┐
│OrderId   │UserId        │ProductNames      │
├──────────┼──────────────┼──────────────────┤
│1         │1             │"Product A, B"    │
└──────────┴──────────────┴──────────────────┘

Query:
SELECT o.ProductNames
FROM Orders o
WHERE o.UserId = 1
(1 query, faster but denormalized data)

Trade-off: Speed vs Data Redundancy
```

---

## Database Design EHR Example

```sql
-- Users (Core)
CREATE TABLE Users (
    UserId INT PRIMARY KEY,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    Role NVARCHAR(50),
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

-- Patients (Domain)
CREATE TABLE Patients (
    PatientId INT PRIMARY KEY,
    MRN NVARCHAR(50) UNIQUE NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    DOB DATE,
    BloodType NVARCHAR(3),
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

-- Patient Allergies (One-to-Many)
CREATE TABLE PatientAllergies (
    AllergyId INT PRIMARY KEY,
    PatientId INT FOREIGN KEY REFERENCES Patients(PatientId),
    Allergen NVARCHAR(200),
    Severity NVARCHAR(20),
    CreatedAt DATETIME
);

-- Appointments (One-to-Many)
CREATE TABLE Appointments (
    AppointmentId INT PRIMARY KEY,
    PatientId INT FOREIGN KEY REFERENCES Patients(PatientId),
    ProviderId INT FOREIGN KEY REFERENCES Users(UserId),
    AppointmentDate DATETIME,
    Status NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

-- Indexes for performance
CREATE INDEX IX_Patients_MRN ON Patients(MRN);
CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);
CREATE INDEX IX_Appointments_ProviderId ON Appointments(ProviderId);
CREATE INDEX IX_Appointments_Date ON Appointments(AppointmentDate);
```

---

## Interview Q&A

**Q: What's normalization and why does it matter?**

A: Normalization reduces data redundancy and improves data integrity. Downside: more joins = slower queries. Balance with denormalization when needed.

**Q: When to denormalize?**

A: When queries are slow and normalization prevents optimization. Example: Analytics queries reading millions of rows.

**Q: Difference between 2NF and 3NF?**

A:
- 2NF: No partial dependencies (every non-key attribute depends on ENTIRE key)
- 3NF: No transitive dependencies (non-key attributes don't depend on other non-key attributes)
