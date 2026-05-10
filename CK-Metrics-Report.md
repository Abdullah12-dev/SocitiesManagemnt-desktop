# CK Metrics Suite Evaluation Report

| Field | Value |
|---|---|
| Student name | Abdul Qudoos |
| Roll number | 22i-8774 |
| Course | Software Maintenance and Metrics (SMM) |
| Project | FAST Societies Management System |
| Technology | C# / .NET 8 / WinForms / SQL Server |
| Class diagram | `ClassDiagram.excalidraw` |

---

## 1. Introduction

This report applies the six Chidamber and Kemerer metrics to my project, the FAST Societies Management System. The system is a desktop application that lets students join societies, attend events, receive tasks and announcements, and lets admins manage everything from one dashboard. The code is organised in five layers: Models, Data (DatabaseConnection and Repositories), Services, Helpers, and UI (Forms and Controls).

Every metric value in this report was computed by reading the actual source code. The per-class calculations are presented in Section 3, the summary table in Section 4, the answers to the assignment questions in Section 5, and the refactoring suggestions in Section 6.

### 1.1 Evaluation of best feature using structural metrics other than CK

Before applying the CK Suite class-by-class, I also checked the main features of the project using simple structural metrics other than CK. The purpose is to identify which feature is structurally the cleanest, not just which class has the best CK score.

The structural metrics used here are:

- Feature spread: how many project classes are directly involved in the feature.
- Layer coverage: how many architecture layers the feature crosses.
- Database table count: how many database tables the feature owns or depends on.
- Dependency direction: whether the feature follows the expected flow UI then Service then Repository then Database.
- External dependency count: whether the feature depends on extra libraries or framework-heavy logic.
- Change impact: how many files would change if a small requirement is added.

Comparison of the main features:

| Feature | Direct classes | Layers | Main DB tables | External dependency | Result |
|---|---:|---:|---:|---|---|
| Authentication and registration | 7 | 4 | 2 | BCrypt | Good, but login also knows all dashboards |
| Society management | 8 | 4 | 3 | None | Powerful but structurally wide |
| Event management | 6 | 4 | 2 | Ticket helper | Good, but lifecycle and registration are mixed |
| Membership workflow | 5 | 4 | 2 | None | Useful, but repository mixes two tables |
| Task management | 5 | 4 | 1 | None | Clean and simple |
| Announcement management | 5 | 4 | 1 | None | Cleanest full feature |
| Reporting | 2 | 3 | many read-only | ADO.NET DataTable | Useful, but SQL-heavy |

**Best feature structurally: Announcement Management.** It has one model (`Announcement`), one repository (`AnnouncementRepository`), one service (`AnnouncementService`), one form dialog (`AnnouncementFormDialog`), and two dashboard entry points (Society Dashboard for creating, Student Dashboard for viewing). The feature follows the intended project structure exactly. It uses one main database table (`Announcements`), has no third-party library dependency, and does not need cross-module coordination. If a new field like `ExpiryDate` is added, the change ripples down a single vertical path: model, repository, service, dialog, dashboards. That is a healthy structure because the change path is narrow and predictable.

**Second best: Task Management.** Same shape as Announcements (one model, one repository, one service, one dialog, dashboard callers). It comes second only because tasks tie more closely to membership and assigned students, so changes are slightly more likely to touch member-related logic.

**Weakest structural area: the Society Dashboard feature group.** This UI class pulls almost every service into one file: `SocietyService`, `EventService`, `TaskService`, `AnnouncementService`, `MembershipService`, and `ReportService`. This is why it later appears as the highest WMC, RFC, CBO, and LCOM class in the CK analysis.

So before even applying CK metrics, the structural check already points to the same conclusion: vertical features such as Announcement and Task Management are clean, while the dashboard classes need refactoring because they act as feature aggregators.

---

## 2. Measurement rules

**WMC.** Count of methods defined in the class, each weighted 1. Auto-properties are not counted. Expression-bodied computed properties such as `FullName => $"{FirstName} {LastName}"` count as 1. Static helper classes are included.

**DIT.** Number of edges from the class up to `System.Object`. For WinForms classes the full framework chain is traced: Object (0), MarshalByRefObject (1), Component (2), Control (3), ScrollableControl (4), ContainerControl (5), Form (6).

**NOC.** Number of direct subclasses (inheritance only). Interface realisations are reported separately where relevant.

**CBO.** Number of distinct external classes the class couples to through fields, parameters, return types, instantiations, and static calls. Primitives such as `int`, `bool`, `string` are excluded. For UI classes, framework types reached only via deep inheritance are excluded; framework types the class explicitly instantiates or holds as fields are counted.

**RFC.** Own method count plus distinct external methods called by those methods (one level deep). Methods with the same name on different classes are counted separately.

**LCOM.** P = pairs of methods sharing no instance field. Q = pairs sharing at least one instance field. If P > Q, LCOM = P - Q, otherwise LCOM = 0.

---

## 3. Per-class calculations

The format used in this section is: a one-line description, a metrics line, and a short reasoning paragraph only when the calculation needs more than the obvious counts.

### 3.1 Models layer

**`User`** is the base identity entity. Properties only, no methods.

Metrics: WMC = 0, DIT = 1, NOC = 2, CBO = 0, RFC = 0, LCOM = 0.

NOC = 2 because `Student` and `Admin` extend it.

**`Admin` (extends `User`)** adds one computed property `FullName`.

Metrics: WMC = 1, DIT = 2, NOC = 0, CBO = 1, RFC = 1, LCOM = 0.

**`Student` (extends `User`)** has the same shape as `Admin` with its own `FullName`.

Metrics: WMC = 1, DIT = 2, NOC = 0, CBO = 1, RFC = 1, LCOM = 0.

**Other entity classes.** `Society`, `SocietyMember`, `MembershipRequest`, `Event`, `EventRegistration`, `SocietyTask`, and `Announcement` are pure POCO entities. No methods, only auto-properties.

Metrics for each of the seven: WMC = 0, DIT = 1, NOC = 0, CBO = 0, RFC = 0, LCOM = 0.

### 3.2 Data layer

**`DatabaseConnection`** is a singleton that builds and hands out `SqlConnection` objects.

