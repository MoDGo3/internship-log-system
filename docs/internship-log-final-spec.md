# 📘 ระบบจดบันทึกการฝึกงาน — เอกสารส่วนที่ 12 (FINAL SPEC)
> สเปกฉบับสมบูรณ์: สิ่งที่ต้องทำทั้งหมด (Frontend + Backend) หลังปรับ Requirements รอบสุดท้าย
> จัดทำเมื่อ 14 สิงหาคม 2569 | ชื่อไฟล์แนะนำ: `internship-log-final-spec.md`

---

## 1. สรุปการเปลี่ยนแปลงรอบสุดท้าย (v3)

| # | การเปลี่ยนแปลง | ผลกระทบ |
|---|---|---|
| 1 | ตัด Role **Supervisor (พี่เลี้ยง)** ออก | เหลือ 3 roles: student / advisor / admin |
| 2 | ตัดโมดูล **Chat** ออก | ลบตาราง Messages, ลบหน้าแชท, ลบ API |
| 3 | **WorkLogs = บันทึกจริง** (ตัด flow ขออนุมัติ) | นักศึกษาบันทึกได้ทันที + แก้ไขได้ตลอดเวลา, อาจารย์คอมเมนต์ได้ |
| 4 | **Advisor เพิ่ม/แก้ไขบริษัทของตัวเองได้** | Companies มี FK → Advisor (1 คนดูแลได้หลายบริษัท) |
| 5 | **Reports แยกตาม Role** | Student ดูของตัวเอง / Advisor ดูของ นศ. ที่ดูแล / Admin ดูทั้งระบบ |
| 6 | **Profile แก้ไขได้** + นศ. กรอกที่อยู่+เบอร์โทรบริษัท | เพิ่มฟิลด์ใน Internships |
| 7 | **Admin ดู Dashboard นักศึกษาได้** (Impersonation) | เพิ่ม endpoint + banner |
| 8 | Tasks มอบหมายโดย **Advisor/Admin** (แทน supervisor) | ปรับ AssignedBy |

**มติเสริม (ที่เลือกให้เพื่อให้ระบบสมบูรณ์):**
- WorkLogs: บันทึกทันที + อาจารย์คอมเมนต์ติชมได้ (ไม่มีอนุมัติ/ตีกลับ)
- Evaluations: อาจารย์ประเมินนักศึกษา / นักศึกษาประเมินบริษัท (แทน supervisor)
- Impersonation: Admin ดูได้อย่างเดียว (read-only) + มี Audit Log

---

## 2. Roles & Permission Matrix (3 Roles)

| ความสามารถ | student | advisor | admin |
|---|:---:|:---:|:---:|
| ลงเวลาเข้า-ออก / ขอแก้เวลา | ✅ | — | — |
| อนุมัติคำขอแก้เวลา / วันลา | — | ✅ | 👁 |
| บันทึกงาน (บันทึกจริง + แก้ไขได้ตลอด) | ✅ | — | — |
| คอมเมนต์บันทึกงาน | — | ✅ | 👁 |
| รับงาน / อัปเดตสถานะงาน | ✅ | — | 👁 |
| มอบหมายงาน | — | ✅ | ✅ |
| ดูการนิเทศ | 👁 | ✅ | 👁 |
| บันทึกการนิเทศ | — | ✅ | — |
| เพิ่ม/แก้ไขบริษัท | — | ✅ (ของตัวเอง) | ✅ (ทั้งหมด) |
| จัดการผู้ใช้ / จับคู่การฝึก / ภาคเรียน | — | — | ✅ |
| Impersonate ดู Dashboard นศ. | — | — | ✅ |
| รายงานของตัวเอง | ✅ | — | — |
| รายงานของ นศ. ที่ดูแล | — | ✅ | — |
| รายงานรวมทั้งระบบ | — | — | ✅ |
| ประเมิน (นศ.↔อาจารย์) | ✅ | ✅ | 👁 |

---

## 3. เมนู Sidebar ตาม Role

