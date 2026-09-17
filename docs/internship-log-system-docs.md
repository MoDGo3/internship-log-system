# 📘 ระบบจดบันทึกการฝึกงาน — Internship Training Log System
> เอกสารประกอบโปรเจกต์ฉบับสมบูรณ์ | จัดทำเมื่อ 14 สิงหาคม 2569
> ชื่อไฟล์แนะนำ: `internship-log-system-docs.md`

---

## 📌 1. ภาพรวมโปรเจกต์

**วัตถุประสงค์:** เว็บแอปพลิเคชันจดบันทึกการฝึกงาน คอนเซปต์คล้าย Microsoft Teams —
นักศึกษาลงเวลา/บันทึกงานประจำวัน/ขอลา, อาจารย์นิเทศก์อนุมัติและบันทึกการนิเทศ,
พี่เลี้ยงมอบหมายงาน, มีรายงานสรุป Export PDF ได้

**ลำดับการทำงาน:**
1. ออกแบบทุกหน้าจอใน **Figma**
2. สร้างฐานข้อมูลใน **SSMS** (SQL Server)
3. พัฒนา **Backend (API)** เชื่อมต่อ DB
4. พัฒนา **Frontend** ด้วย **Angular** เรียกใช้ API

**Tech Stack:**
| Layer | เทคโนโลยี |
|---|---|
| Design | Figma |
| Frontend | Angular |
| Backend | ASP.NET Core Web API (.NET 8) + Entity Framework Core |
| Database | SQL Server (จัดการผ่าน SSMS) |
| Auth | JWT + Role-Based Authorization |
| API Docs | Swagger UI |

---

## 👥 2. ผู้ใช้งาน (User Roles)

| Role | สิทธิ์หลัก |
|---|---|
| 🎓 student (นักศึกษา) | ลงเวลา, บันทึกงาน, ขอลา, ส่งรายงาน, ดูงานมอบหมาย |
| 🧑 advisor (อาจารย์นิเทศก์) | อนุมัติ/ตีกลับ บันทึก-ลา-รายงาน, คอมเมนต์, บันทึกการนิเทศ |
| 💼 supervisor (พี่เลี้ยงบริษัท) | มอบหมายงาน, ประเมินนักศึกษา |
| 🛠 admin | จัดการผู้ใช้ / บริษัท / ภาคเรียน / จับคู่การฝึก |

---

## 🧭 3. โครงสร้าง Layout (สไตล์ MS Teams)

- **Left Sidebar:** แดชบอร์ด • ลงเวลา • วันลา • ปฏิทิน • บันทึกงาน • งานมอบหมาย • การนิเทศ • แชท • รายงาน • โปรไฟล์
- **Top Bar:** ช่องค้นหา (Ctrl+E), กระดิ่งแจ้งเตือน, รูปโปรไฟล์
- **Main Content:** เนื้อหาแต่ละหน้า

**สีสถานะบนปฏิทิน:** 🟢 อนุมัติ | 🟡 รออนุมัติ | 🔴 ไม่ได้บันทึก | 🟣 วันลา | ⚪ วันหยุด

---

## 🗂 4. หน้าเว็บทั้งหมด + สิ่งที่ต้องมี

### 4.1 Login / Register
- ฟอร์มล็อกอิน, ลืมรหัสผ่าน, สมัครสมาชิก (หรือ admin เพิ่มให้)

### 4.2 Dashboard
- ความคืบหน้าการฝึก (x/y วัน), ชั่วโมงสะสม, วันลาคงเหลือ
- สถานะวันนี้ + ปุ่มลัด "บันทึกงานวันนี้" / ปุ่มลงเวลา
- แจ้งเตือนล่าสุด, สถานะบันทึกล่าสุด, นัดนิเทศถัดไป
- *เวอร์ชันอาจารย์:* คิวบันทึกรออนุมัติ

### 4.3 ลงเวลาปฏิบัติงาน (Attendance)
- ปุ่มใหญ่ ลงเวลาเข้า / ลงเวลาออก (บันทึกเวลาปัจจุบันอัตโนมัติ)
- แสดงเวลามา–เวลากลับวันนี้ + รวมชั่วโมง
- ตารางประวัติ: วันที่ | เวลามา | เวลากลับ | รวมชั่วโมง | สถานะ (ตรงเวลา/สาย)
- สรุปประจำเดือน: รวมชั่วโมง, มาสาย, ขาด
- ขอแก้ไขกรณีลืมลงเวลา (ส่งอนุมัติ)

