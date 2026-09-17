# 🎨 ระบบจดบันทึกการฝึกงาน — เอกสารส่วนที่ 6
> รายการงาน Figma ที่ต้องออกแบบ (Figma Design Checklist)
> จัดทำเมื่อ 14 สิงหาคม 2569 | ชื่อไฟล์แนะนำ: `internship-log-figma-checklist.md`
>
> **แผนที่เอกสารทั้งชุด**
> - ส่วนที่ 1–5: โครงสร้างระบบ, UI Design, Analysis, Review, Roadmap
> - ส่วนที่ 6 `internship-log-figma-checklist.md` → เอกสารฉบับนี้

---

## 1. Design System (ต้องทำก่อนทุกหน้า)

### 1.1 Color Tokens (Style ใน Figma)
- [ ] Primary `#5B5FC7` + Primary Dark `#464AB9`
- [ ] Background `#F5F6FA`
- [ ] Surface `#FFFFFF`
- [ ] Text `#1F2328` + Secondary Text `#6B7280`
- [ ] Success `#16A34A` / Warning `#D97706` / Danger `#DC2626` / Leave `#9333EA` / Info `#2563EB`

### 1.2 Typography Styles
- [ ] H1: Prompt 24px Bold
- [ ] H2: Prompt 20px SemiBold
- [ ] H3: Prompt 16px SemiBold
- [ ] Body: Sarabun 14–16px Regular
- [ ] Caption: Sarabun 12px Regular

### 1.3 Spacing & Radius
- [ ] การ์ด radius 12px + shadow
- [ ] ปุ่ม/Input radius 8px
- [ ] Padding การ์ด 16–24px
- [ ] ระยะห่างระหว่าง elements 16px

---

## 2. Components (ทำครั้งเดียว ใช้ซ้ำทุกหน้า)

### 2.1 Form Controls
- [ ] **Button** (4 variants): Primary / Secondary / Ghost / Danger
  - States: Default / Hover / Disabled / Loading
- [ ] **Input** (Text, Email, Password)
  - States: Default / Focus / Error / Disabled
- [ ] **Select / Dropdown**
- [ ] **Textarea**
- [ ] **DatePicker**
- [ ] **TimePicker**
- [ ] **File Upload** (drag & drop + click to select)
- [ ] **Checkbox / Radio**
- [ ] **Toggle Switch**

### 2.2 Data Display
- [ ] **Card** (container พื้นฐาน)
- [ ] **StatCard** (ไอคอน + ตัวเลขใหญ่ + คำอธิบาย)
- [ ] **StatusBadge** (ทุกสถานะ)
  - บันทึกงาน: draft / submitted / approved / returned
  - ลงเวลา: ontime / late / leave / absent / pending_edit
  - วันลา/รายงาน: pending / approved / rejected
  - Tasks: todo / doing / done
  - Priority: low / medium / high
- [ ] **Table** (header + row + hover state + pagination)
- [ ] **ProgressBar**
- [ ] **Avatar** (ภาพโปรไฟล์ + fallback ตัวอักษร)

### 2.3 Navigation & Feedback
- [ ] **Sidebar** (เมนูหลัก + collapse state)
- [ ] **Topbar** (search + notifications + profile)
- [ ] **Tabs**
- [ ] **Modal / Dialog**
- [ ] **SideDrawer** (เลื่อนจากขวา)
- [ ] **Toast / Snackbar** (แจ้งเตือน)
- [ ] **Empty State** (ไม่มีข้อมูล)
- [ ] **Loading Spinner**

### 2.4 Special Components
- [ ] **ChatBubble** (sent / received)
- [ ] **Calendar Cell** (วัน + สี status)
- [ ] **Kanban Card** (ชื่องาน + priority + due date)
- [ ] **Kanban Column** (header + card list)

---

## 3. หน้าจอทั้งหมด (20 หน้า)

