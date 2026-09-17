# Internship Log System - AI Handoff Report

วันที่: 28 สิงหาคม 2569
Repository: `c:\Internship_Log_System`

## วัตถุประสงค์ของรายงาน

เอกสารนี้สรุปงานที่ทำในรอบนี้ เพื่อให้ AI ตัวถัดไปอ่านแล้วทำงานต่อได้ทันที โดยแยกสิ่งที่แก้แล้วออกจากประเด็นที่ยังต้องตรวจสอบ

## เอกสารหลักที่ต้องยึด

1. `docs/internship-log-final-spec.md` เป็นข้อกำหนดหลัก
2. `docs/daily-reports/2026-08-27-daily-summary.md` เป็นสถานะล่าสุด
3. `docs/internship-log-modular-architecture.md` เป็นแนวทางโครงสร้างโค้ด
4. `.github/skills/internship-log-system/SKILL.md` เป็น workflow และกฎสำหรับ AI ใน repository นี้

เมื่อเอกสารขัดแย้งกัน ให้ยึด Final Spec และรายงานความขัดแย้ง ห้ามเลือกเงียบ ๆ

## งานที่ทำเสร็จแล้ว

### 1. สร้าง Workspace Skill

ไฟล์: `.github/skills/internship-log-system/SKILL.md`

Skill นี้กำหนดให้ AI:

- อ่าน Final Spec ก่อนแก้ไข
- รักษา Angular + ASP.NET Core Modular Architecture
- ตรวจ role และ ownership scope
- คำนวณ WorkLog hours จาก Attendance ที่ Backend
- ไม่ใช้ Supervisor, Chat, Messages หรือ WorkLog approval ตาม Final Spec
- ตรวจ security, validation, test และ build หลังแก้ไข
- สรุป root cause, ไฟล์ที่แก้, validation และ test gap

เรียกใช้ได้ด้วย `/internship-log-system`

### 2. ปรับเมนู Sidebar ตาม Role

ไฟล์ที่แก้:

- `InternLog.Web/src/app/layout/sidebar/sidebar.component.ts`
- `InternLog.Web/src/app/layout/sidebar/sidebar.component.html`
- `InternLog.Web/src/app/app.routes.ts`

การเปลี่ยนแปลง:

- อ่าน role จาก `AuthService.currentUser()` แทนค่า mock
- แยกเมนูสำหรับ `student`, `advisor`, `admin`
- ลบเมนู Chat ออกจาก Sidebar
- เพิ่มเมนูจัดการบริษัทสำหรับ Advisor
- เพิ่ม route `/companies` พร้อม `roleGuard` สำหรับ `advisor` และ `admin`
- Admin ใช้ `/admin/companies` สำหรับหน้าจัดการบริษัทของ Admin
- Admin เห็นจัดการผู้ใช้, จัดการบริษัท, จัดการจับคู่ และจัดการระบบ
- ลิงก์ `/admin` ที่ไม่มี route ถูกนำออกจาก Sidebar

หมายเหตุ: พบ Sidebar อีกชุดที่ `InternLog.Web/src/app/core/components/layout/sidebar/` แต่ Shell ปัจจุบันใช้ชุดจาก `InternLog.Web/src/app/layout/sidebar/` เป็นหลัก จึงแก้ชุดที่ใช้งานจริง

### 3. แก้ C# nullable compile error ตอน Seed Internship

ไฟล์: `InternLog.API/Program.cs`

อาการเดิม:

```text
Program.cs(214,33): error CS0266: Cannot implicitly convert type 'int?' to 'int'
warning CS8629: Nullable value type may be null
```

สาเหตุ:

- `Internship.AdvisorID` เป็น `int` ที่ต้องมีค่า
- โค้ดเดิมใช้ `advisor?.UserID` ซึ่งมีชนิดเป็น `int?`

วิธีแก้:

- เปลี่ยนเงื่อนไข seed ให้ตรวจ `advisor != null` ก่อน
- ใช้ `AdvisorID = advisor.UserID`

หมายเหตุ: ในโค้ดปัจจุบันยังมี `SupervisorID = supervisor?.UserID` อยู่ใน model/seed บางส่วน ซึ่งขัดกับ Final Spec ที่ตัด Supervisor ออก งานถัดไปควรตรวจทั้ง model, migration, seed, endpoint และ role ให้สอดคล้องกันเป็นชุดเดียว ไม่ควรแก้เฉพาะจุดแบบสุ่ม