### 4.4 วันลา (Leave)
- การ์ดโควตา: ลาป่วย/ลากิจ/พักร้อน ใช้ไป–คงเหลือ
- ฟอร์มขอลา: ประเภท, ตั้งแต่–ถึง, จำนวนวัน (อัตโนมัติ), เหตุผล, แนบใบรับรองแพทย์
- รายการคำขอ + สถานะ: รออนุมัติ / อนุมัติ / ไม่อนุมัติ

### 4.5 ปฏิทิน (Calendar)
- มุมมองเดือน/สัปดาห์, พ.ศ. ไทย
- สีประจำวันที่ตามสถานะ (ตามหัวข้อ 3)
- คลิกวันที่ → เพิ่ม/ดูบันทึกงาน, เพิ่มนัดหมาย

### 4.6 บันทึกงาน (Work Log)
- รายการ + ฟิลเตอร์ (เดือน/สถานะ/ประเภท) + ค้นหา
- ฟอร์ม: วันที่, เวลาเริ่ม–จบ, ชื่องาน, รายละเอียด, ปัญหา–วิธีแก้, ประเภทงาน, แนบรูป/ไฟล์
- สถานะ: ร่าง / ส่งแล้ว / อนุมัติ / ตีกลับ + คอมเมนต์อาจารย์

### 4.7 งานมอบหมาย (Tasks)
- Kanban: ยังไม่ทำ / กำลังทำ / เสร็จ
- ผู้มอบหมาย, กำหนดส่ง, ความสำคัญ, ลิงก์ไปบันทึกงาน

### 4.8 การนิเทศ (Advisor Visits)
- ตาราง: คนที่ | ครั้งที่ | วันที่ | ชื่ออาจารย์นิเทศ | รูปแบบ (Onsite/Online) | สรุปผล | คะแนน

### 4.9 แชท / แจ้งเตือน
- แชท 1:1 นักศึกษา ↔ อาจารย์ ↔ พี่เลี้ยง
- แจ้งเตือนระบบ: อนุมัติ/ตีกลับ, งานใหม่, ใกล้ครบกำหนดส่ง

### 4.10 รายงาน (Reports)
- สรุปรายสัปดาห์/เดือน อัตโนมัติจากบันทึก + ลงเวลา + วันลา
- กราฟชั่วโมงตามประเภทงาน
- ตารางการนิเทศ, Export PDF, อาจารย์อนุมัติรายฉบับ

### 4.11 ประเมิน (Evaluation)
- พี่เลี้ยงประเมินนักศึกษา / นักศึกษาประเมินสถานประกอบการ

### 4.12 โปรไฟล์ / ตั้งค่า
- ข้อมูลส่วนตัว, รหัสนศ., บริษัท, ช่วงฝึก, อาจารย์ที่ปรึกษา, เปลี่ยนรหัสผ่าน

### 4.13 Admin Panel
- จัดการผู้ใช้ (import รายชื่อ), บริษัท, ภาคเรียน, จับคู่ นศ.–บริษัท–อาจารย์

---

## 🔁 5. Status Flow

```
บันทึกงาน:  draft → submitted → approved
                              → returned → (แก้) → submitted
วันลา:      pending → approved / rejected
รายงาน:     draft → submitted → approved / returned
ลงเวลาแก้:  pending_edit → approved / rejected
```

---

## 🗄 6. ERD เต็มรูปแบบ (18 ตาราง)

### 6.1 Roles
| Field | Type | หมายเหตุ |
|---|---|---|
| RoleID | INT | PK |
| RoleName | NVARCHAR(50) | student / advisor / supervisor / admin |

### 6.2 Users
| Field | Type | หมายเหตุ |
|---|---|---|
| UserID | INT | PK |
| RoleID | INT | FK → Roles |
| Email | NVARCHAR(100) | UNIQUE (ใช้ล็อกอิน) |
| PasswordHash | NVARCHAR(255) | BCrypt |
| FirstName | NVARCHAR(100) | |
| LastName | NVARCHAR(100) | |
| StudentID | NVARCHAR(20) | NULL |
| Phone | NVARCHAR(20) | |
| IsActive | BIT | default 1 |
| CreatedAt | DATETIME | default GETDATE() |