Methods (3): private constructor, `GetConnection`, `TestConnection`. Field: `_connectionString`.

Metrics: WMC = 3, DIT = 1, NOC = 0, CBO = 2, RFC = 6, LCOM = 1.

CBO covers `SqlConnection` (instantiated in `GetConnection`) and `Properties.Settings` (read in the constructor). RFC adds 3 external calls: `SqlConnection` constructor, `conn.Open()`, and `Properties.Settings.Default.ConnectionString`. LCOM is 1 because `_connectionString` is touched directly only by the constructor and `GetConnection`; `TestConnection` reaches it indirectly through `GetConnection`. So one pair shares the field (Q = 1) and two pairs do not (P = 2), giving LCOM = P - Q = 1.

**`IRepository<T>`** is the generic CRUD interface.

Methods declared (5): `GetById`, `GetAll`, `Insert`, `Update`, `Delete`.

Metrics: WMC = 5, DIT = 0, NOC = 8 (interface realisations), CBO = 0, RFC = 5, LCOM = not applicable.

NOC counts the eight concrete repositories that realise the interface, not subclasses.

**`UserRepository`** with methods (9): `GetById`, `GetByEmail`, `GetAll`, `Insert`, `Update`, `UpdateRole`, `UpdatePassword`, `Delete`, `MapUser`. Field: `_db`.

Metrics: WMC = 9, DIT = 1, NOC = 0, CBO = 4, RFC = 16, LCOM = 0.

CBO covers `DatabaseConnection`, `User`, `SqlCommand`, `SqlDataReader`. RFC adds 7 external calls used across the methods: `_db.GetConnection`, `conn.Open`, `cmd.Parameters.AddWithValue`, `cmd.ExecuteReader`, `reader.Read`, `cmd.ExecuteScalar`, `cmd.ExecuteNonQuery`. LCOM is 0 because every method uses `_db`, so every method pair shares a field.

**`StudentRepository`** with methods (9): `GetById`, `GetByUserId`, `GetAll`, `GetAllForAdmin`, `Search`, `Insert`, `Update`, `Delete`, `MapStudent`. Field: `_db`.

Metrics: WMC = 9, DIT = 1, NOC = 0, CBO = 4, RFC = 16, LCOM = 0.

Same external call pattern as `UserRepository`.

**`SocietyRepository`** with methods (10): `GetById`, `GetAll`, `GetActive`, `GetByHeadStudent`, `Insert`, `Update`, `UpdateStatus`, `AssignHead`, `Delete`, `MapSociety`. Field: `_db`.

Metrics: WMC = 10, DIT = 1, NOC = 0, CBO = 4, RFC = 17, LCOM = 0.

**`MembershipRepository`** with methods (15): `GetById`, `GetAll`, `GetByStatus`, `GetByStudent`, `GetBySociety`, `Insert`, `UpdateStatus`, `HasPendingRequest`, `IsMember`, `AddMember`, `GetMembers`, `RemoveMember`, `Update` (stub returning false), `Delete` (stub returning false), `MapRequest`. Field: `_db`.

Metrics: WMC = 15, DIT = 1, NOC = 0, CBO = 5, RFC = 22, LCOM = 0.

CBO adds `MembershipRequest` and `SocietyMember` to the usual ADO.NET set. The strict CK LCOM is 0 because the single `_db` field is shared by all methods. The class still covers two distinct database tables (`MembershipRequests` and `SocietyMembers`), which makes it a refactoring candidate. The split is described in Section 6.

**`EventRepository`** with methods (10): `GetById`, `GetAll`, `GetUpcoming`, `GetBySociety`, `GetPending`, `Insert`, `Update`, `UpdateStatus`, `Delete`, `MapEvent`. Field: `_db`.

Metrics: WMC = 10, DIT = 1, NOC = 0, CBO = 4, RFC = 17, LCOM = 0.

**`EventRegistrationRepository`** with methods (10): `GetById`, `GetAll`, `GetByStudent`, `GetByEvent`, `IsRegistered`, `Insert`, `UpdateAttendance`, `Update` (stub), `Delete` (stub), `MapRegistration`. Field: `_db`.

Metrics: WMC = 10, DIT = 1, NOC = 0, CBO = 4, RFC = 17, LCOM = 0.

**`TaskRepository`** with methods (9): `GetById`, `GetAll`, `GetBySociety`, `GetByStudent`, `Insert`, `Update`, `UpdateStatus`, `Delete`, `MapTask`. Field: `_db`.

Metrics: WMC = 9, DIT = 1, NOC = 0, CBO = 4, RFC = 16, LCOM = 0.

**`AnnouncementRepository`** with methods (8): `GetById`, `GetAll`, `GetBySociety`, `GetBySocieties`, `Insert`, `Update`, `Delete`, `MapAnnouncement`. Field: `_db`.

Metrics: WMC = 8, DIT = 1, NOC = 0, CBO = 4, RFC = 15, LCOM = 0.

### 3.3 Services layer

**`AuthService`** with methods (3): `Login`, `RegisterStudent`, `GetStudentProfile`. Fields: `_userRepo`, `_studentRepo`.

Metrics: WMC = 3, DIT = 1, NOC = 0, CBO = 6, RFC = 12, LCOM = 0.

CBO covers `UserRepository`, `StudentRepository`, `PasswordHelper`, `ValidationHelper`, `User`, `Student`. RFC adds 9 external calls: `_userRepo.GetByEmail`, `_userRepo.Insert`, `_studentRepo.GetByUserId`, `_studentRepo.Insert`, `PasswordHelper.Verify`, `PasswordHelper.Hash`, `ValidationHelper.IsValidEmail`, `ValidationHelper.IsStrongPassword`, `ValidationHelper.IsValidRegistrationNumber`. LCOM = 0 because of the three method pairs, two share at least one field (Q = 2), one does not (P = 1), so P is not greater than Q.

**`SocietyService`** with methods (14): `GetAllSocieties`, `GetActiveSocieties`, `GetSocietiesByHead`, `GetById`, `CreateSociety`, `UpdateSociety`, `AdminCreateSociety`, `AssignHead`, `ApproveSociety`, `SuspendSociety`, `DeleteSociety`, `GetMembers`, `RemoveMember`, `GetMemberSocieties`. Fields: `_societyRepo`, `_memberRepo`.

