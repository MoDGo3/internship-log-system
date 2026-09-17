# 🎨 ระบบจดบันทึกการฝึกงาน — เอกสารส่วนที่ 9
> รายละเอียดการออกแบบแต่ละหน้าสำหรับ Figma (Page-by-Page Design Specs)
> จัดทำเมื่อ 14 สิงหาคม 2569 | ชื่อไฟล์แนะนำ: `internship-log-figma-page-specs.md`
>
> **แผนที่เอกสารทั้งชุด**
> - ส่วนที่ 1–8: โครงสร้างระบบ, UI Design, Analysis, Review, Roadmap, Figma Checklist, Auth Fix, Modular
> - ส่วนที่ 9 `internship-log-figma-page-specs.md` → เอกสารฉบับนี้ (รายละเอียด 21 หน้า)

---

## 🎯 Design System สรุป (ใช้ร่วมกันทุกหน้า)

### Color Tokens
| Token | ค่า | การใช้งาน |
|---|---|---|
| Primary | `#5B5FC7` | ปุ่มหลัก, เมนู active, ลิงก์ |
| Primary Dark | `#464AB9` | ปุ่ม hover |
| Background | `#F5F6FA` | พื้นหลังแอป |
| Surface | `#FFFFFF` | การ์ด, โมดัล |
| Text | `#1F2328` | ตัวหนังสือหลัก |
| Text Secondary | `#6B7280` | คำอธิบาย |
| Success | `#16A34A` | อนุมัติ, ตรงเวลา |
| Warning | `#D97706` | รออนุมัติ |
| Danger | `#DC2626` | ตีกลับ, สาย |
| Leave | `#9333EA` | วันลา |
| Info | `#2563EB` | กำลังทำ |
| Border | `#E5E7EB` | เส้นขอบ |

### Typography
| Style | Font | Size | Weight |
|---|---|---|---|
| H1 | Prompt | 24px | Bold |
| H2 | Prompt | 20px | SemiBold |
| H3 | Prompt | 16px | SemiBold |
| Body | Sarabun | 14-16px | Regular |
| Caption | Sarabun | 12px | Regular |

### Spacing & Radius
- Grid: 8px base (8, 16, 24, 32, 48)
- Card radius: 12px
- Button/Input radius: 8px
- Shadow: `0 1px 3px rgba(0,0,0,0.1)`

---

## 📄 หน้าทั้งหมด (21 หน้า)

---

### 🔐 กลุ่ม Authentication (3 หน้า)

---

#### S1 — Login

**1) Layout Structure**
- Split screen 50/50
- ซ้าย: พื้น Primary (#5B5FC7) + Logo + Illustration + Tagline
- ขวา: Card ขาว (Surface) กลางจอ, กว้าง 420px
- Auto Layout: Vertical, gap 24px, padding 48px

**2) UX Flow & Interaction**
- States: Default / Loading (ปุ่มหมุน) / Error (ข้อความแดงใต้ input)
- Focus: Input ขอบเปลี่ยนเป็น Primary
- Hover: ปุ่มเปลี่ยนเป็น Primary Dark
- Tab order: Email → Password → Remember me → Submit

**3) Content & Data Mapping**
- API: `POST /api/auth/login`
- Input: Email (type=email), Password (type=password, มีปุ่ม 👁 toggle)
- Checkbox: Remember me (เก็บ refreshToken นานขึ้น)
- Links: ลืมรหัสผ่าน → /forgot-password, สมัครสมาชิก → /register
- Error message: "อีเมลหรือรหัสผ่านไม่ถูกต้อง" (ไม่บอกว่าอันไหนผิด)

**4) Design Tokens & Specs**
- Logo: 48×48px, สีขาว (บนพื้น Primary)
- Input height: 44px
- Button: Full width, height 44px, Primary
- Link: สี Primary, underline on hover

**5) Handoff Notes (Angular)**
- Component: `features/auth/login/login.component.ts` (Standalone)
- Form: Reactive Forms + Validators (email, required)
- State: Loading state ปิดปุ่ม + แสดง spinner
- Route: `/login` (guest guard)
- Assets: Logo SVG, Illustration SVG

**6) Prototype Flow**
- Submit สำเร็จ → `/dashboard`
- ลืมรหัส → `/forgot-password`
- สมัครสมาชิก → `/register`

---

#### S2 — Register

**1) Layout Structure**
- Layout เดียวกับ Login (split screen)
- ฟอร์มยาวกว่า: 5 ฟิลด์ + validation messages
- Auto Layout: Vertical, gap 16px

**2) UX Flow & Interaction**
- Real-time validation: Password strength meter (4 ระดับ: อ่อน/ปานกลาง/ดี/ดีมาก)
- Confirm password: เช็กตรงกันแบบ live
- Error: แสดงใต้ input แต่ละตัว (สีแดง)
- Success: Toast แจ้ง "สมัครสำเร็จ" → redirect /login

**3) Content & Data Mapping**
- API: `POST /api/auth/register`
- Fields: FirstName, LastName, Email, Password, ConfirmPassword, StudentID (optional), Phone (optional)
- Role: hidden = "student" (default)
- Validation: Password ≥ 8 chars + uppercase + lowercase + number + special