### 6.3 Companies
| Field | Type | หมายเหตุ |
|---|---|---|
| CompanyID | INT | PK |
| Name | NVARCHAR(200) | |
| Address | NVARCHAR(500) | |
| Phone | NVARCHAR(20) | |
| WorkStartTime | TIME | default 09:00 (เกณฑ์ "สาย") |
| WorkEndTime | TIME | default 18:00 |

### 6.4 TrainingPeriods
| Field | Type | หมายเหตุ |
|---|---|---|
| PeriodID | INT | PK |
| PeriodName | NVARCHAR(100) | เช่น ภาคเรียนที่ 1/2569 |
| StartDate | DATE | |
| EndDate | DATE | |
| IsActive | BIT | |

### 6.5 Internships ⭐ (ตารางแกนกลาง)
| Field | Type | หมายเหตุ |
|---|---|---|
| InternshipID | INT | PK |
| StudentID | INT | FK → Users |
| CompanyID | INT | FK → Companies |
| AdvisorID | INT | FK → Users (อาจารย์นิเทศก์) |
| SupervisorID | INT | FK → Users NULL (พี่เลี้ยง) |
| PeriodID | INT | FK → TrainingPeriods |
| StartDate | DATE | |
| EndDate | DATE | |
| RequiredDays | INT | เช่น 60 |
| RequiredHours | INT | เช่น 480 |
| Status | NVARCHAR(20) | active / completed |

### 6.6 Attendances
| Field | Type | หมายเหตุ |
|---|---|---|
| AttendanceID | INT | PK |
| StudentID | INT | FK → Users |
| AttDate | DATE | UNIQUE (StudentID, AttDate) |
| CheckInTime | TIME | NULL |
| CheckOutTime | TIME | NULL |
| TotalHours | DECIMAL(5,2) | คำนวณอัตโนมัติ |
| Status | NVARCHAR(20) | ontime / late / leave / absent / pending_edit |
| Note | NVARCHAR(500) | เหตุผลขอแก้เวลา |

### 6.7 LeaveTypes
| Field | Type | หมายเหตุ |
|---|---|---|
| LeaveTypeID | INT | PK |
| TypeName | NVARCHAR(50) | ลาป่วย / ลากิจ / ลาพักร้อน |
| MaxDays | INT | โควตาสูงสุด |

### 6.8 Leaves
| Field | Type | หมายเหตุ |
|---|---|---|
| LeaveID | INT | PK |
| StudentID | INT | FK → Users |
| LeaveTypeID | INT | FK → LeaveTypes |
| StartDate | DATE | |
| EndDate | DATE | |
| TotalDays | DECIMAL(4,1) | รองรับครึ่งวัน |
| Reason | NVARCHAR(500) | |
| AttachmentPath | NVARCHAR(255) | NULL (ใบรับรองแพทย์) |
| Status | NVARCHAR(20) | pending / approved / rejected |
| ApprovedBy | INT | FK → Users NULL |
| ApprovedAt | DATETIME | NULL |
| CreatedAt | DATETIME | |

### 6.9 WorkCategories
| Field | Type | หมายเหตุ |
|---|---|---|
| CategoryID | INT | PK |
| CategoryName | NVARCHAR(100) | Development / Meeting / Doc / Testing / อื่น ๆ |

### 6.10 WorkLogs
| Field | Type | หมายเหตุ |
|---|---|---|
| LogID | INT | PK |
| StudentID | INT | FK → Users |
| LogDate | DATE | |
| Title | NVARCHAR(200) | |
| Description | NVARCHAR(MAX) | |
| Problem | NVARCHAR(MAX) | NULL |
| Solution | NVARCHAR(MAX) | NULL |
| CategoryID | INT | FK → WorkCategories |
| Hours | DECIMAL(5,2) | ดึงจาก Attendances |
| Status | NVARCHAR(20) | draft / submitted / approved / returned |
| CreatedAt | DATETIME | |
| UpdatedAt | DATETIME | NULL |

### 6.11 Attachments
| Field | Type | หมายเหตุ |
|---|---|---|
| AttachmentID | INT | PK |
| LogID | INT | FK → WorkLogs |
| FileName | NVARCHAR(255) | |
| FilePath | NVARCHAR(500) | |
| FileType | NVARCHAR(20) | |
| FileSize | BIGINT | |
| UploadedAt | DATETIME | |