### 4. แก้ Angular warning NG8107

ไฟล์ที่แก้:

- `InternLog.Web/src/app/features/admin/admin-users/admin-users.component.html`
- `InternLog.Web/src/app/features/visits/visits-page/visits-page.component.html`

การเปลี่ยนแปลง:

- `user.firstName?.charAt(0)` เป็น `user.firstName.charAt(0)`
- `user.username?.charAt(0)` เป็น `user.username.charAt(0)`
- `visit.advisorName?.charAt(2)` เป็น `visit.advisorName.charAt(2)`

สาเหตุ: Angular ตรวจชนิดข้อมูลแล้วพบว่าค่าดังกล่าวเป็น string ที่ไม่ nullable จึงเตือนว่า optional chaining ไม่จำเป็น

### 5. แก้ 415 Unsupported Media Type ของการยื่นใบลา

อาการเดิม:

```text
Http failure response for http://localhost:5005/api/leaves: 415 Unsupported Media Type
```

จุดสำคัญ:

- `InternLog.API/src/Modules/Leaves/LeavesController.cs` รับ `POST /api/leaves` ด้วย `[FromForm] LeaveRequestDto` และ `IFormFile? attachment`
- Frontend service เดิมส่ง object JSON

ไฟล์ที่แก้:

- `InternLog.Web/src/app/core/services/leaves.service.ts`
- `InternLog.Web/src/app/features/leaves/leaves-page/leaves-page.component.ts`
- `InternLog.Web/src/app/models/leave.model.ts`

วิธีแก้:

- เปลี่ยน `requestLeave()` ให้สร้าง `FormData`
- ส่ง `leaveTypeID`, `startDate`, `endDate`, `reason` เป็น form fields
- ส่งไฟล์ใน field ชื่อ `attachment` เมื่อมีไฟล์
- เพิ่ม `attachment?: File | null` ใน `LeaveRequestDto`
- ส่งค่าไฟล์จาก Reactive Form ไปยัง service
- ไม่กำหนด `Content-Type` เอง เพื่อให้ Angular สร้าง multipart boundary

ผล: อาการ 415 หายไปแล้ว แต่หลังจากนั้นพบ 400 ซึ่งเป็น validation/business error ของ API และต้องตรวจสาเหตุตามหัวข้อถัดไป

### 6. แก้ 400 ที่อาจเกิดจาก body ว่างของ Attendance

ไฟล์: `InternLog.API/src/Modules/Attendance/AttendanceController.cs`

อาการ:

```text
POST http://localhost:5005/api/attendance/check-in 400 (Bad Request)
```

Frontend เรียก check-in ด้วย body ว่างหรือ `null` แต่ Controller รับ `[FromBody] string? note`

วิธีแก้:

```csharp
[FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string? note
```

ใช้กับทั้ง `CheckIn` และ `CheckOut` เพื่อรองรับการเรียกโดยไม่มี note

## ผลการตรวจสอบ

### ผ่านแล้ว

- `dotnet build c:\Internship_Log_System\InternLog.API\InternLog.API.csproj` ผ่านในรอบก่อนหน้า
- API เคย start สำเร็จที่ `http://localhost:5005`
- Database migration รายงานว่า up to date
- Seed Internship สำเร็จ
- `npm run build` ผ่านหลังแก้ Sidebar, warning และ Leaves
- Diagnostics ของไฟล์ C# และ TypeScript ที่แก้ไม่พบ error
- Angular warning NG8107 ทั้ง 3 จุดหายไปจาก build ล่าสุด

### สิ่งที่ต้องเข้าใจจาก log ล่าสุด

ข้อความต่อไปนี้ไม่ใช่ root cause หลักของระบบ:

- `contentscript.js MaxListenersExceededWarning`
- `ObjectMultiplex - orphaned data`
- `VM... Cannot read properties of undefined (reading 'startTime')`

ข้อความเหล่านี้มาจาก Browser Extension หรือ injected script ควรทดสอบใน Incognito/ปิด extension เพื่อแยกปัญหา

## ประเด็นที่ยังต้องทำต่อ

### A. ตรวจ 400 ของ Leave ให้เห็นข้อความจริง

Backend `LeaveService.SubmitLeaveAsync()` สามารถตอบ 400 จากกรณีเหล่านี้:

- `EndDate < StartDate`
- วันที่เริ่มต้นย้อนหลัง
- ไม่พบ Internship ของผู้ใช้
- เลือกประเภทลาไม่ถูกต้อง
- ช่วงวันที่ไม่มีวันทำการ
- โควตาวันลาไม่พอ

หน้า `InternLog.Web/src/app/features/leaves/leaves-page/leaves-page.component.ts` ถูกปรับให้แสดง `err.error?.message` แล้ว ให้ดูข้อความใน alert และ Network response เพื่อยืนยันสาเหตุจริง

ต้องทดสอบด้วย:

- นักศึกษาที่มี active internship
- LeaveType ID ที่มีอยู่จริงในฐานข้อมูล
- วันที่ปัจจุบันหรืออนาคตที่เป็นวันทำการ
- โควตาที่ยังเหลือ
- reason ที่ไม่ว่าง

### B. ตรวจ 400 ของ Attendance

หากยัง 400 หลังใช้ `EmptyBodyBehavior.Allow` ให้ตรวจ response body และ log ของ API เพราะอาจเป็น business rule เช่น:

- นักศึกษาไม่มี active internship
- ลงเวลาเข้างานวันนี้ไปแล้ว
- token เป็น role อื่นหรือหมดอายุ

### C. ทำให้ Final Spec กับ Backend ตรงกัน

ยังพบ code ที่อ้างถึงสิ่งที่ Final Spec ตัดออกแล้ว:

- `SupervisorID` ใน `Internship`
- role `supervisor` ใน `[Authorize]` ของ Leaves/Attendance บาง endpoint
- logic ที่เปิดให้ advisor เห็นข้อมูลกว้างเกิน ownership scope
- route/component/service เก่าที่อาจอ้าง Chat หรือ Messages

ต้องตรวจครบทั้ง entity, DbContext, migration, SQL, seed, controller, service, Angular route, menu และ tests ก่อนลบ เพราะ migration ที่ถูกใช้งานแล้วไม่ควรถูกแก้ย้อนหลังโดยไม่จำเป็น

### D. ตรวจ contract และ role field ให้เป็นมาตรฐานเดียว

พบ frontend บางส่วนใช้ `user.role` และบางส่วนใช้ `user.roleName` ต้องกำหนด field กลางจาก API แล้วปรับจุดใช้งานให้สอดคล้อง มิฉะนั้นเมนูบริษัทหรือ permission อาจไม่แสดงตาม role จริง

### E. ตรวจ upload และ wwwroot

API เคยแจ้งว่าไม่มี `wwwroot` และ LeaveService สร้างโฟลเดอร์ upload เอง ควรตรวจ:

- การสร้าง `wwwroot/uploads/leaves`
- จำกัด extension, MIME type และขนาดไฟล์
- ป้องกัน path traversal
- การเสิร์ฟไฟล์อย่างปลอดภัย

### F. แนวทางการยื่นใบลาย้อนหลังและล่วงหน้า

#### พฤติกรรมปัจจุบัน

ใน `InternLog.API/src/Modules/Leaves/LeaveService.cs` ระบบปัจจุบัน:

- อนุญาตให้ยื่นใบลาวันนี้และวันล่วงหน้า
- ปฏิเสธวันที่ย้อนหลังด้วย `400 Bad Request`
- ปฏิเสธช่วงวันที่สิ้นสุดก่อนวันที่เริ่มต้น
- ไม่นับวันเสาร์และวันอาทิตย์เป็นวันลา
- ตรวจสอบ Internship, ประเภทลา และโควตาก่อนบันทึก
- นับใบลาสถานะ `pending` และ `approved` รวมกันเพื่อกันการใช้โควตาเกิน

#### แนวทางที่แนะนำ

ให้ใช้กติกานี้เป็น baseline:

| กรณี | พฤติกรรม |
|---|---|
| ลาวันนี้ | อนุญาต ถ้าเป็นวันทำการและโควตาเพียงพอ |
| ลาล่วงหน้า | อนุญาต ถ้าอยู่ในช่วง Internship และไม่ทับซ้อนใบลาเดิม |
| ลาย้อนหลังไม่เกิน 3 วันทำการ | อนุญาตแบบ `pending` ต้องมีเหตุผล และควรแนบเอกสารกรณีป่วย/เหตุจำเป็น |
| ลาย้อนหลังเกิน 3 วันทำการ | ปฏิเสธผ่าน API ปกติ ให้ติดต่อ Advisor/Admin |
| วันเสาร์-อาทิตย์ทั้งหมด | ปฏิเสธ เพราะไม่มีวันทำการ |

