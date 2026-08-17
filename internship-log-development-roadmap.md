# 🗺️ ระบบจดบันทึกการฝึกงาน — เอกสารส่วนที่ 5
> แผนงานพัฒนาตามหลักวิศวกรรม (Development Roadmap)
> จัดทำเมื่อ 14 สิงหาคม 2569 | ชื่อไฟล์แนะนำ: `internship-log-development-roadmap.md`
>
> **แผนที่เอกสารทั้งชุด**
> - ส่วนที่ 1 `internship-log-system-docs.md` → โครงสร้างระบบ + ERD + Backend
> - ส่วนที่ 2 `internship-log-ui-design.md` → UI Design 20 หน้า
> - ส่วนที่ 3 `internship-log-analysis-sql.md` → Use Cases/SRS + Sequence + SQL
> - ส่วนที่ 4 `internship-log-review-decisions.md` → มติแก้ไข (Review v2)
> - ส่วนที่ 5 `internship-log-development-roadmap.md` → เอกสารฉบับนี้

---

## 1. Executive Summary

แผนพัฒนาแบ่งเป็น **5 งานหลัก** จัดลำดับตาม dependencies และ risk:
- **Foundation Layer** (งาน 1–3): SQL + Backend Setup + Auth API
- **Frontend Foundation** (งาน 4): Angular Shell + Auth Flow
- **First Feature** (งาน 5): Attendance API + Page (pattern example)

**หลักการ:** ทำ foundation ให้แข็งแรงก่อน แล้วใช้ Attendance เป็น pattern สำหรับโมดูลที่เหลือ (Leaves, WorkLogs, Tasks, …)

---

## 2. Phase 1: Foundation Layer

### 2.1 🗄️ งานที่ 1: SQL Script สมบูรณ์ (19 ตาราง)

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

### 2.2 🏗️ งานที่ 2: ASP.NET Core Project Setup

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

### 2.3 🔐 งานที่ 3: Auth API (Core Authentication)

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

## 3. Phase 2: Frontend Foundation

### 3.1 🅰️ งานที่ 4: Angular Project Setup

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

## 4. Phase 3: First Feature (Pattern Example)

### 4.1 📅 งานที่ 5: Attendance API + Angular Page

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

## 5. Dependencies Diagram

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

## 6. Timeline Estimation

### Solo Developer (sequential)
| งาน | เวลาโดยประมาณ | สะสม |
|---|---|---|
| 1. SQL Script | 4–6 ชม. | 6 ชม. |
| 2. Backend Setup | 6–8 ชม. | 14 ชม. |
| 3. Auth API | 8–10 ชม. | 24 ชม. |
| 4. Angular Setup | 8–10 ชม. | 34 ชม. |
| 5. Attendance (full stack) | 10–12 ชม. | **46 ชม. (~6 วันทำงาน)** |

### Team (2 developers, parallel)
| Developer A (Backend) | Developer B (Frontend) |
|---|---|
| 1. SQL Script (4 ชม.) | — |
| 2. Backend Setup (6 ชม.) | — |
| 3. Auth API (8 ชม.) | 4. Angular Setup (8 ชม.) คู่ขนาน |
| 5. Attendance Backend (6 ชม.) | 5. Attendance Frontend (6 ชม.) คู่ขนาน |
| **รวม ~24 ชม. (~3 วันทำงาน)** | |

---

## 7. Definition of Done (ทั้ง Phase)

Phase 1–3 เสร็จสมบูรณ์เมื่อ:
- [ ] ฐานข้อมูล 19 ตาราง + seed data พร้อมใช้
- [ ] Backend API รันได้ + Swagger UI เปิดได้ + Auth flow ทำงาน
- [ ] Angular app รันได้ + login/logout/refresh ทำงาน + routing + guards
- [ ] Attendance feature ทำงานครบทั้ง backend + frontend
- [ ] ทุก API มี test cases ผ่าน
- [ ] โค้ด commit ลง Git repo (branch `develop`)
- [ ] ไม่มี lint error / build warning

---

## 8. Next Steps (หลังจบ Phase 3)

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

## 9. Git Workflow

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

## 10. Ready to Start

**คำแนะนำ:** เริ่มจากงานที่ 1 (SQL Script) → งานที่ 2 (Backend Setup) → งานที่ 3 (Auth API)

เมื่อ 3 งานนี้เสร็จ → ระบบมี foundation ที่แข็งแรง พร้อมต่อยอด feature อื่น ๆ ได้เร็ว