### 6.12 LogComments
| Field | Type | หมายเหตุ |
|---|---|---|
| CommentID | INT | PK |
| LogID | INT | FK → WorkLogs |
| UserID | INT | FK → Users (ผู้คอมเมนต์) |
| Message | NVARCHAR(MAX) | |
| CreatedAt | DATETIME | |

### 6.13 Tasks
| Field | Type | หมายเหตุ |
|---|---|---|
| TaskID | INT | PK |
| InternID | INT | FK → Users (ผู้รับงาน) |
| AssignedBy | INT | FK → Users (ผู้มอบหมาย) |
| Title | NVARCHAR(200) | |
| Detail | NVARCHAR(MAX) | NULL |
| DueDate | DATE | NULL |
| Priority | NVARCHAR(10) | low / medium / high |
| Status | NVARCHAR(20) | todo / doing / done |
| CreatedAt | DATETIME | |

### 6.14 AdvisorVisits
| Field | Type | หมายเหตุ |
|---|---|---|
| VisitID | INT | PK |
| InternshipID | INT | FK → Internships |
| PersonNo | INT | คนที่ |
| VisitNo | INT | ครั้งที่ |
| VisitDate | DATE | วันที่ |
| AdvisorID | INT | FK → Users (ชื่ออาจารย์ได้จากการ JOIN) |
| Method | NVARCHAR(20) | onsite / online |
| Summary | NVARCHAR(MAX) | ผลการนิเทศ |
| Score | DECIMAL(5,2) | NULL |
| CreatedAt | DATETIME | |

### 6.15 Messages
| Field | Type | หมายเหตุ |
|---|---|---|
| MessageID | INT | PK |
| SenderID | INT | FK → Users |
| ReceiverID | INT | FK → Users |
| Content | NVARCHAR(MAX) | |
| IsRead | BIT | default 0 |
| SentAt | DATETIME | |

### 6.16 Notifications
| Field | Type | หมายเหตุ |
|---|---|---|
| NotificationID | INT | PK |
| UserID | INT | FK → Users (ผู้รับ) |
| Message | NVARCHAR(500) | |
| RelatedType | NVARCHAR(50) | NULL: worklog / leave / task / report |
| RelatedID | INT | NULL |
| IsRead | BIT | default 0 |
| CreatedAt | DATETIME | |

### 6.17 Reports
| Field | Type | หมายเหตุ |
|---|---|---|
| ReportID | INT | PK |
| StudentID | INT | FK → Users |
| ReportType | NVARCHAR(20) | weekly / monthly |
| StartDate | DATE | |
| EndDate | DATE | |
| Summary | NVARCHAR(MAX) | NULL |
| Status | NVARCHAR(20) | draft / submitted / approved / returned |
| ApprovedBy | INT | FK → Users NULL |
| CreatedAt | DATETIME | |

### 6.18 Evaluations
| Field | Type | หมายเหตุ |
|---|---|---|
| EvaluationID | INT | PK |
| InternshipID | INT | FK → Internships |
| EvaluatorID | INT | FK → Users |
| EvalType | NVARCHAR(50) | supervisor_eval / student_eval |
| TotalScore | DECIMAL(5,2) | |
| Comment | NVARCHAR(MAX) | NULL |
| EvaluatedAt | DATETIME | |

### 6.19 ความสัมพันธ์ (1 → N)
```
Roles ─< Users
Users ─< Internships >─ Companies , TrainingPeriods
Internships ─< AdvisorVisits , Evaluations
Users ─< Attendances , Leaves >─ LeaveTypes
Users ─< WorkLogs >─ WorkCategories
WorkLogs ─< Attachments , LogComments
Users ─< Tasks (ผู้รับ & ผู้มอบหมาย)
Users ─< Messages (ผู้ส่ง & ผู้รับ) , Notifications , Reports
```

### 6.20 Constraints สำคัญ
- UNIQUE (Email) ใน Users
- UNIQUE (StudentID, AttDate) ใน Attendances — 1 วันลงเวลาได้ครั้งเดียว
- Default: IsActive=1, IsRead=0, CreatedAt=GETDATE()
- FK ทุกตัวตามรายการความสัมพันธ์

---

## 🛠 7. ออกแบบหลังบ้าน (Backend)