| เมนู | student | advisor | admin |
|---|:---:|:---:|:---:|
| 🏠 แดชบอร์ด | ✅ | ✅ | ✅ |
| ⏰ ลงเวลา | ✅ | 👁 |  |
| 🏖 วันลา | ✅ | ✅ (อนุมัติ) | 👁 |
| 📅 ปฏิทิน | ✅ | ✅ | ✅ |
| 📝 บันทึกงาน | ✅ | 👁 |  |
| 📋 งานมอบหมาย | ✅ | ✅ | 👁 |
| 🧑 การนิเทศ | 👁 | ✅ | 👁 |
| 📊 รายงาน | ✅ | ✅ | ✅ |
| 🏢 บริษัท | — | ✅ | ✅ |
| 👥 ผู้ใช้ | — | — | ✅ |
| 🔗 จับคู่การฝึก | — | — | ✅ |
| ⚙️ โปรไฟล์ | ✅ | ✅ | ✅ |

*(👁 = ดูได้อย่างเดียว / เมนู Admin เพิ่ม: ภาคเรียน อยู่ใต้หน้าจับคู่การฝึก)*

---

## 4. ERD ฉบับปรับปรุง (19 ตาราง)

**ตัดออก:** Messages (Chat) ❌
**ปรับ Roles:** เหลือ 3 ค่า (student/advisor/admin)

| # | ตาราง | การเปลี่ยนแปลงรอบนี้ |
|---|---|---|
| 1 | Roles | เหลือ 3 roles |
| 2 | Users | เดิม |
| 3 | RefreshTokens | เดิม |
| 4 | PasswordResetTokens | เดิม |
| 5 | Companies | **+ AdvisorID INT NULL FK→Users** (อาจารย์ผู้ดูแล, 1 คนได้หลายบริษัท) |
| 6 | TrainingPeriods | เดิม |
| 7 | Internships | **− SupervisorID** • **+ CompanyAddress NVARCHAR(500) NULL** • **+ CompanyPhone NVARCHAR(20) NULL** (นศ. กรอกใน Profile) |
| 8 | Attendances | เดิม |
| 9 | LeaveTypes | เดิม |
| 10 | Leaves | เดิม (อนุมัติโดย advisor) |
| 11 | WorkCategories | เดิม |
| 12 | WorkLogs | **− Status** (บันทึกจริง ไม่มีอนุมัติ) • คง CreatedAt/UpdatedAt |
| 13 | Attachments | เดิม |
| 14 | LogComments | เดิม (อาจารย์คอมเมนต์) |
| 15 | Tasks | AssignedBy = advisor/admin |
| 16 | AdvisorVisits | เดิม |
| 17 | Reports | เดิม (scope ตาม role) |
| 18 | Notifications | เดิม |
| 19 | Evaluations | EvalType: advisor_eval / student_eval |

**Constraints คงเดิม:** UNIQUE(Email), UNIQUE(StudentID+AttDate), UNIQUE(InternshipID+VisitNo), WorkLogs.Hours = backend computed

---

## 5. ✅ สิ่งที่ต้องทำ — BACKEND (ASP.NET Core 8, Modular)

### 5.1 Foundation
- [ ] Project setup + EF Core + JWT + BCrypt + Swagger + CORS
- [ ] AppDbContext + Entities 19 ตาราง + Fluent API
- [ ] ExceptionMiddleware + ApiResponse `{success,message,data}`
- [ ] DbSeeder (Roles 3, LeaveTypes, WorkCategories, admin คนแรก)
- [ ] SQL Script 19 ตาราง รันใน SSMS

### 5.2 Modules + Endpoints ทั้งหมด

**M1 Auth**
- [ ] POST /api/auth/login • register • refresh • logout
- [ ] POST /api/auth/forgot-password • reset-password
- [ ] GET /api/auth/me • PUT /api/auth/change-password

**M2 Profile**
- [ ] PUT /api/profile (ชื่อ, เบอร์โทร, รูป, คณะ/สาขา/ปี)
- [ ] PUT /api/profile/company-contact (นศ.: ที่อยู่+เบอร์โทรบริษัท → Internships.CompanyAddress/Phone)

**M3 Attendance**
- [ ] POST check-in / check-out (ตัดสายอัตโนมัติ)
- [ ] GET list + summary
- [ ] PUT {id}/request-edit (นศ.) • PUT {id}/review (advisor)

**M4 Leaves**
- [ ] GET list + quota • POST create (นศ.)
- [ ] GET pending • PUT {id}/review (advisor)