Metrics: WMC = 14, DIT = 1, NOC = 0, CBO = 6, RFC = 29, LCOM = 0.

CBO includes `SocietyRepository`, `MembershipRepository`, `Society`, `SocietyMember`, plus `StudentRepository` and `UserRepository` which are instantiated inside `AssignHead`. RFC adds 15 external calls across the two repository fields and two locally constructed repositories. LCOM analysis: 9 methods use only `_societyRepo` (group A), 5 methods use `_memberRepo` and sometimes both (group B). Counting all method pairs: P = 18 (the two group-B methods that use `_memberRepo` only versus the 9 in group A), Q = 73 (same-group pairs plus cross-group pairs that share `_societyRepo`). P is not greater than Q, so LCOM = 0.

**`EventService`** with methods (15): `GetAllEvents`, `GetUpcomingEvents`, `GetEventsBySociety`, `GetPendingEvents`, `GetById`, `CreateEvent`, `UpdateEvent`, `ApproveEvent`, `CancelEvent`, `CompleteEvent`, `RegisterForEvent`, `GetStudentRegistrations`, `GetEventAttendees`, `IsRegistered`, `UpdateAttendance`. Fields: `_eventRepo`, `_regRepo`.

Metrics: WMC = 15, DIT = 1, NOC = 0, CBO = 5, RFC = 29, LCOM = 0.

CBO covers `EventRepository`, `EventRegistrationRepository`, `Event`, `EventRegistration`, `TicketHelper`. RFC adds 14 external calls. LCOM is 0 because P = 40 and Q = 65 (lifecycle methods cluster around `_eventRepo`, registration methods cluster around `_regRepo`, and the bridge method `RegisterForEvent` uses both).

**`MembershipService`** with methods (7): `ApplyForMembership`, `ProcessRequest`, `GetRequestsForSociety`, `GetStudentRequests`, `GetAllRequests`, `IsMember`, `HasPending`. Field: `_memberRepo`.

Metrics: WMC = 7, DIT = 1, NOC = 0, CBO = 2, RFC = 16, LCOM = 0.

RFC adds 9 external calls on `_memberRepo`. LCOM = 0 because every method uses the single field.

**`TaskService`** with methods (7): `GetTasksBySociety`, `GetTasksByStudent`, `GetById`, `AssignTask`, `UpdateTaskStatus`, `UpdateTask`, `DeleteTask`. Field: `_taskRepo`.

Metrics: WMC = 7, DIT = 1, NOC = 0, CBO = 2, RFC = 14, LCOM = 0.

**`AnnouncementService`** with methods (7): `GetBySociety`, `GetForStudent`, `GetAll`, `GetById`, `PostAnnouncement`, `UpdateAnnouncement`, `DeleteAnnouncement`. Field: `_annRepo`.

Metrics: WMC = 7, DIT = 1, NOC = 0, CBO = 2, RFC = 14, LCOM = 0.

**`ReportService`** with methods (8): `GetAdminDashboardStats`, `GetStudentDashboardStats`, `GetSocietyDashboardStats`, `GetSocietyMembersReport`, `GetSocietyEventsReport`, `GetEventRegistrationsReport`, `GetUniversityReport`, `ExecuteDataTable` (private). Field: `_db`.

Metrics: WMC = 8, DIT = 1, NOC = 0, CBO = 4, RFC = 14, LCOM = 0.

CBO covers `DatabaseConnection`, `SqlCommand`, `SqlDataReader`, `SqlDataAdapter`. All eight methods use `_db`.

### 3.4 Helpers layer

**`PasswordHelper`** is a static class with 2 methods: `Hash`, `Verify`.

Metrics: WMC = 2, DIT = 1, NOC = 0, CBO = 1, RFC = 4, LCOM = not applicable.

CBO is the static `BCrypt.Net.BCrypt` class.

**`ValidationHelper`** is a static class with 5 methods: `IsValidEmail`, `IsValidPhone`, `IsValidRegistrationNumber`, `IsStrongPassword`, `RequiredField`.

Metrics: WMC = 5, DIT = 1, NOC = 0, CBO = 1, RFC = 8, LCOM = not applicable.

CBO is `Regex`. RFC adds 3 external calls: `Regex.IsMatch`, `string.IsNullOrWhiteSpace`, `string.IsNullOrEmpty`.

**`TicketHelper`** is a static class with 1 method: `Generate`.

Metrics: WMC = 1, DIT = 1, NOC = 0, CBO = 0, RFC = 1, LCOM = not applicable.

### 3.5 UI layer

WinForms inheritance reference for this section: Object (0), MarshalByRefObject (1), Component (2), Control (3), ScrollableControl (4), ContainerControl (5), Form (6). A class inheriting directly from `Form` has DIT = 7. A class inheriting from `BaseShellForm` has DIT = 8.

**`AppTheme`** is a static class holding colours, fonts, and sizes.

Metrics: WMC = 2, DIT = 1, NOC = 0, CBO = 0, RFC = 2, LCOM = not applicable.

**`Program`** is the static entry point with one meaningful method: `Main`.

Metrics: WMC = 1, DIT = 1, NOC = 0, CBO = 4, RFC = 6, LCOM = not applicable.

CBO covers `DatabaseConnection`, `LoginForm`, `Application` (WinForms), `MessageBox`. RFC adds 5 external calls.

**`ModernButton` (extends `Button`)** with methods (4): constructor, `OnPaint`, `OnMouseEnter`, `OnMouseLeave`.

Metrics: WMC = 4, DIT = 6, NOC = 0, CBO = 2, RFC = 10, LCOM = 0.

CBO is `AppTheme` and `Graphics`. LCOM = 0 because every pair of these methods shares either `_isHovered` or `ButtonStyle`.

**`ModernTextBox` (extends `TextBox`)** with methods (8): constructor, `OnPaint`, `OnGotFocus`, `OnLostFocus`, placeholder setter, `WndProc`, `OnTextChanged`, `CreateParams`.

Metrics: WMC = 8, DIT = 6, NOC = 0, CBO = 2, RFC = 15, LCOM = 5.