**4) Design Tokens & Specs**
- Password strength bar: 4 segments (Danger/Warning/Info/Success)
- Input error state: ขอบ Danger + ไอคอน ⚠ + ข้อความแดง

**5) Handoff Notes (Angular)**
- Form: Reactive Forms + Custom Validators (password strength)
- Service: `auth.service.ts` → `register()`
- UX: Debounce validation 300ms

**6) Prototype Flow**
- Submit สำเร็จ → Toast → `/login`
- Email ซ้ำ → 409 Conflict → แสดง error

---

#### S3 — Forgot Password

**1) Layout Structure**
- Layout เดียวกับ Login
- ฟอร์มสั้น: Email เพียงฟิลด์เดียว + ปุ่มส่งลิงก์
- เพิ่มข้อความอธิบายใต้ฟอร์ม

**2) UX Flow & Interaction**
- Submit → แสดงข้อความ "หากอีเมลมีอยู่ในระบบ จะได้รับลิงก์รีเซ็ต" (เสมอ ไม่บอกว่ามี email หรือไม่)
- Loading state 2 วินาที (จำลองส่ง email)
- Success state: แสดงข้อความยืนยัน + ปุ่ม "กลับสู่หน้าล็อกอิน"

**3) Content & Data Mapping**
- API: `POST /api/auth/forgot-password`
- Input: Email
- ไม่แสดง error กรณี email ไม่มี (กัน user enumeration)

**4) Design Tokens & Specs**
- Success message: สี Success, พื้นหลังเขียวอ่อน, มีไอคอน ✅
- Icon: 📧 ขนาด 64px กลางจอ (success state)

**5) Handoff Notes (Angular)**
- Component: `features/auth/forgot-password/`
- State machine: form → loading → success
- Route: `/forgot-password`

**6) Prototype Flow**
- Submit → Success message
- กลับ → `/login`

---

### 🏠 กลุ่ม Dashboard (2 หน้า)

---

#### S4 — Dashboard นักศึกษา

**1) Layout Structure**
- Grid 3 คอลัมน์ (12-column grid)
- แถว 1: StatCards 4 ใบ (แต่ละใบกว้าง 3 cols)
- แถว 2: การ์ดลงเวลา (8 cols) + การ์ดแจ้งเตือน (4 cols)
- แถว 3: ตารางบันทึกล่าสุด (full width)
- Gap: 24px ระหว่างการ์ด

**2) UX Flow & Interaction**
- StatCards: Hover ยกขึ้นเล็กน้อย + shadow เพิ่ม
- ปุ่มลงเวลา: ใหญ่ 160×56px, กดแล้วเปลี่ยนสถานะทันที
- นาฬิกา: อัปเดตทุก 1 วินาที (real-time)
- ตาราง: คลิกแถว → ไปหน้าบันทึกนั้น

**3) Content & Data Mapping**
- APIs:
  - `GET /api/dashboard` (ข้อมูลรวม)
  - `POST /api/attendance/check-in` / `check-out`
  - `GET /api/notifications` (5 รายการล่าสุด)
  - `GET /api/worklogs?limit=5`
- StatCards: ฝึกแล้ว x/y วัน, ชั่วโมงสะสม, วันลาคงเหลือ, นิเทศถัดไป

**4) Design Tokens & Specs**
- StatCard: ไอคอน 32px (สี Primary), ตัวเลข 32px Bold, คำอธิบาย 14px Secondary
- ปุ่มลงเวลา: Primary (เข้า) / Secondary (ออก), icon + text
- นาฬิกา: Prompt 48px Bold, สี Text

**5) Handoff Notes (Angular)**
- Component: `features/dashboard/dashboard-student/`
- Services: `dashboard.service.ts`, `attendance.service.ts`, `notification.service.ts`
- State: ใช้ Signal หรือ BehaviorSubject สำหรับนาฬิกา
- Lazy load: `loadComponent` ใน routes

**6) Prototype Flow**
- คลิก StatCard "นิเทศถัดไป" → `/visits`
- คลิกแจ้งเตือน → หน้าปลายทาง (worklog/leave/task)
- ปุ่มลงเวลา → อัปเดตการ์ดทันที (optimistic update)

---

#### S5 — Dashboard อาจารย์

**1) Layout Structure**
- เหมือน S4 แต่เนื้อหาเปลี่ยน
- แถว 1: StatCards (นศ.ที่ดูแล, คิวรอ, อนุมัติสัปดาห์นี้, ข้อความยังไม่อ่าน)
- แถว 2: ตารางคิวรออนุมัติ (8 cols) + การ์ด นศ. แต่ละคน (4 cols, scroll ได้)
- คิวรออนุมัติ: มี badge สีแดงถ้าจำนวน > 0

**2) UX Flow & Interaction**
- ตารางคิว: ปุ่ม ✅ อนุมัติ / ↩ ตีกลับ ในแต่ละแถว (inline actions)
- คลิกตีกลับ → เปิด Modal ใส่เหตุผล
- การ์ด นศ.: คลิก → ไปหน้า dashboard ของ นศ. คนนั้น (advisor view)

