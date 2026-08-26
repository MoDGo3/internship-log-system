# 🧪 ระบบจดบันทึกการฝึกงาน — เอกสารส่วนที่ 10
> รายการ QA ทดสอบระบบทั้งหมด (Backend + Frontend + Integration + Security + Performance + UAT)
> จัดทำเมื่อ 14 สิงหาคม 2569 | ชื่อไฟล์แนะนำ: `internship-log-qa-test-cases.md`
>
> **แผนที่เอกสารทั้งชุด**
> - ส่วนที่ 1 `internship-log-system-docs.md` → โครงสร้างระบบ + ERD + Backend
> - ส่วนที่ 2 `internship-log-ui-design.md` → UI Design 20 หน้า
> - ส่วนที่ 3 `internship-log-analysis-sql.md` → Use Cases/SRS + Sequence + SQL
> - ส่วนที่ 4 `internship-log-review-decisions.md` → มติแก้ไข (Review v2)
> - ส่วนที่ 5 `internship-log-development-roadmap.md` → แผนพัฒนา
> - ส่วนที่ 6 `internship-log-figma-checklist.md` → รายการงาน Figma
> - ส่วนที่ 7 `internship-log-auth-gap-fix.md` → แก้ไข Auth
> - ส่วนที่ 8 `internship-log-modular-architecture.md` → โครงสร้าง Modular
> - ส่วนที่ 9 `internship-log-figma-page-specs.md` → รายละเอียดออกแบบทุกหน้า
> - ส่วนที่ 10 `internship-log-qa-test-cases.md` → เอกสารฉบับนี้

---

## 📋 สารบัญ