ทุกกรณีต้องตรวจที่ Backend:

- `StartDate <= EndDate`
- วันที่ลาอยู่ในช่วง Internship
- นักศึกษาเป็นเจ้าของ Internship
- ไม่ทับซ้อนใบลา `pending` หรือ `approved`
- โควตาเพียงพอ
- `reason` ไม่ว่างและไม่เกินความยาวที่กำหนด

ใบลาย้อนหลังต้องไม่แก้ Attendance ทันที ควรเปลี่ยน Attendance เป็น `leave` หลัง Advisor อนุมัติเท่านั้น ส่วนใบลาล่วงหน้าควรถูกสร้างเป็น `pending` จนกว่าจะได้รับการอนุมัติ

#### ผลกระทบหากเปิดย้อนหลัง

ถ้าต้องการรองรับย้อนหลัง ให้แก้พร้อมกันทั้ง Backend validation, Frontend date validation, ข้อความแจ้งเตือน, attachment rule และ QA tests ห้ามแก้เฉพาะเงื่อนไขที่ปฏิเสธวันที่ย้อนหลัง เพราะอาจเปิดช่องให้แก้ประวัติการลงเวลาโดยไม่มีหลักฐาน

#### Test cases ที่ต้องเพิ่ม

- ยื่นลาวันนี้
- ยื่นลาล่วงหน้า
- ยื่นลาย้อนหลัง 1 วันทำการ
- ยื่นลาย้อนหลังครบ 3 วันทำการ
- ยื่นลาย้อนหลังเกิน 3 วันทำการ
- ช่วงวันที่เริ่มต้นมากกว่าวันสิ้นสุด
- ยื่นลาในวันหยุดสุดสัปดาห์
- ยื่นลานอกช่วง Internship
- ยื่นใบลาทับซ้อน
- โควตาไม่พอ
- ลาย้อนหลังกรณีจำเป็นโดยมีไฟล์แนบ

## อัปเดตงานล่าสุด หลังการตรวจ Final Spec

### งานที่แก้เพิ่มเติมแล้ว

#### 1. ปิด Supervisor ใน Runtime

ไฟล์ที่แก้:

- `InternLog.API/Program.cs`
- `InternLog.API/src/Core/Common/Constants.cs`
- `InternLog.API/src/Modules/Attendance/AttendanceController.cs`
- `InternLog.API/src/Modules/Attendance/AttendanceService.cs`
- `InternLog.API/src/Modules/Leaves/LeavesController.cs`
- `InternLog.API/src/Modules/Tasks/TasksController.cs`
- `InternLog.API/src/Modules/Tasks/TaskService.cs`
- `InternLog.API/src/Modules/WorkLogs/WorkLogsController.cs`
- `InternLog.API/src/Modules/Evaluations/EvaluationsController.cs`
- `InternLog.API/src/Modules/Evaluations/EvaluationService.cs`
- `InternLog.API/src/Modules/Dashboard/DashboardService.cs`

สาเหตุ: ระบบยัง seed และอนุญาต role `supervisor` ทั้งที่ Final Spec เหลือเพียง `student`, `advisor`, `admin`

วิธีแก้:

- หยุด seed Supervisor role/user
- เพิ่ม admin test user ด้วย RoleID 4
- เอา Supervisor ออกจาก authorization ของ endpoint หลัก
- เปลี่ยน Evaluation เป็น `advisor_eval`
- ลบ Supervisor branch จาก Dashboard และ Task service
- Attendance review ตรวจ Advisor ownership จริง และไม่ bypass เป็น success อีกต่อไป

ข้อจำกัด: `SupervisorID` ยังอยู่ใน Entity, DbContext, migration และ SQL legacy เพื่อหลีกเลี่ยงการทำลาย schema ที่ใช้งานอยู่ ต้องสร้าง migration แยกในงานถัดไป

#### 2. แก้ Advisor Data Isolation

ไฟล์ที่แก้:

- `InternLog.API/src/Modules/Leaves/LeaveService.cs`
- `InternLog.API/src/Modules/Tasks/TaskService.cs`
- `InternLog.API/src/Modules/WorkLogs/WorkLogService.cs`

วิธีแก้:

- Leave pending กรองเฉพาะ Internship ที่ Advisor คนนั้นดูแล; Admin ดูได้ทั้งหมด
- Leave review ตรวจ Advisor ownership
- Advisor เห็น Tasks ของนักศึกษาที่ดูแล ไม่ใช่เฉพาะงานที่ตนสร้าง
- WorkLog pending/detail/review/comment ตรวจ scope ของ Advisor
- Admin ดู WorkLog ได้แบบ read-only ตาม role ที่ Controller กำหนด

#### 3. ปรับ WorkLogs เป็น Instant-save เบื้องต้น

ไฟล์ที่แก้:

- `InternLog.API/src/Modules/WorkLogs/WorkLogsController.cs`
- `InternLog.API/src/Modules/WorkLogs/IWorkLogService.cs`
- `InternLog.API/src/Modules/WorkLogs/WorkLogService.cs`
- `InternLog.Web/src/app/core/services/worklogs.service.ts`
- `InternLog.Web/src/app/features/work-logs/work-log-form/work-log-form.component.ts`
- `InternLog.Web/src/app/features/work-logs/work-log-form/work-log-form.component.html`

วิธีแก้:

- `POST /api/worklogs` ไม่รับ query `submit` แล้ว
- ถอด route `/submit` และ `/review` จาก Controller
- ฟอร์ม Frontend ใช้ปุ่ม “บันทึกงาน” เพียงรูปแบบเดียว
- เพิ่ม `DELETE /api/worklogs/{id}`
- แก้ไขและแนบไฟล์ได้โดยไม่ติดเงื่อนไข draft/returned เดิม
- WorkLog ใหม่ใช้สถานะภายใน `active` และ Hours ยังมาจาก Attendance ฝั่ง Server
- Advisor listing ไม่กรองเฉพาะ `submitted` เพื่อให้เห็นบันทึก `active`

ข้อจำกัด: โค้ด approval เก่าบางเมธอด/DTO และ field `Status` ยังอยู่ใน service/entity/Frontend service ต้องล้างต่อให้หมด และควรทำ migration/contract cleanup อย่างเป็นระบบ

### Validation ล่าสุด

- `dotnet build c:\Internship_Log_System\InternLog.API\InternLog.API.csproj` ผ่านหลังแก้ authorization และ WorkLogs
- `npm run build` ใน `InternLog.Web` ผ่านหลังแก้ WorkLogs
- ก่อนหน้า API เคย start ที่ `http://localhost:5005`

### งานถัดไปตามลำดับ

1. สร้าง migration ใหม่เพื่อลบ `Messages`, `SupervisorID` และ schema ที่ไม่ใช้ โดยตรวจข้อมูลจริงก่อน
2. ลบ `Message` module, `DbSet`, Dashboard unread message และ Frontend service/component ที่เหลือ
3. ล้าง WorkLog approval remnants จาก service, DTO, entity และ Frontend service/model
4. เพิ่ม WorkLog categories, upload validation และทดสอบ create/edit/delete/comment ด้วย 3 roles
5. แก้ TypeScript diagnostics ใน `InternLog.Web/tsconfig.json`
6. เชื่อม Mock modules ได้แก่ Profile, Calendar, Dashboard, Reports และ Admin เข้ากับ API จริง
7. ทำ impersonation, Audit Log, notifications และ file security
8. รัน QA/UAT ครบ Student, Advisor และ Admin พร้อมตรวจ 401/403/IDOR

## คำสั่งสำหรับ AI ตัวถัดไป

ให้เริ่มด้วยข้อความนี้:

> อ่าน `docs/internship-log-ai-handoff-2026-08-28.md` และ `.github/skills/internship-log-system/SKILL.md` ก่อนทำงาน ต่อจากสถานะล่าสุดโดยตรวจ Network response ของ `POST /api/leaves` และ `POST /api/attendance/check-in` ว่า 400 เกิดจาก validation ข้อใด แก้ที่ root cause โดยรักษา Final Spec, role scope และ API contract จากนั้นรัน focused build/test และรายงานผล ห้ามเดาสาเหตุจาก status code เพียงอย่างเดียว

## สรุปสั้น

รอบนี้แก้ปัญหา compile error ของ API, warning ของ Angular, 415 ของ Leave, รองรับ body ว่างของ Attendance, ปิด Supervisor ใน runtime, แก้ Advisor ownership และเริ่มปรับ WorkLogs เป็น instant-save แล้ว โดย API/Angular build ผ่าน งานสำคัญถัดไปคือ migration cleanup ของ Supervisor/Messages, ล้าง approval remnants และเชื่อม Mock modules ให้ตรงกับ Final Spec
 
 