**3) Content & Data Mapping**
- APIs:
  - `GET /api/dashboard` (advisor version)
  - `GET /api/worklogs/pending`
  - `GET /api/leaves/pending`
  - `GET /api/reports/pending`
  - `PUT /api/worklogs/{id}/review`
  - `PUT /api/leaves/{id}/review`

**4) Design Tokens & Specs**
- Badge คิวรอ: Danger สี, ตัวเลขขาว, radius 12px (pill shape)
- ปุ่มอนุมัติ: Success (ghost), ปุ่มตีกลับ: Warning (ghost)
- Avatar นศ.: 32×32px, วงกลม

**5) Handoff Notes (Angular)**
- Component: `features/dashboard/dashboard-advisor/`
- ใช้ `roleGuard(['advisor'])`
- Table: มี pagination ถ้าคิวเยอะ (> 10)

**6) Prototype Flow**
- อนุมัติ/ตีกลับ → refresh ตาราง + ส่ง notification
- คลิก นศ. → `/students/{id}` (ถ้ามี) หรือเปิด drawer

---

### ⏰ กลุ่ม Features หลัก (13 หน้า)

---

#### S6 — ลงเวลาปฏิบัติงาน

**1) Layout Structure**
- แถว 1: การ์ดใหญ่ (full width) — นาฬิกา + ปุ่ม
- แถว 2: StatCards 3 ใบ (ชั่วโมง/สาย/ขาด)
- แถว 3: ตารางประวัติรายเดือน + FilterBar
- Gap: 24px

**2) UX Flow & Interaction**
- นาฬิกา: ตัวเลขใหญ่ 64px, อัปเดตทุกวิ
- ปุ่ม check-in/out: ใหญ่ 200×64px, disabled ถ้าลงแล้ววันนี้
- กดปุ่ม → Toast "ลงเวลาเข้าสำเร็จ" + อัปเดต UI ทันที
- ตาราง: Hover row → highlight, คลิก "ขอแก้เวลา" → Modal
- Filter เดือน: เปลี่ยน → reload ตาราง

**3) Content & Data Mapping**
- APIs:
  - `POST /api/attendance/check-in`
  - `POST /api/attendance/check-out`
  - `GET /api/attendance?year=&month=`
  - `GET /api/attendance/summary?year=&month=`
  - `PUT /api/attendance/{id}/request-edit`
- ตาราง columns: วันที่ | เวลามา | เวลากลับ | รวม ชม. | สถานะ (badge) | Actions

**4) Design Tokens & Specs**
- นาฬิกา: Prompt 64px Bold, สี Primary
- ปุ่มใหญ่: radius 12px, shadow ใหญ่
- Status badge: ontime=Success, late=Danger, leave=Leave, absent=Text Secondary
- Modal ขอแก้: กว้าง 480px, มี DatePicker + TimePicker + Textarea

**5) Handoff Notes (Angular)**
- Component: `features/attendance/attendance-page/`
- Modal: `features/attendance/request-edit-modal/`
- Service: `attendance.service.ts`
- ใช้ `date-fns` จัดการเดือน/ปี
- Optimistic update: กดปุ่ม → อัปเดต UI ก่อน API ตอบ

**6) Prototype Flow**
- กด check-in → ปุ่มเปลี่ยนเป็น disabled + แสดงเวลา
- กดขอแก้ → Modal → ส่ง → badge เปลี่ยนเป็น 
- Filter เดือน → ตาราง reload

---

#### S7 — วันลา

**1) Layout Structure**
- 2 คอลัมน์ (8:4)
- ซ้าย (8 cols): โควตาวันลา 3 การ์ด (แนวตั้ง)
- ขวา (4 cols): ฟอร์มขอลา
- ล่าง (full width): ตารางประวัติคำขอ

**2) UX Flow & Interaction**
- โควตาการ์ด: ProgressBar แสดง % ที่ใช้ไป, สีเปลี่ยนตาม % (เขียว→เหลือง→แดง)
- ฟอร์ม: เลือกประเภท → แสดงโควตาคงเหลือ, เลือกวันที่ → คำนวณจำนวนวันอัตโนมัติ
- Submit → Toast + reload ตาราง
- ตาราง: Filter สถานะ (tabs: ทั้งหมด/รอ/อนุมัติ/ไม่อนุมัติ)

**3) Content & Data Mapping**
- APIs:
  - `GET /api/leaves/quota`
  - `POST /api/leaves`
  - `GET /api/leaves?status=`
- Fields ฟอร์ม: LeaveType, StartDate, EndDate, TotalDays (auto), Reason, Attachment (optional)

**4) Design Tokens & Specs**
- โควตาการ์ด: ไอคอนใหญ่ 40px, ตัวเลข 24px Bold, ProgressBar สูง 8px
- ฟอร์ม: Input height 44px, Textarea สูง 120px
- File upload: dashed border, ไอคอน 📎, drag & drop zone

**5) Handoff Notes (Angular)**
- Component: `features/leaves/leaves-page/`
- Form: Reactive Forms + async validation (เช็กโควตาก่อน submit)
- File upload: `FormData` + `HttpClient`
- ProgressBar: custom component หรือ Angular Material

**6) Prototype Flow**
- เลือกประเภท → โควตาอัปเดต
- เลือกวันที่ → จำนวนวันคำนวณ
- Submit → Toast → reload ตาราง