### 3.1 Authentication (3 หน้า)
**S1 — Login**
- [ ] Split screen: ซ้าย (พื้น Primary + logo + illustration) / ขวา (Card ฟอร์ม)
- [ ] ฟอร์ม: Email, Password, Remember me, ลิงก์ลืมรหัส
- [ ] ปุ่ม "เข้าสู่ระบบ" (Primary, full width)
- [ ] States: Default / Loading / Error (รหัสผิด)

**S2 — Register**
- [ ] ฟอร์ม: ชื่อ, นามสกุล, Email, Password, Confirm Password, Role (เลือก student เท่านั้น)
- [ ] Validation: password ต้องมี 8+ chars + uppercase + lowercase + number + special
- [ ] ลิงก์กลับไป Login

**S3 — Forgot Password**
- [ ] ฟอร์ม: Email
- [ ] ข้อความยืนยัน "ส่งลิงก์รีเซ็ตรหัสไปทางอีเมลแล้ว"

---

### 3.2 Dashboard (2 หน้า)
**S4 — Dashboard นักศึกษา**
- [ ] **StatCards แถวบน (4 การ์ด):**
  - 📅 ฝึกแล้ว x/y วัน (ProgressBar)
  - ⏱ ชั่วโมงสะสม
  - 🏖 วันลาคงเหลือ
  - 🧑 นัดนิเทศถัดไป
- [ ] **การ์ดซ้าย: ลงเวลาวันนี้**
  - นาฬิกา real-time
  - ปุ่มใหญ่ [ลงเวลาเข้า] [ลงเวลาออก]
  - แสดงเวลาที่ลงวันนี้
- [ ] **การ์ดขวา: แจ้งเตือนล่าสุด** (list 3-5 รายการล่าสุด)
- [ ] **ตารางด้านล่าง: บันทึกล่าสุด** (5 แถว + ลิงก์ดูทั้งหมด)

**S5 — Dashboard อาจารย์**
- [ ] **StatCards แถวบน (4 การ์ด):**
  - จำนวน นศ. ที่ดูแล
  - คิวรออนุมัติ (badge สีแดงถ้า > 0)
  - อนุมัติสัปดาห์นี้
  - ข้อความยังไม่อ่าน
- [ ] **การ์ดซ้าย: คิวรออนุมัติ** (ตาราง + ปุ่มอนุมัติ/ตีกลับ)
- [ ] **การ์ดขวา: การ์ด นศ. แต่ละคน** (Avatar + ชื่อ + ProgressBar ความคืบหน้า + ปุ่มดู)

---

### 3.3 ลงเวลา (1 หน้า)
**S6 — ลงเวลาปฏิบัติงาน**
- [ ] **การ์ดบน: นาฬิกา + ปุ่ม**
  - นาฬิกา real-time (ใหญ่, กลางการ์ด)
  - วันที่วันนี้
  - ปุ่มใหญ่ [ลงเวลาเข้า] [ลงเวลาออก]
  - แสดงเวลาที่ลงวันนี้ (ถ้าลงแล้ว)
- [ ] **StatCards กลาง (3 การ์ด):**
  - ชั่วโมงรวมเดือนนี้
  - มาสายกี่ครั้ง
  - ขาดกี่ครั้ง
- [ ] **ตารางล่าง: ประวัติรายเดือน**
  - Columns: วันที่ | เวลามา | เวลากลับ | รวม ชม. | สถานะ (badge) | Actions
  - Filter: เลือกเดือน/ปี
  - Actions: [ขอแก้เวลา] (เปิด Modal)
- [ ] **Modal: ขอแก้เวลา**
  - เลือกวันที่ + เวลาใหม่ + เหตุผล
  - ปุ่ม [ส่งคำขอ]

---

### 3.4 วันลา (1 หน้า)
**S7 — วันลา**
- [ ] **การ์ดซ้าย: โควตาวันลา (3 การ์ด)**
  - ลาป่วย: ใช้ไป x / คงเหลือ y (ProgressBar)
  - ลากิจ: ใช้ไป x / คงเหลือ y
  - ลาพักร้อน: ใช้ไป x / คงเหลือ y