### 7.1 สถาปัตยกรรม (Layered)
```
Angular ──HTTP+JWT──▶ [Controller] ─▶ [Service] ─▶ [EF Core DbContext] ─▶ SQL Server
                        validate        business logic    18 Entities
                        response กลาง   แจ้งเตือนอัตโนมัติ
```
- **Controller:** รับ request / validate / คืน response
- **Service:** กฎธุรกิจทั้งหมด
- **Data:** DbContext + Entities ตาม ERD
- **Middleware:** JWT Auth, Exception กลาง, CORS (ยอม localhost:4200)

### 7.2 โครงสร้างโฟลเดอร์
```
InternLog.API/
├── Controllers/   (Auth, Attendance, Leaves, WorkLogs, Tasks,
│                   AdvisorVisits, Reports, Messages, Notifications,
│                   Evaluations, Admin, Dashboard)
├── Services/
├── Data/          (AppDbContext.cs)
├── Models/        (Entities 18 ตาราง)
├── DTOs/
├── Middleware/    (ExceptionMiddleware.cs)
├── Utils/         (JwtHelper, FileHelper)
└── appsettings.json
```

### 7.3 ตารางสิทธิ์
| ความสามารถ | student | advisor | supervisor | admin |
|---|---|---|---|---|
| ลงเวลา/บันทึกงาน/ขอลา | ✅ | 👁 | 👁 | — |
| อนุมัติบันทึก/ลา/รายงาน | — | ✅ | — | — |
| มอบหมายงาน | — | ✅ | ✅ | — |
| บันทึกการนิเทศ | 👁 | ✅ | — | — |
| ประเมิน | 👁 | — | ✅ | — |
| จัดการผู้ใช้/บริษัท/ภาคเรียน | — | — | — | ✅ |

**JWT Claims:** `sub=userId, role, name, exp=8ชม.`

### 7.4 API Endpoints
**Auth**
| POST | /api/auth/login | ล็อกอิน → JWT | ทุก role |
| GET | /api/auth/me | ข้อมูลผู้ใช้ปัจจุบัน | ทุก role |
| PUT | /api/auth/change-password | เปลี่ยนรหัส | ทุก role |

**Dashboard / Calendar**
| GET | /api/dashboard | หน้าแรกตาม role | ทุก role |
| GET | /api/calendar?month= | สถานะรายวันสำหรับสีปฏิทิน | student, advisor |

**Attendance**
| POST | /api/attendance/check-in , check-out | ลงเวลา | student |
| GET | /api/attendance?month= | ประวัติ | student, advisor |
| GET | /api/attendance/summary | สรุปชั่วโมง/สาย/ขาด | student |
| PUT | /api/attendance/{id}/request-edit | ขอแก้เวลา | student |
| PUT | /api/attendance/{id}/review | อนุมัติคำขอแก้ | advisor, supervisor |

**Leaves**
| GET | /api/leaves , /api/leaves/quota | ประวัติ , โควตาคงเหลือ | student |
| POST | /api/leaves | ยื่นลา (+ไฟล์) | student |
| GET | /api/leaves/pending | คิวอนุมัติ | advisor |
| PUT | /api/leaves/{id}/review | อนุมัติ/ไม่อนุมัติ | advisor |

**WorkLogs**
| GET | /api/worklogs , /{id} , /pending | รายการ , รายละเอียด , คิวอนุมัติ | student/advisor |
| POST | /api/worklogs | สร้าง (draft/submit) | student |
| PUT | /api/worklogs/{id} , /submit , /review | แก้ไข , ส่งอนุมัติ , อนุมัติ/ตีกลับ | ตามสิทธิ์ |
| POST | /api/worklogs/{id}/attachments , /comments | แนบไฟล์ , คอมเมนต์ | ตามสิทธิ์ |
| DELETE | /api/attachments/{id} | ลบไฟล์ | เจ้าของ |

**Tasks**
| GET | /api/tasks | รายการ | student, supervisor, advisor |
| POST | /api/tasks | มอบหมายงาน | supervisor, advisor |
| PUT | /api/tasks/{id} , /{id}/status | แก้ไข , อัปเดตสถานะ | ผู้มอบหมาย / student |
| DELETE | /api/tasks/{id} | ลบ | ผู้มอบหมาย |

