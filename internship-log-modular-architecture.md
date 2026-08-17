# 🧩 ระบบจดบันทึกการฝึกงาน — เอกสารส่วนที่ 8
> ปรับโครงสร้างเป็น Modular Architecture (แยกตามฟีเจอร์)
> จัดทำเมื่อ 14 สิงหาคม 2569 | ชื่อไฟล์แนะนำ: `internship-log-modular-architecture.md`

---

## 1. ทำไมต้องแยกเป็นโมดูล

**โครงสร้างเดิม (Monolithic):** ทุก Controller/Service อยู่ในโฟลเดอร์เดียว → เมื่อระบบขยาย (20+ หน้า, 55+ endpoints) จะดูแลยาก

**โครงสร้างใหม่ (Modular):** แยกตามฟีเจอร์ (Auth, Attendance, Leaves, WorkLogs, …) แต่ละโมดูลมี Controller + Service + DTOs + Models ของตัวเอง

**ข้อดี:**
- ✅ ดูแลง่าย — แก้ฟีเจอร์หนึ่งไม่กระทบฟีเจอร์อื่น
- ✅ ทีมทำงานคู่ขนานได้ —每人รับผิดชอบโมดูลของตัวเอง
- ✅ ทดสอบง่าย — Unit test แยกโมดูล
- ✅ Reusable — โมดูล Auth ใช้ซ้ำในโปรเจกต์อื่นได้
- ✅ Scalable — ถ้าฟีเจอร์ใหญ่ขึ้น แยกเป็น Microservice ได้ในอนาคต

**ข้อเสีย:**
- ⚠️ โครงสร้างโฟลเดอร์ซับซ้อนขึ้นเล็กน้อย
- ️ ต้องจัดการ Dependency ระหว่างโมดูล (เช่น WorkLogs ต้องใช้ Attendances)

---

## 2. โครงสร้าง Backend (ASP.NET Core) — แบบ Modular

### 2.1 โครงสร้างโฟลเดอร์ใหม่

```
InternLog.API/
├── src/
│   ├── Core/                          ← Shared Core (ใช้ร่วมกันทุกโมดูล)
│   │   ├── Common/
│   │   │   ├── ApiResponse.cs         ← Response กลาง
│   │   │   ├── AppException.cs        ← Custom exception
│   │   │   └── Constants.cs           ← Status constants
│   │   ├── Interfaces/
│   │   │   ├── ICurrentUserService.cs ← ดึง UserId จาก JWT
│   │   │   └── IDateTimeService.cs    ← เวลาปัจจุบัน (สำหรับ test)
│   │   └── Security/
│   │       ├── JwtHelper.cs
│   │       ── PasswordHasher.cs      ← BCrypt wrapper
│   │
│   ├── Infrastructure/                ← Cross-cutting concerns
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs        ← รวมทุก Entity
│   │   │   ├── Configurations/        ← EF Core Fluent API แยกไฟล์
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── AttendanceConfiguration.cs
│   │   │   │   └── ...
│   │   │   └── Migrations/
│   │   ├── Middleware/
│   │   │   ├── ExceptionMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Services/
│   │   │   ├── EmailService.cs        ← ส่ง email (reset password)
│   │   │   └── FileStorageService.cs  ← อัปโหลดไฟล์
│   │   └── Identity/
│   │       ├── JwtAuthentication.cs
│   │       └── RoleAuthorization.cs
│   │
│   └── Modules/                       ←  แยกตามฟีเจอร์
│       ├── Auth/
│       │   ├── AuthController.cs
│       │   ├── AuthService.cs
│       │   ├── IAuthService.cs
│       │   ├── DTOs/
│       │   │   ├── LoginRequest.cs
│       │   │   ├── RegisterRequest.cs
│       │   │   └── ...
│       │   └── Models/
│       │       ├── User.cs
│       │       ├── Role.cs
│       │       └── RefreshToken.cs
│       │
│       ├── Attendance/
│       │   ├── AttendanceController.cs
│       │   ├── AttendanceService.cs
│       │   ├── IAttendanceService.cs
│       │   ├── DTOs/
│       │   └── Models/
│       │       └── Attendance.cs
│       │
│       ├── Leaves/
│       ├── WorkLogs/
│       ├── Tasks/
│       ├── AdvisorVisits/
│       ├── Reports/
│       ├── Messages/
│       ├── Notifications/
│       ├── Evaluations/
│       └── Admin/
│
├── Program.cs                         ← Register ทุกโมดูล
└── appsettings.json
```