- [ ] **การ์ดขวา: ฟอร์มขอลา**
  - Select: ประเภทวันลา
  - DatePicker: ตั้งแต่ – ถึง (คำนวณจำนวนวันอัตโนมัติ)
  - Textarea: เหตุผล
  - File Upload: แนบใบรับรองแพทย์ (optional)
  - ปุ่ม [ส่งคำขอลา]
- [ ] **ตารางล่าง: ประวัติคำขอ**
  - Columns: ประเภท | ตั้งแต่–ถึง | จำนวนวัน | เหตุผล | สถานะ (badge) | Actions
  - Filter: สถานะ (ทั้งหมด / รออนุมัติ / อนุมัติ / ไม่อนุมัติ)

---

### 3.5 ปฏิทิน (1 หน้า)
**S8 — ปฏิทิน**
- [ ] **Header:**
  - ปุ่ม [< เดือนนี้ >]
  - Toggle: [เดือน | สัปดาห์]
  - Legend สี (5 ช่อง): 🟢 อนุมัติ | 🟡 รออนุมัติ | 🔴 ไม่ได้บันทึก | 🟣 วันลา | ⚪ วันหยุด
- [ ] **Calendar Grid:**
  - 7 columns (อา. – ส.) × 5-6 rows
  - แต่ละ cell: เลขวันที่ + สีพื้นตาม status + ไอคอนเล็ก (ถ้ามีบันทึก)
  - Hover: แสดง tooltip สรุปวันนั้น
  - Click: เปิด SideDrawer
- [ ] **SideDrawer (เลื่อนจากขวา):**
  - Header: วันที่
  - Section 1: ข้อมูลลงเวลา (เวลามา–กลับ, สถานะ)
  - Section 2: บันทึกงานของวัน (list + ปุ่มดู)
  - ปุ่ม [+ เพิ่มบันทึกวันนี้]

---

### 3.6 บันทึกงาน (2 หน้า)
**S9 — Work Log List**
- [ ] **FilterBar:**
  - Select: เดือน/ปี
  - Select: สถานะ (ทั้งหมด / draft / submitted / approved / returned)
  - Select: ประเภทงาน
  - Search input
  - ปุ่ม [+ เพิ่มบันทึก]
- [ ] **Table:**
  - Columns: วันที่ | ชื่องาน | ประเภท (chip) | ชม. | สถานะ (badge) | Actions
  - Actions: [ดู] [แก้ไข] (draft เท่านั้น)
  - Pagination

**S10 — Work Log Form (2 คอลัมน์)**
- [ ] **คอลัมน์ซ้าย: ฟอร์ม**
  - DatePicker: วันที่
  - Input: เวลา (auto-fill จาก Attendance)
  - Input: ชื่องาน
  - Select: ประเภทงาน (Development / Meeting / Doc / Testing / อื่น ๆ)
  - Textarea: รายละเอียด
  - Textarea: ปัญหาที่พบ
  - Textarea: วิธีแก้ไข
- [ ] **คอลัมน์ขวา:**
  - **Section บน: ไฟล์แนบ**
    - Drag & drop zone
    - List ไฟล์ที่อัปโหลด (ชื่อ + ขนาด + ลบ)
  - **Section กลาง: สถานะ** (StatusBadge ใหญ่)
  - **Section ล่าง: คอมเมนต์อาจารย์**
    - List คอมเมนต์ (Avatar + ชื่อ + เวลา + ข้อความ)
    - Input เพิ่มคอมเมนต์ (ถ้าเป็น advisor)
- [ ] **ปุ่มล่าง:**
  - [บันทึกร่าง] (Secondary)
  - [ส่งอนุมัติ] (Primary) — disabled ถ้า draft

---

### 3.7 งานมอบหมาย (1 หน้า)
**S11 — Tasks (Kanban)**
- [ ] **Header:**
  - ปุ่ม [+ มอบหมายงาน] (supervisor/advisor เท่านั้น)
- [ ] **Kanban Board (3 คอลัมน์):**
  - **ยังไม่ทำ (todo)**
  - **กำลังทำ (doing)**
  - **เสร็จ (done)**