---

#### S8 — ปฏิทิน

**1) Layout Structure**
- Header: ปุ่ม [< เดือนนี้ >] + Toggle [เดือน|สัปดาห์] + Legend สี
- Grid: 7 cols × 5-6 rows (อา.-ส.)
- คลิกวัน → SideDrawer เลื่อนจากขวา (กว้าง 400px)

**2) UX Flow & Interaction**
- Cell ปฏิทิน: สูง 120px, แสดงเลขวันที่ + สีพื้นตาม status + ไอคอนเล็ก
- Hover cell: tooltip สรุปวันนั้น (ลงเวลา/บันทึก/ลา)
- Click cell: เปิด SideDrawer
- SideDrawer: มี 2 section (ลงเวลา + บันทึกงาน) + ปุ่ม [+ เพิ่มบันทึก]
- Navigate เดือน: ปุ่ม < > เปลี่ยนเดือน

**3) Content & Data Mapping**
- APIs:
  - `GET /api/calendar?year=&month=` (ข้อมูลสีสถานะรายวัน)
  - `GET /api/attendance?date=` (ใน SideDrawer)
  - `GET /api/worklogs?date=` (ใน SideDrawer)
- สี cell: 🟢 approved, 🟡 pending, 🔴 absent, 🟣 leave, ⚪ holiday

**4) Design Tokens & Specs**
- Cell: border 1px Border, padding 8px
- Date number: 14px Bold, มุมบนซ้าย
- Color dot: 8×8px วงกลม, มุมล่างขวา
- SideDrawer: shadow ใหญ่, overlay จาง

**5) Handoff Notes (Angular)**
- Component: `features/calendar/calendar-page/`
- SideDrawer: `@angular/cdk` หรือ custom
- ใช้ CSS Grid สำหรับปฏิทิน
- State: เก็บเดือน/ปีปัจจุบันใน signal

**6) Prototype Flow**
- คลิก < > → เปลี่ยนเดือน + reload data
- คลิก cell → SideDrawer เลื่อนเข้ามา
- คลิก [+ เพิ่มบันทึก] → `/work-logs/new?date=...`

---

#### S9 — Work Log List

**1) Layout Structure**
- Header: ชื่อหน้า + ปุ่ม [+ เพิ่มบันทึก]
- FilterBar: เดือน/ปี + สถานะ + ประเภท + Search
- Table: full width, มี pagination

**2) UX Flow & Interaction**
- Filter: เปลี่ยน → reload ตาราง (debounce 300ms สำหรับ search)
- Table row: Hover → highlight, คลิก → ไปหน้า detail
- Actions: ดู (ทุกสถานะ), แก้ไข (draft เท่านั้น), ลบ (draft เท่านั้น)
- Pagination: 10 rows/page, มี page size selector

**3) Content & Data Mapping**
- APIs:
  - `GET /api/worklogs?year=&month=&status=&category=&search=`
  - `DELETE /api/worklogs/{id}` (draft เท่านั้น)
- Columns: วันที่ | ชื่องาน | ประเภท (chip) | ชม. | สถานะ (badge) | Actions

**4) Design Tokens & Specs**
- FilterBar: พื้น Surface, border 1px, padding 16px, gap 12px
- Chip ประเภท: พื้นสีจางของ Primary, ตัวหนังสือ Primary, radius 16px
- Table header: พื้น Background, ตัวหนังสือ Secondary, uppercase 12px
- Row height: 56px

**5) Handoff Notes (Angular)**
- Component: `features/work-logs/work-log-list/`
- Table: custom หรือ Angular Material Table
- Filter: ใช้ `FormGroup` + `valueChanges` (debounce)
- Route: `/work-logs`

**6) Prototype Flow**
- คลิก [+ เพิ่มบันทึก] → `/work-logs/new`
- คลิกแถว → `/work-logs/{id}`
- Filter → reload

---

#### S10 — Work Log Form

**1) Layout Structure**
- 2 คอลัมน์ (8:4)
- ซ้าย (8 cols): ฟอร์มหลัก
- ขวา (4 cols): ไฟล์แนบ + สถานะ + คอมเมนต์
- Footer: ปุ่ม บันทึกร่าง / ส่งอนุมัติ

**2) UX Flow & Interaction**
- ฟอร์ม: Auto-save ทุก 30 วินาที (ถ้า draft)
- ไฟล์แนบ: Drag & drop + click to select, แสดง thumbnail
- คอมเมนต์: แสดงเฉพาะ submitted/approved/returned, advisor เท่านั้นเพิ่มได้
- ปุ่มส่งอนุมัติ: disabled ถ้าฟอร์มไม่ครบ

**3) Content & Data Mapping**
- APIs:
  - `GET /api/worklogs/{id}` (edit mode)
  - `POST /api/worklogs` (create)
  - `PUT /api/worklogs/{id}` (update draft)
  - `PUT /api/worklogs/{id}/submit`
  - `POST /api/worklogs/{id}/attachments`
  - `GET/POST /api/worklogs/{id}/comments`
- Fields: LogDate, Title, Category, Description, Problem, Solution, Hours (auto จาก attendance)

