# การออกแบบ UI (UI & Figma Design)

_Design System, Layout, รายการงาน Figma และสเปกรายหน้า_

> ⚠️ **เอกสารอ้างอิงจากช่วงออกแบบ/พัฒนา** — รวมจากไฟล์เดิมหลายไฟล์ เนื้อหาคงไว้ตามต้นฉบับ
> บางส่วนอาจไม่ตรงกับโค้ดปัจจุบัน (เช่น ระบุ .NET 8 แต่ใช้ .NET 10 จริง, โมดูล Messages ถูกลบแล้ว,
> ระบบคำนวณมาสายถูกยกเลิก) — ข้อมูลที่ตรงกับโค้ดล่าสุดดูที่ [architecture.md](../architecture.md), [api-reference.md](../api-reference.md) และ [project-status.md](../project-status.md)

## สารบัญ

1. [UI Design ทั้งระบบ (ส่วนที่ 2)](#ส่วนที่-1-ui-design-ทั้งระบบ-ส่วนที่-2)
2. [Figma Design Checklist (ส่วนที่ 6)](#ส่วนที่-2-figma-design-checklist-ส่วนที่-6)
3. [Figma Page-by-Page Specs (ส่วนที่ 9)](#ส่วนที่-3-figma-page-by-page-specs-ส่วนที่-9)

---

## ส่วนที่ 1: UI Design ทั้งระบบ (ส่วนที่ 2)

> ต้นฉบับ: `docs/internship-log-ui-design.md` · จัดทำ 14 ส.ค. 2569

### 1. Design System

#### 1.1 สี (Color Tokens)
| Token | ค่า | การใช้ |
|---|---|---|
| Primary | `#5B5FC7` (ม่วงโทน Teams) | ปุ่มหลัก, เมนู active, ลิงก์ |
| Primary Dark | `#464AB9` | ปุ่มตอน hover |
| Background | `#F5F6FA` | พื้นหลังทั้งแอป |
| Surface | `#FFFFFF` | การ์ด, โมดัล, ตาราง |
| Text | `#1F2328` | ตัวหนังสือหลัก |
| Secondary Text | `#6B7280` | คำอธิบาย, ป้ายกำกับ |
| Success | `#16A34A` | อนุมัติ, ตรงเวลา, เสร็จ |
| Warning | `#D97706` | รออนุมัติ, รอแก้ไข |
| Danger | `#DC2626` | ตีกลับ, มาสาย, ขาด |
| Leave | `#9333EA` | วันลา |
| Info | `#2563EB` | กำลังทำ, ลิงก์ข้อมูล |

#### 1.2 ฟอนต์และขนาด
- **Sarabun** — เนื้อหาทั้งหมด (รองรับไทย)
- **Prompt** — หัวเรื่อง / โลโก้
- ขนาด: H1 24 / H2 20 / H3 16 / Body 14–16 / Caption 12

#### 1.3 รูปทรงและระยะ
- การ์ด radius **12px** + shadow บาง
- ปุ่ม / Input radius **8px**
- Padding การ์ด 16–24px, ระยะห่างระหว่างการ์ด 16px
- Frame หลักเดสก์ท็อป **1440×1024**

#### 1.4 Components กลาง (ทำครั้งเดียว ใช้ซ้ำทุกหน้า)
- Button: primary / secondary / ghost / danger
- Input, Select, Textarea, DatePicker
- Card, StatCard (ไอคอน + ตัวเลข + คำอธิบาย)
- StatusBadge (variants ตามหัวข้อ 3)
- Table (header เทาอ่อน, แถว hover)
- Modal, SideDrawer, Tabs
- Avatar, ProgressBar, ChatBubble, EmptyState

---

### 2. Global Layout

```
┌ TopBar: [☰] [🔍 ค้นหา (Ctrl+E)..........]        [🔔 3] [👤 ชื่อผู้ใช้ ▾] ┐
├ Sidebar ──┬──────────────────────────────────────┤
│ 🏠 แดชบอร์ด  │                                      │
│ ⏰ ลงเวลา    │                                      │
│ 🏖 วันลา     │            Main Content              │
│ 📅 ปฏิทิน    │          (เปลี่ยนตาม route)           │
│ 📝 บันทึกงาน │                                      │
│ 📋 งานมอบหมาย│                                      │
│ 🧑 การนิเทศ  │                                      │
│ 💬 แชท       │                                      │
│ 📊 รายงาน    │                                      │
│ ⚙️ โปรไฟล์   │                                      │
│ 🛠 Admin*   │   (*แสดงเฉพาะ role admin)             │
└───────────┴──────────────────────────────────────┘
```

#### Route Map (ใช้ต่อตอนเขียน Angular)
| Route | หน้า | Role ที่เห็น |
|---|---|---|
| /login | Login / Register / ลืมรหัส | ทุก role |
| /dashboard | แดชบอร์ด (แยกตาม role) | ทุก role |
| /attendance | ลงเวลาปฏิบัติงาน | student |
| /leaves | วันลา | student (+คิวอนุมัติของ advisor) |
| /calendar | ปฏิทิน | student, advisor |
| /work-logs | รายการบันทึกงาน | student, advisor |
| /work-logs/new , /work-logs/:id | ฟอร์มบันทึกงาน | student |
| /tasks | งานมอบหมาย (Kanban) | student, supervisor, advisor |
| /visits | การนิเทศ | student, advisor |
| /chat | แชท | ทุก role |
| /notifications | แจ้งเตือน | ทุก role |
| /reports | รายงาน + Export PDF | student, advisor |
| /evaluations | ประเมิน | supervisor, student |
| /profile | โปรไฟล์/ตั้งค่า | ทุก role |
| /admin/users , /admin/companies , /admin/internships | จัดการระบบ | admin |

---

### 3. สถานะและสี Badge (ใช้一致กันทั้งระบบ)

**บันทึกงาน:** ร่าง ⚪เทา | ส่งแล้ว 🟡เหลือง | อนุมัติ 🟢เขียว | ตีกลับ 🔴แดง
**ลงเวลา:** ตรงเวลา 🟢 | สาย 🔴 | ลา 🟣 | ขาด ⚫ | รอแก้ 🟡
**วันลา/รายงาน:** รออนุมัติ 🟡 | อนุมัติ 🟢 | ไม่อนุมัติ/ตีกลับ 🔴
**Tasks:** ยังไม่ทำ ⚪ | กำลังทำ 🔵 | เสร็จ 🟢 • Priority: low 🟢 / medium 🟡 / high 🔴
**สีช่องปฏิทิน:** 🟢 อนุมัติ | 🟡 รออนุมัติ | 🔴 ไม่ได้บันทึก | 🟣 วันลา | ⚪ วันหยุด

---

### 4. Wireframe ทั้ง 20 หน้า

#### S1 — Login (Split Screen)
```
┌──────────────────────────┬─────────────────────────┐
│ พื้น Primary              │        Card ขาว          │
│ [Logo] ระบบบันทึกการฝึกงาน  │  Email    [__________]  │
│ Tagline + Illustration   │  รหัสผ่าน [__________]  │
│                          │  ☑ จำฉัน   ลืมรหัสผ่าน?   │
│                          │  [ เข้าสู่ระบบ ] เต็มกว้าง │
└──────────────────────────┴─────────────────────────┘
```

#### S2 — Register / ลืมรหัส
- แท็บสลับ: สมัครสมาชิก | ลืมรหัสผ่าน
- Register: ชื่อ-นามสกุล, Email, รหัส, ยืนยันรหัส, Role (เลือกได้เฉพาะ นศ. — อาจารย์/admin ให้ admin เพิ่ม)
- ลืมรหัส: Email → ส่งลิงก์รีเซ็ต

#### S3 — Dashboard นักศึกษา
```
[📅 ฝึกแล้ว 24/60 วัน ▓▓░] [⏱ 192 ชม.] [🏖 ลาคงเหลือ 5] [🧑 นิเทศถัดไป 15 ส.ค.]
┌────────────────────────────┐ ┌──────────────────────┐
│ ลงเวลาวันนี้                 │ │ การแจ้งเตือนล่าสุด      │
│ เวลาปัจจุบัน 08:52:14        │ │ • บันทึก 12 ส.ค. อนุมัติ │
│ [ลงเวลาเข้า]  [ลงเวลาออก]    │ │ • ได้งานใหม่: ทำ UI Login│
└────────────────────────────┘ └──────────────────────┘
[ตารางบันทึกล่าสุด: วันที่ | ชื่องาน | ชม. | สถานะ | 👁 ดู]
[งานที่ครบกำหนดสัปดาห์นี้]
```

#### S4 — Dashboard อาจารย์
- StatCards: นศ.ที่ดูแล | คิวรออนุมัติ | อนุมัติสัปดาห์นี้ | ข้อความยังไม่อ่าน
- ตารางคิวรออนุมัติ (บันทึก/ลา/รายงาน) + ปุ่ม ✅ อนุมัติ / ↩ ตีกลับ
- การ์ด นศ. แต่ละคน: ชื่อ + ProgressBar ความคืบหน้า + ปุ่มดูหน้า นศ.

#### S5 — ลงเวลาปฏิบัติงาน
```
┌──────────────────────────────────────────────┐
│  ⏱ 08:52:14 (นาฬิกาเดินจริง)   วันนี้ 14 ส.ค. 69   │
│  [ ⬇ ลงเวลาเข้า ]   [ ⬆ ลงเวลาออก ]             │
│  สรุปเดือน: [ชั่วโมง 96] [สาย 1] [ขาด 0]          │
└──────────────────────────────────────────────┘
[ตารางประวัติ: วันที่ | เวลามา | เวลากลับ | รวม ชม. | สถานะ | ⚙ ขอแก้]
```

#### S6 — วันลา
- ซ้าย: การ์ดโควตา 3 ใบ (ลาป่วย/ลากิจ/พักร้อน) แสดง ใช้ไป x / คงเหลือ y + ProgressBar
- ขวา: ฟอร์มขอลา (ประเภท, ตั้งแต่–ถึง, จำนวนวัน auto, เหตุผล, แนบไฟล์)
- ล่าง: ตารางประวัติคำขอ + StatusBadge

#### S7 — ปฏิทิน
```
[< สิงหาคม 2569 >]   [เดือน | สัปดาห์]   Legend สี 5 ช่อง
┌─────┬─────┬─────┬─────┬─────┬─────┬─────┐
│ อา. │ จ.  │ อ.  │ พ.  │ พฤ. │ ศ.  │ ส.  │
│     │ 1 🟢│ 2 🟢│ 3 🟡│ 4 🟣│ 5 ⚪│ 6 ⚪│
└─────┴─────┴─────┴─────┴─────┴─────┴─────┘
คลิกวันที่ → SideDrawer: [ข้อมูลลงเวลา] [บันทึกงานของวัน] [+ เพิ่มบันทึก]
```

#### S8 — Work Log List
- FilterBar: [เดือน ▾] [สถานะ ▾] [ประเภท ▾] [🔍 ค้นหา] [+ เพิ่มบันทึก]
- ตาราง: วันที่ | ชื่องาน | ประเภท(chip) | ชม. | สถานะ(badge) | แก้ไข/ดู

#### S9 — Work Log Form (2 คอลัมน์)
```
┌──────────────────────────────┬─────────────────────┐
│ วันที่ [14 ส.ค. 69] เวลา auto    │ 📎 ไฟล์แนบ            │
│ ชื่องาน [____________]         │ [ลากมาวาง / เลือกไฟล์]  │
│ ประเภท [Development ▾]        │ [img][img][pdf]      │
│ รายละเอียด [textarea]          ├─────────────────────┤
│ ปัญหา [textarea]              │ สถานะ: 🟡 รออนุมัติ    │
│ วิธีแก้ [textarea]             │ 💬 คอมเมนต์อาจารย์      │
│ [บันทึกร่าง]  [ส่งอนุมัติ]      │  - "ดีมาก ✅" อ.สมชาย   │
└──────────────────────────────┴─────────────────────┘
```

#### S10 — Tasks (Kanban)
```
┌ ยังไม่ทำ (2) ──┐ ┌ กำลังทำ (1) ──┐ ┌ เสร็จ (5) ────┐
│ [Card]        │ │ [Card]        │ │ [Card]        │
│ ชื่องาน        │ │ ...           │ │ ...           │
│ 🔴 high • 18 ส.ค.│              │ │               │
└───────────────┘ └───────────────┘ └───────────────┘
Card = ชื่องาน | priority chip | กำหนดส่ง | ผู้มอบหมาย  [+ มอบหมายงาน]
```

#### S11 — การนิเทศ
- ตาราง: คนที่ | ครั้งที่ | วันที่ | ชื่ออาจารย์ | รูปแบบ(Onsite/Online chip) | สรุป | คะแนน
- ปุ่ม [+ บันทึกการนิเทศ] (advisor) → Modal ฟอร์ม

#### S12 — แชท
```
┌ บทสนทนา ────────┐ ┌──────────────────────────────┐
│ 👤 อ.สมชาย ●2    │ │ อ.สมชาย ใจดี        [ออนไลน์]  │
│ 👤 พี่แนน        │ │ (•) สวัสดีครับ ส่งบันทึกหรือยัง  │
│                 │ │     (•) ส่งแล้วครับอาจารย์ ✅   │
│                 │ │ [พิมพ์ข้อความ.........] [➤]    │
└─────────────────┘ └──────────────────────────────┘
```

#### S13 — Notifications
- รายการ: ไอคอนแยกประเภท (อนุมัติ ✅ / งานใหม่ 📋 / ข้อความ 💬 / ระบบ ⚙️)
- ยังไม่อ่าน = พื้นสีจาง + จุดสี Primary, ปุ่ม "อ่านทั้งหมด"

#### S14 — รายงาน
```
[เลือกช่วง: สัปดาห์ 2–8 ส.ค. 69 ▾]  [+ สร้างรายงาน]  [⬇ Export PDF]
[StatCards: ชม.รวม 41 | บันทึก 5 วัน | ลา 0]
[กราฟแท่ง: ชั่วโมงตามประเภทงาน]
[ตารางลงเวลาสัปดาห์]  [ตารางการนิเทศ]
[รายการรายงาน: สัปดาห์ที่ | ช่วงวันที่ | สถานะ | ส่งอนุมัติ/ดู]
```

#### S15 — Preview PDF
- จำลองเล่มรายงาน: หัวกระดาษ (โลโก้มหาวิทยาลัย + ชื่อนศ. + บริษัท)
- ส่วนที่ 1 สรุปชั่วโมง | ส่วนที่ 2 ตารางบันทึกงานรายวัน | ส่วนที่ 3 วันลา | ส่วนที่ 4 การนิเทศ | ส่วนที่ 5 ลายเซ็นอนุมัติ

#### S16 — Evaluation
- 2 การ์ดฟอร์ม: พี่เลี้ยง→นักศึกษา | นักศึกษา→บริษัท
- ให้คะแนน 1–5 ดาว รายข้อ + ช่องความเห็น + ปุ่มส่งประเมิน

#### S17 — Profile
- การ์ด Avatar + ฟอร์มข้อมูลส่วนตัว (ชื่อ, Email, โทร, รหัสนศ.)
- การ์ดข้อมูลการฝึก: บริษัท, ช่วงวัน, อาจารย์ที่ปรึกษา, พี่เลี้ยง
- ส่วนเปลี่ยนรหัสผ่าน (เก่า/ใหม่/ยืนยัน)

#### S18 — Admin Users
- ตาราง: ชื่อ | Email | Role chip | สถานะ Active | แก้ไข/ปิดใช้
- ปุ่ม [+ เพิ่มผู้ใช้] → Modal, ปุ่ม [⬆ Import รายชื่อ]

#### S19 — Admin Companies
- ตาราง: ชื่อบริษัท | ที่อยู่ | โทร | เวลาเข้า-ออกงาน | จำนวน นศ.
- Modal เพิ่ม/แก้ไขบริษัท

#### S20 — Admin Internships (จับคู่การฝึก)
- ตาราง: นศ. | บริษัท | อาจารย์นิเทศ | พี่เลี้ยง | ภาคเรียน | ช่วงวัน | สถานะ
- Modal จับคู่: เลือก นศ. → บริษัท → อาจารย์ → พี่เลี้ยง → ภาคเรียน → วันเริ่ม-จบ

---

### 5. ทิปตอนวาด Figma

- ใช้ **Auto Layout** กับทุกการ์ด/ตาราง/ฟอร์ม
- ทำ **Component + Variants**: StatusBadge (ทุกสถานะ), Button (4 แบบ), Table Row
- ตั้งสีเป็น **Color Styles** ตาม Design System (หัวข้อ 1) เพื่อแก้ทีเดียวทั้งไฟล์
- วาง Flow เชื่อมหน้าจอด้วย Prototype (คลิกปุ่ม → ไปหน้าปลายทาง) เพื่อเทสการนำทาง

---

### 6. ภาคผนวก — Mapping สู่ Angular (ใช้ตอนเขียนโค้ด)

| Screen | Component | Guard |
|---|---|---|
| S1–S2 | login, register, forgot-password | guest |
| S3/S4 | dashboard-student, dashboard-advisor | auth+role |
| S5 | attendance | student |
| S6 | leaves | student |
| S7 | calendar | student, advisor |
| S8–S9 | worklog-list, worklog-form | student, advisor |
| S10 | tasks (kanban) | student, supervisor, advisor |
| S11 | visits | student, advisor |
| S12 | chat | auth |
| S13 | notifications | auth |
| S14–S15 | reports, report-pdf | student, advisor |
| S16 | evaluations | supervisor, student |
| S17 | profile | auth |
| S18–S20 | admin-users, admin-companies, admin-internships | admin |
| — | layout: sidebar + topbar (shell component) | auth |

---

## ส่วนที่ 2: Figma Design Checklist (ส่วนที่ 6)

> ต้นฉบับ: `docs/internship-log-figma-checklist.md` · จัดทำ 14 ส.ค. 2569

### 1. Design System (ต้องทำก่อนทุกหน้า)

#### 1.1 Color Tokens (Style ใน Figma)
- [ ] Primary `#5B5FC7` + Primary Dark `#464AB9`
- [ ] Background `#F5F6FA`
- [ ] Surface `#FFFFFF`
- [ ] Text `#1F2328` + Secondary Text `#6B7280`
- [ ] Success `#16A34A` / Warning `#D97706` / Danger `#DC2626` / Leave `#9333EA` / Info `#2563EB`

#### 1.2 Typography Styles
- [ ] H1: Prompt 24px Bold
- [ ] H2: Prompt 20px SemiBold
- [ ] H3: Prompt 16px SemiBold
- [ ] Body: Sarabun 14–16px Regular
- [ ] Caption: Sarabun 12px Regular

#### 1.3 Spacing & Radius
- [ ] การ์ด radius 12px + shadow
- [ ] ปุ่ม/Input radius 8px
- [ ] Padding การ์ด 16–24px
- [ ] ระยะห่างระหว่าง elements 16px

---

### 2. Components (ทำครั้งเดียว ใช้ซ้ำทุกหน้า)

#### 2.1 Form Controls
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

#### 2.2 Data Display
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

#### 2.3 Navigation & Feedback
- [ ] **Sidebar** (เมนูหลัก + collapse state)
- [ ] **Topbar** (search + notifications + profile)
- [ ] **Tabs**
- [ ] **Modal / Dialog**
- [ ] **SideDrawer** (เลื่อนจากขวา)
- [ ] **Toast / Snackbar** (แจ้งเตือน)
- [ ] **Empty State** (ไม่มีข้อมูล)
- [ ] **Loading Spinner**

#### 2.4 Special Components
- [ ] **ChatBubble** (sent / received)
- [ ] **Calendar Cell** (วัน + สี status)
- [ ] **Kanban Card** (ชื่องาน + priority + due date)
- [ ] **Kanban Column** (header + card list)

---

### 3. หน้าจอทั้งหมด (20 หน้า)

#### 3.1 Authentication (3 หน้า)
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

#### 3.2 Dashboard (2 หน้า)
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

#### 3.3 ลงเวลา (1 หน้า)
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

#### 3.4 วันลา (1 หน้า)
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

#### 3.5 ปฏิทิน (1 หน้า)
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

#### 3.6 บันทึกงาน (2 หน้า)
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

#### 3.7 งานมอบหมาย (1 หน้า)
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

#### 3.8 การนิเทศ (1 หน้า)
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

#### 3.9 แชท (1 หน้า)
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

#### 3.10 แจ้งเตือน (1 หน้า)
**S14 — Notifications**
- [ ] **Header:**
  - ปุ่ม [อ่านทั้งหมด]
- [ ] **List:**
  - แต่ละ item: ไอคอน (แยกประเภท) + ข้อความ + เวลา + dot (ยังไม่อ่าน)
  - Click: ไปหน้าปลายทาง (เช่น คลิก "บันทึกถูกอนุมัติ" → ไปหน้าบันทึกนั้น)
  - ปุ่ม [X] ลบ
- [ ] **Empty state:** "ไม่มีการแจ้งเตือน"

---

#### 3.11 รายงาน (2 หน้า)
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

#### 3.12 ประเมิน (1 หน้า)
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

#### 3.13 โปรไฟล์ (1 หน้า)
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

#### 3.14 Admin (3 หน้า)
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

### 4. Flow & Interaction (ต้องแสดงใน Prototype)

- [ ] **Login flow:** Login → Dashboard → Logout → Login
- [ ] **ลงเวลา:** กดปุ่ม → นาฬิกาอัปเดต → สถานะเปลี่ยน
- [ ] **บันทึกงาน:** สร้างร่าง → ส่งอนุมัติ → อาจารย์เห็นในคิว → อนุมัติ/ตีกลับ → นักศึกษาเห็นแจ้งเตือน
- [ ] **ขอลา:** ส่งคำขอ → อาจารย์อนุมัติ → โควตาลด → ปฏิทินเปลี่ยนสี
- [ ] **Refresh token:** Token หมดอายุ → interceptor เรียก refresh → retry คำขอ

---

### 5. Deliverables จาก Figma

#### 5.1 ไฟล์ Design
- [ ] **Figma file** พร้อม:
  - ทุก Page (Login, Dashboard, Attendance, …)
  - ทุก Component (Auto Layout + Variants)
  - Design System (Color Styles, Text Styles)
  - Prototype (flow เชื่อมหน้า)

#### 5.2 Export Assets
- [ ] **ไอคอน** (SVG): logo, sidebar menu icons, status icons
- [ ] **Illustrations:** Login page, Empty states
- [ ] **Images:** Avatar placeholders

#### 5.3 Documentation
- [ ] **Component spec:** ขนาด, สี, spacing, states
- [ ] **Interaction spec:** hover, click, animation
- [ ] **Responsive spec:** breakpoint (ถ้ามี mobile version)

---

### 6. Checklist สรุป (ทั้งหมด 21 หน้า)

#### Authentication (3)
- [ ] S1 Login
- [ ] S2 Register
- [ ] S3 Forgot Password

#### Dashboard (2)
- [ ] S4 Dashboard นักศึกษา
- [ ] S5 Dashboard อาจารย์

#### Features (13)
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

#### Admin (3)
- [ ] S19 Admin Users
- [ ] S20 Admin Companies
- [ ] S21 Admin Internships

**รวม: 21 หน้า + Design System + Components**

---

### 7. ลำดับการทำ (แนะนำ)

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

### 8. Tips สำหรับทำ Figma

1. **ใช้ Auto Layout ทุกที่** → แก้ไขง่าย, responsive
2. **ทำ Component Variants** → Button (4 แบบ), StatusBadge (ทุกสถานะ)
3. **ตั้ง Color/Text Styles** → เปลี่ยนทีเดียวทั้งไฟล์
4. **ใช้ Instances** → แก้ Component แม่ → ลูกเปลี่ยนทั้งหมด
5. **จัด Layer ชื่อชัดเจน** → หาง่าย, ส่งต่อ dev ง่าย
6. **Prototype ทีละ flow** → อย่าทำทั้งหมดพร้อมกัน
7. **Export SVG สำหรับไอคอน** → คุณภาพดี, ขนาดเล็ก

---

## ส่วนที่ 3: Figma Page-by-Page Specs (ส่วนที่ 9)

> ต้นฉบับ: `docs/internship-log-figma-page-specs.md` · จัดทำ 14 ส.ค. 2569

### 🎯 Design System สรุป (ใช้ร่วมกันทุกหน้า)

#### Color Tokens
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

#### Typography
| Style | Font | Size | Weight |
|---|---|---|---|
| H1 | Prompt | 24px | Bold |
| H2 | Prompt | 20px | SemiBold |
| H3 | Prompt | 16px | SemiBold |
| Body | Sarabun | 14-16px | Regular |
| Caption | Sarabun | 12px | Regular |

#### Spacing & Radius
- Grid: 8px base (8, 16, 24, 32, 48)
- Card radius: 12px
- Button/Input radius: 8px
- Shadow: `0 1px 3px rgba(0,0,0,0.1)`

---

### 📄 หน้าทั้งหมด (21 หน้า)

---

#### 🔐 กลุ่ม Authentication (3 หน้า)

---

##### S1 — Login

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

##### S2 — Register

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

##### S3 — Forgot Password

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

#### 🏠 กลุ่ม Dashboard (2 หน้า)

---

##### S4 — Dashboard นักศึกษา

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

##### S5 — Dashboard อาจารย์

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

#### ⏰ กลุ่ม Features หลัก (13 หน้า)

---

##### S6 — ลงเวลาปฏิบัติงาน

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

##### S7 — วันลา

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

##### S8 — ปฏิทิน

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

##### S9 — Work Log List

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

##### S10 — Work Log Form

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

##### S11 — Tasks (Kanban)

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

##### S12 — การนิเทศ

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

##### S13 — แชท

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

##### S14 — แจ้งเตือน

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

##### S15 — Reports

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

##### S16 — Preview PDF

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

##### S17 — Evaluation

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

##### S18 — Profile

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

####  กลุ่ม Admin (3 หน้า)

---

##### S19 — Admin Users

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

##### S20 — Admin Companies

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

##### S21 — Admin Internships

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

### 📦 Handoff Guide สรุป (Figma → Angular)

#### Assets ที่ต้อง Export
| ประเภท | รูปแบบ | ตำแหน่งใน Angular |
|---|---|---|
| Icons | SVG | `assets/icons/` |
| Logo | SVG | `assets/` |
| Illustrations | SVG | `assets/illustrations/` |
| Avatar placeholder | SVG | `assets/` |

#### Design Tokens → CSS Variables
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

#### Component Mapping
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

### ✅ Checklist สรุป (21 หน้า)

#### Authentication (3)
- [ ] S1 Login
- [ ] S2 Register
- [ ] S3 Forgot Password

#### Dashboard (2)
- [ ] S4 Dashboard นักศึกษา
- [ ] S5 Dashboard อาจารย์

#### Features (13)
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

#### Admin (3)
- [ ] S19 Admin Users
- [ ] S20 Admin Companies
- [ ] S21 Admin Internships

**รวม: 21 หน้า + Design System + Handoff Guide**
