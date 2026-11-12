# สถาปัตยกรรมระบบ (Architecture)

เอกสารนี้อธิบายโครงสร้างตาม **โค้ดปัจจุบัน** (12 พ.ย. 2569) สำหรับเหตุผลเบื้องหลังการออกแบบดู [reference/design-decisions.md](reference/design-decisions.md)

## 1. ภาพรวม

```mermaid
flowchart LR
    Browser["Browser<br/>Angular 17 SPA<br/>localhost:4200"]
    API["ASP.NET Core API (.NET 10)<br/>localhost:5005"]
    DB[("SQL Server<br/>InternLogDB")]
    FS[["App_Data/uploads<br/>ไฟล์แนบ + PDF"]]

    Browser -- "HTTP JSON + Bearer JWT<br/>+ refresh cookie (/api/auth)" --> API
    API -- "EF Core" --> DB
    API -- "IFileStorage<br/>(ดาวน์โหลดผ่าน endpoint ที่ตรวจสิทธิ์)" --> FS
```

| ชั้น | เทคโนโลยีหลัก |
|---|---|
| Frontend | Angular 17 standalone components, Signals, RxJS, `date-fns`, `jwt-decode` |
| Backend | ASP.NET Core Web API, EF Core (SQL Server), JWT Bearer, BCrypt.Net, QuestPDF, Swashbuckle |
| Database | SQL Server, schema จัดการด้วย EF Core Migrations (`InternLog.API/Migrations/`) |

## 2. Backend (`InternLog.API/`)

### 2.1 โครงสร้างโฟลเดอร์

```
InternLog.API/
├── Program.cs                 # DI, JWT, CORS, Swagger, middleware pipeline, dev seed
├── appsettings.json           # connection string, JWT issuer/audience (key อยู่ใน user-secrets)
├── Migrations/                # EF Core migrations
└── src/
    ├── Core/                  # ของที่ใช้ร่วมกันทุกโมดูล
    │   ├── Common/            # ApiResponse<T>, AppException, AppControllerBase, Constants, AppClock
    │   ├── Interfaces/        # ICurrentUserService
    │   ├── Security/          # CurrentUserService, PasswordHasher (BCrypt), Sha256Hasher
    │   └── Storage/           # IFileStorage / LocalFileStorage (ตรวจ + เก็บไฟล์อัปโหลด)
    ├── Infrastructure/
    │   ├── Data/              # AppDbContext (Fluent config + HasData), Models/Entities.cs, DevDataSeeder
    │   └── Middleware/        # ExceptionMiddleware
    └── Modules/               # แยกตามฟีเจอร์
        ├── Auth/  Attendance/  Leaves/  WorkLogs/  Tasks/
        ├── AdvisorVisits/  Reports/  Evaluations/  Notifications/
        └── Dashboard/  Admin/  Companies/
```

### 2.2 รูปแบบของแต่ละโมดูล

ทุกโมดูลมีโครงเดียวกัน:

```
Modules/<Name>/
├── <Name>Controller.cs   # รับ request, ตรวจ role ด้วย [Authorize(Roles=...)], ห่อผลด้วย ApiResponse
├── I<Name>Service.cs     # interface
├── <Name>Service.cs      # business logic + ตรวจสิทธิ์ระดับข้อมูล + query ผ่าน AppDbContext
└── DTOs/                 # request/response records
```

Service ลงทะเบียนใน `Program.cs` แบบ `AddScoped<IXService, XService>()` · `IFileStorage` เป็น singleton

### 2.3 Request pipeline

```mermaid
flowchart LR
    R[Request] --> EM[ExceptionMiddleware] --> C[CORS] --> AN[Authentication<br/>JWT] --> AZ[Authorization<br/>Roles] --> CT[Controller] --> S[Service] --> DB[(DbContext)]
```

### 2.4 Response และ error

ทุก endpoint ตอบในรูปแบบเดียวกัน (`Core/Common/ApiResponse.cs`):

```json
{ "success": true, "message": "สำเร็จ", "data": { } }
```