LCOM is non-zero because `WndProc`, `OnTextChanged`, and `CreateParams` do not touch the `_placeholder` or `_isFocused` fields, while `OnPaint`, `OnGotFocus`, and `OnLostFocus` do. Cross-pairs without a shared field: 15. Pairs that share a field: 10. P > Q, so LCOM = 15 - 10 = 5.

**`RoundedPanel` (extends `Panel`)** with methods (2): constructor, `OnPaint`.

Metrics: WMC = 2, DIT = 7, NOC = 0, CBO = 1, RFC = 5, LCOM = 0.

CBO is `AppTheme`. `Graphics` appears as a parameter, not a stored field. RFC adds `base.OnPaint`, `Graphics.FillPath`, `GraphicsPath`.

**`StatCard` (extends `Panel`)** with methods (3): constructor, `SetValue`, `OnPaint`.

Metrics: WMC = 3, DIT = 7, NOC = 0, CBO = 2, RFC = 6, LCOM = 0.

**`BaseShellForm` (extends `Form`)** with methods (4): `BuildShell`, `AddNavItem`, `ActivateFirstNav`, `ShowPage`. Fields: `_navItems`, `_activeItem`, `_navEntries`.

Metrics: WMC = 4, DIT = 7, NOC = 3, CBO = 3, RFC = 12, LCOM = 4.

NOC = 3 covers `AdminDashboard`, `SocietyDashboard`, `StudentDashboard`. LCOM analysis: `BuildShell` works mainly with the protected layout properties (`SidebarPanel`, `ContentPanel`) and not the three list/panel fields, while `AddNavItem`, `ActivateFirstNav`, and `ShowPage` touch some combination of those fields. Pairs without a shared instance field: 5. Pairs with a shared field: 1. LCOM = 5 - 1 = 4.

**`LoginForm` (extends `Form`)** with methods (4 instance, 1 static excluded from WMC): constructor, `InitializeComponents`, `OnLoginClick`, `OpenDashboard`.

Metrics: WMC = 4, DIT = 7, NOC = 0, CBO = 8, RFC = 13, LCOM = 0.

CBO is unusually high because `LoginForm` references `AuthService`, `AppTheme`, `ModernTextBox`, `ModernButton`, plus all three concrete dashboards (`AdminDashboard`, `SocietyDashboard`, `StudentDashboard`) and `RegisterForm`. The role-based routing logic is hardcoded inside `OpenDashboard`, which is the source of the high CBO.

**`RegisterForm` (extends `Form`)** with methods (4 instance): constructor, `InitializeComponents`, `OnRegisterClick`, `ShowError`.

Metrics: WMC = 4, DIT = 7, NOC = 0, CBO = 4, RFC = 9, LCOM = 0.

**`AdminDashboard` (extends `BaseShellForm`)** with methods (17): constructor, `BuildUI`, `LoadStudents`, `SearchStudents`, `ToggleStudent`, `LoadSocieties`, `ApproveSociety`, `SuspendSociety`, `LoadEvents`, `ApproveEvent`, `CancelEvent`, `AssignHead`, `ManageSocietyMembers`, `LoadDashboard`, `ExportReport`, `RefreshData`, `ShowStudentsTab`.

Metrics: WMC = 17, DIT = 8, NOC = 0, CBO = 7, RFC = 30, LCOM = 70.

LCOM is large because methods are grouped by tab (Students, Societies, Events, Reports, Dashboard) and methods in different tabs do not share service fields. Approximate pair counts: P = 90, Q = 20.

**`SocietyDashboard` (extends `BaseShellForm`)** with methods (19): constructor, `BuildUI`, `LoadOverview`, `LoadMembers`, `RemoveMember`, `LoadMembershipRequests`, `ProcessRequest`, `LoadEvents`, `ShowEventDialog`, `LoadTasks`, `ShowTaskDialog`, `LoadAnnouncements`, `ShowAnnouncementDialog`, `DeleteAnnouncement`, `LoadReport`, `RefreshData`, `NavigateTo`, `SetActiveSociety`, `ShowSocietyDetails`.

Metrics: WMC = 19, DIT = 8, NOC = 0, CBO = 10, RFC = 34, LCOM = 85.

This is the worst class on every CK metric. Six business tabs (overview, members, events, tasks, announcements, reports) all live in this single form. Approximate LCOM pair counts: P = 110, Q = 25.

**`StudentDashboard` (extends `BaseShellForm`)** with methods (15): constructor, `BuildUI`, `LoadDashboard`, `LoadSocieties`, `JoinSociety`, `LoadMyRequests`, `LoadMyEvents`, `RegisterForEvent`, `LoadMyTasks`, `UpdateTaskStatus`, `LoadAnnouncements`, `LoadProfile`, `RefreshData`, `NavigateTo`, `SetStudent`.

Metrics: WMC = 15, DIT = 8, NOC = 0, CBO = 6, RFC = 27, LCOM = 50.

Same per-tab structure as `AdminDashboard` and `SocietyDashboard`. Approximate pair counts: P = 70, Q = 20.

**`EventFormDialog` (extends `Form`)** with methods (6): constructor, `InitializeComponents`, `PopulateForm`, `OnSaveClick`, `Validate`, `GetEventData`.

Metrics: WMC = 6, DIT = 7, NOC = 0, CBO = 4, RFC = 11, LCOM = 0.

**`TaskFormDialog` (extends `Form`)** with methods (4): constructor, `InitializeComponents`, `PopulateForm`, `OnSaveClick`.

Metrics: WMC = 4, DIT = 7, NOC = 0, CBO = 4, RFC = 9, LCOM = 0.

**`AnnouncementFormDialog` (extends `Form`)** with methods (3): constructor, `InitializeComponents`, `OnSaveClick`.

Metrics: WMC = 3, DIT = 7, NOC = 0, CBO = 4, RFC = 8, LCOM = 0.

---

## 4. Summary table