- [ ] **Kanban Card:**
  - ชื่องาน
  - Priority chip (low 🟢 / medium 🟡 / high 🔴)
  - กำหนดส่ง (date)
  - ผู้มอบหมาย (Avatar + ชื่อ)
  - Drag & drop ได้
- [ ] **Modal: มอบหมายงาน / แก้ไขงาน**
  - Input: ชื่องาน
  - Textarea: รายละเอียด
  - Select: ผู้รับงาน (student)
  - DatePicker: กำหนดส่ง
  - Select: Priority
  - ปุ่ม [บันทึก]

---

### 3.8 การนิเทศ (1 หน้า)
**S12 — การนิเทศ**
- [ ] **Table:**
  - Columns: คนที่ | ครั้งที่ | วันที่ | ชื่ออาจารย์ | รูปแบบ (Onsite/Online chip) | สรุปผล | คะแนน | Actions
  - Actions: [ดู] [แก้ไข] (advisor เท่านั้น)
- [ ] **ปุ่ม [+ บันทึกการนิเทศ]** (advisor เท่านั้น) → เปิด Modal
- [ ] **Modal: บันทึกการนิเทศ**
  - Input: คนที่ (ลำดับ นศ. ในกลุ่ม)
  - Input: ครั้งที่
  - DatePicker: วันที่
  - Select: ชื่ออาจารย์
  - Select: รูปแบบ (Onsite / Online)
  - Textarea: สรุปผลการนิเทศ
  - Input: คะแนน (optional)
  - ปุ่ม [บันทึก]

---

### 3.9 แชท (1 หน้า)
**S13 — แชท**
- [ ] **Layout 2 คอลัมน์:**
  - **ซ้าย: รายการบทสนทนา**
    - Search input
    - List: Avatar + ชื่อ + ข้อความล่าสุด + badge จำนวนยังไม่อ่าน
  - **ขวา: หน้าต่างแชท**
    - Header: Avatar + ชื่อ + สถานะ (ออนไลน์/ออฟไลน์)
    - Message list: ChatBubble (sent = ขวา สี Primary / received = ซ้าย สีเทา)
    - Footer: Input + ปุ่มส่ง
- [ ] **Empty state:** "เลือกบทสนทนาเพื่อเริ่มแชท"

---

### 3.10 แจ้งเตือน (1 หน้า)
**S14 — Notifications**
- [ ] **Header:**
  - ปุ่ม [อ่านทั้งหมด]
- [ ] **List:**
  - แต่ละ item: ไอคอน (แยกประเภท) + ข้อความ + เวลา + dot (ยังไม่อ่าน)
  - Click: ไปหน้าปลายทาง (เช่น คลิก "บันทึกถูกอนุมัติ" → ไปหน้าบันทึกนั้น)
  - ปุ่ม [X] ลบ
- [ ] **Empty state:** "ไม่มีการแจ้งเตือน"

---

### 3.11 รายงาน (2 หน้า)
**S15 — Reports**
- [ ] **Header:**
  - Select: ช่วงเวลา (สัปดาห์ที่ x / เดือน x)
  - ปุ่ม [+ สร้างรายงาน]
  - ปุ่ม [⬇ Export PDF]
- [ ] **StatCards แถวบน (3 การ์ด):**
  - ชั่วโมงรวม
  - บันทึกกี่วัน
  - วันลา
- [ ] **กราฟแท่ง: ชั่วโมงตามประเภทงาน**
- [ ] **ตาราง: ลงเวลาสัปดาห์/เดือน**
- [ ] **ตาราง: การนิเทศ**
- [ ] **List: รายการรายงานที่สร้างแล้ว**
  - Columns: สัปดาห์ที่ | ช่วงวันที่ | สถานะ (badge) | Actions [ส่งอนุมัติ] [ดู]

**S16 — Preview PDF**
- [ ] **Document Preview (A4):**
  - **Header:** โลโก้มหาวิทยาลัย + ชื่อ นศ. + รหัสนศ. + บริษัท + ช่วงวันฝึก
  - **Section 1:** สรุปชั่วโมง (ตาราง)
  - **Section 2:** ตารางบันทึกงานรายวัน
  - **Section 3:** วันลา
  - **Section 4:** การนิเทศ
  - **Footer:** ลายเซ็น (นศ. / อาจารย์ / พี่เลี้ยง)