**4) Design Tokens & Specs**
- Input/Textarea: height 44px/120px, border 1px Border
- File drop zone: dashed 2px Primary, พื้น Primary 5%, สูง 160px
- คอมเมนต์: Avatar 32px, bubble พื้น Background, radius 12px
- ปุ่ม footer: ติดล่าง (sticky), gap 12px

**5) Handoff Notes (Angular)**
- Component: `features/work-logs/work-log-form/`
- Form: Reactive Forms + auto-save (timer)
- File upload: `FormData` + progress bar
- Route: `/work-logs/new` หรือ `/work-logs/{id}/edit`
- Guard: edit ได้เฉพาะ draft + เจ้าของเท่านั้น

**6) Prototype Flow**
- บันทึกร่าง → Toast + stay
- ส่งอนุมัติ → Toast → redirect `/work-logs`
- Upload ไฟล์ → progress bar → thumbnail ปรากฏ

---

#### S11 — Tasks (Kanban)

**1) Layout Structure**
- Header: ชื่อหน้า + ปุ่ม [+ มอบหมายงาน]
- Board: 3 คอลัมน์เท่ากัน (todo/doing/done)
- แต่ละคอลัมน์: Header (ชื่อ + จำนวน) + Card list (scroll ได้)

**2) UX Flow & Interaction**
- Card: Drag & drop ระหว่างคอลัมน์ (เปลี่ยนสถานะ)
- Card hover: ยกขึ้น + shadow
- คลิก Card: เปิด Modal ดูรายละเอียด + แก้ไข
- Filter: ตาม priority, due date
- ปุ่มมอบหมาย: เปิด Modal ฟอร์ม

**3) Content & Data Mapping**
- APIs:
  - `GET /api/tasks?status=`
  - `POST /api/tasks`
  - `PUT /api/tasks/{id}`
  - `PUT /api/tasks/{id}/status`
  - `DELETE /api/tasks/{id}`
- Card fields: Title, Priority (chip), DueDate, AssignedBy (Avatar)

**4) Design Tokens & Specs**
- Column: พื้น Background, radius 12px, padding 16px
- Card: พื้น Surface, radius 8px, padding 12px, shadow บาง
- Priority chip: low=Success, medium=Warning, high=Danger (pill shape)
- Avatar: 24×24px

**5) Handoff Notes (Angular)**
- Component: `features/tasks/tasks-kanban/`
- Drag & drop: `@angular/cdk/drag-drop`
- State: เก็บ tasks แยกตาม status ใน signals
- Route: `/tasks`

**6) Prototype Flow**
- Drag card → เปลี่ยนคอลัมน์ → API update status
- คลิก card → Modal detail
- [+ มอบหมายงาน] → Modal form → submit

---

#### S12 — การนิเทศ

**1) Layout Structure**
- Header: ชื่อหน้า + ปุ่ม [+ บันทึกการนิเทศ] (advisor เท่านั้น)
- Table: full width, มี pagination
- Modal: ฟอร์มบันทึก (กว้าง 560px)

**2) UX Flow & Interaction**
- Table: เรียงตาม VisitNo (ล่าสุดบน)
- คลิกแถว: เปิด Modal ดูรายละเอียด (view mode)
- ปุ่มบันทึก: เปิด Modal form mode
- Form: มี validation (PersonNo/VisitNo ต้องไม่ซ้ำ)

**3) Content & Data Mapping**
- APIs:
  - `GET /api/visits?internshipId=`
  - `POST /api/visits`
  - `PUT /api/visits/{id}`
- Columns: คนที่ | ครั้งที่ | วันที่ | อาจารย์ | รูปแบบ (chip) | สรุป | คะแนน | Actions

**4) Design Tokens & Specs**
- Chip รูปแบบ: Onsite=Info, Online=Warning
- Modal form: 2 คอลัมน์ (ซ้ายฟอร์ม/ขวา preview)
- คะแนน: Input number 0-100, step 0.01

**5) Handoff Notes (Angular)**
- Component: `features/visits/visits-page/`
- Modal: `features/visits/visit-form-modal/`
- Route: `/visits`
- Guard: บันทึกได้เฉพาะ advisor

**6) Prototype Flow**
- [+ บันทึก] → Modal form → submit → reload table
- คลิกแถว → Modal view mode

---

#### S13 — แชท

**1) Layout Structure**
- 2 คอลัมน์ (4:8)
- ซ้าย: รายการบทสนทนา (search + list)
- ขวา: หน้าต่างแชท (header + messages + input)
- Empty state: "เลือกบทสนทนาเพื่อเริ่มแชท" (กลางจอขวา)

**2) UX Flow & Interaction**
- List ซ้าย: Hover → highlight, คลิก → load chat ขวา
- Badge: จำนวนข้อความยังไม่อ่าน (สีแดง, วงกลม)
- Messages: Scroll auto ลงล่างสุด, load more เมื่อ scroll ขึ้น
- Input: Enter ส่ง, Shift+Enter ขึ้นบรรทัดใหม่
- Online status: จุดเขียวข้าง Avatar

**3) Content & Data Mapping**
- APIs:
  - `GET /api/messages?userId=` (list บทสนทนา)
  - `GET /api/messages/{userId}/history` (ข้อความ)
  - `POST /api/messages` (ส่ง)