| # | Class | Layer | WMC | DIT | NOC | CBO | RFC | LCOM |
|---|---|---|---:|---:|---:|---:|---:|---:|
| 1 | `User` | Models | 0 | 1 | 2 | 0 | 0 | 0 |
| 2 | `Admin` | Models | 1 | 2 | 0 | 1 | 1 | 0 |
| 3 | `Student` | Models | 1 | 2 | 0 | 1 | 1 | 0 |
| 4 | `Society` | Models | 0 | 1 | 0 | 0 | 0 | 0 |
| 5 | `SocietyMember` | Models | 0 | 1 | 0 | 0 | 0 | 0 |
| 6 | `MembershipRequest` | Models | 0 | 1 | 0 | 0 | 0 | 0 |
| 7 | `Event` | Models | 0 | 1 | 0 | 0 | 0 | 0 |
| 8 | `EventRegistration` | Models | 0 | 1 | 0 | 0 | 0 | 0 |
| 9 | `SocietyTask` | Models | 0 | 1 | 0 | 0 | 0 | 0 |
| 10 | `Announcement` | Models | 0 | 1 | 0 | 0 | 0 | 0 |
| 11 | `DatabaseConnection` | Data | 3 | 1 | 0 | 2 | 6 | 1 |
| 12 | `IRepository<T>` | Data | 5 | 0 | 8 | 0 | 5 | n/a |
| 13 | `UserRepository` | Data | 9 | 1 | 0 | 4 | 16 | 0 |
| 14 | `StudentRepository` | Data | 9 | 1 | 0 | 4 | 16 | 0 |
| 15 | `SocietyRepository` | Data | 10 | 1 | 0 | 4 | 17 | 0 |
| 16 | `MembershipRepository` | Data | 15 | 1 | 0 | 5 | 22 | 0 |
| 17 | `EventRepository` | Data | 10 | 1 | 0 | 4 | 17 | 0 |
| 18 | `EventRegistrationRepository` | Data | 10 | 1 | 0 | 4 | 17 | 0 |
| 19 | `TaskRepository` | Data | 9 | 1 | 0 | 4 | 16 | 0 |
| 20 | `AnnouncementRepository` | Data | 8 | 1 | 0 | 4 | 15 | 0 |
| 21 | `AuthService` | Services | 3 | 1 | 0 | 6 | 12 | 0 |
| 22 | `SocietyService` | Services | 14 | 1 | 0 | 6 | 29 | 0 |
| 23 | `EventService` | Services | 15 | 1 | 0 | 5 | 29 | 0 |
| 24 | `MembershipService` | Services | 7 | 1 | 0 | 2 | 16 | 0 |
| 25 | `TaskService` | Services | 7 | 1 | 0 | 2 | 14 | 0 |
| 26 | `AnnouncementService` | Services | 7 | 1 | 0 | 2 | 14 | 0 |
| 27 | `ReportService` | Services | 8 | 1 | 0 | 4 | 14 | 0 |
| 28 | `PasswordHelper` | Helpers | 2 | 1 | 0 | 1 | 4 | n/a |
| 29 | `ValidationHelper` | Helpers | 5 | 1 | 0 | 1 | 8 | n/a |
| 30 | `TicketHelper` | Helpers | 1 | 1 | 0 | 0 | 1 | n/a |
| 31 | `AppTheme` | UI | 2 | 1 | 0 | 0 | 2 | n/a |
| 32 | `Program` | UI | 1 | 1 | 0 | 4 | 6 | n/a |
| 33 | `ModernButton` | UI | 4 | 6 | 0 | 2 | 10 | 0 |
| 34 | `ModernTextBox` | UI | 8 | 6 | 0 | 2 | 15 | 5 |
| 35 | `RoundedPanel` | UI | 2 | 7 | 0 | 1 | 5 | 0 |
| 36 | `StatCard` | UI | 3 | 7 | 0 | 2 | 6 | 0 |
| 37 | `BaseShellForm` | UI | 4 | 7 | 3 | 3 | 12 | 4 |
| 38 | `LoginForm` | UI | 4 | 7 | 0 | 8 | 13 | 0 |
| 39 | `RegisterForm` | UI | 4 | 7 | 0 | 4 | 9 | 0 |
| 40 | `AdminDashboard` | UI | 17 | 8 | 0 | 7 | 30 | 70 |
| 41 | `SocietyDashboard` | UI | 19 | 8 | 0 | 10 | 34 | 85 |
| 42 | `StudentDashboard` | UI | 15 | 8 | 0 | 6 | 27 | 50 |
| 43 | `EventFormDialog` | UI | 6 | 7 | 0 | 4 | 11 | 0 |
| 44 | `TaskFormDialog` | UI | 4 | 7 | 0 | 4 | 9 | 0 |
| 45 | `AnnouncementFormDialog` | UI | 3 | 7 | 0 | 4 | 8 | 0 |

Notes on the table:

- The NOC of 8 for `IRepository<T>` counts realisations (eight concrete repositories), not subclasses, because the strict CK definition uses inheritance only.
- The LCOM of 0 for `MembershipRepository` is the strict CK result because the single `_db` field is shared by all 15 methods. The class still mixes two database tables (`MembershipRequests` and `SocietyMembers`); see Section 6 for the split.
- LCOM values for the three dashboards are approximate manual estimates. Counting every cross-tab method pair by hand is impractical for forms with 15 to 19 methods; the numbers reflect the structure (one service field per tab, no shared business field across tabs).

---

## 5. Answers to assignment questions

### Q1. Maximum depth of inheritance

DIT = 8, reached by three classes: `AdminDashboard`, `SocietyDashboard`, and `StudentDashboard`.

Full inheritance path for all three:

```
Object
  -> MarshalByRefObject
    -> Component
      -> Control
        -> ScrollableControl
          -> ContainerControl
            -> Form
              -> BaseShellForm
                -> AdminDashboard / SocietyDashboard / StudentDashboard
```

Of the eight levels, six are framework levels up to `Form`, one is `BaseShellForm` (a shared sidebar shell I wrote), and one is the concrete dashboard. The DIT > 5 threshold is crossed, but I cannot remove the framework levels. The only level I introduced beyond `Form` is `BaseShellForm`. Replacing it with composition (passing a `UserControl` into each dashboard instead of subclassing) would drop DIT to 7.

### Q2. Highest and lowest WMC

Highest WMC = 19 (`SocietyDashboard`).