**AdvisorVisits**
| GET | /api/visits?internshipId= | รายการ | student, advisor |
| POST | /api/visits | บันทึก คนที่/ครั้งที่/วันที่/อาจารย์ | advisor |
| PUT | /api/visits/{id} | แก้ไข | advisor |

**Reports**
| GET | /api/reports | รายการ | student, advisor |
| POST | /api/reports/generate | สร้างจาก logs ในช่วงวันที่ | student |
| PUT | /api/reports/{id}/submit , /review | ส่ง , อนุมัติ/ตีกลับ | ตามสิทธิ์ |
| GET | /api/reports/{id}/export-pdf | Export PDF | student, advisor |

**Messages / Notifications**
| GET | /api/messages/{userId} | บทสนทนา | ทุก role |
| POST | /api/messages | ส่งข้อความ | ทุก role |
| GET | /api/notifications | รายการแจ้งเตือน | ทุก role |
| PUT | /api/notifications/{id}/read | อ่านแล้ว | ทุก role |

**Evaluations**
| GET | /api/evaluations?internshipId= | ดูผลประเมิน | ผู้เกี่ยวข้อง |
| POST | /api/evaluations | ส่งประเมิน | supervisor, student |

**Admin** (admin เท่านั้น)
| CRUD | /api/admin/users | จัดการผู้ใช้ |
| CRUD | /api/admin/companies | จัดการบริษัท |
| CRUD | /api/admin/periods | จัดการภาคเรียน |
| POST | /api/admin/internships | จับคู่ นศ.–บริษัท–อาจารย์ |

### 7.5 Business Logic (Service Layer)
1. **CalcHours** — TotalHours = check-out − check-in
2. **DetectLate** — check-in > Companies.WorkStartTime = สาย
3. **CheckLeaveQuota** — ผลรวมวันลา (อนุมัติ+รอ) ≤ LeaveTypes.MaxDays
4. **GenReport** — รวม WorkLogs + Attendance + Leaves ในช่วงวันที่
5. **BuildCalendar** — merge รายวัน → status สีต่อวัน
6. **AutoNotify** — สร้าง Notification ทุกครั้งที่สถานะเปลี่ยน
7. **OwnershipCheck** — แก้ได้เฉพาะของตนเอง / อนุมัติได้เฉพาะ นศ. ที่ดูแล

### 7.6 รูปแบบ Response กลาง
```json
{ "success": true,  "message": "บันทึกสำเร็จ",   "data": { } }
{ "success": false, "message": "สิทธิ์ไม่เพียงพอ", "errors": [] }
```

### 7.7 การเชื่อมต่อ DB + Seed Data
- Connection String: `Server=.;Database=InternLogDB;Trusted_Connection=True;TrustServerCertificate=True;`
- Workflow: รัน CREATE TABLE ใน SSMS → `dotnet ef dbcontext scaffold` (DB First)
- Seed: Roles (4 แถว), LeaveTypes (ป่วย 30 / กิจ 3 / พักร้อน 6), WorkCategories (5 หมวด)

---

## 🎨 8. รายการหน้าจอใน Figma (~20 screens)

1. Login  2. Register/ลืมรหัส  3. Dashboard (นศ.)  4. Dashboard (อาจารย์)
5. ลงเวลา  6. วันลา + ฟอร์มขอลา  7. ปฏิทินเดือน  8. Popup บันทึกงาน
9. Work Log List  10. Work Log Form  11. Tasks  12. การนิเทศ + ฟอร์ม
13. Chat  14. Notifications  15. Reports  16. Preview PDF
17. Evaluation  18. Profile  19. Admin Users  20. Admin Internships

---

## 🗺 9. แผนการพัฒนา (Roadmap)

1. ✅ วางโครงสร้าง + ERD + Backend (เอกสารนี้)
2. 🧾 สร้าง DB: รัน CREATE TABLE 18 ตาราง + Seed ใน SSMS
3. 💻 ตั้งโปรเจกต์ API + Auth (JWT) + Swagger
4. 🔌 พัฒนา API ทีละโมดูล: Attendance → Leaves → WorkLogs → Tasks → Visits → Reports → Chat/Notify → Admin
5. 🎨 ออกแบบ Figma ครบ 20 screens
6. 🅰️ พัฒนา Angular ตาม Figma + เรียก API
7. 🧪 เทสทั้งระบบ + Export PDF + ส่งมอบ