- [ ] **ปุ่มล่าง:** [⬇ ดาวน์โหลด PDF] [↩ กลับ]

---

### 3.12 ประเมิน (1 หน้า)
**S17 — Evaluation**
- [ ] **2 การ์ดฟอร์ม (วางแนวตั้ง):**
  - **การ์ด 1: พี่เลี้ยงประเมินนักศึกษา** (supervisor เท่านั้นแก้ไข, student ดูได้)
    - List: ข้อประเมิน 5-10 ข้อ
    - แต่ละข้อ: ชื่อข้อ + Rating 1-5 ดาว
    - Textarea: ความเห็นเพิ่มเติม
    - ปุ่ม [ส่งประเมิน]
  - **การ์ด 2: นักศึกษาประเมินสถานประกอบการ** (student เท่านั้นแก้ไข)
    - โครงสร้างเหมือนกัน
- [ ] **สรุปคะแนน:** Total Score ใหญ่ + เกรด (A/B/C/D/F)

---

### 3.13 โปรไฟล์ (1 หน้า)
**S18 — Profile**
- [ ] **การ์ดบน: ข้อมูลส่วนตัว**
  - Avatar (ใหญ่, กลาง) + ปุ่มเปลี่ยนรูป
  - ฟอร์ม: ชื่อ, นามสกุล, Email (disabled), Phone, รหัสนักศึกษา
  - ปุ่ม [บันทึกการเปลี่ยนแปลง]
- [ ] **การ์ดกลาง: ข้อมูลการฝึก**
  - ชื่อบริษัท + ที่อยู่
  - ช่วงวันฝึก (Start – End)
  - อาจารย์ที่ปรึกษา (Avatar + ชื่อ)
  - พี่เลี้ยงบริษัท (Avatar + ชื่อ)
- [ ] **การ์ดล่าง: เปลี่ยนรหัสผ่าน**
  - Input: รหัสผ่านเก่า
  - Input: รหัสผ่านใหม่ + Validation
  - Input: ยืนยันรหัสผ่านใหม่
  - ปุ่ม [เปลี่ยนรหัสผ่าน]

---

### 3.14 Admin (3 หน้า)
**S19 — Admin Users**
- [ ] **Header:**
  - ปุ่ม [+ เพิ่มผู้ใช้]
  - ปุ่ม [⬆ Import จาก Excel]
- [ ] **Table:**
  - Columns: Avatar + ชื่อ | Email | Role (chip) | สถานะ (Active/Inactive badge) | Actions
  - Actions: [แก้ไข] [ปิดการใช้งาน]
  - Search + Filter role
- [ ] **Modal: เพิ่ม/แก้ไขผู้ใช้**
  - ฟอร์ม: ชื่อ, Email, Password (เฉพาะตอนเพิ่ม), Role, รหัสนักศึกษา (ถ้า student)
  - ปุ่ม [บันทึก]

**S20 — Admin Companies**
- [ ] **Header:** ปุ่ม [+ เพิ่มบริษัท]
- [ ] **Table:**
  - Columns: ชื่อบริษัท | ที่อยู่ | Phone | เวลาเข้า-ออก | จำนวน นศ. | Actions
  - Actions: [แก้ไข] [ลบ]
- [ ] **Modal: เพิ่ม/แก้ไขบริษัท**
  - ฟอร์ม: ชื่อ, ที่อยู่, Phone, WorkStartTime, WorkEndTime
  - ปุ่ม [บันทึก]

**S21 — Admin Internships (จับคู่การฝึก)**
- [ ] **Header:** ปุ่ม [+ จับคู่การฝึกใหม่]
- [ ] **Table:**
  - Columns: นักศึกษา | บริษัท | อาจารย์นิเทศ | พี่เลี้ยง | ภาคเรียน | ช่วงวัน | สถานะ | Actions
  - Actions: [แก้ไข] [ลบ]
  - Search + Filter ภาคเรียน