`SocietyDashboard` has 19 methods because it manages six distinct tabs: overview, members, membership requests, events, tasks, and announcements. Each tab has its own load method, action handlers, and dialog launchers. The methods themselves are individually small, but there are 19 of them in one class.

Second highest: `AdminDashboard` at 17. Third: `EventService` at 15.

Lowest WMC = 0, shared by eight POCO model classes: `User`, `Society`, `SocietyMember`, `MembershipRequest`, `Event`, `EventRegistration`, `SocietyTask`, `Announcement`.

These are data containers only. No business logic lives on the entities; it lives in the service layer. WMC = 0 is correct and intentional for this design.

WMC distribution:

| WMC range | Classes |
|---|---|
| 0 | 8 POCO models |
| 1 to 5 | Admin, Student, DatabaseConnection, IRepository, AuthService, PasswordHelper, TicketHelper, AppTheme, Program, ModernButton, RoundedPanel, StatCard, ValidationHelper, BaseShellForm, LoginForm, RegisterForm, AnnouncementFormDialog |
| 6 to 10 | All 8 repositories, MembershipService, TaskService, AnnouncementService, ReportService, ModernTextBox, EventFormDialog, TaskFormDialog |
| 11 to 19 | SocietyService (14), EventService (15), MembershipRepository (15), StudentDashboard (15), AdminDashboard (17), SocietyDashboard (19) |

### Q3. Class with the greatest number of children

By inheritance: `BaseShellForm` with NOC = 3 (`AdminDashboard`, `SocietyDashboard`, `StudentDashboard`).

`User` is second with NOC = 2 (`Admin`, `Student`). All other concrete classes have NOC = 0.

By interface realisation: `IRepository<T>` with 8 implementers (`UserRepository`, `StudentRepository`, `SocietyRepository`, `MembershipRepository`, `EventRepository`, `EventRegistrationRepository`, `TaskRepository`, `AnnouncementRepository`).

The strict CK definition uses inheritance only, so the formal answer is `BaseShellForm`. `IRepository<T>` is more fragile by interface signature: adding or removing a method forces 8 source files to change.

### Q4. Most complex class

Taking complexity as the combined worst score across WMC, RFC, and CBO:

| Class | WMC | RFC | CBO |
|---|---:|---:|---:|
| `SocietyDashboard` | 19 | 34 | 10 |
| `AdminDashboard` | 17 | 30 | 7 |
| `EventService` | 15 | 29 | 5 |
| `SocietyService` | 14 | 29 | 6 |

`SocietyDashboard` is the most complex class in the project. It has the highest WMC, highest RFC, and highest CBO. It is the view that a society head uses to run their society, which means it aggregates all six business services and hosts every tab those services power. Every new feature for societies adds at least one method here, which is why it has grown to be the busiest class.

### Q5. Most coupled class

`SocietyDashboard` with CBO = 10.

It is coupled to `SocietyService`, `EventService`, `TaskService`, `AnnouncementService`, `MembershipService`, `ReportService`, `Society` (used directly), `EventFormDialog`, `TaskFormDialog`, and `AnnouncementFormDialog`.

This puts it in the high-coupling band (above 6). It is coupled to every service in the project because it is the one view that exposes all society functions.

Second: `LoginForm` at CBO = 8. It must know all three dashboards to route the user after login.

Third: `SocietyService` and `AuthService`, both at CBO = 6.

### Q6. Least cohesive class

`SocietyDashboard` with LCOM around 85 (strict CK, counting cross-tab method pairs that share no business service field).

The 19 methods divide across six tabs. Methods within the same tab share their service field (Q pairs), but methods across different tabs do not. For example, `LoadEvents` uses `_eventService` and `LoadAnnouncements` uses `_annService`; these two methods share only the context fields `_societyId` and `_userId`, which are always present and not a business field that groups them by responsibility. Cross-tab pairs dominate: P around 110, Q around 25, LCOM around 85.

`AdminDashboard` (LCOM around 70) and `StudentDashboard` (LCOM around 50) follow for the same structural reason.

`MembershipRepository` deserves a separate note. Its strict CK LCOM is 0 because the single `_db` field is shared by all 15 methods, forcing every pair into Q. The class still covers two clearly separate database tables: 8 methods work on `MembershipRequests` and 5 methods work on `SocietyMembers`. If only the business-level fields were counted (treating the two groups as using different logical fields), P would be 8 times 5 = 40 and Q would be C(8,2) + C(5,2) = 38, giving LCOM = 2. This is a real design issue that the strict CK formula does not catch cleanly.

---

## 6. Refactoring suggestions for all modules with issues

This section walks through every class that crossed at least one CK threshold, what the underlying problem is, the specific refactoring action, and the expected metric values after the fix. Classes with all metrics in the green band are not listed.

### 6.1 Data layer

**`MembershipRepository`** (WMC = 15, RFC = 22, mixed responsibilities). The class touches two unrelated database tables (`MembershipRequests` and `SocietyMembers`). 8 methods serve the first table, 5 serve the second. The strict CK LCOM is 0 because all 15 methods share `_db`, but the class still violates Single Responsibility.

Refactor: split into two classes.

- `MembershipRequestRepository`: `GetById`, `GetAll`, `GetByStatus`, `GetByStudent`, `GetBySociety`, `Insert`, `UpdateStatus`, `HasPendingRequest`, `MapRequest` (8 methods).
- `SocietyMemberRepository`: `IsMember`, `AddMember`, `GetMembers`, `RemoveMember`, `MapMember` (5 methods).
- Drop the two stub `Update` and `Delete` methods that just return false.

Then update `SocietyService` and `MembershipService` to inject the specific repository they need.

Expected metrics after refactor: `MembershipRequestRepository` WMC = 8, RFC = 15. `SocietyMemberRepository` WMC = 5, RFC = 12. Both LCOM = 0.

**`DatabaseConnection`** (LCOM = 1, minor). `TestConnection()` does not directly read the `_connectionString` field, only indirectly through `GetConnection()`. This breaks the field-sharing rule for the (constructor, `TestConnection`) pair. The field is logically used by all three methods. No structural change is needed. Inlining the connection-string read inside `TestConnection()` would technically clear the LCOM hit, but that is a step backwards.

Verdict: leave as is.