### 2.2 ตัวอย่าง: โมดูล Attendance (ครบชุด)

**Modules/Attendance/Models/Attendance.cs**
```csharp
namespace InternLog.API.Modules.Attendance.Models;

public class Attendance {
  public int AttendanceID { get; set; }
  public int StudentID { get; set; }
  public DateOnly AttDate { get; set; }
  public TimeOnly? CheckInTime { get; set; }
  public TimeOnly? CheckOutTime { get; set; }
  public decimal? TotalHours { get; set; }
  public string Status { get; set; } = "ontime";
  public string? Note { get; set; }
}
```

**Modules/Attendance/DTOs/AttendanceDtos.cs**
```csharp
namespace InternLog.API.Modules.Attendance.DTOs;

public record AttendanceDto(int AttendanceID, DateOnly AttDate, 
  TimeOnly? CheckInTime, TimeOnly? CheckOutTime, decimal? TotalHours, string Status);

public record CheckInResponse(string Status, TimeOnly CheckInTime);
public record AttendanceSummary(int WorkDays, decimal TotalHours, int LateCount);
```

**Modules/Attendance/IAttendanceService.cs**
```csharp
namespace InternLog.API.Modules.Attendance;

public interface IAttendanceService {
  Task<CheckInResponse> CheckInAsync(int studentId);
  Task<AttendanceDto> CheckOutAsync(int studentId);
  Task<List<AttendanceDto>> ListAsync(int studentId, int year, int month);
  Task<AttendanceSummary> SummaryAsync(int studentId, int year, int month);
}
```

**Modules/Attendance/AttendanceService.cs**
```csharp
using InternLog.API.Infrastructure.Data;
using InternLog.API.Modules.Attendance.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InternLog.API.Modules.Attendance;

public class AttendanceService(AppDbContext db) : IAttendanceService {
  public async Task<CheckInResponse> CheckInAsync(int studentId) {
    // ... business logic ...
  }
  // ... methods อื่นๆ ...
}
```

**Modules/Attendance/AttendanceController.cs**
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Core.Common;
using InternLog.API.Modules.Attendance.DTOs;

namespace InternLog.API.Modules.Attendance;

[ApiController]
[Route("api/attendance")]
[Authorize(Roles = "student")]
public class AttendanceController(IAttendanceService svc) : ControllerBase {
  
  [HttpPost("check-in")]
  public async Task<IActionResult> CheckIn() {
    // ...
  }
}
```

### 2.3 Program.cs — Register ทุกโมดูล

```csharp
// Core
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Infrastructure
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddScoped<IEmailService, EmailService>();

// Modules — แยก register ชัดเจน
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IWorkLogService, WorkLogService>();
// ... โมดูลอื่นๆ ...
```

---

## 3. โครงสร้าง Frontend (Angular) — แบบ Modular

### 3.1 โครงสร้างโฟลเดอร์ใหม่

```
src/
├── app/
│   ├── core/                          ← Shared Core (singleton services)
│   │   ├── auth/
│   │   │   ├── auth.service.ts
│   │   │   ├── auth.guard.ts
│   │   │   └── auth.interceptor.ts
│   │   ├── guards/
│   │   │   └── role.guard.ts
│   │   ├── services/
│   │   │   ├── http.service.ts        ← HTTP wrapper
│   │   │   └── notification.service.ts
│   │   └── models/
│   │       ├── user.model.ts
│   │       ── api-response.model.ts
│   │
│   ├── shared/                        ← Reusable components
│   │   ├── components/
│   │   │   ├── button/
│   │   │   ├── input/
│   │   │   ├── status-badge/
│   │   │   └── table/
│   │   ├── pipes/
│   │   │   └── date-format.pipe.ts
│   │   └── directives/
│   │
│   ├── layout/                        ← Shell components
│   │   ├── sidebar/
│   │   ├── topbar/
│   │   └── shell.component.ts
│   │
│   └── features/                      ← ⭐ แยกตามฟีเจอร์ (Lazy Loading)
│       ├── auth/
│       │   ├── login/
│       │   │   ├── login.component.ts
│       │   │   ├── login.component.html
│       │   │   └── login.component.scss
│       │   ├── register/
│       │   ├── forgot-password/
│       │   └── auth.routes.ts
│       │
│       ├── dashboard/
│       │   ├── dashboard-student/
│       │   ├── dashboard-advisor/
│       │   ── dashboard.routes.ts
│       │
│       ├── attendance/
│       │   ├── services/
│       │   │   └── attendance.service.ts
│       │   ├── components/
│       │   │   ├── attendance-page/
│       │   │   └── request-edit-modal/
│       │   ├── models/
│       │   │   └── attendance.model.ts
│       │   └── attendance.routes.ts
│       │
│       ├── leaves/
│       ├── work-logs/
│       ├── tasks/
│       ├── visits/
│       ├── chat/
│       ├── notifications/
│       ├── reports/
│       ├── evaluations/
│       ├── profile/
│       └── admin/
│
├── assets/
├── environments/
└── styles.scss                        ← Global styles + Design System
```

### 3.2 ตัวอย่าง: โมดูล Attendance (Angular)

**features/attendance/models/attendance.model.ts**
```typescript
export interface Attendance {
  attendanceID: number;
  attDate: string;
  checkInTime?: string;
  checkOutTime?: string;
  totalHours?: number;
  status: 'ontime' | 'late' | 'leave' | 'absent';
}

