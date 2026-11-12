# สถานะโปรเจกต์ (Project Status)

> อัปเดตล่าสุด: **12 พ.ย. 2569** · อ้างอิงจากโค้ดใน branch `agents/add-project-to-git-repo` (รวมงานที่ยังไม่ commit)
> ประวัติการพัฒนาทั้งหมดดูที่ [reference/progress-log.md](reference/progress-log.md)

## 1. สรุป

- Backend ครบ 12 โมดูล, `dotnet build` ผ่าน (0 error, 4 warning เรื่อง package ที่มีช่องโหว่)
- Frontend ครบ 21 หน้า เชื่อม API จริงทุกโมดูล (ไม่มี mock data แล้ว)
- QA ระดับ API ทำครบทุกโมดูลเมื่อ 18 ก.ย. 2569 · ยังไม่มี automated test
- ความเสี่ยงหลักตอนนี้คือ **ความปลอดภัยของไฟล์อัปโหลด** (ข้อ 3.1)

## 2. สถานะรายโมดูล

| โมดูล | Backend | Frontend | หมายเหตุ |
|---|:---:|:---:|---|
| Auth (login, register, refresh, profile, เปลี่ยนรหัส) | ✅ | ✅ | |
| Forgot / Reset password | ✅ | ⚠️ | หน้า `/reset-password` ยังไม่มี · ยังไม่ส่งอีเมลจริง (ลิงก์อยู่ใน console) |
| Attendance | ✅ | ✅ | ยกเลิกการคำนวณ "มาสาย" ตามเวลาบริษัทแล้ว · advisor ดูประวัตินักศึกษาได้ผ่าน `?studentId=` |
| Leaves | ✅ | ⚠️ | หน้า leaves ยังไม่มี `<input type="file">` สำหรับแนบไฟล์ · ยังไม่มีหน้า advisor อนุมัติใบลาแยก |
| Work Logs | ✅ | ✅ | บันทึกทันที (ไม่มีสถานะ draft/submit) · advisor คอมเมนต์ได้ |
| Tasks (Kanban) | ✅ | ✅ | |
| Advisor Visits | ✅ | ⚠️ | advisor ต้องระบุ `internshipId` เอง |
| Reports + PDF | ✅ | ⚠️ | ฝั่ง advisor ต้องส่ง `internshipId` เพื่อดูของนักศึกษา · PDF ยังไม่ฝังฟอนต์ไทย |
| Evaluations | ✅ | ⚠️ | เหมือน Reports เรื่อง `internshipId` |
| Notifications | ✅ | ✅ | |
| Dashboard | ✅ | ⚠️ | API คืนข้อมูลตาม role แล้ว แต่ทุก role ใช้ component `dashboard-student` · calendar API คืนข้อมูลเฉพาะ student |
| Companies (advisor) | ✅ | ⚠️ | มีแค่ GET — `companies.service.ts` ฝั่งเว็บเรียก `PUT /companies/{id}` ซึ่งไม่มีใน API |
| Admin (users, companies, internships) | ✅ | ⚠️ | หน้า admin users แสดง role ว่าง (เว็บใช้ `role` แต่ API ส่ง `roleName`) |

## 3. ปัญหาที่รู้อยู่ (Known Issues)

### 3.1 ความปลอดภัย

| # | ระดับ | ปัญหา | ตำแหน่ง |
|---|:---:|---|---|
| S1 | 🔴 | ไฟล์ใน `wwwroot/uploads` (ใบลา, ไฟล์แนบ work log, PDF รายงาน) เปิดให้โหลดได้โดยไม่ต้อง login ผ่าน `UseStaticFiles()` | `Program.cs`, `LeaveService`, `WorkLogService`, `ReportService` |
| S2 | 🔴 | ไฟล์แนบใบลาไม่ตรวจขนาด นามสกุล และ magic bytes (Work Log ตรวจแล้ว) | `LeaveService.cs` ~บรรทัด 134 |
| S3 | 🟠 | Package `System.Security.Cryptography.Pkcs 9.0.11` มีช่องโหว่ (high + moderate) | dependency ทางอ้อม |
| S4 | 🟡 | Login ข้าม BCrypt เมื่อไม่พบ email ทำให้เวลาตอบต่างกัน (คอมเมนต์ในโค้ดบอก timing-safe แต่ไม่จริง) | `AuthService.LoginAsync` |
| S5 | 🟡 | Access + refresh token เก็บใน `localStorage` (เสี่ยงถ้ามี XSS) | `core/auth/auth.service.ts` |