- [ ] **Modal: จับคู่การฝึก**
  - Select: นักศึกษา (searchable)
  - Select: บริษัท
  - Select: อาจารย์นิเทศ
  - Select: พี่เลี้ยง (optional)
  - Select: ภาคเรียน
  - DatePicker: Start – End
  - Input: RequiredDays, RequiredHours
  - ปุ่ม [บันทึก]

---

## 4. Flow & Interaction (ต้องแสดงใน Prototype)

- [ ] **Login flow:** Login → Dashboard → Logout → Login
- [ ] **ลงเวลา:** กดปุ่ม → นาฬิกาอัปเดต → สถานะเปลี่ยน
- [ ] **บันทึกงาน:** สร้างร่าง → ส่งอนุมัติ → อาจารย์เห็นในคิว → อนุมัติ/ตีกลับ → นักศึกษาเห็นแจ้งเตือน
- [ ] **ขอลา:** ส่งคำขอ → อาจารย์อนุมัติ → โควตาลด → ปฏิทินเปลี่ยนสี
- [ ] **Refresh token:** Token หมดอายุ → interceptor เรียก refresh → retry คำขอ

---

## 5. Deliverables จาก Figma

### 5.1 ไฟล์ Design
- [ ] **Figma file** พร้อม:
  - ทุก Page (Login, Dashboard, Attendance, …)
  - ทุก Component (Auto Layout + Variants)
  - Design System (Color Styles, Text Styles)
  - Prototype (flow เชื่อมหน้า)

### 5.2 Export Assets
- [ ] **ไอคอน** (SVG): logo, sidebar menu icons, status icons
- [ ] **Illustrations:** Login page, Empty states
- [ ] **Images:** Avatar placeholders

### 5.3 Documentation
- [ ] **Component spec:** ขนาด, สี, spacing, states
- [ ] **Interaction spec:** hover, click, animation
- [ ] **Responsive spec:** breakpoint (ถ้ามี mobile version)

---

## 6. Checklist สรุป (ทั้งหมด 21 หน้า)

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

**รวม: 21 หน้า + Design System + Components**

---

## 7. ลำดับการทำ (แนะนำ)

```
Phase 1: Foundation (วันแรก)
1. Design System (สี, ฟอนต์, spacing)
2. Components หลัก (Button, Input, Card, StatusBadge, Table)
3. Layout (Sidebar + Topbar)

Phase 2: Core Pages (วันที่ 2-3)
4. Login
5. Dashboard (ทั้ง 2 roles)
6. ลงเวลา

Phase 3: Main Features (วันที่ 4-6)
7. วันลา
8. ปฏิทิน
9. บันทึกงาน (List + Form)
10. งานมอบหมาย (Kanban)

Phase 4: Secondary Features (วันที่ 7-8)
11. การนิเทศ
12. แชท
13. แจ้งเตือน
14. รายงาน + Preview PDF
15. ประเมิน
16. โปรไฟล์

Phase 5: Admin + Polish (วันที่ 9-10)
17. Admin Users
18. Admin Companies
19. Admin Internships
20. Prototype (เชื่อมต่อ flow)
21. Export assets + documentation
```

**เวลาโดยประมาณ:** 10 วันทำงาน (8 ชม./วัน) สำหรับคนที่มีประสบการณ์ Figma

---

## 8. Tips สำหรับทำ Figma

1. **ใช้ Auto Layout ทุกที่** → แก้ไขง่าย, responsive
2. **ทำ Component Variants** → Button (4 แบบ), StatusBadge (ทุกสถานะ)
3. **ตั้ง Color/Text Styles** → เปลี่ยนทีเดียวทั้งไฟล์
4. **ใช้ Instances** → แก้ Component แม่ → ลูกเปลี่ยนทั้งหมด
5. **จัด Layer ชื่อชัดเจน** → หาง่าย, ส่งต่อ dev ง่าย
6. **Prototype ทีละ flow** → อย่าทำทั้งหมดพร้อมกัน
7. **Export SVG สำหรับไอคอน** → คุณภาพดี, ขนาดเล็ก