**`IRepository<T>`** (NOC = 8, interface signature fragility). Eight repositories implement this interface. Adding a method forces 8 source files to change, even though several repositories already provide trivial stubs for `Update` and `Delete` (`MembershipRepository`, `EventRegistrationRepository`).

Refactor: split the interface by capability.

- `IReadRepository<T>`: `GetById`, `GetAll`.
- `IWriteRepository<T>`: `Insert`, `Update`, `Delete`.
- Concrete repositories implement only the capabilities they truly support.

Expected metrics: `IReadRepository<T>` realised by 8 repositories, `IWriteRepository<T>` realised by 6 (stubs gone).

### 6.2 Services layer

**`SocietyService`** (WMC = 14, RFC = 29, CBO = 6, instantiates dependencies inside method body). Two issues. First, the class is doing too much (society lifecycle plus member management plus queries). Second, `AssignHead()` instantiates `StudentRepository` and `UserRepository` directly with `new`, which bypasses dependency injection and inflates CBO from 4 to 6.

Refactor:

- Move all member-related methods (`GetMembers`, `RemoveMember`, `GetMemberSocieties`) into `MembershipService`.
- Move read-only queries (`GetAllSocieties`, `GetActiveSocieties`, `GetSocietiesByHead`) into a new `SocietyQueryService` for read-side caching later.
- Promote `_studentRepo` and `_userRepo` to constructor-injected fields instead of being constructed inside `AssignHead()`.

After the split, `SocietyService` keeps lifecycle methods only: `GetById`, `CreateSociety`, `UpdateSociety`, `AdminCreateSociety`, `AssignHead`, `ApproveSociety`, `SuspendSociety`, `DeleteSociety` (8 methods).

Expected metrics: `SocietyService` WMC = 8, RFC = 16, CBO = 4. `SocietyQueryService` WMC = 3, RFC = 6, CBO = 2. `MembershipService` (with the 3 new methods) WMC = 10, RFC = 22, CBO = 3.

**`EventService`** (WMC = 15, RFC = 29). Same shape as `SocietyService`. Mixes event lifecycle (`CreateEvent`, `UpdateEvent`, `ApproveEvent`, `CancelEvent`, `CompleteEvent`) with registration concerns (`RegisterForEvent`, `GetStudentRegistrations`, `GetEventAttendees`, `IsRegistered`, `UpdateAttendance`).

Refactor: split along the registration boundary.

- `EventService`: lifecycle plus queries (`GetAllEvents`, `GetUpcomingEvents`, `GetEventsBySociety`, `GetPendingEvents`, `GetById`, `CreateEvent`, `UpdateEvent`, `ApproveEvent`, `CancelEvent`, `CompleteEvent`) - 10 methods.
- `EventRegistrationService`: `RegisterForEvent`, `GetStudentRegistrations`, `GetEventAttendees`, `IsRegistered`, `UpdateAttendance` - 5 methods.

Expected metrics: `EventService` WMC = 10, RFC = 18, CBO = 3. `EventRegistrationService` WMC = 5, RFC = 12, CBO = 3.

**`AuthService`** (CBO = 6, acceptable but could be tightened). Borderline coupling. Touches `UserRepository`, `StudentRepository`, `PasswordHelper`, `ValidationHelper`, `User`, `Student`. The service is small (WMC = 3), so the coupling is mostly justified.

Optional refactor: extract a `RegistrationValidator` class that owns the three `ValidationHelper` calls in `RegisterStudent`. This drops `AuthService` CBO from 6 to 5 and makes `RegisterStudent` testable in isolation.

Verdict: low priority. Worth doing only if more validation rules are added.

**`MembershipService`** (RFC = 16, slightly high for 7 methods). RFC is high because the class chains multiple repository calls inside `ApplyForMembership` and `ProcessRequest` (membership check plus pending check plus insert; or fetch plus status check plus update plus member add).

The high RFC reflects necessary business logic, not poor design. If `MembershipService` absorbs the 3 member-related methods from `SocietyService`, RFC will rise to around 22. That is still acceptable for a domain-rich service.

Verdict: leave the RFC, but absorb the `SocietyService` member methods as noted above.

### 6.3 UI layer

**`SocietyDashboard`** (WMC = 19, RFC = 34, CBO = 10, LCOM = 85, worst class in project). Six business tabs (overview, members, events, tasks, announcements, reports) all live in one form. Every service in the project is held as a field. Cross-tab method pairs share no business field, producing the highest LCOM in the codebase.

Refactor: decompose into a thin host plus one `UserControl` per tab.

```
SocietyDashboard (navigation host)
  - SocietyOverviewTab (UserControl)
  - SocietyMembersTab (UserControl)
  - SocietyEventsTab (UserControl)
  - SocietyTasksTab (UserControl)
  - SocietyAnnouncementsTab (UserControl)
  - SocietyReportsTab (UserControl)
```

Each tab owns only the service or services it actually uses. `SocietyDashboard` keeps `_societyId`, `_userId`, the inherited `BuildShell()`, and a small `NavigateTo(tab)` switch.

Expected metrics:

| Class | WMC | RFC | CBO | LCOM |
|---|---:|---:|---:|---:|
| `SocietyDashboard` (after) | 4 | 10 | 7 | 0 |
| `SocietyMembersTab` | 4 | 8 | 2 | 0 |
| `SocietyEventsTab` | 3 | 7 | 3 | 0 |
| `SocietyTasksTab` | 3 | 6 | 2 | 0 |
| `SocietyAnnouncementsTab` | 3 | 6 | 2 | 0 |
| `SocietyReportsTab` | 2 | 5 | 2 | 0 |
| `SocietyOverviewTab` | 2 | 5 | 2 | 0 |

**`AdminDashboard`** (WMC = 17, RFC = 30, LCOM = 70). Same structural issue as `SocietyDashboard`. Tabs for students, societies, events, reports, and dashboard overview.

Refactor: same per-tab decomposition.

```
AdminDashboard (host)
  - AdminStudentsTab
  - AdminSocietiesTab
  - AdminEventsTab
  - AdminReportsTab
  - AdminOverviewTab
```