### 3.2 Bug และความถูกต้อง

| # | ระดับ | ปัญหา | ตำแหน่ง |
|---|:---:|---|---|
| B1 | 🟠 | Refresh rotation หา "token ใหม่" จาก token ล่าสุดของ user — ผิดตัวได้เมื่อ login หลายเครื่องพร้อมกัน และไม่อยู่ใน transaction | `AuthService.RefreshAsync` |
| B2 | 🟠 | ใช้ `DateTime.Now` ปนกับ `DateTime.UtcNow` — เพี้ยนเมื่อ server ไม่ได้อยู่ timezone ไทย | หลาย service |
| B3 | 🟠 | ลิงก์ "ดูไฟล์" ใบลาใช้ path `/uploads/...` ซึ่งชี้ไปที่ `localhost:4200` ไม่ใช่ API → เปิดไม่ได้ | `leave-page.component.ts` |
| B4 | 🟡 | ไม่มีหน้า `/reset-password` ลิงก์รีเซ็ตจะ redirect ไป login | `app.routes.ts` |
| B5 | 🟡 | Seed ใน `Program.cs` ใช้ `RoleID = 1/2` ตายตัว · seed roles ใน `Program.cs` ไม่เคยทำงานเพราะ migration ใส่ roles แล้ว · ภาคเรียนที่ seed (มิ.ย.–ส.ค. 2569) ผ่านไปแล้ว | `Program.cs` |
| B6 | 🟡 | `HasData` ของ admin ใช้ `BCrypt.HashPassword(...)` ตอนสร้าง model → hash เปลี่ยนทุกครั้ง ทำให้ `migrations add` สร้าง diff ที่ไม่ต้องการ | `AppDbContext.cs` ~บรรทัด 404 |
| B7 | 🟡 | Role `supervisor` ยังถูก seed ทั้งที่เลิกใช้แล้ว · `supervisor@test.com` อาจยังอยู่ใน DB เดิม | `AppDbContext.cs`, DB |

### 3.3 คุณภาพโค้ดและโครงสร้าง

| # | ปัญหา | ตำแหน่ง |
|---|---|---|
| Q1 | Service ฝั่งเว็บมี 2 ชุด: `core/services/` และ `features/*/services/` (admin, attendance, auth, dashboard, leaves ฯลฯ) บางตัวเรียก endpoint ที่ไม่มีจริง เช่น `features/attendance/services` เรียก `/attendance/me` | `InternLog.Web/src/app` |
| Q2 | ไฟล์ `features/*/*.routes.ts` ทั้ง 11 ไฟล์ไม่ถูก import ที่ไหนเลย · `core/components/layout/` ซ้ำกับ `layout/` | `InternLog.Web/src/app` |
| Q3 | ไม่มี test project ฝั่ง API และฝั่งเว็บมีแค่ `app.component.spec.ts` | — |
| Q4 | `CompaniesController` query DbContext ตรง ไม่ผ่าน service และไม่ได้สืบทอด `AppControllerBase` | `Modules/Companies` |
| Q5 | ใช้ string role (`"admin"`, `"advisor"`) ตรง ๆ หลายที่ แทน `Constants.Roles` | หลาย service |
| Q6 | `package.json` ที่ root (มีแค่ `mssql`) ไม่ได้ใช้ · โฟลเดอร์ `InternLog.API/Controllers/` ว่าง · `InternLog.API.http` ยังเป็น template `weatherforecast` | root, `InternLog.API/` |

## 4. งานถัดไป (เรียงตามลำดับที่แนะนำ)

1. Commit งานที่ค้างอยู่ใน working tree (20 ไฟล์ที่แก้ + 2 ไฟล์ใหม่ใน API)
2. S1 + S2 + B3 — ย้ายไฟล์อัปโหลดออกจาก `wwwroot` แล้วให้ดาวน์โหลดผ่าน endpoint ที่ตรวจสิทธิ์
3. S3 — อัปเดต package
4. B1, B4, S4 — แก้ auth flow
5. Frontend ที่ค้าง: แนบไฟล์ใบลา, หน้า advisor อนุมัติใบลา, ส่ง `internshipId` อัตโนมัติ, dashboard ตาม role, role ในหน้า admin users
6. Q1–Q6 — เก็บกวาดโครงสร้าง + เพิ่ม test (เริ่มจาก AuthService และการตรวจสิทธิ์ข้อมูล) โดยใช้ [reference/qa-test-cases.md](reference/qa-test-cases.md) เป็นฐาน