- Fields: SenderID, ReceiverID, Content, IsRead, SentAt

**4) Design Tokens & Specs**
- ChatBubble sent: พื้น Primary, ตัวหนังสือขาว, radius 12px (มุมล่างขวาเหลี่ยม)
- ChatBubble received: พื้น Background, ตัวหนังสือ Text, radius 12px (มุมล่างซ้ายเหลี่ยม)
- Input area: สูง 60px, border-top 1px Border
- Avatar online: จุดเขียว 8px, ขอบขาว 2px

**5) Handoff Notes (Angular)**
- Component: `features/chat/chat-page/`
- Real-time: SignalR หรือ polling ทุก 5 วินาที
- Scroll: `ViewChild` + `scrollIntoView`
- Route: `/chat`

**6) Prototype Flow**
- คลิกบทสนทนา → load messages
- พิมพ์ + Enter → ส่ง → bubble ปรากฏทันที (optimistic)
- Scroll ขึ้น → load more (infinite scroll)

---

#### S14 — แจ้งเตือน

**1) Layout Structure**
- Header: ชื่อหน้า + ปุ่ม [อ่านทั้งหมด]
- List: vertical, แต่ละ item มีไอคอน + ข้อความ + เวลา + dot
- Empty state: "ไม่มีการแจ้งเตือน"

**2) UX Flow & Interaction**
- Item: Hover → พื้นจาง, คลิก → ไปหน้าปลายทาง + mark read
- Dot: สี Primary (ยังไม่อ่าน), หายไปเมื่ออ่านแล้ว
- ปุ่มอ่านทั้งหมด: confirm dialog
- Delete: ปุ่ม X ปรากฏเมื่อ hover

**3) Content & Data Mapping**
- APIs:
  - `GET /api/notifications`
  - `PUT /api/notifications/{id}/read`
  - `PUT /api/notifications/read-all`
- ไอคอนแยกประเภท: ✅ อนุมัติ, 📋 งานใหม่, 💬 ข้อความ, ⚙️ ระบบ

**4) Design Tokens & Specs**
- Item height: 64px, padding 16px
- Dot: 8×8px วงกลม, สี Primary
- ไอคอนประเภท: 24×24px, สีตามประเภท
- เวลา: Caption 12px, สี Secondary

**5) Handoff Notes (Angular)**
- Component: `features/notifications/notifications-page/`
- Service: `notification.service.ts`
- Route: `/notifications`
- Badge ใน Topbar: count จาก API

**6) Prototype Flow**
- คลิก item → navigate + mark read
- อ่านทั้งหมด → confirm → reload

---

#### S15 — Reports

**1) Layout Structure**
- Header: ชื่อหน้า + Select ช่วงเวลา + ปุ่ม [+ สร้างรายงาน] + [ Export PDF]
- แถว 1: StatCards 3 ใบ (ชม.รวม, บันทึก, ลา)
- แถว 2: กราฟแท่ง (ชั่วโมงตามประเภท)
- แถว 3: ตารางลงเวลา + ตารางนิเทศ (2 คอลัมน์)
- แถว 4: List รายงานที่สร้างแล้ว

**2) UX Flow & Interaction**
- Select ช่วง: เปลี่ยน → reload ทุกส่วน
- กราฟ: Hover bar → tooltip แสดงค่า
- ตาราง: คลิกแถว → เปิด report detail
- Export PDF: เปิดหน้า Preview PDF (S16)

**3) Content & Data Mapping**
- APIs:
  - `GET /api/reports?period=`
  - `POST /api/reports/generate`
  - `GET /api/reports/{id}/export-pdf`
- กราฟ: ใช้ Chart.js หรือ ngx-charts

**4) Design Tokens & Specs**
- กราฟ: สี Primary + สีจางของ Primary (5 ระดับตามประเภท)
- StatCards: เหมือน Dashboard
- ตาราง: เหมือน Work Log List

**5) Handoff Notes (Angular)**
- Component: `features/reports/reports-page/`
- Chart: `ng2-charts` (Chart.js wrapper)
- Route: `/reports`
- PDF: เปิดใน tab ใหม่ หรือ download

**6) Prototype Flow**
- เลือกช่วง → reload data
- [+ สร้างรายงาน] → generate → toast → reload list
- Export PDF → tab ใหม่

---

#### S16 — Preview PDF

**1) Layout Structure**
- Header: ชื่อรายงาน + ปุ่ม [⬇ ดาวน์โหลด] + [↩ กลับ]
- Content: A4 document preview (กว้าง 794px, กลางจอ)
- Background: Background (#F5F6FA)

**2) UX Flow & Interaction**
- Document: Scroll ได้, zoom 50-150%
- ปุ่มดาวน์โหลด: save as PDF
- ปุ่มกลับ: `/reports`

**3) Content & Data Mapping**
- API: `GET /api/reports/{id}/export-pdf` (return blob)
- เนื้อหา: Header (โลโก้+ชื่อ), Section 1-5 (ตาม spec)

**4) Design Tokens & Specs**
- Document: พื้นขาว, shadow ใหญ่, padding 48px
- Header doc: โลโก้ 48px, ชื่อ 18px Bold
- Section: H2 16px, margin-bottom 24px
- ตารางใน doc: border 1px, padding 8px