- Service โยน `AppException` (`BadRequest`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`) → `ExceptionMiddleware` แปลงเป็น status code + `success: false`
- Exception อื่น → 500 โดยซ่อนรายละเอียดเมื่อไม่ใช่ Development
- 401/403 จาก JWT ตอบเป็น JSON ภาษาไทยผ่าน `JwtBearerEvents` ใน `Program.cs`

### 2.5 การตรวจสิทธิ์ 2 ชั้น

1. **ระดับ endpoint (RBAC)** — `[Authorize(Roles = "student,advisor")]` บน controller/action
2. **ระดับข้อมูล (กัน IDOR)** — ใน service ใช้ `ICurrentUserService` (อ่าน `sub`/role จาก JWT) แล้วตรวจ:
   - **student** เข้าถึงได้เฉพาะข้อมูลที่ `StudentID == ตัวเอง`
   - **advisor** เข้าถึงได้เฉพาะนักศึกษาที่มี `Internship.AdvisorID == ตัวเอง`
   - **admin** เข้าถึงได้ทั้งหมด

## 3. Authentication

```mermaid
sequenceDiagram
    participant W as Angular
    participant A as API /api/auth
    participant D as DB

    W->>A: POST /login {email, password}
    A->>D: หา user (IsActive) + BCrypt.Verify
    A->>D: บันทึก SHA-256(refreshToken)
    A-->>W: body: accessToken (JWT 120 นาที) + user<br/>Set-Cookie: refreshToken (HttpOnly, 7 วัน)
    Note over W: accessToken + user ใน localStorage<br/>refresh token JavaScript อ่านไม่ได้
    W->>A: GET /api/... (Authorization: Bearer)
    A-->>W: 401 เมื่อ access token หมดอายุ
    W->>A: POST /refresh (browser แนบ cookie เอง)
    A->>D: revoke ตัวเก่าแบบ atomic → ออกคู่ใหม่ → ผูก ReplacedByTokenID
    A-->>W: accessToken ใหม่ + cookie ใหม่ แล้ว retry request เดิม
```

| เรื่อง | รายละเอียด |
|---|---|
| Access token | JWT HS256, อายุ 120 นาที, claims: `sub` (UserID), `name`, role, `jti` · `ClockSkew = 0` |
| Refresh token | 64 bytes สุ่ม, ส่งผ่าน cookie `HttpOnly; SameSite=Strict; Path=/api/auth` (Secure เมื่อเป็น HTTPS), เก็บใน DB เป็น SHA-256 hash, อายุ 7 วัน, หมุนทุกครั้งที่ใช้ — ใช้ซ้ำหรือ user ถูกปิดใช้งาน → 401 |
| Login | รัน BCrypt ทุกครั้งแม้ไม่พบ email (เวลาตอบเท่ากัน กันเดา email) |
| Password | BCrypt work factor 12 |
| Reset password | token สุ่ม 64 bytes, เก็บเป็น SHA-256, อายุ 1 ชั่วโมง, ใช้ได้ครั้งเดียว |
| เปลี่ยน/รีเซ็ตรหัสผ่าน | revoke refresh token ทั้งหมดของ user |
| สมัครสมาชิก | ได้เฉพาะ role `student` |

ฝั่งเว็บ:
- `core/interceptors/auth.interceptor.ts` แนบ Bearer token, ถ้า token หมดอายุหรือได้ 401 จะ refresh แล้ว retry · ไม่ refresh สำหรับ endpoint auth ที่ไม่ต้อง login (login, refresh, logout ฯลฯ)
- `AuthService.refreshTokens()` ใช้ refresh ที่กำลังทำอยู่ร่วมกัน (ยิงพร้อมกันหลายครั้งจะล้มเหลวเพราะ token หมุน)
- `authGuard` / `roleGuard` เรียก `ensureSession()` — ไม่มี access token ก็ลอง refresh ด้วย cookie ก่อนพาไป login

### เวลา

ใช้ `AppClock` (`Core/Common/AppClock.cs`) แทน `DateTime.Now` — คืนเวลาประเทศไทยเสมอไม่ว่า server ตั้ง timezone อะไร
เวลาธุรกิจ (ลงเวลา, วันลา, CreatedAt) ใช้ `AppClock.Now` / `AppClock.Today` · เวลา token ใช้ `DateTime.UtcNow`

## 4. ข้อมูล (Database)

19 ตาราง (entity ทั้งหมดอยู่ใน `src/Infrastructure/Data/Models/Entities.cs`)

```mermaid
erDiagram
    Role ||--o{ User : has
    User ||--o{ Internship : "student / advisor"
    Company ||--o{ Internship : hosts
    TrainingPeriod ||--o{ Internship : in
    Internship ||--o{ Leave : ""
    Internship ||--o{ TaskItem : ""
    Internship ||--o{ AdvisorVisit : ""
    Internship ||--o{ Report : ""
    Internship ||--o{ Evaluation : ""
    User ||--o{ Attendance : "student"
    User ||--o{ WorkLog : "student"
    WorkCategory ||--o{ WorkLog : ""
    WorkLog ||--o{ Attachment : ""
    WorkLog ||--o{ LogComment : ""
    LeaveType ||--o{ Leave : ""
    User ||--o{ Notification : ""
    User ||--o{ RefreshToken : ""
    User ||--o{ PasswordResetToken : ""
```

แกนกลางของระบบคือ **`Internship`** (นักศึกษา 1 คน + อาจารย์ 1 คน + บริษัท + ภาคเรียน) — สิทธิ์ของ advisor คำนวณผ่านตารางนี้
`Attendance` และ `WorkLog` ผูกกับ `StudentID` โดยตรง ส่วน `Leave`, `TaskItem`, `AdvisorVisit`, `Report`, `Evaluation` ผูกกับ `InternshipID`

ข้อมูลตั้งต้น (`HasData`): roles (`student`, `advisor`, `supervisor`, `admin`), ประเภทการลา 3 แบบ, หมวดงาน 5 หมวด, admin 1 คน

ค่าสถานะที่ใช้ทั้งระบบอยู่ใน `Core/Common/Constants.cs` (attendance, leave, task, report, internship status)

## 5. Frontend (`InternLog.Web/`)

```
src/app/
├── app.routes.ts          # route ทั้งหมด (lazy loadComponent)
├── app.config.ts          # provideRouter, provideHttpClient(withInterceptors)
├── core/
│   ├── auth/              # AuthService (signals: currentUser, isAuthenticated)
│   ├── guards/            # authGuard, roleGuard (data.roles)
│   ├── interceptors/      # authInterceptor (Bearer + refresh)
│   └── services/          # service เรียก API ของแต่ละโมดูล
├── layout/                # shell, sidebar (เมนูตาม role), topbar
├── features/<feature>/    # หน้าจอแยกตามฟีเจอร์ (บางฟีเจอร์มี services/ models/ ของตัวเอง)
└── models/                # interface ที่ใช้ร่วมกัน
```

### 5.1 Routes และสิทธิ์

ทุกหน้าใต้ `ShellComponent` ต้องผ่าน `authGuard` หน้าที่จำกัด role ใช้ `roleGuard`

| Path | หน้าที่ | Role (guard) | เมนู sidebar แสดงให้ |
|---|---|---|---|
| `/login`, `/register`, `/forgot-password`, `/reset-password` | Auth | ไม่ต้อง login | — |
| `/dashboard` | สรุปภาพรวม (student: `dashboard-student`, advisor/admin: `dashboard-staff`) | ทุก role | ทุก role |
| `/attendance` | ลงเวลา/ประวัติ | ทุก role | ทุก role |
| `/leaves` | ใบลา | ทุก role | ทุก role |
| `/calendar` | ปฏิทิน | ทุก role | ทุก role |
| `/work-logs`, `/work-logs/new`, `/work-logs/:id` | บันทึกงานรายวัน | ทุก role | student, advisor |
| `/tasks` | Kanban งานที่มอบหมาย | ทุก role | ทุก role |
| `/visits` | การนิเทศ | ทุก role | ทุก role |
| `/reports`, `/reports/:id/preview` | รายงาน | ทุก role | ทุก role |
| `/evaluations` | การประเมิน | ทุก role | student, advisor |
| `/notifications`, `/profile` | แจ้งเตือน / โปรไฟล์ | ทุก role | ทุก role |
| `/companies` | บริษัทที่ดูแล | advisor, admin | advisor |
| `/admin/users`, `/admin/companies`, `/admin/internships` | จัดการระบบ | admin | admin |

> การซ่อนเมนูเป็นแค่ UX — การป้องกันจริงอยู่ที่ API (ข้อ 2.5)

### 5.2 การเรียก API

- Base URL: `environment.apiUrl` (`http://localhost:5005/api`)
- ทุก service คืน `Observable<ApiResponse<T>>` ตรงกับ format ฝั่ง API
- Service ส่วนใหญ่อยู่ใน `core/services/` · `work-logs` และ `companies` มี service ใน `features/<feature>/services/`
- ไฟล์ที่ต้อง login (ไฟล์แนบ, PDF) โหลดผ่าน `core/services/file-download.service.ts` — ใช้ HttpClient (แนบ token) แล้วเปิดจาก blob แทน `<a href>`

## 6. ไฟล์อัปโหลด

`Core/Storage/FileStorage.cs` (`IFileStorage` / `LocalFileStorage`) จัดการไฟล์ทั้งหมด:

- เก็บที่ `App_Data/uploads/<category>/<guid>.<ext>` (ตั้งค่าได้ด้วย `FileStorage:Root`) — **ไม่อยู่ใน wwwroot และไม่มี `UseStaticFiles()`**
- ตรวจขนาด, นามสกุล (`.pdf .jpg .jpeg .png .doc .docx`) และ magic bytes ก่อนบันทึก · ไม่ใช้ชื่อไฟล์ของผู้ใช้บน disk
- กัน path traversal ตอนเปิดไฟล์ · อ่าน path แบบเก่า (`/uploads/...`) ได้
- ลบไฟล์จริงเมื่อลบไฟล์แนบหรือ Work Log

| ประเภท | category | ขนาดสูงสุด | ดาวน์โหลดที่ | ใครดาวน์โหลดได้ |
|---|---|---|---|---|
| ไฟล์แนบใบลา | `leaves` | 10 MB | `GET /api/leaves/{id}/attachment` | เจ้าของ, อาจารย์ที่ดูแล, admin |
| ไฟล์แนบ Work Log | `worklogs` | 5 MB | `GET /api/worklogs/attachments/{id}` | เจ้าของ, อาจารย์ที่ดูแล, admin |
| PDF รายงาน | `reports` | — | `GET /api/reports/{id}/export-pdf` (สร้างใหม่ทุกครั้ง) | นักศึกษาเจ้าของ, อาจารย์ที่ดูแล |

DB เก็บ path ภายใน ส่วน DTO ที่ส่งให้เว็บ (`attachmentUrl`, `fileUrl`, `filePath`) เป็น URL ของ endpoint ดาวน์โหลดข้างบน

## 7. Tests (`InternLog.API.Tests/`)

| ไฟล์ | ทดสอบ |
|---|---|
| `AuthServiceTests` | login, refresh rotation, reuse token, user ถูกปิดใช้งาน, logout, reset password |
| `FileStorageTests` | magic bytes, นามสกุล, ขนาด, path traversal, path แบบเก่า |
| `AttachmentAccessTests` | สิทธิ์ดาวน์โหลดไฟล์แนบใบลา/Work Log ของแต่ละ role |
| `AuthCookieFlowTests` | ผ่าน HTTP จริง (`WebApplicationFactory`): cookie flow, `/uploads` ไม่ถูกเสิร์ฟ, endpoint ดาวน์โหลดต้อง login |

ใช้ SQLite in-memory (`Infrastructure/TestDb.cs`) แทน SQL Server — `GETDATE()` ถูกแทนด้วย `CURRENT_TIMESTAMP` เฉพาะใน test