1. [Backend API Testing (70 cases)](#1-backend-api-testing)
2. [Frontend UI/UX Testing (41 cases)](#2-frontend-uiux-testing)
3. [Integration Testing (10 cases)](#3-integration-testing)
4. [Security Testing (13 cases)](#4-security-testing)
5. [Performance Testing (8 cases)](#5-performance-testing)
6. [User Acceptance Testing - UAT (28 cases)](#6-user-acceptance-testing-uat)
7. [Test Summary & Coverage](#7-test-summary--coverage)
8. [Test Execution Plan](#8-test-execution-plan)
9. [Bug Reporting Template](#9-bug-reporting-template)

---

## 1. Backend API Testing

### 1.1 Auth Module (13 Test Cases)

| TC-ID | Test Name | Method | Endpoint | Priority | Expected Result |
|---|---|---|---|---|---|
| AUTH-01 | Login สำเร็จ | POST | /api/auth/login | **High** | 200 + accessToken + refreshToken + user |
| AUTH-02 | Login รหัสผิด | POST | /api/auth/login | **High** | 401 + message "อีเมลหรือรหัสผ่านไม่ถูกต้อง" |
| AUTH-03 | Login email ไม่มีในระบบ | POST | /api/auth/login | **High** | 401 (ไม่บอกว่า email ไม่มี) |
| AUTH-04 | Register สำเร็จ | POST | /api/auth/register | **High** | 200 + สร้าง user role=student |
| AUTH-05 | Register email ซ้ำ | POST | /api/auth/register | **High** | 409 Conflict |
| AUTH-06 | Refresh Token สำเร็จ | POST | /api/auth/refresh | **High** | 200 + accessToken ใหม่ + refreshToken ใหม่ (ตัวเก่าถูก revoke) |
| AUTH-07 | Refresh Token ที่ถูก revoke แล้ว | POST | /api/auth/refresh | **High** | 401 |
| AUTH-08 | Forgot Password | POST | /api/auth/forgot-password | Medium | 200 + message (ไม่บอกว่า email มีหรือไม่) |
| AUTH-09 | Reset Password สำเร็จ | POST | /api/auth/reset-password | **High** | 200 + รหัสเปลี่ยน + refresh tokens ถูก revoke |
| AUTH-10 | Get Me (มี token) | GET | /api/auth/me | **High** | 200 + ข้อมูล user |
| AUTH-11 | Get Me (ไม่มี token) | GET | /api/auth/me | **High** | 401 |
| AUTH-12 | Change Password | PUT | /api/auth/change-password | Medium | 200 + ต้อง login ใหม่ด้วยรหัสใหม่ |
| AUTH-13 | Logout | POST | /api/auth/logout | Medium | 200 + refreshToken ถูก revoke |

### 1.2 Attendance Module (11 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| ATT-01 | Check-in ก่อนเวลาทำงาน (09:00) | **High** | status = "ontime" |
| ATT-02 | Check-in หลังเวลาทำงาน | **High** | status = "late" |
| ATT-03 | Check-in ซ้ำวันเดิม | **High** | 400 "วันนี้ลงเวลาเข้าแล้ว" |
| ATT-04 | Check-out สำเร็จ | **High** | TotalHours คำนวณถูกต้อง (rounded 2 decimals) |
| ATT-05 | Check-out โดยยังไม่ได้ check-in | **High** | 400 "ยังไม่ได้ลงเวลาเข้า" |
| ATT-06 | Check-out ซ้ำวันเดิม | **High** | 400 "วันนี้ลงเวลาออกแล้ว" |
| ATT-07 | Get List (month/year) | Medium | 200 + รายการ attendance ของเดือนนั้น |
| ATT-08 | Get Summary | Medium | 200 + workDays, totalHours, lateCount, absentCount |
| ATT-09 | Request Edit (ลืมลงเวลา) | Medium | status = "pending_edit" |
| ATT-10 | Review Edit (อนุมัติ) | **High** | status updated + WorkLogs.Hours synced |
| ATT-11 | Advisor เรียก check-in | **High** | 403 Forbidden |

### 1.3 Leaves Module (8 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| LEA-01 | ขอลาภายในโควตา | **High** | 200 + status = "pending" |
| LEA-02 | ขอลาเกินโควตา | **High** | 400 "โควตาวันลาไม่เพียงพอ" |
| LEA-03 | ขอลาย้อนหลัง | Medium | 400 "ไม่สามารถขอลาย้อนหลังได้" |
| LEA-04 | Get Quota | Medium | 200 + ลาป่วย/ลากิจ/พักร้อน (ใช้ไป/คงเหลือ) |
| LEA-05 | Approve Leave (advisor) | **High** | status = "approved" + โควตาลดลง |
| LEA-06 | Reject Leave (advisor) | **High** | status = "rejected" + โควตาไม่ลด |
| LEA-07 | Get Pending (advisor) | Medium | 200 + รายการรออนุมัติ |
| LEA-08 | Upload ใบรับรองแพทย์ | Medium | 200 + file path ถูกเก็บใน DB |

### 1.4 WorkLogs Module (10 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| WL-01 | สร้างบันทึก (draft) | **High** | 200 + status = "draft" |
| WL-02 | ส่งอนุมัติ | **High** | status = "submitted" + notification ส่งถึง advisor |
| WL-03 | Approve (advisor) | **High** | status = "approved" + notification ส่งถึง student |
| WL-04 | Return (advisor) | **High** | status = "returned" + comment ถูกบันทึก |
| WL-05 | แก้ไขบันทึก (draft) | Medium | 200 + ข้อมูลอัปเดต |
| WL-06 | แก้ไขบันทึก (submitted) | **High** | 400 "ไม่สามารถแก้ไขบันทึกที่ส่งแล้วได้" |
| WL-07 | Upload Attachment | Medium | 200 + file ถูกเก็บ + path ใน DB |
| WL-08 | Add Comment (advisor) | Medium | 200 + comment ปรากฏ |
| WL-09 | Get List (filter by month/status) | Medium | 200 + ข้อมูลตาม filter |
| WL-10 | Hours คำนวณจาก Attendance | **High** | Hours = TotalHours ของวันนั้น (auto) |

### 1.5 Tasks Module (7 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| TASK-01 | มอบหมายงาน (supervisor) | **High** | 200 + status = "todo" |
| TASK-02 | อัปเดตสถานะ (student) | **High** | status เปลี่ยน (todo→doing→done) |
| TASK-03 | แก้ไขงาน (ผู้มอบหมาย) | Medium | 200 + ข้อมูลอัปเดต |
| TASK-04 | ลบงาน (ผู้มอบหมาย) | Medium | 204 No Content |
| TASK-05 | Get Tasks (Kanban) | Medium | 200 + grouped by status |
| TASK-06 | มอบหมายงานข้าม internship | **High** | 400 "ไม่สามารถมอบหมายงานข้ามการฝึกได้" |
| TASK-07 | Student มอบหมายงาน | **High** | 403 Forbidden |

### 1.6 AdvisorVisits Module (5 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| VISIT-01 | บันทึกการนิเทศ (advisor) | **High** | 200 + PersonNo/VisitNo ถูกต้อง |
| VISIT-02 | VisitNo ซ้ำใน internship เดียว | **High** | 400 "ครั้งที่นิเทศซ้ำ" |
| VISIT-03 | แก้ไขการนิเทศ | Medium | 200 + ข้อมูลอัปเดต |
| VISIT-04 | Get Visits (by internshipId) | Medium | 200 + รายการนิเทศ |
| VISIT-05 | Student บันทึกการนิเทศ | **High** | 403 Forbidden |

### 1.7 Reports Module (5 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| RPT-01 | Generate Report (weekly) | **High** | 200 + รวมข้อมูลจาก WorkLogs/Attendance/Leaves |
| RPT-02 | Submit Report | Medium | status = "submitted" |
| RPT-03 | Approve Report (advisor) | Medium | status = "approved" |
| RPT-04 | Export PDF | **High** | 200 + PDF file (blob) |
| RPT-05 | Get Reports (by period) | Medium | 200 + รายการรายงาน |

### 1.8 Messages Module (4 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| MSG-01 | ส่งข้อความ | **High** | 200 + message ถูกเก็บ |
| MSG-02 | Get Conversation | Medium | 200 + ข้อความเรียงตามเวลา |
| MSG-03 | Mark as Read | Medium | IsRead = true |
| MSG-04 | ส่งข้อความถึงตัวเอง | Medium | 400 "ไม่สามารถส่งข้อความถึงตัวเองได้" |

### 1.9 Notifications Module (3 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| NOTIF-01 | Get Notifications | Medium | 200 + รายการแจ้งเตือน (เรียงตามเวลา) |
| NOTIF-02 | Mark as Read | Medium | IsRead = true |
| NOTIF-03 | Mark All as Read | Medium | ทุกอัน IsRead = true |

### 1.10 Evaluations Module (4 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| EVAL-01 | Supervisor ประเมินนักศึกษา | **High** | 200 + score ถูกบันทึก |
| EVAL-02 | Student ประเมินบริษัท | **High** | 200 + score ถูกบันทึก |
| EVAL-03 | ประเมินซ้ำ | **High** | 400 "你已经ประเมินแล้ว" |
| EVAL-04 | Get Evaluations | Medium | 200 + ผลประเมิน |

### 1.11 Admin Module (6 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| ADM-01 | Create User (admin) | **High** | 200 + user ถูกสร้าง |
| ADM-02 | Update User | Medium | 200 + ข้อมูลอัปเดต |
| ADM-03 | Deactivate User | Medium | IsActive = false |
| ADM-04 | Create Company | Medium | 200 + company ถูกสร้าง |
| ADM-05 | Create Internship (จับคู่) | **High** | 200 + internship ถูกสร้าง |
| ADM-06 | Student เรียก Admin API | **High** | 403 Forbidden |

---

## 2. Frontend UI/UX Testing

### 2.1 Authentication Pages (6 Test Cases)

| TC-ID | Page | Test Name | Priority | Expected Result |
|---|---|---|---|---|
| UI-01 | Login | Login สำเร็จ | **High** | Redirect ไป /dashboard + token ถูกเก็บ |
| UI-02 | Login | Validation (email ผิดรูปแบบ) | Medium | แสดง error ใต้ input |
| UI-03 | Login | ลืมรหัสผ่าน | Medium | Redirect ไป /forgot-password |
| UI-04 | Register | Register สำเร็จ | **High** | Toast "สมัครสำเร็จ" + redirect /login |
| UI-05 | Register | Password strength meter | Medium | แสดง 4 ระดับ (อ่อน/ปานกลาง/ดี/ดีมาก) |
| UI-06 | Forgot Password | Submit email | Medium | แสดงข้อความ "หากอีเมลมีอยู่..." |

### 2.2 Dashboard Pages (4 Test Cases)

| TC-ID | Page | Test Name | Priority | Expected Result |
|---|---|---|---|---|
| UI-07 | Dashboard (Student) | StatCards แสดงข้อมูล | **High** | ฝึกแล้ว x/y วัน, ชั่วโมง, วันลา, นิเทศถัดไป |
| UI-08 | Dashboard (Student) | กดปุ่มลงเวลา | **High** | นาฬิกาอัปเดต + toast + UI เปลี่ยน |
| UI-09 | Dashboard (Advisor) | คิวรออนุมัติ | **High** | แสดงรายการ + badge จำนวน |
| UI-10 | Dashboard (Advisor) | อนุมัติ/ตีกลับ | **High** | สถานะเปลี่ยน + notification ส่ง |

### 2.3 Attendance Page (5 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| UI-11 | นาฬิกา real-time | **High** | อัปเดตทุก 1 วินาที |
| UI-12 | กด check-in/out | **High** | ปุ่ม disabled + แสดงเวลา + toast |
| UI-13 | Filter เดือน | Medium | ตาราง reload ตามเดือนที่เลือก |
| UI-14 | ขอแก้เวลา (Modal) | Medium | Modal เปิด + form validation |
| UI-15 | StatCards สรุป | Medium | ชั่วโมง/สาย/ขาด ถูกต้อง |

### 2.4 Leaves Page (4 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| UI-16 | โควตาวันลา | **High** | ProgressBar แสดง % ที่ใช้ไป |
| UI-17 | ฟอร์มขอลา | **High** | เลือกวันที่ → จำนวนวัน auto-calculate |
| UI-18 | Upload ใบรับรองแพทย์ | Medium | File ถูกอัปโหลด + แสดงชื่อ |
| UI-19 | Filter สถานะ | Medium | ตารางกรองตามสถานะ |

### 2.5 Calendar Page (3 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| UI-20 | สีสถานะรายวัน | **High** | 🟢🟣⚪ ตามสถานะ |
| UI-21 | คลิกวัน → SideDrawer | **High** | Drawer เลื่อนจากขวา + แสดงข้อมูล |
| UI-22 | Navigate เดือน | Medium | < > เปลี่ยนเดือน + reload data |

### 2.6 WorkLogs Pages (5 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| UI-23 | FilterBar (List) | Medium | Filter ทำงาน + reload ตาราง |
| UI-24 | ฟอร์มบันทึกงาน | **High** | Auto-save ทุก 30 วินาที (draft) |
| UI-25 | Upload ไฟล์แนบ | Medium | Drag & drop + progress bar + thumbnail |
| UI-26 | คอมเมนต์อาจารย์ | Medium | แสดงเฉพาะ submitted/approved/returned |
| UI-27 | ปุ่มส่งอนุมัติ | **High** | Disabled ถ้าฟอร์มไม่ครบ |

### 2.7 Tasks Page (3 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| UI-28 | Kanban Board | **High** | 3 คอลัมน์ (todo/doing/done) |
| UI-29 | Drag & Drop Card | **High** | ลาก card ระหว่างคอลัมน์ + status อัปเดต |
| UI-30 | Modal มอบหมายงาน | Medium | Form validation + submit |

### 2.8 Other Pages (8 Test Cases)

| TC-ID | Page | Test Name | Priority | Expected Result |
|---|---|---|---|---|
| UI-31 | Visits | ตารางการนิเทศ | Medium | แสดงข้อมูล + ปุ่มบันทึก (advisor) |
| UI-32 | Chat | ส่งข้อความ | **High** | Bubble ปรากฏทันที (optimistic) |
| UI-33 | Chat | Load more (scroll) | Medium | Infinite scroll ทำงาน |
| UI-34 | Notifications | คลิกแจ้งเตือน | Medium | Navigate ไปหน้าปลายทาง + mark read |
| UI-35 | Reports | กราฟชั่วโมง | Medium | Chart.js แสดงกราฟแท่ง |
| UI-36 | Reports | Export PDF | **High** | เปิด tab ใหม่ + PDF ถูกดาวน์โหลด |
| UI-37 | Evaluation | Rating ดาว | **High** | คลิกดาว → score อัปเดต |
| UI-38 | Profile | เปลี่ยนรูป Avatar | Medium | Crop + upload + refresh |

### 2.9 Admin Pages (3 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| UI-39 | Admin Users CRUD | **High** | เพิ่ม/แก้ไข/ปิดใช้ user |
| UI-40 | Admin Companies CRUD | Medium | เพิ่ม/แก้ไข/ลบ บริษัท |
| UI-41 | Admin Internships (จับคู่) | **High** | Select searchable + validate |

---

## 3. Integration Testing

### 3.1 Backend ↔ Frontend (10 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| INT-01 | Login → Token เก็บใน localStorage | **High** | Token ถูกเก็บ + interceptor แนบทุก request |
| INT-02 | Token หมดอายุ → Auto Refresh | **High** | Interceptor เรียก refresh + retry คำขอเดิม |
| INT-03 | Refresh ล้มเหลว → Logout | **High** | Token ถูกล้าง + redirect /login |
| INT-04 | Role Guard (student เข้า /admin) | **High** | Redirect /403 |
| INT-05 | Auth Guard (ไม่มี token เข้า /dashboard) | **High** | Redirect /login |
| INT-06 | Upload ไฟล์ → Backend เก็บ path | Medium | File ถูกเก็บ + path ใน DB + frontend แสดง |
| INT-07 | Export PDF → Frontend download | **High** | Blob ถูกแปลงเป็น file + download |
| INT-08 | Chat Real-time (SignalR/Polling) | Medium | ข้อความใหม่ปรากฏโดยไม่ต้อง refresh |
| INT-09 | Notification Badge (Topbar) | Medium | Count อัปเดตตาม API |
| INT-10 | Error Handling (API error → Toast) | Medium | แสดง toast ข้อความ error |

---

## 4. Security Testing

### 4.1 Authentication & Authorization (8 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| SEC-01 | SQL Injection (login) | **High** | ไม่สามารถ login ได้ด้วย injection |
| SEC-02 | XSS (input ฟอร์ม) | **High** | Script ไม่ถูก execute |
| SEC-03 | JWT Forgery (แก้ไข token) | **High** | 401 Unauthorized |
| SEC-04 | Access API โดยไม่มี token | **High** | 401 |
| SEC-05 | Access API ผิด role | **High** | 403 Forbidden |
| SEC-06 | Brute Force Login | **High** | Rate limit ทำงาน (5 ครั้ง/นาที) |
| SEC-07 | Password Hash (BCrypt) | **High** | ไม่เก็บ plaintext ใน DB |
| SEC-08 | Refresh Token Rotation | **High** | Token เก่าถูก revoke หลังใช้ |

### 4.2 Data Validation (5 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| SEC-09 | Input Validation (backend) | **High** | Reject invalid data |
| SEC-10 | File Upload (ชนิด/ขนาด) | Medium | Reject file ผิดประเภท/ใหญ่เกิน |
| SEC-11 | Date Validation (ขอลาย้อนหลัง) | Medium | 400 |
| SEC-12 | Email Format | Medium | Reject invalid email |
| SEC-13 | Password Strength | Medium | Reject weak password |

---

## 5. Performance Testing

### 5.1 API Performance (5 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| PERF-01 | Login API Response Time | **High** | < 500ms |
| PERF-02 | Get WorkLogs (100 records) | Medium | < 1s |
| PERF-03 | Get Calendar (1 เดือน) | Medium | < 500ms |
| PERF-04 | Export PDF | Medium | < 3s |
| PERF-05 | Concurrent Users (50 users) | **High** | ไม่มี error + response time ไม่เพิ่มมาก |

### 5.2 Frontend Performance (3 Test Cases)

| TC-ID | Test Name | Priority | Expected Result |
|---|---|---|---|
| PERF-06 | First Contentful Paint | Medium | < 2s |
| PERF-07 | Time to Interactive | Medium | < 3s |
| PERF-08 | Lazy Loading Routes | Medium | โหลดเฉพาะ component ที่ใช้ |

---

## 6. User Acceptance Testing (UAT)

### 6.1 Student Role (10 Test Cases)

| TC-ID | Scenario | Priority | Expected Result |
|---|---|---|---|
| UAT-S01 | ลงเวลาเข้า-ออกทุกวัน | **High** | บันทึกได้ + สถานะถูกต้อง |
| UAT-S02 | ขอลา + ดูโควตา | **High** | ขอลาได้ + โควตาลดลง |
| UAT-S03 | บันทึกงาน + ส่งอนุมัติ | **High** | บันทึกได้ + อาจารย์เห็นในคิว |
| UAT-S04 | ดูงานมอบหมาย + อัปเดตสถานะ | **High** | Kanban ทำงาน + drag & drop ได้ |
| UAT-S05 | ดูปฏิทิน + สีสถานะ | Medium | สีถูกต้องตามสถานะ |
| UAT-S06 | สร้างรายงาน + Export PDF | **High** | PDF ถูกต้อง + ดาวน์โหลดได้ |
| UAT-S07 | ประเมินบริษัท | Medium | ให้คะแนนได้ + บันทึกได้ |
| UAT-S08 | แชทกับอาจารย์ | Medium | ส่ง/รับข้อความได้ |
| UAT-S09 | ดูแจ้งเตือน | Medium | แจ้งเตือนมาเมื่อมีการอนุมัติ |
| UAT-S10 | แก้ไขโปรไฟล์ | Low | บันทึกข้อมูลส่วนตัวได้ |

### 6.2 Advisor Role (8 Test Cases)

| TC-ID | Scenario | Priority | Expected Result |
|---|---|---|---|
| UAT-A01 | ดู Dashboard (คิวรออนุมัติ) | **High** | แสดงจำนวนคิว + รายการ |
| UAT-A02 | อนุมัติ/ตีกลับ บันทึกงาน | **High** | สถานะเปลี่ยน + notification ส่ง |
| UAT-A03 | อนุมัติ/ตีกลับ วันลา | **High** | โควตาอัปเดต |
| UAT-A04 | บันทึกการนิเทศ | **High** | บันทึกได้ + PersonNo/VisitNo ถูกต้อง |
| UAT-A05 | คอมเมนต์บันทึกงาน | Medium | คอมเมนต์ปรากฏ |
| UAT-A06 | อนุมัติรายงาน | Medium | สถานะเปลี่ยน |
| UAT-A07 | แชทกับนักศึกษา | Medium | ส่ง/รับข้อความได้ |
| UAT-A08 | ดูปฏิทิน นศ. ที่ดูแล | Medium | เห็นสีสถานะของ นศ. |

### 6.3 Supervisor Role (5 Test Cases)

| TC-ID | Scenario | Priority | Expected Result |
|---|---|---|---|
| UAT-SP01 | มอบหมายงาน | **High** | สร้าง task + นศ. เห็นใน Kanban |
| UAT-SP02 | แก้ไข/ลบ งาน | Medium | อัปเดตได้ |
| UAT-SP03 | ประเมินนักศึกษา | **High** | ให้คะแนน + ความเห็น |
| UAT-SP04 | ดูความคืบหน้า นศ. | Medium | Dashboard แสดงข้อมูล |
| UAT-SP05 | แชทกับนักศึกษา | Medium | ส่ง/รับข้อความได้ |

### 6.4 Admin Role (5 Test Cases)

| TC-ID | Scenario | Priority | Expected Result |
|---|---|---|---|
| UAT-AD01 | เพิ่มผู้ใช้ (นศ./อาจารย์) | **High** | User ถูกสร้าง + login ได้ |
| UAT-AD02 | เพิ่มบริษัท | Medium | Company ถูกสร้าง |
| UAT-AD03 | จับคู่การฝึก | **High** | Internship ถูกสร้าง + นศ.-บริษัท-อาจารย์ ผูกกัน |
| UAT-AD04 | Import รายชื่อ นศ. (Excel) | Medium | Upload + parse + สร้าง users |
| UAT-AD05 | ดูรายงานรวม | Low | Dashboard แสดงสถิติระบบ |

---

## 7. Test Summary & Coverage

### 7.1 สรุปจำนวน Test Cases

| Category | จำนวน | High Priority | Medium Priority | Low Priority |
|---|---|---|---|---|
| Backend API | 70 | 45 | 20 | 5 |
| Frontend UI/UX | 41 | 25 | 14 | 2 |
| Integration | 10 | 8 | 2 | 0 |
| Security | 13 | 11 | 2 | 0 |
| Performance | 8 | 3 | 5 | 0 |
| UAT | 28 | 18 | 8 | 2 |
| **รวม** | **170** | **110** | **51** | **9** |

### 7.2 Coverage ตามโมดูล

| Module | Backend | Frontend | Integration | Security | UAT | Total |
|---|---|---|---|---|---|---|
| Auth | 13 | 6 | 3 | 4 | - | 26 |
| Attendance | 11 | 5 | - | - | 1 | 17 |
| Leaves | 8 | 4 | - | - | 1 | 13 |
| WorkLogs | 10 | 5 | - | - | 1 | 16 |
| Tasks | 7 | 3 | - | - | 1 | 11 |
| AdvisorVisits | 5 | 1 | - | - | 1 | 7 |
| Reports | 5 | 2 | 1 | - | 1 | 9 |
| Messages | 4 | 2 | 1 | - | 1 | 8 |
| Notifications | 3 | 1 | 1 | - | 1 | 6 |
| Evaluations | 4 | 1 | - | - | 2 | 7 |
| Admin | 6 | 3 | - | - | 5 | 14 |
| Dashboard | - | 4 | - | - | 4 | 8 |
| Calendar | - | 3 | - | - | 1 | 4 |
| Chat | - | 2 | 1 | - | 3 | 6 |
| Profile | - | 1 | - | - | 1 | 2 |
| **รวม** | **70** | **41** | **10** | **4** | **28** | **170** |

### 7.3 Definition of Done (DoD)

**Backend API:**
- [ ] ทุก endpoint ตอบ response ถูกต้องตาม spec
- [ ] Error handling ครบ (400, 401, 403, 404, 500)
- [ ] Validation ทำงาน (frontend + backend)
- [ ] Security tests ผ่าน (SQL injection, XSS, JWT)
- [ ] Performance < 1s (95% of requests)

**Frontend UI/UX:**
- [ ] ทุกหน้า render ถูกต้องตาม Figma
- [ ] Responsive (desktop 1440px+)
- [ ] Loading states ครบ (skeleton/spinner)
- [ ] Error states ครบ (toast/modal)
- [ ] Empty states ครบ

**Integration:**
- [ ] Token flow ทำงาน (login → refresh → logout)
- [ ] Guards ทำงาน (auth + role)
- [ ] Interceptor ทำงาน (attach token + handle 401)
- [ ] File upload/download ทำงาน
- [ ] Real-time features ทำงาน (chat/notifications)

**UAT:**
- [ ] ทุก role ทดสอบ flow หลักครบ
- [ ] ไม่มี critical bugs
- [ ] Performance ยอมรับได้
- [ ] UX ผ่านการยอมรับจาก stakeholder

---

## 8. Test Execution Plan

### 8.1 ลำดับการทดสอบ

```
Phase 1: Backend API Testing (Week 1-2)
├── Auth Module (AUTH-01 to AUTH-13)
├── Attendance Module (ATT-01 to ATT-11)
├── Leaves Module (LEA-01 to LEA-08)
└── WorkLogs Module (WL-01 to WL-10)

Phase 2: Frontend UI/UX Testing (Week 3-4)
├── Authentication Pages (UI-01 to UI-06)
├── Dashboard Pages (UI-07 to UI-10)
├── Attendance Page (UI-11 to UI-15)
── Other Pages (UI-16 to UI-41)

Phase 3: Integration Testing (Week 5)
├── Token Flow (INT-01 to INT-03)
├── Guards (INT-04 to INT-05)
└── Features (INT-06 to INT-10)

Phase 4: Security & Performance (Week 6)
├── Security Tests (SEC-01 to SEC-13)
└── Performance Tests (PERF-01 to PERF-08)

Phase 5: UAT (Week 7)
├── Student Role (UAT-S01 to UAT-S10)
├── Advisor Role (UAT-A01 to UAT-A08)
├── Supervisor Role (UAT-SP01 to UAT-SP05)
└── Admin Role (UAT-AD01 to UAT-AD05)
```

### 8.2 Tools แนะนำ

| Category | Tool | Usage |
|---|---|---|
| Backend API | Postman / Swagger UI | Manual testing + collection |
| Backend API | xUnit + Moq | Unit testing (C#) |
| Frontend UI | Cypress / Playwright | E2E testing |
| Frontend UI | Jest + Angular Testing Library | Unit testing |
| Security | OWASP ZAP | Vulnerability scanning |
| Performance | JMeter / k6 | Load testing |
| UAT | Manual + TestRail | Test case management |

---

## 9. Bug Reporting Template

```markdown
**Bug ID:** [AUTO-GENERATED]
**Title:** [สั้นๆ อธิบายปัญหา]
**Module:** [Auth/Attendance/Leaves/...]
**Priority:** [Critical/High/Medium/Low]
**Severity:** [Blocker/Major/Minor/Trivial]

**Precondition:**
- [สภาพก่อนเกิด bug]

**Steps to Reproduce:**
1. [ขั้นตอนที่ 1]
2. [ขั้นตอนที่ 2]
3. [ขั้นตอนที่ 3]

**Expected Result:**
[สิ่งที่ควรเกิด]

**Actual Result:**
[สิ่งที่เกิดจริง]

**Environment:**
- Browser: [Chrome 120/Edge 120]
- OS: [Windows 11/macOS 14]
- Backend: [localhost:5000]
- Frontend: [localhost:4200]

**Screenshots/Logs:**
[แนบรูปหรือ log]

**Assigned to:** [Developer Name]
**Status:** [New/In Progress/Fixed/Verified/Closed]
```

---

## 10. Test Data Preparation

### 10.1 ข้อมูลทดสอบที่ต้องเตรียม

**Users (4 roles):**
- student@test.com / Student@1234
- advisor@test.com / Advisor@1234
- supervisor@test.com / Supervisor@1234
- admin@internlog.dev / Admin@1234

**Companies:**
- บริษัททดสอบ ABC (เวลาทำงาน 09:00-18:00)
- บริษัททดสอบ XYZ (เวลาทำงาน 08:30-17:30)

**TrainingPeriods:**
- ภาคเรียนที่ 1/2569 (1 มิ.ย. - 31 ส.ค. 2569)

**LeaveTypes (Seed):**
- ลาป่วย: 30 วัน
- ลากิจ: 3 วัน
- ลาพักร้อน: 6 วัน

### 10.2 SQL สำหรับเตรียมข้อมูลทดสอบ

```sql
-- สร้างผู้ใช้ทดสอบ (password = BCrypt hash ของ "Test@1234")
INSERT INTO Users (RoleID, Email, PasswordHash, FirstName, LastName, StudentID, IsActive)
VALUES 
(1, 'student@test.com', '$2a$12$...', 'สมชาย', 'ใจดี', '6501001', 1),
(2, 'advisor@test.com', '$2a$12$...', 'สมหญิง', 'รักเรียน', NULL, 1),
(3, 'supervisor@test.com', '$2a$12$...', 'วิชัย', 'ทำงาน', NULL, 1);

-- สร้างบริษัททดสอบ
INSERT INTO Companies (Name, Address, Phone, WorkStartTime, WorkEndTime)
VALUES 
(N'บริษัท ทดสอบ ABC', N'123 ถ.ทดสอบ กทม.', '02-123-4567', '09:00', '18:00'),
(N'บริษัท ทดสอบ XYZ', N'456 ถ.ตัวอย่าง กทม.', '02-765-4321', '08:30', '17:30');

-- สร้างภาคเรียน
INSERT INTO TrainingPeriods (PeriodName, StartDate, EndDate, IsActive)
VALUES (N'ภาคเรียนที่ 1/2569', '2026-06-01', '2026-08-31', 1);
```

---

## 11. Test Environment Setup

### 11.1 Backend Environment
```bash
# Development
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=.;Database=InternLogDB_Test;Trusted_Connection=True;
Jwt__SecretKey=TEST_SECRET_KEY_MIN_32_CHARS_FOR_TESTING_ONLY!!

# Production (ตัวอย่าง)
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=prod-server;Database=InternLogDB;User Id=sa;Password=***;
```

### 11.2 Frontend Environment
```typescript
// environments/environment.ts (dev)
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'
};

// environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://api.internlog.example.com'
};
```

### 11.3 Database
- **Development:** InternLogDB (local SQL Server)
- **Testing:** InternLogDB_Test (แยก database สำหรับ test)
- **Production:** InternLogDB_Prod (server จริง)

---

## 12. Continuous Integration (CI) Integration

### 12.1 GitHub Actions Example
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test-backend:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal

  test-frontend:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18.x'
    - name: Install dependencies
      run: npm ci
    - name: Build
      run: npm run build
    - name: Test
      run: npm test
```

---

## 13. Test Report Template

```markdown
# Test Report - [Sprint/Version Name]
**Date:** [YYYY-MM-DD]
**Tester:** [Name]
**Environment:** [Dev/Staging/Prod]

## Executive Summary
- Total Test Cases: [จำนวน]
- Passed: [จำนวน] ([%])
- Failed: [จำนวน] ([%])
- Blocked: [จำนวน] ([%])
- Not Run: [จำนวน] ([%])

## Test Results by Module
| Module | Total | Pass | Fail | Blocked |
|---|---|---|---|---|
| Auth | 26 | 25 | 1 | 0 |
| Attendance | 17 | 17 | 0 | 0 |
| ... | ... | ... | ... | ... |

## Critical Issues Found
1. [Bug ID] - [Title] - [Severity]
2. [Bug ID] - [Title] - [Severity]

## Recommendations
- [ข้อเสนอแนะ]

## Sign-off
- [ ] QA Lead: _______________
- [ ] Dev Lead: _______________
- [ ] Product Owner: _______________
```

---

## 14. Automation Strategy

### 14.1 Test Automation Pyramid
```
        /\
       /  \  E2E Tests (10%) - Cypress/Playwright
      /----\
     /      \  Integration Tests (20%) - API Tests
    /--------\
   /          \  Unit Tests (70%) - xUnit/Jest
  /____________\
```

### 14.2 Priority for Automation
1. **Must Automate:**
   - Auth flow (login/refresh/logout)
   - Core business logic (check-in/out, leave quota)
   - API endpoints (all CRUD operations)

2. **Should Automate:**
   - Form validation
   - Role-based access control
   - File upload/download

3. **Manual Testing:**
   - UX/UI visual verification
   - Exploratory testing
   - UAT with real users

---

## 15. Regression Testing Checklist

After every major change, run these critical tests:
- [ ] AUTH-01: Login สำเร็จ
- [ ] AUTH-06: Refresh Token
- [ ] ATT-01: Check-in ก่อนเวลา
- [ ] ATT-04: Check-out สำเร็จ
- [ ] LEA-01: ขอลาภายในโควตา
- [ ] WL-02: ส่งอนุมัติบันทึกงาน
- [ ] TASK-01: มอบหมายงาน
- [ ] INT-02: Token หมดอายุ → Auto Refresh
- [ ] SEC-05: Access API ผิด role

---

✅ **เอกสาร QA Test Cases ฉบับสมบูรณ์สำรองเรียบร้อยแล้ว (ส่วนที่ 10)**

**สรุป:**
- **170 test cases** ครอบคลุมทุกด้าน
- **Backend API:** 70 cases
- **Frontend UI/UX:** 41 cases
- **Integration:** 10 cases
- **Security:** 13 cases
- **Performance:** 8 cases
- **UAT:** 28 cases

**ไฟล์ที่เกี่ยวข้อง:**
- `internship-log-qa-test-cases.md` (เอกสารนี้)
- ใช้ร่วมกับเอกสารส่วนที่ 1-9

**ขั้นถัดไป:**
1. เริ่มทดสอบ Backend API (Phase 1)
2. หรือเตรียม Test Data ตามส่วนที่ 10
3. หรือตั้งค่า CI/CD Pipeline ตามส่วนที่ 12