**5) Handoff Notes (Angular)**
- Component: `features/reports/report-preview/`
- PDF: ใช้ `pdf-lib` หรือ backend generate
- Route: `/reports/{id}/preview`
- Download: `blob` + `URL.createObjectURL`

**6) Prototype Flow**
- ดาวน์โหลด → save dialog
- กลับ → `/reports`

---

#### S17 — Evaluation

**1) Layout Structure**
- Header: ชื่อหน้า
- 2 การ์ดฟอร์ม (แนวตั้ง, gap 24px)
  - การ์ด 1: พี่เลี้ยง→นักศึกษา
  - การ์ด 2: นักศึกษา→บริษัท
- แต่ละการ์ด: Header + ข้อประเมิน 5-10 ข้อ + Total Score

**2) UX Flow & Interaction**
- Rating: ดาว 5 ดวง, hover แสดง preview, คลิก lock
- ข้อประเมิน: ชื่อข้อ + description + rating
- Total Score: คำนวณอัตโนมัติ, แสดงเกรด (A/B/C/D/F)
- Submit: confirm dialog → toast

**3) Content & Data Mapping**
- APIs:
  - `GET /api/evaluations?internshipId=`
  - `POST /api/evaluations`
- Fields: EvalType, Items[{question, score}], TotalScore, Comment

**4) Design Tokens & Specs**
- ดาว: 24×24px, สี Warning (เต็ม), Border (ว่าง)
- การ์ด: padding 24px
- Total Score: 48px Bold, สี Primary
- เกรด: Badge ใหญ่ (A=Success, B=Info, C=Warning, D/F=Danger)

**5) Handoff Notes (Angular)**
- Component: `features/evaluations/evaluations-page/`
- Rating: custom component (5 stars)
- Route: `/evaluations`
- Guard: แก้ไขได้เฉพาะ role ที่เกี่ยวข้อง

**6) Prototype Flow**
- คลิกดาว → อัปเดต score
- Submit → confirm → toast → redirect

---

#### S18 — Profile

**1) Layout Structure**
- 3 การ์ด (แนวตั้ง, gap 24px)
  - การ์ด 1: ข้อมูลส่วนตัว (Avatar + ฟอร์ม)
  - การ์ด 2: ข้อมูลการฝึก (บริษัท/ช่วง/อาจารย์/พี่เลี้ยง)
  - การ์ด 3: เปลี่ยนรหัสผ่าน

**2) UX Flow & Interaction**
- Avatar: คลิก → upload รูปใหม่ (crop)
- ฟอร์ม: Auto-save หรือปุ่มบันทึก
- รหัสผ่าน: validation แบบ real-time (เหมือน register)
- Success: Toast "บันทึกสำเร็จ"

**3) Content & Data Mapping**
- APIs:
  - `GET /api/auth/me`
  - `PUT /api/users/profile`
  - `PUT /api/auth/change-password`
  - `POST /api/users/avatar`

**4) Design Tokens & Specs**
- Avatar: 96×96px วงกลม, border 4px Surface
- ฟอร์ม: 2 คอลัมน์ (label ซ้าย, input ขวา)
- การ์ดข้อมูลการฝึก: Icon + label + value (แนวตั้ง)

**5) Handoff Notes (Angular)**
- Component: `features/profile/profile-page/`
- Avatar upload: crop ด้วย `ngx-image-cropper`
- Route: `/profile`

**6) Prototype Flow**
- แก้ไข → บันทึก → toast
- เปลี่ยนรูป → crop → upload → refresh

---

###  กลุ่ม Admin (3 หน้า)

---

#### S19 — Admin Users

**1) Layout Structure**
- Header: ชื่อหน้า + ปุ่ม [+ เพิ่มผู้ใช้] + [⬆ Import Excel]
- FilterBar: Search + Filter role
- Table: full width, pagination

**2) UX Flow & Interaction**
- Table: คลิกแถว → เปิด Modal แก้ไข
- ปุ่มเพิ่ม: เปิด Modal form
- Import: upload Excel → preview → confirm
- Toggle Active/Inactive: switch ในตาราง

**3) Content & Data Mapping**
- APIs:
  - `GET /api/admin/users?role=&search=`
  - `POST /api/admin/users`
  - `PUT /api/admin/users/{id}`
  - `POST /api/admin/users/import`
- Columns: Avatar+ชื่อ | Email | Role (chip) | สถานะ (switch) | Actions

**4) Design Tokens & Specs**
- Chip Role: student=Info, advisor=Success, supervisor=Warning, admin=Danger
- Switch: Primary color
- Modal form: กว้าง 560px

**5) Handoff Notes (Angular)**
- Component: `features/admin/admin-users/`
- Import: `xlsx` library parse Excel
- Route: `/admin/users`
- Guard: `roleGuard(['admin'])`

**6) Prototype Flow**
- [+ เพิ่ม] → Modal → submit → reload
- Import → upload → preview → confirm → toast

---

#### S20 — Admin Companies