**M5 WorkLogs (บันทึกจริง)**
- [ ] POST create (บันทึกทันที, Hours auto จาก Attendance)
- [ ] PUT {id} (เจ้าของแก้ไขได้ตลอดเวลา) • DELETE {id}
- [ ] GET list (filters + studentId สำหรับ advisor) • GET {id}
- [ ] POST {id}/attachments • DELETE attachments/{id}
- [ ] GET/POST {id}/comments (อาจารย์คอมเมนต์)

**M6 Tasks**
- [ ] POST create (advisor/admin) • PUT {id} • DELETE {id}
- [ ] PUT {id}/status (นศ.) • GET list (Kanban)

**M7 AdvisorVisits**
- [ ] POST create (advisor) • PUT {id} • GET list

**M8 Reports (แยก Role)**
- [ ] GET /api/reports (scope: นศ.=ตัวเอง / advisor=นศ.ที่ดูแล / admin=ทั้งหมด)
- [ ] POST generate • GET {id}/export-pdf
- [ ] GET /api/reports/stats (สถิติตาม role สำหรับหน้ารายงาน)

**M9 Notifications**
- [ ] GET list • PUT {id}/read • PUT read-all
- [ ] Auto-notify: ลาอนุมัติ/ตีกลับ, ได้งานใหม่, อาจารย์คอมเมนต์, แก้เวลาอนุมัติ

**M10 Evaluations**
- [ ] POST (advisor→นศ. / นศ.→บริษัท) • GET list

**M11 Calendar (Aggregation)**
- [ ] GET /api/calendar?year&month&studentId (สีรายวัน)
- [ ] GET /api/calendar/events?date (SideDrawer)

**M12 Dashboard (แยก Role)**
- [ ] GET /api/dashboard → student / advisor / admin คนละ payload

**M13 Companies**
- [ ] GET (scope: advisor=ของตัวเอง, admin=ทั้งหมด, นศ.=ของตัวเอง)
- [ ] POST create (advisor/admin) • PUT {id} (advisor เฉพาะของตัวเอง)

**M14 Admin**
- [ ] CRUD /api/admin/users + PATCH status + reset-password
- [ ] POST import Excel • GET export
- [ ] POST {id}/impersonate • POST impersonate/stop + Audit Log
- [ ] CRUD /api/admin/periods • CRUD /api/admin/internships

### 5.3 Business Logic สำคัญ
- [ ] WorkLogs.Hours computed จาก Attendances (sync เมื่อแก้เวลาอนุมัติ)
- [ ] ตัดสายจาก Companies.WorkStartTime
- [ ] เช็กโควตาวันลา
- [ ] Advisor เห็นเฉพาะ นศ./บริษัท ที่ตัวเองดูแล (scope ทุก query)
- [ ] Impersonation token 30 นาที + read-only + Audit Log

---

## 6. ✅ สิ่งที่ต้องทำ — FRONTEND (Angular 17+, Modular)

### 6.1 Foundation
- [ ] Project + Routing + Guards (authGuard, roleGuard)
- [ ] Auth Interceptor (JWT + refresh + retry)
- [ ] Layout Shell: Topbar + Sidebar (เมนูเปลี่ยนตาม role) + Command Bar + SideDrawer
- [ ] Design System: สี/ฟอนต์/Components กลาง (Button, Input, StatusBadge, Table, Modal)

### 6.2 หน้าตาม Role

**หน้าร่วม:** Login • Register • Forgot Password • Profile (แก้ไขได้)

**Student (10 หน้า)**
- [ ] Dashboard นศ. (StatCards + ลงเวลาวันนี้ + แจ้งเตือน)
- [ ] ลงเวลา (นาฬิกา + ปุ่ม + ประวัติ + ขอแก้)
- [ ] วันลา (โควตา + ฟอร์ม + ประวัติ)
- [ ] ปฏิทิน (ของตัวเอง + SideDrawer)
- [ ] บันทึกงาน List + Form (บันทึกจริง + แก้ไขได้ + แนบไฟล์ + ดูคอมเมนต์อาจารย์)
- [ ] งานมอบหมาย (Kanban + อัปเดตสถานะ)
- [ ] การนิเทศ (ดู)
- [ ] รายงาน (ของตัวเอง + Export PDF)
- [ ] ประเมินบริษัท
- [ ] Profile (+ กรอกที่อยู่/เบอร์โทรบริษัท)