export interface AttendanceSummary {
  workDays: number;
  totalHours: number;
  lateCount: number;
}
```

**features/attendance/services/attendance.service.ts**
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env';
import { Attendance, AttendanceSummary } from '../models/attendance.model';
import { ApiResponse } from '@core/models/api-response.model';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/attendance`;

  checkIn(): Observable<ApiResponse<Attendance>> {
    return this.http.post<ApiResponse<Attendance>>(`${this.baseUrl}/check-in`, {});
  }

  checkOut(): Observable<ApiResponse<Attendance>> {
    return this.http.post<ApiResponse<Attendance>>(`${this.baseUrl}/check-out`, {});
  }

  list(year: number, month: number): Observable<ApiResponse<Attendance[]>> {
    return this.http.get<ApiResponse<Attendance[]>>(
      `${this.baseUrl}?year=${year}&month=${month}`
    );
  }

  summary(year: number, month: number): Observable<ApiResponse<AttendanceSummary>> {
    return this.http.get<ApiResponse<AttendanceSummary>>(
      `${this.baseUrl}/summary?year=${year}&month=${month}`
    );
  }
}
```

**features/attendance/attendance.routes.ts**
```typescript
import { Routes } from '@angular/router';
import { authGuard } from '@core/guards/auth.guard';

export const ATTENDANCE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => 
      import('./components/attendance-page/attendance-page.component')
        .then(m => m.AttendancePageComponent),
    canActivate: [authGuard],
    title: 'ลงเวลาปฏิบัติงาน'
  }
];
```

**app.routes.ts — รวมทุกฟีเจอร์**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('@features/auth/login/login.component') },
  { 
    path: 'attendance', 
    loadChildren: () => import('@features/attendance/attendance.routes')
      .then(m => m.ATTENDANCE_ROUTES) 
  },
  { 
    path: 'leaves', 
    loadChildren: () => import('@features/leaves/leaves.routes')
      .then(m => m.LEAVES_ROUTES) 
  },
  // ... ฟีเจอร์อื่นๆ ...
  { path: '**', redirectTo: '/dashboard' }
];
```

---

## 4. โครงสร้าง Database — ยังรวมกัน (Monolithic DB)

**เหตุผล:** โปรเจกต์ขนาดกลาง (20 ตาราง) ไม่จำเป็นต้องแยก Database — ใช้ Schema เดียว แต่แยกตารางตามโมดูลชัดเจน

```
InternLogDB
├── [Auth Module]
│   ├── Roles
│   ├── Users
│   ├── RefreshTokens
│   └── PasswordResetTokens
│
├── [Attendance Module]
│   ── Attendances
│
├── [Leaves Module]
│   ├── LeaveTypes
│   └── Leaves
│
├── [WorkLogs Module]
│   ├── WorkCategories
│   ├── WorkLogs
│   ├── Attachments
│   └── LogComments
│
├── [Tasks Module]
│   └── Tasks
│
├── [AdvisorVisits Module]
│   └── AdvisorVisits
│
├── [Reports Module]
│   └── Reports
│
├── [Messages Module]
│   └── Messages
│
├── [Notifications Module]
│   ── Notifications
│
├── [Evaluations Module]
│   └── Evaluations
│
└── [Admin Module]
    ├── Companies
    └── TrainingPeriods
    └── Internships (เชื่อมโยงทุกโมดูล)
```