**1) Layout Structure**
- Header: ชื่อหน้า + ปุ่ม [+ เพิ่มบริษัท]
- Table: full width, pagination
- Modal: ฟอร์มบริษัท (กว้าง 560px)

**2) UX Flow & Interaction**
- Table: คลิกแถว → Modal แก้ไข
- ปุ่มลบ: confirm dialog
- ฟอร์ม: มี validation (ชื่อห้ามว่าง, เวลาต้องถูกต้อง)

**3) Content & Data Mapping**
- APIs:
  - `GET /api/admin/companies`
  - `POST /api/admin/companies`
  - `PUT /api/admin/companies/{id}`
  - `DELETE /api/admin/companies/{id}`
- Columns: ชื่อ | ที่อยู่ | Phone | เวลาเข้า-ออก | จำนวน นศ. | Actions

**4) Design Tokens & Specs**
- เหมือน Admin Users
- Time input: 2 ช่อง (เข้า/ออก)

**5) Handoff Notes (Angular)**
- Component: `features/admin/admin-companies/`
- Route: `/admin/companies`

**6) Prototype Flow**
- CRUD ปกติ + confirm delete

---

#### S21 — Admin Internships

**1) Layout Structure**
- Header: ชื่อหน้า + ปุ่ม [+ จับคู่การฝึกใหม่]
- FilterBar: Search + Filter ภาคเรียน
- Table: full width, pagination
- Modal: ฟอร์มจับคู่ (กว้าง 640px, 2 คอลัมน์)

**2) UX Flow & Interaction**
- ฟอร์ม: Select searchable (type to search) สำหรับ นศ./บริษัท/อาจารย์
- Validation: นศ. 1 คนฝึกได้แค่ 1 ที่ต่อภาคเรียน
- Submit: confirm → toast

**3) Content & Data Mapping**
- APIs:
  - `GET /api/admin/internships?periodId=`
  - `POST /api/admin/internships`
  - `PUT /api/admin/internships/{id}`
  - `DELETE /api/admin/internships/{id}`
- Columns: นักศึกษา | บริษัท | อาจารย์ | พี่เลี้ยง | ภาคเรียน | ช่วงวัน | สถานะ | Actions

**4) Design Tokens & Specs**
- Select searchable: dropdown มี search input ด้านใน
- Modal 2 คอลัมน์: ซ้ายเลือก/ขวา preview ข้อมูล

**5) Handoff Notes (Angular)**
- Component: `features/admin/admin-internships/`
- Select searchable: `@ng-select/ng-select` หรือ custom
- Route: `/admin/internships`

**6) Prototype Flow**
- เลือก นศ. → โหลดข้อมูล → เลือกบริษัท → validate → submit

---

## 📦 Handoff Guide สรุป (Figma → Angular)

### Assets ที่ต้อง Export
| ประเภท | รูปแบบ | ตำแหน่งใน Angular |
|---|---|---|
| Icons | SVG | `assets/icons/` |
| Logo | SVG | `assets/` |
| Illustrations | SVG | `assets/illustrations/` |
| Avatar placeholder | SVG | `assets/` |

### Design Tokens → CSS Variables
```scss
// styles.scss
:root {
  --color-primary: #5B5FC7;
  --color-primary-dark: #464AB9;
  --color-background: #F5F6FA;
  --color-surface: #FFFFFF;
  --color-text: #1F2328;
  --color-text-secondary: #6B7280;
  --color-success: #16A34A;
  --color-warning: #D97706;
  --color-danger: #DC2626;
  --color-leave: #9333EA;
  --color-info: #2563EB;
  --color-border: #E5E7EB;
  
  --radius-card: 12px;
  --radius-button: 8px;
  
  --shadow-sm: 0 1px 3px rgba(0,0,0,0.1);
  --shadow-md: 0 4px 6px rgba(0,0,0,0.1);
  --shadow-lg: 0 10px 15px rgba(0,0,0,0.1);
}
```

### Component Mapping
| Figma Component | Angular Component |
|---|---|
| Button | `shared/button/` |
| Input | `shared/input/` |
| StatusBadge | `shared/status-badge/` |
| Card | `shared/card/` |
| Table | `shared/table/` |
| Modal | `shared/modal/` |
| Sidebar | `layout/sidebar/` |
| Topbar | `layout/topbar/` |

---

## ✅ Checklist สรุป (21 หน้า)

### Authentication (3)
- [ ] S1 Login
- [ ] S2 Register
- [ ] S3 Forgot Password

### Dashboard (2)
- [ ] S4 Dashboard นักศึกษา
- [ ] S5 Dashboard อาจารย์

### Features (13)
- [ ] S6 ลงเวลา
- [ ] S7 วันลา
- [ ] S8 ปฏิทิน
- [ ] S9 Work Log List
- [ ] S10 Work Log Form
- [ ] S11 Tasks (Kanban)
- [ ] S12 การนิเทศ
- [ ] S13 แชท
- [ ] S14 แจ้งเตือน
- [ ] S15 Reports
- [ ] S16 Preview PDF
- [ ] S17 Evaluation
- [ ] S18 Profile

### Admin (3)
- [ ] S19 Admin Users
- [ ] S20 Admin Companies
- [ ] S21 Admin Internships

**รวม: 21 หน้า + Design System + Handoff Guide**