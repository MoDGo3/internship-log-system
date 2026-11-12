# แผนพัฒนาและใบสั่งงาน (Roadmap & Work Order)

_แผนงานตาม Phase และรายการงาน Frontend/Backend ทีละขั้น_

> ⚠️ **เอกสารอ้างอิงจากช่วงออกแบบ/พัฒนา** — รวมจากไฟล์เดิมหลายไฟล์ เนื้อหาคงไว้ตามต้นฉบับ
> บางส่วนอาจไม่ตรงกับโค้ดปัจจุบัน (เช่น ระบุ .NET 8 แต่ใช้ .NET 10 จริง, โมดูล Messages ถูกลบแล้ว,
> ระบบคำนวณมาสายถูกยกเลิก) — ข้อมูลที่ตรงกับโค้ดล่าสุดดูที่ [architecture.md](../architecture.md), [api-reference.md](../api-reference.md) และ [project-status.md](../project-status.md)

## สารบัญ

1. [Development Roadmap (ส่วนที่ 5)](#ส่วนที่-1-development-roadmap-ส่วนที่-5)
2. [Work Order — ใบสั่งงานฉบับปฏิบัติการ (ส่วนที่ 13)](#ส่วนที่-2-work-order--ใบสั่งงานฉบับปฏิบัติการ-ส่วนที่-13)

---

## ส่วนที่ 1: Development Roadmap (ส่วนที่ 5)

> ต้นฉบับ: `docs/internship-log-development-roadmap.md` · จัดทำ 14 ส.ค. 2569

### 1. Executive Summary

แผนพัฒนาแบ่งเป็น **5 งานหลัก** จัดลำดับตาม dependencies และ risk:
- **Foundation Layer** (งาน 1–3): SQL + Backend Setup + Auth API
- **Frontend Foundation** (งาน 4): Angular Shell + Auth Flow
- **First Feature** (งาน 5): Attendance API + Page (pattern example)

**หลักการ:** ทำ foundation ให้แข็งแรงก่อน แล้วใช้ Attendance เป็น pattern สำหรับโมดูลที่เหลือ (Leaves, WorkLogs, Tasks, …)

---

### 2. Phase 1: Foundation Layer

#### 2.1 🗄️ งานที่ 1: SQL Script สมบูรณ์ (19 ตาราง)

**วัตถุประสงค์:** สร้างฐานข้อมูลที่ถูกต้องครบถ้วน เป็น foundation ของระบบ

**งานย่อย:**
- [ ] รวมสคริปต์จากส่วนที่ 3 + ส่วนที่ 4 (Tasks แก้ + RefreshTokens เพิ่ม)
- [ ] เพิ่ม Foreign Keys ครบทุกความสัมพันธ์ (19 ตาราง)
- [ ] เพิ่ม Indexes สำหรับ performance (WHERE clause ที่ใช้บ่อย)
- [ ] เพิ่ม Constraints (UNIQUE, CHECK, DEFAULT)
- [ ] Seed data:
  - `Roles` 4 แถว (student / advisor / supervisor / admin)
  - `LeaveTypes` 3 แถว (ป่วย 30 / กิจ 3 / พักร้อน 6)
  - `WorkCategories` 5 แถว
  - `Users` admin คนแรก (BCrypt hash)
- [ ] สร้าง Database Diagram ใน SSMS

**Deliverables:**
```sql
-- ไฟล์: database/InternLogDB.sql
-- 19 tables + 30+ FKs + 15+ indexes + constraints + seed
```

**Definition of Done:**
- [ ] รันสคริปต์ใน SSMS สำเร็จ ไม่มี error
- [ ] Object Explorer เห็น 19 ตาราง
- [ ] Query `SELECT * FROM Users WHERE Email = 'admin@internlog.dev'` ได้ข้อมูล
- [ ] Generate Diagram ครบทุกความสัมพันธ์

---

#### 2.2 🏗️ งานที่ 2: ASP.NET Core Project Setup

**วัตถุประสงค์:** สร้างโครงสร้าง backend ที่ scalable + maintainable

**งานย่อย:**
- [ ] `dotnet new webapi -n InternLog.API` + ติดตั้ง packages:
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `BCrypt.Net-Next`
  - `Swashbuckle.AspNetCore` (Swagger)
- [ ] สร้างโฟลเดอร์โครงสร้าง:
  ```
  InternLog.API/
  ├── Controllers/
  ├── Services/
  ├── Data/
  ├── Models/
  ├── DTOs/
  ├── Middleware/
  └── Utils/
  ```
- [ ] สร้าง `AppDbContext` + Entity models ครบ 19 ตาราง
- [ ] สร้าง `AppControllerBase` (helper ดึง UserId จาก JWT claims)
- [ ] สร้าง `ApiResponse<T>` (response กลางรูปแบบ `{success, message, data}`)
- [ ] สร้าง `ExceptionMiddleware` (จัดการ error กลาง, แปลง exception → BadRequest)
- [ ] Configure ใน `Program.cs`:
  - CORS (อนุญาต `http://localhost:4200`)
  - JWT Bearer Authentication
  - Swagger (พร้อม Bearer token input)
  - EF Core DbContext
- [ ] สร้าง `DbSeeder` (seed ข้อมูลเริ่มต้น)

**Deliverables:**
```
InternLog.API/
├── Controllers/       (AuthController เปล่า)
├── Services/          (interfaces เปล่า)
├── Data/              (AppDbContext.cs + 19 entities)
├── Models/            (19 models)
├── DTOs/              (ApiResponse.cs)
├── Middleware/        (ExceptionMiddleware.cs)
├── Utils/             (AppControllerBase.cs)
└── Program.cs
```

**Definition of Done:**
- [ ] `dotnet build` สำเร็จ ไม่มี warning/error
- [ ] `dotnet run` → Swagger UI เปิดได้ http://localhost:5xxx/swagger
- [ ] Log แสดงเชื่อมต่อ DB สำเร็จ
- [ ] Seed data ทำงาน (มี admin user ใน DB)

---

#### 2.3 🔐 งานที่ 3: Auth API (Core Authentication)

**วัตถุประสงค์:** ระบบ login/JWT/refresh ที่เป็น foundation ของทุก API อื่น

**งานย่อย:**
- [ ] **AuthService:**
  - `LoginAsync()` → BCrypt verify → สร้าง accessToken + refreshToken
  - `RefreshAsync()` → verify token → revoke ตัวเก่า → ออกคู่ใหม่ (rotation)
  - `LogoutAsync()` → revoke refreshToken
  - `GetMeAsync()` → ดึงข้อมูล user ปัจจุบัน
  - `ChangePasswordAsync()` → verify รหัสเก่า → hash รหัสใหม่
- [ ] **AuthController:** 5 endpoints
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
  - `POST /api/auth/logout`
  - `GET /api/auth/me`
  - `PUT /api/auth/change-password`
- [ ] **AuthDtos:**
  - `LoginRequest` (Email, Password)
  - `LoginResponse` (accessToken, refreshToken, UserDto)
  - `RefreshRequest` (refreshToken)
  - `UserDto` (UserID, Email, FirstName, LastName, Role, StudentID)
  - `ChangePasswordRequest` (oldPassword, newPassword, confirmPassword)
- [ ] **RefreshToken logic:**
  - Generate random token (base64)
  - Hash ด้วย BCrypt ก่อนเก็บ DB
  - Rotation: revoke ตัวเก่า + สร้างตัวใหม่ (เก็บ `ReplacedByTokenID`)
  - Expiration: Access 2 ชม. / Refresh 7 วัน
- [ ] **JWT config:**
  - Algorithm: HS256
  - Claims: `sub` (UserID), `name`, `role`
  - `ValidateLifetime = true`, `ClockSkew = 0`
- [ ] **Password validation:**
  - Minimum 8 chars
  - ต้องมี uppercase + lowercase + number + special char

**Deliverables:**
```csharp
Controllers/AuthController.cs       // 5 endpoints
Services/AuthService.cs             // business logic
Services/IAuthService.cs            // interface
DTOs/AuthDtos.cs                    // request/response DTOs
Data/AppDbContext.cs                // + RefreshTokens entity
```

**Definition of Done (Test Cases):**
- [ ] TC-01: POST /login email+password ถูก → 200 + accessToken + refreshToken + user
- [ ] TC-02: POST /login รหัสผิด → 401 (ไม่บอกว่า user ไม่มี)
- [ ] TC-03: POST /refresh + refreshToken ถูก → 200 + คู่ใหม่ (token เก่าถูก revoke)
- [ ] TC-04: POST /refresh + token ที่ revoke แล้ว → 401
- [ ] TC-05: GET /me + Bearer token → 200 + user info
- [ ] TC-06: GET /me ไม่มี token → 401
- [ ] TC-07: PUT /change-password → 200 + ต้อง login ใหม่ด้วยรหัสใหม่
- [ ] TC-08: POST /logout → revoke token + ไม่สามารถใช้ refresh ได้อีก

---

### 3. Phase 2: Frontend Foundation

#### 3.1 🅰️ งานที่ 4: Angular Project Setup

**วัตถุประสงค์:** สร้างโครงสร้าง frontend ที่ ready สำหรับ feature development

**งานย่อย:**
- [ ] `ng new InternLog.Web --routing --style=scss --ssr=false`
- [ ] ติดตั้ง packages:
  - Angular Material หรือ PrimeNG (เลือกอย่างใดอย่างหนึ่ง)
  - `date-fns` (จัดการวันที่)
  - `jwt-decode` (decode token)
- [ ] สร้างโฟลเดอร์โครงสร้าง:
  ```
  src/app/
  ├── core/           (services, interceptors, guards)
  ├── shared/         (design system components)
  ├── features/       (feature modules)
  ├── models/         (interfaces)
  └── layout/         (shell components)
  ```
- [ ] **Routing (`app.routes.ts`):**
  - `/login`, `/dashboard`, `/attendance`, `/leaves`, …
  - Guards: `authGuard`, `roleGuard(['student'])`
- [ ] **AuthService:**
  - `login()`, `logout()`, `refresh()`, `getMe()`
  - Token management: เก็บใน localStorage
  - `isAuthenticated()` → check token expiration
- [ ] **HttpInterceptor (`auth.interceptor.ts`):**
  - แนบ `Authorization: Bearer <accessToken>` ทุกคำขอ
  - Handle 401 → เรียก refresh ครั้งเดียว
  - Queue คำขอที่ค้างระหว่าง refresh → retry ทั้งหมด
  - ถ้า refresh ล้มเหลว → logout + redirect /login
- [ ] **Guards:**
  - `authGuard` → check token → redirect /login ถ้าไม่มี
  - `roleGuard` → check role จาก JWT claims → redirect /403 ถ้าไม่มีสิทธิ์
- [ ] **Design System (`shared/`):**
  - Color tokens (จากเอกสารส่วนที่ 2)
  - Typography (Sarabun + Prompt)
  - Shared components: `ButtonComponent`, `InputComponent`, `CardComponent`, `StatusBadgeComponent`
- [ ] **Layout Component:**
  - `SidebarComponent` + `TopbarComponent` + `<router-outlet>`
  - Responsive design (collapse sidebar เมื่อ mobile)

**Deliverables:**
```
src/app/
├── core/
│   ├── auth/            (auth.service.ts)
│   ├── interceptors/    (auth.interceptor.ts)
│   └── guards/          (auth.guard.ts, role.guard.ts)
├── shared/              (design system components)
├── features/
│   └── auth/            (login.component.ts)
├── layout/              (sidebar, topbar, shell)
├── models/              (user.model.ts, api-response.model.ts)
└── app.routes.ts
```

**Definition of Done:**
- [ ] `ng serve` → เปิด http://localhost:4200
- [ ] เข้า /dashboard → redirect ไป /login (ไม่มี token)
- [ ] Login สำเร็จ → redirect ไป /dashboard
- [ ] Logout → token ถูกล้าง + redirect ไป /login
- [ ] Refresh token ทำงาน:
  - เปลี่ยน accessToken expiration ใน backend เป็น 1 นาที
  - รอจน token หมดอายุ → เรียก API → interceptor refresh อัตโนมัติ
- [ ] Role guard ทำงาน:
  - login เป็น student → เข้า /admin → redirect /403

---

### 4. Phase 3: First Feature (Pattern Example)

#### 4.1 📅 งานที่ 5: Attendance API + Angular Page

**วัตถุประสงค์:** สร้าง feature แรกเป็น pattern สำหรับโมดูลที่เหลือ (สูตร 4 ขั้น: DTO → Service → Controller → Register)

**งานย่อย (Backend):**
- [ ] **AttendanceService:**
  - `CheckInAsync()` → เทียบเวลาปัจจุบันกับ `Company.WorkStartTime` → status = ontime/late
  - `CheckOutAsync()` → คำนวณ TotalHours = check-out − check-in (rounded 2 decimals)
  - `ListAsync()` → ดึงประวัติตาม month/year
  - `SummaryAsync()` → รวมชั่วโมง/สาย/ขาด
  - `RequestEditAsync()` → status = pending_edit
  - `ReviewEditAsync()` → อนุมัติ/ปฏิเสธ + sync WorkLogs.Hours
- [ ] **AttendanceController:** 6 endpoints
  - `POST /api/attendance/check-in` (student)
  - `POST /api/attendance/check-out` (student)
  - `GET /api/attendance?month=&year=` (student/advisor)
  - `GET /api/attendance/summary?month=&year=` (student)
  - `PUT /api/attendance/{id}/request-edit` (student)
  - `PUT /api/attendance/{id}/review` (advisor)
- [ ] **AttendanceDtos:**
  - `AttendanceDto` (AttendanceID, AttDate, CheckInTime, CheckOutTime, TotalHours, Status, Note)
  - `AttendanceSummaryDto` (WorkDays, TotalHours, LateCount, AbsentCount)
  - `RequestEditDto` (Note)
  - `ReviewEditDto` (Approve, NewCheckInTime?, NewCheckOutTime?)
- [ ] **Business logic:**
  - UNIQUE (StudentID, AttDate) → กันลงเวลาซ้ำ
  - ตัดสายอัตโนมัติ: `now > Company.WorkStartTime` → late
  - Auto-sync: หลังอนุมัติแก้เวลา → update WorkLogs.Hours ของวันนั้น

**งานย่อย (Frontend):**
- [ ] **AttendanceService:** เรียก API ทั้งหมด
- [ ] **AttendancePage Component:** UI ตาม wireframe S5 (เอกสารส่วนที่ 2)
  - นาฬิกา real-time (interval 1 วินาที)
  - ปุ่ม check-in/out (disabled ถ้าลงเวลาแล้ว)
  - ตารางประวัติรายเดือน + filter เดือน
  - สรุปชั่วโมง/สาย/ขาด (StatCards)
  - Modal ขอแก้เวลา
- [ ] **State management:**
  - แสดง status วันนี้ real-time
  - Auto-refresh หลัง check-in/out

**Deliverables:**
```
Backend:
- Controllers/AttendanceController.cs    // 6 endpoints
- Services/AttendanceService.cs          // business logic
- Services/IAttendanceService.cs         // interface
- DTOs/AttendanceDtos.cs                 // request/response DTOs

Frontend:
- features/attendance/
  ├── attendance.service.ts
  ├── attendance.component.ts/html/scss
  ├── request-edit-modal.component.ts
  └── attendance.routes.ts
```

**Definition of Done (Test Cases):**
- [ ] TC-01: check-in ก่อน 09:00 → status = ontime
- [ ] TC-02: check-in หลัง 09:00 → status = late
- [ ] TC-03: check-in ซ้ำวันเดิม → 400 "วันนี้ลงเวลาเข้าแล้ว"
- [ ] TC-04: check-out → TotalHours ถูกคำนวณ (rounded 2 decimals)
- [ ] TC-05: GET list (month=8, year=2026) → ได้ข้อมูลเดือน ส.ค.
- [ ] TC-06: GET summary → ยอดรวมตรงตามประวัติ
- [ ] TC-07: role advisor เรียก check-in → 403
- [ ] TC-08: request-edit → status = pending_edit
- [ ] TC-09: review-edit (approve) → status updated + WorkLogs.Hours synced
- [ ] TC-10: Angular page แสดงนาฬิกา + กดปุ่ม → เรียก API + UI updated

---

### 5. Dependencies Diagram

```
Phase 1: Foundation Layer (ต้องทำตามลำดับ)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1. SQL Script (19 tables)
   ↓
2. ASP.NET Core Setup (connect DB + entities)
   ↓
3. Auth API (login/JWT/refresh) ← ทุก API อื่นต้องใช้
   ↓

Phase 2: Frontend Foundation (ทำคู่ขนานกับ Phase 1.3 ได้)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

4. Angular Setup (routing + auth + interceptor)
   ↓

Phase 3: First Feature
━━━━━━━━━━━━━━━━━━━━━━

5. Attendance API + Page (pattern example)
   ↓
   (ใช้ pattern นี้กับ Leaves, WorkLogs, Tasks, …)
```

---

### 6. Timeline Estimation

#### Solo Developer (sequential)
| งาน | เวลาโดยประมาณ | สะสม |
|---|---|---|
| 1. SQL Script | 4–6 ชม. | 6 ชม. |
| 2. Backend Setup | 6–8 ชม. | 14 ชม. |
| 3. Auth API | 8–10 ชม. | 24 ชม. |
| 4. Angular Setup | 8–10 ชม. | 34 ชม. |
| 5. Attendance (full stack) | 10–12 ชม. | **46 ชม. (~6 วันทำงาน)** |

#### Team (2 developers, parallel)
| Developer A (Backend) | Developer B (Frontend) |
|---|---|
| 1. SQL Script (4 ชม.) | — |
| 2. Backend Setup (6 ชม.) | — |
| 3. Auth API (8 ชม.) | 4. Angular Setup (8 ชม.) คู่ขนาน |
| 5. Attendance Backend (6 ชม.) | 5. Attendance Frontend (6 ชม.) คู่ขนาน |
| **รวม ~24 ชม. (~3 วันทำงาน)** | |

---

### 7. Definition of Done (ทั้ง Phase)

Phase 1–3 เสร็จสมบูรณ์เมื่อ:
- [ ] ฐานข้อมูล 19 ตาราง + seed data พร้อมใช้
- [ ] Backend API รันได้ + Swagger UI เปิดได้ + Auth flow ทำงาน
- [ ] Angular app รันได้ + login/logout/refresh ทำงาน + routing + guards
- [ ] Attendance feature ทำงานครบทั้ง backend + frontend
- [ ] ทุก API มี test cases ผ่าน
- [ ] โค้ด commit ลง Git repo (branch `develop`)
- [ ] ไม่มี lint error / build warning

---

### 8. Next Steps (หลังจบ Phase 3)

เมื่อ Attendance เสร็จ → ใช้ pattern เดียวกับโมดูลที่เหลือ:
1. 🏖 **Leaves** (ขอลา + เช็กโควตา + อนุมัติ)
2. 📝 **WorkLogs** (บันทึกงาน + flow อนุมัติ + แนบไฟล์)
3. 📋 **Tasks** (Kanban)
4. 🧑 **AdvisorVisits** (บันทึกการนิเทศ)
5. 💬 **Messages** (แชท 1:1)
6. 🔔 **Notifications** (แจ้งเตือนอัตโนมัติ)
7. 📊 **Reports** (สรุป + Export PDF)
8. ⭐ **Evaluations** (ประเมิน 2 ทาง)
9. 🛠 **Admin Panel** (จัดการผู้ใช้/บริษัท/ภาคเรียน)

แต่ละโมดูลใช้เวลา ~8–12 ชม. (backend + frontend) → รวม ~80–100 ชม. สำหรับทั้งระบบ

---

### 9. Git Workflow

```
main          ← production-ready
  └─ develop  ← integration branch
      ├─ feature/auth-api
      ├─ feature/attendance
      ├─ feature/leaves
      └─ feature/work-logs
```

**Commit convention:**
```
feat(auth): add login + JWT refresh flow
fix(attendance): sync hours after edit approval
docs(sql): add RefreshTokens table
```

---

### 10. Ready to Start

**คำแนะนำ:** เริ่มจากงานที่ 1 (SQL Script) → งานที่ 2 (Backend Setup) → งานที่ 3 (Auth API)

เมื่อ 3 งานนี้เสร็จ → ระบบมี foundation ที่แข็งแรง พร้อมต่อยอด feature อื่น ๆ ได้เร็ว

---

## ส่วนที่ 2: Work Order — ใบสั่งงานฉบับปฏิบัติการ (ส่วนที่ 13)

> ต้นฉบับ: `docs/internship-log-work-order.md.md` · จัดทำ 14 ส.ค. 2569

### 📖 วิธีใช้เอกสารนี้

- แต่ละ **Task** มี: ผู้รับผิดชอบ / ไฟล์ที่ต้องสร้าง / รายละเอียดงาน / Test ที่ต้องผ่าน / DoD
- ทำตามลำดับ Phase (งานมี dependencies)
- แต่ละงานต้องผ่าน **Module Test Checklist** ก่อนจึงจะปิดงานได้
- สถานะงาน: ⬜ ยังไม่เริ่ม | 🔄 กำลังทำ | ✅ เสร็จ | ❌ ติดปัญหา

---

### 🗓 PHASE 0 — Database + Project Setup (สัปดาห์ 1)

#### 🔧 BE-0.1: สร้างฐานข้อมูล 19 ตารางใน SSMS

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Deliverable | `database/01_schema.sql` + `database/02_seed.sql` |
| ไฟล์ที่ต้องสร้าง | สคริปต์ SQL 2 ไฟล์ |

**รายละเอียดงาน:**
1. สร้าง `InternLogDB` + ตาราง 19 ตารางตาม ERD ส่วนที่ 12
2. จุดที่ต้องระวัง (การเปลี่ยนแปลงรอบล่าสุด):
   - `Roles` = 3 ค่าเท่านั้น (student / advisor / admin)
   - `Companies` เพิ่ม `AdvisorID INT NULL FK→Users`
   - `Internships` ตัด `SupervisorID` + เพิ่ม `CompanyAddress NVARCHAR(500)` + `CompanyPhone NVARCHAR(20)`
   - `WorkLogs` ไม่มีคอลัมน์ `Status` (บันทึกจริง ไม่มีอนุมัติ)
   - ไม่มีตาราง `Messages` (ตัด Chat)
3. Constraints: `UNIQUE(Users.Email)`, `UNIQUE(Attendances.StudentID+AttDate)`, `UNIQUE(AdvisorVisits.InternshipID+VisitNo)`
4. Indexes: `WorkLogs(StudentID,LogDate)`, `Notifications(UserID,IsRead)`, `Messages` ตัดออก, `RefreshTokens(UserID)`
5. Seed: Roles(3), LeaveTypes(ป่วย30/กิจ3/พักร้อน6), WorkCategories(5), Admin คนแรก

**Test ที่ต้องผ่าน:**
- [ ] รันสคริปต์สำเร็จ ไม่มี error
- [ ] เห็น 19 ตารางใน Object Explorer
- [ ] Query seed data ได้ครบ
- [ ] สร้าง Database Diagram ใน SSMS ได้

**DoD:** ⬜ ฐานข้อมูลพร้อมใช้งาน

---

#### 🔧 BE-0.2: ตั้งโปรเจกต์ ASP.NET Core + โครงสร้าง Modular

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Deliverable | โปรเจกต์ `InternLog.API` รันได้ + เชื่อม DB ได้ |

**รายละเอียดงาน:**
1. `dotnet new webapi -n InternLog.API` (.NET 8)
2. ติดตั้ง: `EF Core SqlServer`, `JwtBearer`, `BCrypt.Net-Next`, `Swashbuckle`
3. สร้างโครงสร้างโฟลเดอร์:
```
InternLog.API/
├── src/Core/ (ApiResponse, AppException, Constants, ICurrentUserService)
├── src/Infrastructure/ (AppDbContext, Configurations/, Middleware/, EmailService, FileStorageService)
├── src/Modules/{Auth,Attendance,Leaves,WorkLogs,Tasks,AdvisorVisits,Reports,Notifications,Evaluations,Calendar,Dashboard,Profile,Companies,Admin}/
└── Program.cs
```
4. `AppDbContext` + Entities 19 ตัว + Fluent API Configurations แยกไฟล์
5. `ExceptionMiddleware` + `ApiResponse<T>` กลาง
6. `Program.cs`: DbContext, JWT Bearer, CORS (`localhost:4200`), Swagger
7. `DbSeeder` (รันตอนเริ่มแอป ถ้าตารางว่าง)

**Test ที่ต้องผ่าน:**
- [ ] `dotnet build` สำเร็จ
- [ ] `dotnet run` → Swagger UI เปิดได้
- [ ] เชื่อมต่อ DB สำเร็จ + Seeder สร้างข้อมูล
- [ ] `/health` endpoint ตอบ 200

**DoD:** ⬜ โปรเจกต์รันได้ + เชื่อม DB ได้

---

#### 🅰️ FE-0.1: ตั้งโปรเจกต์ Angular + โครงสร้าง Modular

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Deliverable | โปรเจกต์ `InternLog.Web` รันได้ + แสดงหน้า placeholder |

**รายละเอียดงาน:**
1. `ng new InternLog.Web --routing --style=scss`
2. ติดตั้ง: `date-fns`, `jwt-decode`, `@angular/cdk`, `ng2-charts`, `ngx-image-cropper`
3. สร้างโครงสร้างโฟลเดอร์:
```
src/app/
├── core/ (auth/, guards/, interceptors/, services/, models/)
├── shared/ (components/, pipes/, directives/)
├── layout/ (shell/, sidebar/, topbar/, side-drawer/, command-bar/)
├── features/{auth,dashboard,attendance,leaves,calendar,work-logs,tasks,visits,reports,evaluations,profile,companies,admin}/
└── styles/ (tokens.scss, globals.scss)
```
4. ตั้ง **CSS Design Tokens** ตาม Design System (สี, ฟอนต์, radius, shadow)
5. ตั้ง `environments` (apiUrl dev/prod)
6. ตั้ง alias paths (`@core`, `@shared`, `@features`, `@env`)

**Test ที่ต้องผ่าน:**
- [ ] `ng build` สำเร็จ
- [ ] `ng serve` → เปิด localhost:4200 ได้
- [ ] Routes placeholder ทำงาน
- [ ] Alias paths resolve ถูก

**DoD:** ⬜ โปรเจกต์รันได้ + โครงสร้างพร้อม

---

### 🗓 PHASE 1 — Auth + Profile + Layout Shell (สัปดาห์ 1-2)

#### 🔧 BE-1.1: Auth Module (Backend)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | BE-0.1, BE-0.2 |
| Deliverable | `Modules/Auth/` สมบูรณ์ 8 endpoints |

**ไฟล์ที่ต้องสร้าง:**
- `Modules/Auth/AuthController.cs`
- `Modules/Auth/AuthService.cs` + `IAuthService.cs`
- `Modules/Auth/DTOs/AuthDtos.cs`
- `Modules/Auth/Models/` (RefreshToken, PasswordResetToken entities)
- `Core/Security/JwtHelper.cs`, `PasswordHasher.cs`

**Endpoints ที่ต้องทำ:**
| Method | Endpoint | Logic สำคัญ |
|---|---|---|
| POST | /api/auth/login | BCrypt verify → ออก access(2ชม.)+refresh(7วัน) + user |
| POST | /api/auth/register | ตรวจ email/studentId ซ้ำ → สร้าง user (บังคับ role=student) |
| POST | /api/auth/refresh | hash verify + rotation (revoke ตัวเก่า) |
| POST | /api/auth/logout | revoke refresh token |
| POST | /api/auth/forgot-password | สร้าง token → (จำลองส่ง email) ไม่บอกว่า email มีหรือไม่ |
| POST | /api/auth/reset-password | verify token → เปลี่ยนรหัส → revoke ทุก refresh token |
| GET | /api/auth/me | [Authorize] คืนข้อมูล user ปัจจุบัน |
| PUT | /api/auth/change-password | verify รหัสเก่า → hash ใหม่ → ต้อง login ใหม่ |

**Business Rules:**
- รหัสผ่าน: ≥8 ตัว มี upper+lower+number+special
- Login ผิด: ตอบ 401 ข้อความเดียว (กัน user enumeration)
- RefreshToken: เก็บเป็น hash, มี `ReplacedByTokenID`

**Test ที่ต้องผ่าน:** AUTH-01 ถึง AUTH-13 (ส่วนที่ 10)

**DoD:** ⬜ Auth API ครบ 8 ตัว ทดสอบผ่านทุกข้อ

---

#### 🅰️ FE-1.1: Auth Pages + Auth Flow (Frontend)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Dependencies | BE-1.1, FE-0.1 |
| Deliverable | หน้า Login, Register, Forgot Password + auth flow ครบ |

**ไฟล์ที่ต้องสร้าง:**
- `core/auth/auth.service.ts` (login, register, refresh, logout, getMe, isAuthenticated)
- `core/auth/auth.interceptor.ts` (แนบ token + handle 401 + refresh queue)
- `core/auth/auth.guard.ts`, `core/auth/guest.guard.ts`, `core/auth/role.guard.ts`
- `features/auth/login/login.component.ts`
- `features/auth/register/register.component.ts`
- `features/auth/forgot-password/forgot-password.component.ts`
- `features/auth/reset-password/reset-password.component.ts`
- `shared/components/password-strength-meter/`

**รายละเอียดงาน:**
1. **Login:** split screen ตาม S1, validation, loading state, redirect ตาม role
2. **Register:** ฟอร์ม 4 ส่วน (ส่วนตัว/การฝึก/ความปลอดภัย/ยอมรับเงื่อนไข) + password strength meter + real-time validation (email/studentId ซ้ำ ตรวจกับ API debounce 500ms)
3. **Forgot/Reset Password:** ส่งอีเมล → ใส่รหัสใหม่
4. **Interceptor:** แนบ Bearer ทุกคำขอ, เจอ 401 → เรียก refresh ครั้งเดียว (queue คำขออื่น) → retry, refresh ล้มเหลว → ล้างข้อมูล + ไป /login
5. **Route Guard:** `/admin/*` ต้องเป็น admin, `/login` ต้องไม่ล็อกอินแล้ว

**Test ที่ต้องผ่าน:**
- [ ] Login สำเร็จ/ล้มเหลว + เก็บ token
- [ ] Register ครบ + validation ทุกข้อ (REG-01 ถึง REG-14)
- [ ] Token หมดอายุ → auto refresh → retry คำขอเดิม (INT-01 ถึง INT-03)
- [ ] Role guard ทำงาน (student เข้า /admin → 403)

**DoD:** ⬜ Auth flow ครบทั้งหน้าบ้าน + หลังบ้าน

---

#### 🅰️ FE-1.2: Layout Shell + Sidebar ตาม Role (Frontend)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Dependencies | FE-1.1 |
| Deliverable | Shell + Sidebar + Topbar + CommandBar + SideDrawer |

**ไฟล์ที่ต้องสร้าง:**
- `layout/shell/shell.component.ts` (Topbar + Sidebar + router-outlet)
- `layout/sidebar/sidebar.component.ts` (เมนูเปลี่ยนตาม role)
- `layout/topbar/topbar.component.ts` (search + กระดิ่ง + avatar)
- `layout/command-bar/command-bar.component.ts`
- `layout/side-drawer/side-drawer.component.ts` (ใช้ `@angular/cdk/overlay`)

**รายละเอียดงาน:**
1. **Sidebar:** เมนูตาม role ส่วนที่ 12 ข้อ 3 (student 10 เมนู / advisor 12 / admin 13), expanded 256px + collapsed 64px, active state
2. **Topbar:** ช่องค้นหา, กระดิ่งแจ้งเตือน (badge จำนวนยังไม่อ่าน), avatar dropdown (โปรไฟล์/ออกจากระบบ)
3. **SideDrawer:** กว้าง 400/560px, slide-in 300ms, overlay, header+content+footer
4. **CommandBar:** ชื่อหน้า + filter + action buttons (ใช้ทุกหน้า)

**Test ที่ต้องผ่าน:**
- [ ] เมนูเปลี่ยนตาม role ถูกต้อง
- [ ] Sidebar collapse/expand ได้
- [ ] SideDrawer เปิด/ปิด + คลิกนอกปิดได้
- [ ] กระดิ่งแสดงจำนวนแจ้งเตือนจริง

**DoD:** ⬜ Layout Shell พร้อมใช้ทุกหน้า

---

#### 🔧 BE-1.2: Profile Module (Backend)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | BE-1.1 |
| Deliverable | `Modules/Profile/` |

**Endpoints:**
| Method | Endpoint | รายละเอียด |
|---|---|---|
| PUT | /api/profile | แก้ชื่อ/เบอร์/รูป/คณะ/สาขา/ปี |
| PUT | /api/profile/company-contact | เฉพาะ นศ.: แก้ `Internships.CompanyAddress` + `CompanyPhone` |

**Test ที่ต้องผ่าน:** แก้ข้อมูลตัวเองสำเร็จ, แก้ของคนอื่น → 403

**DoD:** ⬜ Profile API พร้อม

---

#### 🅰️ FE-1.3: หน้า Profile (แก้ไขได้)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Dependencies | BE-1.2, FE-1.2 |
| Deliverable | `features/profile/profile-page.component.ts` |

**รายละเอียดงาน:**
1. การ์ดข้อมูลส่วนตัว: ชื่อ, อีเมล(อ่านอย่างเดียว), เบอร์โทร, รูปโปรไฟล์ (crop ได้)
2. การ์ดข้อมูลการฝึก (อ่านอย่างเดียว): บริษัท, ช่วงวัน, อาจารย์ที่ปรึกษา
3. **การ์ดสถานประกอบการ (เฉพาะ นศ., แก้ไขได้):** ชื่อบริษัท (จากจับคู่) + ที่อยู่ + เบอร์โทรบริษัท
4. การ์ดเปลี่ยนรหัสผ่าน

**Test ที่ต้องผ่าน:** แก้ไขสำเร็จ → toast, อัปโหลดรูป → crop → บันทึก

**DoD:** ⬜ หน้า Profile ครบทุก role

---

### 🗓 PHASE 2 — Attendance + Leaves (สัปดาห์ 2-3)

#### 🔧 BE-2.1: Attendance Module

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | BE-1.1 |
| Deliverable | `Modules/Attendance/` 7 endpoints |

**Endpoints:**
| Method | Endpoint | Role |
|---|---|---|
| POST | /api/attendance/check-in | student |
| POST | /api/attendance/check-out | student |
| GET | /api/attendance?year&month&studentId | student/advisor |
| GET | /api/attendance/summary | student/advisor |
| PUT | /api/attendance/{id}/request-edit | student |
| PUT | /api/attendance/{id}/review | advisor |

**Business Logic:**
- check-in: เทียบ `Companies.WorkStartTime` → ontime/late, UNIQUE ต่อวัน
- check-out: คำนวณ TotalHours = out − in (ปัด 2 ตำแหน่ง)
- review (อนุมัติแก้เวลา): **ต้อง sync `WorkLogs.Hours` ของวันนั้นด้วย**
- ที่ปรึกษาเห็นเฉพาะ นศ. ที่ตัวเองดูแล (ตรวจจาก Internships)

**Test ที่ต้องผ่าน:** ATT-01 ถึง ATT-11

**DoD:** ⬜ Attendance API ครบ + ทดสอบผ่าน

---

#### 🅰️ FE-2.1: หน้าลงเวลา

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Dependencies | BE-2.1, FE-1.2 |
| Deliverable | `features/attendance/` สมบูรณ์ |

**ไฟล์ที่ต้องสร้าง:**
- `attendance.service.ts`, `attendance-page.component.ts`
- `request-edit-modal.component.ts`
- `models/attendance.model.ts`

**รายละเอียดงาน:**
1. การ์ดบน: นาฬิกา real-time + ปุ่ม check-in/out (disabled เมื่อลงแล้ว)
2. StatCards: ชั่วโมงรวม / สาย / ขาด
3. ตารางประวัติรายเดือน + เลือกเดือน + ปุ่มขอแก้
4. โหมดอาจารย์: เลือกดู นศ. แต่ละคน

**Test ที่ต้องผ่าน:**
- [ ] กดปุ่ม → อัปเดตทันที + toast (UI-11, UI-12)
- [ ] ขอแก้เวลา → modal → สถานะ 🟡 (UI-14)
- [ ] อาจารย์ดู + อนุมัติได้

**DoD:** ⬜ หน้าลงเวลาครบทั้ง 2 มุมมอง

---

#### 🔧 BE-2.2: Leaves Module

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | BE-1.1 |
| Deliverable | `Modules/Leaves/` |

**Endpoints:** GET list • GET quota • POST create • GET pending • PUT {id}/review (advisor)

**Business Logic:**
- เช็กโควตา: ผลรวมวันลา (อนุมัติ+รอ) ≤ `LeaveTypes.MaxDays`
- อนุมัติ → ส่ง Notification → อัปเดตปฏิทิน
- อัปโหลดใบรับรองแพทย์ → `FileStorageService` (ตรวจชนิด: jpg/png/pdf, ≤ 5MB)

**Test ที่ต้องผ่าน:** LEA-01 ถึง LEA-08

**DoD:** ⬜ Leaves API ครบ

---

#### 🅰️ FE-2.2: หน้าวันลา

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Dependencies | BE-2.2, FE-1.2 |
| Deliverable | `features/leaves/` |

**รายละเอียดงาน:**
1. การ์ดโควตา 3 ใบ (ProgressBar เปลี่ยนสีตาม % ใช้ไป)
2. ฟอร์มขอลา: เลือกประเภท → แสดงคงเหลือ, เลือกช่วงวัน → จำนวนวันอัตโนมัติ, อัปโหลดใบรับรอง
3. ตารางประวัติ + แท็บกรองสถานะ
4. มุมมองอาจารย์: คิวรอ + ปุ่มอนุมัติ/ไม่อนุมัติ

**Test ที่ต้องผ่าน:**
- [ ] จำนวนวันคำนวณถูก (UI-17)
- [ ] ขอลาเกินโควตา → บล็อก + แสดงเหตุผล
- [ ] อาจารย์อนุมัติ → โควตาอัปเดต + แจ้งเตือน

**DoD:** ⬜ หน้าวันลาครบ

---

### 🗓 PHASE 3 — WorkLogs + Tasks (สัปดาห์ 3-4)

#### 🔧 BE-3.1: WorkLogs Module (บันทึกจริง ไม่มีอนุมัติ)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | BE-2.1 (ใช้ Attendances) |
| Deliverable | `Modules/WorkLogs/` |

**Endpoints:**
| Method | Endpoint | Role |
|---|---|---|
| GET | /api/worklogs | student/advisor (มี filter month/status/category/search) |
| GET | /api/worklogs/{id} | เจ้าของ/อาจารย์ที่ดูแล |
| POST | /api/worklogs | student (บันทึกทันที) |
| PUT | /api/worklogs/{id} | student (เจ้าของ แก้ได้ตลอด) |
| DELETE | /api/worklogs/{id} | student (เจ้าของ) |
| POST | /api/worklogs/{id}/attachments | เจ้าของ |
| DELETE | /api/attachments/{id} | เจ้าของ |
| GET | /api/worklogs/{id}/comments | |
| POST | /api/worklogs/{id}/comments | advisor (คอมเมนต์ → แจ้งเตือน นศ.) |

**Business Logic:**
- `Hours` = ดึงจาก `Attendances.TotalHours` ของวันนั้นอัตโนมัติ (ฝั่งรับไม่มีฟิลด์ hours)
- แก้ไขได้ตลอดเวลา → อัปเดต `UpdatedAt`
- ไฟล์แนบ: ตรวจชนิด/ขนาด, เก็บ `wwwroot/uploads`

**Test ที่ต้องผ่าน:**
- [ ] บันทึกทันทีสำเร็จ + Hours ถูกเติม
- [ ] แก้ไขกี่ครั้งก็ได้ + `UpdatedAt` อัปเดต
- [ ] ลบของตัวอื่น → 403
- [ ] อาจารย์คอมเมนต์ → นศ. ได้แจ้งเตือน

**DoD:** ⬜ WorkLogs API ครบ (รูปแบบบันทึกจริง)

---

#### 🅰️ FE-3.1: หน้าบันทึกงาน (List + Form)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Dependencies | BE-3.1, FE-1.2 |
| Deliverable | `features/work-logs/` |

**ไฟล์ที่ต้องสร้าง:**
- `work-log-list.component.ts` (FilterBar + ตาราง + ลบ)
- `work-log-form.component.ts` (สร้าง/แก้ไข + แนบไฟล์)
- `worklog.service.ts`, `models/worklog.model.ts`

**รายละเอียดงาน:**
1. **List:** กรองเดือน/ประเภท + ค้นหา + ปุ่มเพิ่ม + ปุ่มแก้ไข/ลบ
2. **Form 2 คอลัมน์:** ฟอร์มหลัก (วัน/ชื่อ/ประเภท/รายละเอียด/ปัญหา/วิธีแก้, ชั่วโมงแสดงอัตโนมัติ) | ขวา: อัปโหลดไฟล์ (drag-drop + preview) + คอมเมนต์อาจารย์ (อ่านอย่างเดียว สำหรับ นศ.)
3. ปุ่ม: [บันทึก] [ยกเลิก] — **ไม่มีปุ่มส่งอนุมัติแล้ว** (ตัดออก)
4. มุมมองอาจารย์: รายชื่อ นศ. → เลือกดูบันทึก → คอมเมนต์

**หมายเหตุ:** ตัด `StatusBadge` ของบันทึกงานออกทั้งหมด (ไม่มีสถานะอนุมัติแล้ว)

**Test ที่ต้องผ่าน:**
- [ ] บันทึก/แก้ไขได้ตลอด + ข้อมูลอัปเดต (WL-05 ปรับใหม่)
- [ ] อัปโหลดไฟล์ → preview → ลบได้
- [ ] คอมเมนต์อาจารย์แสดงใต้บันทึก
- [ ] Auto-save ทุก 30 วิ (ถ้ายังอยู่ในฟอร์ม)

**DoD:** ⬜ หน้าบันทึกงานรูปแบบใหม่ครบ

---

#### 🔧 BE-3.2: Tasks Module

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | BE-1.1 |
| Deliverable | `Modules/Tasks/` |

**Endpoints:** GET list • POST create (advisor/admin) • PUT {id} • PUT {id}/status (student) • DELETE {id}

**Business Logic:**
- สร้างงานผ่าน `InternshipID` (ผู้รับ = นศ. ของ internship นั้น)
- มอบหมายข้าม นศ. ที่ไม่ได้รับมอบหมาย → 400/403
- เปลี่ยนสถานะ → แจ้งเตือน

**Test ที่ต้องผ่าน:** TASK-01 ถึง TASK-07 (ตัด supervisor ออก: ผู้มอบหมาย = advisor/admin เท่านั้น)

**DoD:** ⬜ Tasks API ครบ

---

#### 🅰️ FE-3.2: หน้างานมอบหมาย (Kanban)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Dependencies | BE-3.2, FE-1.2 |
| Deliverable | `features/tasks/` |

**รายละเอียดงาน:**
1. กระดาน 3 คอลัมน์ (ยังไม่ทำ/กำลังทำ/เสร็จ) ใช้ `@angular/cdk/drag-drop`
2. Card: ชื่องาน + priority chip + กำหนดส่ง + ผู้มอบหมาย
3. ลากเปลี่ยนสถานะ → เรียก API → toast
4. อาจารย์: ปุ่ม [+ มอบหมายงาน] → modal ฟอร์ม

**Test ที่ต้องผ่าน:**
- [ ] ลาก card → สถานะอัปเดตใน DB (UI-29)
- [ ] มอบหมายงาน → นศ. เห็นในกระดาน + แจ้งเตือน
- [ ] นศ. กดปุ่มมอบหมาย → ไม่เห็น/403

**DoD:** ⬜ Kanban ครบทั้ง 2 มุมมอง

---

### 🗓 PHASE 4 — Calendar + Dashboard (สัปดาห์ 4-5)

#### 🔧 BE-4.1: Calendar Aggregation API

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | BE-2.1, BE-2.2, BE-3.1 |
| Deliverable | `Modules/Calendar/` |

**Endpoints:**
| Method | Endpoint | รายละเอียด |
|---|---|---|
| GET | /api/calendar?year&month&studentId | รวมรายวัน: attendance, worklogs, leaves, tasks, visits + `dayStatus` |
| GET | /api/calendar/events?date&studentId | events ของวัน (สำหรับ SideDrawer) |

**Business Logic — คำนวณ `dayStatus`:**
1. มีวันลา → `leave` (🟣)
2. attendance = late/absent → `late`/`absent` (🔴)
3. ไม่มีข้อมูลลงเวลา/บันทึก (วันฝึก) → `missing` (🔴อ่อน)
4. ทุกอย่างปกติ → `complete` (🟢)
5. วันหยุด → `holiday` (⚪)
- Scope: นศ.=ตัวเอง / อาจารย์=นศ. ที่ดูแล / ไม่พบสิทธิ์ → 403

**Test ที่ต้องผ่าน:**
- [ ] โหลด 1 เดือน < 500ms
- [ ] dayStatus คำนวณถูกทุกกรณี
- [ ] อาจารย์ดึงของ นศ. นอกสังกัด → 403

**DoD:** ⬜ Calendar API ครบ

---

#### 🅰️ FE-4.1: หน้าปฏิทิน (Teams Style)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Dependencies | BE-4.1, FE-1.2 |
| Deliverable | `features/calendar/` |

**รายละเอียดงาน:**
1. มุมมองเดือน (เป็นหลัก) + สัปดาห์, ปุ่มเปลี่ยนเดือน + Today
2. Cell: เลขวันที่ + จุดสีตาม `dayStatus` + ไอคอน events
3. คลิกวัน → **SideDrawer**: ลงเวลาของวัน + บันทึกงาน + วันลา + งาน + การนิเทศ
4. ใน SideDrawer: ปุ่ม [+ เพิ่มบันทึก] → เปิดฟอร์มบันทึกงาน (วันนั้น)
5. อาจารย์: dropdown เลือก นศ. ที่ดูแล
6. Legend สี 5 ช่องบน CommandBar

**Test ที่ต้องผ่าน:**
- [ ] สีตรงตามสถานะ (🟢🔴🟣⚪)
- [ ] SideDrawer แสดงข้อมูลถูก + สร้างบันทึกจากปฏิทินได้
- [ ] เปลี่ยนเดือน → โหลดใหม่

**DoD:** ⬜ ปฏิทินครบ

---

#### 🔧 BE-4.2: Dashboard API (แยก Role)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | ทุกโมดูลก่อนหน้า |
| Deliverable | `Modules/Dashboard/` |

**Endpoint:** `GET /api/dashboard` (คืน payload ต่างกันตาม role ใน JWT)

**Payload แต่ละ role:**
- **student:** ฝึกแล้ว x/y วัน (+%), ชั่วโมงสะสม, วันลาคงเหลือ, สถานะวันนี้, แจ้งเตือนล่าสุด 5, บันทึกล่าสุด 5, งานครบกำหนด
- **advisor:** จำนวน นศ. ที่ดูแล, คำขอลารอ, บันทึกล่าสุดของ นศ., การนิเทศที่ต้องทำ, รายชื่อนศ.+ความคืบหน้า
- **admin:** จำนวนทั้งหมด (นศ./อาจารย์/บริษัท), ยังไม่จับคู่, ไม่ใช้งาน 30 วัน, สถิติรวม

**Test ที่ต้องผ่าน:** แต่ละ role ได้ข้อมูลของตัวเองเท่านั้น + ข้อมูลคำนวณถูก

**DoD:** ⬜ Dashboard API ครบ 3 แบบ

---

#### 🅰️ FE-4.2: Dashboard 3 แบบ

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Frontend |
| Dependencies | BE-4.2, FE-2.1 |
| Deliverable | `features/dashboard/` (3 components) |

**รายละเอียดงาน:**
1. `dashboard-student`: StatCards 4 ใบ + การ์ดลงเวลาวันนี้ (นาฬิกา+ปุ่ม) + แจ้งเตือน + บันทึกล่าสุด + งานครบกำหนด
2. `dashboard-advisor`: StatCards + คิวขอลา (อนุมัติ/ไม่อนุมัติ) + การ์ด นศ. แต่ละคน (ProgressBar)
3. `dashboard-admin`: StatCards ระบบ + รายการยังไม่จับคู่ + กราฟการใช้งาน (ng2-charts)

**Test ที่ต้องผ่าน:**
- [ ] ข้อมูลตรงกับ API + อัปเดตแบบเรียลไทม์ (นาฬิกา)
- [ ] แต่ละ role เห็นเฉพาะของตัวเอง (ตรวจจาก route + payload)

**DoD:** ⬜ Dashboard ครบ 3 role

---

### 🗓 PHASE 5 — Visits + Reports + Evaluations (สัปดาห์ 5-6)

#### 🔧 BE-5.1: AdvisorVisits Module

**Endpoints:** GET list • POST create (advisor) • PUT {id}
**Business Logic:** UNIQUE (InternshipID+VisitNo), คนที่/ครั้งที่/วันที่/รูปแบบ/สรุป/คะแนน
**Test:** VISIT-01 ถึง VISIT-05
**DoD:** ⬜

---

#### 🅰️ FE-5.1: หน้าการนิเทศ
**งาน:** ตาราง + modal บันทึก (อาจารย์) + มุมมองอ่านอย่างเดียว (นศ.)
**Test:** VISIT UI tests
**DoD:** ⬜

---

#### 🔧 BE-5.2: Reports Module (แยก Role)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | BE-2.x, BE-3.1, BE-5.1 |
| Deliverable | `Modules/Reports/` |

**Endpoints:**
| Method | Endpoint | รายละเอียด |
|---|---|---|
| GET | /api/reports/stats?scope= | สถิติตาม role: นศ.=ตัวเอง, อาจารย์=นศ.ที่ดูแล(เปรียบเทียบได้), admin=ทั้งระบบ |
| GET | /api/reports?studentId&period | รายการรายงาน |
| POST | /api/reports/generate | รวม WorkLogs+Attendance+Leaves+Visits ในช่วง |
| GET | /api/reports/{id}/export-pdf | สร้าง PDF (ใช้ QuestPDF) |

**Business Logic:**
- ติดตั้ง `QuestPDF` สำหรับสร้าง PDF
- PDF ประกอบด้วย: หัวรายงาน (โลโก้+ชื่อ+บริษัท), สรุปชั่วโมง, ตารางบันทึกงานรายวัน, วันลา, การนิเทศ
- **การแยก scope สำคัญ:** นศ. ดูของ นศ. คนอื่น → 403 เสมอ

**Test ที่ต้องผ่าน:**
- [ ] สถิติแต่ละ role ถูกต้อง
- [ ] นศ. ขอรายงานของคนอื่น → 403
- [ ] PDF สร้างได้ + ดาวน์โหลดได้

**DoD:** ⬜ Reports ครบทั้ง 3 มุมมอง

---

#### 🅰️ FE-5.2: หน้ารายงาน 3 แบบ

**รายละเอียดงาน:**
1. **นศ.:** สถิติของตัวเอง + กราฟชั่วโมงตามประเภท + ตาราง + สร้างรายงาน + ปุ่ม Export
2. **อาจารย์:** เลือก นศ. (หรือดูเปรียบเทียบหลายคน) + กราฟเปรียบเทียบ + Export รวม
3. **ผู้ดูแลระบบ:** สถิติทั้งระบบ + กราฟรวม + Export
4. หน้า Preview PDF (เปิดแท็บใหม่)

**Test ที่ต้องผ่าน:**
- [ ] กราฟแสดงถูก (ng2-charts)
- [ ] Export → PDF ดาวน์โหลด
- [ ] แต่ละ role เห็นข้อมูลเฉพาะของตัวเอง

**DoD:** ⬜ หน้ารายงานครบ 3 มุมมอง

---

#### 🔧 BE-5.3 + 🅰️ FE-5.3: Evaluations

**Backend:** POST evaluate (advisor→นศ., นศ.→บริษัท) • GET • เช็กประเมินซ้ำ → 400
**Frontend:** หน้าประเมิน (ดาว 1-5 + ความเห็น + คะแนนรวม) แยกตาม role
**Test:** EVAL-01 ถึง EVAL-04
**DoD:** ⬜

---

### 🗓 PHASE 6 — Companies (Advisor) + Admin (สัปดาห์ 6-7)

#### 🔧 BE-6.1: Companies Module

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | BE-1.1 |
| Deliverable | `Modules/Companies/` |

**Endpoints:**
| Method | Endpoint | Scope |
|---|---|---|
| GET | /api/companies | นศ.=บริษัทตัวเอง / อาจารย์=ของตัวเอง+ที่ดูแล / ผู้ดูแลระบบ=ทั้งหมด |
| POST | /api/companies | อาจารย์/ผู้ดูแลระบบ (บันทึก `AdvisorID`) |
| PUT | /api/companies/{id} | อาจารย์เฉพาะที่ `AdvisorID` = ตัวเอง |

**Business Logic:**
- อาจารย์แก้บริษัทของคนอื่น → 403
- บริษัทที่มี นศ. ฝึกอยู่ → ลบไม่ได้ (400)

**Test ที่ต้องผ่าน:**
- [ ] อาจารย์เพิ่มบริษัท → เห็นเฉพาะของตัวเอง
- [ ] อาจารย์แก้ของคนอื่น → 403
- [ ] ผู้ดูแลระบบเห็นทั้งหมด

**DoD:** ⬜ Companies API ครบ

---

#### 🅰️ FE-6.1: หน้าบริษัท (อาจารย์ + ผู้ดูแลระบบ)

**รายละเอียดงาน:**
1. ตารางบริษัท (+ จำนวน นศ.)
2. Modal เพิ่ม/แก้ไข: ชื่อ, ที่อยู่, โทร, เวลาเข้า-ออกงาน
3. อาจารย์: เห็นเฉพาะของตัวเอง / ผู้ดูแลระบบ: ทั้งหมด + จัดการได้

**Test ที่ต้องผ่าน:** CRUD สำเร็จ + ขอบเขตตาม role
**DoD:** ⬜

---

#### 🔧 BE-6.2: Admin Module (Users + Internships + Impersonation)

| รายการ | รายละเอียด |
|---|---|
| ผู้รับผิดชอบ | Backend |
| Dependencies | ทุกโมดูล |
| Deliverable | `Modules/Admin/` |

**Endpoints:**
```
Users:      GET/POST/PUT/DELETE /api/admin/users
            PATCH /api/admin/users/{id}/status
            POST /api/admin/users/{id}/reset-password
            POST /api/admin/users/import (Excel)
            GET  /api/admin/users/export
Periods:    CRUD /api/admin/periods
Internships: CRUD /api/admin/internships (จับคู่ นศ.-บริษัท-อาจารย์)
Impersonate: POST /api/admin/users/{id}/impersonate (ออก token 30 นาที)
             POST /api/admin/impersonate/stop
Audit:      ทุกการกระทำของผู้ดูแลระบบบันทึกลง AuditLog
```

**Business Logic:**
- Import Excel: ตรวจซ้ำ → รายงานแถวที่ผิด
- จับคู่: นศ. 1 คนฝึกได้ 1 ที่ต่อภาคเรียน
- ลบผู้ใช้ที่มีข้อมูล → ต้องเลือกโอน/ลบข้อมูล
- **ห้ามผู้ดูแลระบบลบผู้ดูแลระบบด้วยกัน**

**Test ที่ต้องผ่าน:** ADM-01 ถึง ADM-27 (ปรับจากส่วนที่ 10)

**DoD:** ⬜ Admin API ครบ

---

#### 🅰️ FE-6.2: หน้า Admin (ผู้ใช้ + จับคู่ + ผู้ดูแลระบบ)

**ไฟล์ที่ต้องสร้าง:**
- `features/admin/admin-users.component.ts` (ตาราง + กรอง + ค้นหา + เพิ่ม/แก้ไข/ลบ + นำเข้า/ส่งออก)
- `features/admin/user-form-modal.component.ts`
- `features/admin/admin-companies.component.ts`
- `features/admin/admin-internships.component.ts` (จับคู่แบบ searchable select)
- `features/admin/impersonation-banner.component.ts`

**รายละเอียดงาน:**
1. ตารางผู้ใช้: เลือกหลายรายการ + bulk action + toggle สถานะ
2. นำเข้า Excel: อัปโหลด → พรีวิว → ยืนยัน
3. จับคู่การฝึก: เลือก นศ. → บริษัท → อาจารย์ → ภาคเรียน → ช่วงวัน
4. **ปุ่ม 📊 ดู Dashboard** → โหมดผู้ดูแลระบบ (แบนเนอร์เหลือง + ปิดปุ่มกระทำ + ปุ่มกลับหน้าผู้ดูแลระบบ)

**Test ที่ต้องผ่าน:**
- [ ] CRUD + bulk + import/export ครบ
- [ ] เข้าโหมดผู้ดูแลระบบ → เห็นข้อมูล นศ. แบบอ่านอย่างเดียว + กลับได้
- [ ] นศ. เข้า /admin → redirect 403

**DoD:** ⬜ หน้า Admin ครบทั้งหมด

---

### 🗓 PHASE 7 — QA + UAT (สัปดาห์ 7-8)

#### 🔧 BE-7.1: Test Backend ทั้งระบบ
- [ ] รัน test cases ส่วนที่ 10 (ปรับปรุงแล้ว) ทุกข้อ
- [ ] Security: SQL injection, 401/403 ทุก endpoint, rate limit login
- [ ] Performance: ทุก endpoint < 1s, calendar < 500ms

#### 🅰️ FE-7.1: Test Frontend ทั้งระบบ
- [ ] ทุกหน้าครบตาม checklist ส่วนที่ 6 (ตัดหน้าแชทออก)
- [ ] ทุกหน้ามี Loading / Error / Empty states
- [ ] Responsive ≥ 1024px

#### 🎯 UAT ทั้ง 3 Roles
- [ ] Student flow เต็ม (ลงเวลา→บันทึก→ลา→รายงาน→ประเมิน)
- [ ] Advisor flow เต็ม (อนุมัติ→คอมเมนต์→มอบหมาย→นิเทศ→รายงาน)
- [ ] Admin flow เต็ม (จัดการผู้ใช้→จับคู่→ผู้ดูแลระบบ→รายงานรวม)

**DoD:** ⬜ พร้อมส่งมอบ

---

### 📊 สรุปภาพรวมงานทั้งหมด

| ฝ่าย | จำนวนงาน | ระยะเวลา |
|---|---|---|
| 🔧 Backend | BE-0.1 → BE-7.1 (20 tasks) | ~7 สัปดาห์ |
| 🅰️ Frontend | FE-0.1 → FE-7.1 (21 tasks) | ~7 สัปดาห์ (ทำคู่ขนาน) |
| 🎯 รวม | ~41 งานหลัก | ~8 สัปดาห์ |

### ⚠️ กฎเหล็กที่ต้องยึดตลอด

1. **Module-Complete-First** — จบทีละโมดูล + ทดสอบผ่านก่อน จึงไปต่อ
2. **ห้ามข้าม Phase** — งานถัดไปต้องรองานก่อนหน้าเสร็จ
3. **ทุกการตอบกลับของ API** = `{success, message, data}`
4. **ทุกหน้า** ต้องมี Loading / Error / Empty states
5. **RBAC ทุกจุด** — ตรวจสิทธิ์ทั้งฝั่งหน้าบ้าน (ซ่อนเมนู) และหลังบ้าน (403)