**ถ้าอนาคตต้องการแยก Database:**
- แยกเป็น Microservices → แต่ละโมดูลมี DB ของตัวเอง
- ใช้ API Gateway เชื่อมต่อ
- แต่สำหรับโปรเจกต์นี้ **ไม่แนะนำ** (overkill)

---

## 5. Dependency ระหว่างโมดูล

บางโมดูลต้องเรียกใช้กัน → แก้ไขโดยใช้ **Interface + Dependency Injection**

**ตัวอย่าง: WorkLogs ต้องใช้ Attendances**

```csharp
// Modules/WorkLogs/WorkLogService.cs
public class WorkLogService(
  AppDbContext db, 
  IAttendanceService attendanceSvc  // ← Inject interface ของโมดูลอื่น
) : IWorkLogService {
  
  public async Task<WorkLogDto> CreateAsync(CreateWorkLogRequest req, int studentId) {
    // เรียกใช้ AttendanceService จากโมดูล Attendance
    var attendance = await attendanceSvc.GetByDateAsync(studentId, req.LogDate);
    
    var log = new WorkLog {
      // ...
      Hours = attendance?.TotalHours  // ดึงชั่วโมงจาก Attendance
    };
    
    db.WorkLogs.Add(log);
    await db.SaveChangesAsync();
    return ToDto(log);
  }
}
```

**กฎ:** 
- ✅ โมดูลสามารถ Inject Interface ของโมดูลอื่นได้
- ❌ ห้ามเรียก Controller ของโมดูลอื่นโดยตรง
- ❌ ห้ามเข้าถึง Entity ของโมดูลอื่นโดยตรง (ใช้ Service เป็นตัวกลาง)

---

## 6. Migration Plan (จากโครงสร้างเดิม → Modular)

### ขั้นตอนที่ 1: สร้างโฟลเดอร์ใหม่
```bash
# Backend
mkdir -p src/Core src/Infrastructure src/Modules

# Frontend
mkdir -p src/app/core src/app/shared src/app/features
```

### ขั้นตอนที่ 2: ย้ายไฟล์ทีละโมดูล
```
Auth → Modules/Auth/
Attendance → Modules/Attendance/
Leaves → Modules/Leaves/
...
```

### ขั้นตอนที่ 3: อัปเดต namespace
```csharp
// เดิม
namespace InternLog.API.Services;

// ใหม่
namespace InternLog.API.Modules.Attendance;
```

### ขั้นตอนที่ 4: อัปเดต Program.cs
```csharp
// เพิ่ม using ทุกโมดูล
using InternLog.API.Modules.Auth;
using InternLog.API.Modules.Attendance;
// ...
```

### ขั้นตอนที่ 5: ทดสอบ
```bash
dotnet build    # Backend
ng build        # Frontend
```

---

## 7. สรุป

| ด้าน | โครงสร้างเดิม | โครงสร้างใหม่ (Modular) |
|---|---|---|
| Backend | Controllers/, Services/ รวมกัน | Modules/{Feature}/ แยกชัดเจน |
| Frontend | components/ รวมกัน | features/{Feature}/ + Lazy Loading |
| Database | รวมกัน (20 ตาราง) | รวมกัน (แยกตามโมดูลในเอกสาร) |
| การดูแล | ยากเมื่อระบบขยาย | ง่าย แยก responsibility ชัด |
| ทีมทำงาน | ชนกันง่าย | คู่ขนานได้每人รับผิดชอบโมดูล |

**แนะนำ:** ใช้โครงสร้าง Modular ตั้งแต่เริ่มโปรเจกต์ → ลด technical debt ในอนาคต

---

## 8. Next Steps

1. ✅ อนุมัติโครงสร้าง Modular นี้
2. 🏗️ สร้างโฟลเดอร์ใหม่ตามโครงสร้างด้านบน
3.  ย้ายโค้ดจากโครงสร้างเดิมเข้าโมดูล
4. 🧪 ทดสอบ build + รันระบบ
5.  อัปเดตเอกสารส่วนที่ 1 (Backend) และส่วนที่ 2 (Frontend) ให้สอดคล้อง