**Advisor (9 หน้า)**
- [ ] Dashboard อาจารย์ (คิวลา, นศ. ที่ดูแล, บริษัทที่ดูแล)
- [ ] ปฏิทิน (เลือกดู นศ.)
- [ ] บันทึกงาน (ดู + คอมเมนต์)
- [ ] วันลา (อนุมัติ/ไม่อนุมัติ)
- [ ] งานมอบหมาย (มอบหมาย + Kanban)
- [ ] การนิเทศ (บันทึก คนที่/ครั้งที่/วันที่)
- [ ] รายงาน (ของ นศ. ที่ดูแล + เปรียบเทียบ)
- [ ] บริษัท (เพิ่ม/แก้ไขของตัวเอง)
- [ ] ประเมินนักศึกษา

**Admin (7 หน้า)**
- [ ] Dashboard ระบบ (สถิติรวม)
- [ ] จัดการผู้ใช้ (CRUD + Import/Export + Reset รหัส)
- [ ] **ดู Dashboard นศ. (Impersonation + banner)**
- [ ] จัดการบริษัท (ทั้งหมด)
- [ ] จับคู่การฝึก (นศ.–บริษัท–อาจารย์)
- [ ] จัดการภาคเรียน
- [ ] รายงานรวมทั้งระบบ

### 6.3 UX หลักที่ต้องทำ
- [ ] SideDrawer สำหรับดูรายละเอียด/ฟอร์มด่วน (Teams style)
- [ ] Color coding ปฏิทิน 5 สี
- [ ] Loading/Error/Empty states ทุกหน้า
- [ ] Impersonation banner (สี Warning + ปุ่มกลับ)
- [ ] RBAC UI: ซ่อนปุ่ม/เมนูตาม role

---

## 7. 🧪 Test Cases ที่ต้องปรับ (จากส่วนที่ 10)

- [ ] ตัด Chat tests ออกทั้งหมด
- [ ] ตัด supervisor UAT ออก
- [ ] WorkLogs: เปลี่ยนจาก "ส่งอนุมัติ/อนุมัติ/ตีกลับ" → "บันทึกทันที/แก้ไขได้/อาจารย์คอมเมนต์"
- [ ] เพิ่ม: Advisor เพิ่ม/แก้ไขบริษัท (ของตัวเองเท่านั้น)
- [ ] เพิ่ม: Advisor แก้ไขบริษัทคนอื่น → 403
- [ ] เพิ่ม: Impersonation (admin ดู นศ., read-only, หยุดโหมด)
- [ ] เพิ่ม: Profile นศ. กรอกที่อยู่/เบอร์โทรบริษัท
- [ ] เพิ่ม: Reports แยก scope ตาม role (นศ. ดูของคนอื่นไม่ได้)

---

## 8. 🗺 ลำดับการพัฒนา (Roadmap ปรับใหม่)

```
Phase 0: DB 19 ตาราง + Project setup ทั้ง 2 ฝั่ง
Phase 1: Auth + Profile + Layout Shell (เมนูตาม role)
Phase 2: Attendance + Leaves
Phase 3: WorkLogs (บันทึกจริง) + Tasks
Phase 4: Calendar + Dashboard (3 แบบ)
Phase 5: Visits + Reports (แยก role) + Evaluations
Phase 6: Companies (advisor) + Admin (users/internships/impersonation)
Phase 7: QA ทั้งระบบ + UAT 3 roles
```

---

## 9. สรุปจำนวนงาน

| ฝ่าย | จำนวนงานโดยประมาณ |
|---|---|
| Database | 19 ตาราง + seed + constraints |
| Backend | 14 modules • ~50 endpoints |
| Frontend | ~26 หน้า (รวม auth) • Components กลาง ~15 |
| QA | ~150 test cases (ปรับจาก 170 หลังตัด chat/supervisor) |

**เอกสารนี้คือ "สัญญาการทำงาน" ฉบับสุดท้าย — ทุกการพัฒนาต้องยึดตามนี้**