Expected metrics: `AdminDashboard` after the split has WMC = 4, RFC = 10, LCOM = 0. Each tab `UserControl` ends up with WMC = 3 to 5, RFC = 7 to 10, LCOM = 0.

**`StudentDashboard`** (WMC = 15, RFC = 27, LCOM = 50). Same pattern, with tabs for societies, my requests, my events, my tasks, announcements, and profile.

Refactor: same per-tab decomposition. Six tabs, each becoming a separate `UserControl`.

Expected metrics: `StudentDashboard` after the split has WMC = 4, RFC = 9, LCOM = 0. Each tab `UserControl` ends up with WMC = 2 to 4, RFC = 6 to 9, LCOM = 0.

**`LoginForm`** (CBO = 8). `LoginForm` references all three concrete dashboard classes (`AdminDashboard`, `SocietyDashboard`, `StudentDashboard`) plus `RegisterForm`. The role-based routing logic is hardcoded inside `OpenDashboard()`.

Refactor: extract a `DashboardFactory` that takes a `User` and an optional `Student` and returns the right `Form`. `LoginForm` then depends only on the factory.

```csharp
public static class DashboardFactory
{
    public static Form Create(User user, Student? student) => user.Role switch
    {
        "Admin"       => new AdminDashboard(user),
        "SocietyHead" => new SocietyDashboard(user, student!),
        _             => new StudentDashboard(user, student!)
    };
}
```

Expected metrics: `LoginForm` (after) has CBO = 5, RFC = 10. `DashboardFactory` has CBO = 4, RFC = 4. The total coupling does not vanish; it is relocated to a small factory whose only job is to make this routing decision. The win is that `LoginForm` no longer changes every time a new role is added.

**`BaseShellForm`** (LCOM = 4, NOC = 3, `BuildShell()` is a god method). `BuildShell()` is over 130 lines and constructs sidebar, top bar, content panel, logo, user card, and logout button in one go. It does not touch the same instance fields as `AddNavItem()`, `ActivateFirstNav()`, or `ShowPage()`, which inflates LCOM.

Refactor: break `BuildShell()` into helper methods.

- `BuildSidebar()` returns a configured `Panel`.
- `BuildTopBar()` returns a configured `Panel`.
- `BuildLogo(Panel parent)`.
- `BuildUserCard(Panel parent, string userName, string role)`.

Then `BuildShell()` orchestrates these calls and assigns to `SidebarPanel`, `TopBar`, `ContentPanel`. Each helper stays short and testable.

Expected metrics: `BaseShellForm` (after) has WMC = 7, LCOM = 0. WMC goes up because there are more named methods, but each method is tiny and the cohesion problem disappears.

**`ModernTextBox`** (LCOM = 5). Methods `WndProc`, `OnTextChanged`, and `CreateParams` do not touch `_placeholder` or `_isFocused`, while `OnPaint`, `OnGotFocus`, and `OnLostFocus` do. Fifteen cross-pair combinations have no shared field.

Two options:

1. Move placeholder logic into a separate `PlaceholderBehavior` helper class that attaches to the textbox via events. `ModernTextBox` stops carrying `_placeholder` and `_isFocused` directly.
2. Accept the LCOM. The class is small (8 methods) and the LCOM signal is mild. Some of those methods (`WndProc`, `CreateParams`) are framework overrides that have to live on the control, not extracted.

Verdict: option 2 is acceptable for a UI control of this size. Document the field/method separation in a class-level comment.

### 6.4 Models layer

The Models layer scores well on every metric. No structural refactoring required.

One stylistic note: the design follows the Anemic Domain Model pattern (entities are pure data, behaviour lives in services). For a CRUD-oriented application like this one it keeps each layer responsible for one thing, and WMC = 0 here is correct.

If the project grew to include things like "calculate event capacity utilisation" or "validate task assignment rules", those could move onto the `Event` and `SocietyTask` entities respectively, raising their WMC to 2 or 3 without harming any other metric.

### 6.5 Refactoring priority order

Sorted by impact-to-effort ratio:

| Priority | Refactor | Effort | Classes affected | Worst metric improvement |
|:--:|---|:--:|:--:|---|
| 1 | Decompose `SocietyDashboard` | High | 1 to 7 | LCOM 85 to 0 |
| 2 | Split `MembershipRepository` | Low | 1 to 2 | WMC 15 to 8 + 5 |
| 3 | Decompose `AdminDashboard` | High | 1 to 6 | LCOM 70 to 0 |
| 4 | Decompose `StudentDashboard` | High | 1 to 7 | LCOM 50 to 0 |
| 5 | Split `EventService` | Medium | 1 to 2 | WMC 15 to 10 + 5 |
| 6 | Move member methods from `SocietyService` to `MembershipService` | Medium | 2 affected | CBO 6 to 4 |
| 7 | Extract `DashboardFactory` from `LoginForm` | Low | 1 to 2 | CBO 8 to 5 |
| 8 | Break `BuildShell()` in `BaseShellForm` | Low | 1 | LCOM 4 to 0 |
| 9 | Split `IRepository<T>` into read and write | Low | 9 affected | NOC fragility down |
| 10 | Extract `RegistrationValidator` from `AuthService` | Low | 1 to 2 | CBO 6 to 5 (optional) |

Items 1, 2, 8, and 9 give the most metric improvement per hour of work. Items 3 and 4 are the same pattern as item 1 and would be done together.

---

## 7. Conclusion

The project is structured sensibly across its layers. The Models layer is correctly anemic (WMC = 0, every metric low). The Data and Services layers are consistent and predictable. All the metric pressure is concentrated in the UI layer, specifically in the three dashboards, because that is where all the services converge.

The dashboard LCOM values (85, 70, 50) are the clearest signal that those classes are doing too much. The recommended fix (per-tab `UserControl` decomposition) is not a theoretical refactor; it would also make the UI code easier to read, test, and extend. `MembershipRepository` is the second priority: the split costs one afternoon and removes the only class in the repository layer that visibly mixes two table responsibilities.

Section 6 lists 10 refactoring actions covering every class that crossed a CK threshold. Doing items 1, 2, 8, and 9 first moves every cell in the metrics table into the green band with about three days of focused work.

---

*Report prepared by Abdul Qudoos, 22i-8774.*
