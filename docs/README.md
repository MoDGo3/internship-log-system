# เอกสารโปรเจกต์ Internship Log System

ระบบจดบันทึกการฝึกงาน สำหรับนักศึกษา (student) อาจารย์ที่ปรึกษา (advisor) และผู้ดูแลระบบ (admin)
ใช้บันทึกเวลาเข้า-ออกงาน วันลา บันทึกงานรายวัน งานที่ได้รับมอบหมาย การนิเทศ รายงาน และการประเมินผล

- **Backend:** ASP.NET Core (.NET 10) + EF Core + SQL Server — `InternLog.API/`
- **Frontend:** Angular 17 (standalone components) — `InternLog.Web/`

## เริ่มอ่านจากตรงไหน

| ถ้าคุณต้องการ… | อ่าน |
|---|---|
| รันโปรเจกต์บนเครื่องตัวเอง | [getting-started.md](getting-started.md) |
| เข้าใจโครงสร้างโค้ดและการทำงานของระบบ | [architecture.md](architecture.md) |
| ดู endpoint ทั้งหมดและสิทธิ์ของแต่ละ role | [api-reference.md](api-reference.md) |
| รู้ว่าอะไรเสร็จแล้ว อะไรยังค้าง และปัญหาที่รู้อยู่ | [project-status.md](project-status.md) |

> เอกสาร 4 ไฟล์ด้านบนเขียนจากโค้ดปัจจุบัน (อัปเดต 12 พ.ย. 2569) ถ้าขัดกับเอกสารใน `reference/` ให้ถือไฟล์ชุดนี้เป็นหลัก

## เอกสารอ้างอิง (`reference/`)

รวมจากเอกสารเดิมในช่วงออกแบบและพัฒนา เนื้อหาคงไว้ตามต้นฉบับ บางส่วนอาจไม่ตรงกับโค้ดแล้ว

| ไฟล์ | เนื้อหา | ใช้เมื่อ |
|---|---|---|
| [system-spec.md](reference/system-spec.md) | Final Spec, ภาพรวมระบบ, ERD, Use Case/SRS, Sequence, SQL | ต้องการ requirement หรือ business rule ต้นฉบับ |
| [design-decisions.md](reference/design-decisions.md) | มติ Review v2, การเพิ่ม Auth (register/reset), Modular Architecture | อยากรู้ว่าทำไมออกแบบแบบนี้ |
| [ui-design.md](reference/ui-design.md) | Design System, Layout, Figma checklist, สเปกรายหน้า 21 หน้า | ทำหรือแก้ UI |
| [roadmap-and-work-order.md](reference/roadmap-and-work-order.md) | แผนพัฒนาราย Phase และใบสั่งงาน FE/BE | วางแผนงานต่อ |
| [qa-test-cases.md](reference/qa-test-cases.md) | Test cases ครบทุกด้าน (BE/FE/Integration/Security/Performance/UAT) | ทดสอบระบบ หรือเขียน automated test |
| [module-guides.md](reference/module-guides.md) | คู่มือ Work Logs API และ Tasks Frontend | เชื่อมต่อ 2 โมดูลนี้ |
| [progress-log.md](reference/progress-log.md) | รายงานความคืบหน้าและ daily summary ทั้งหมด (14 ส.ค. – 24 ก.ย. 2569) | ย้อนดูประวัติการพัฒนา |

### ตารางเทียบเอกสารเดิม → ที่อยู่ใหม่

เอกสารเดิมบางฉบับอ้างถึงกันด้วย "ส่วนที่ N" ใช้ตารางนี้หาตำแหน่งใหม่

| เดิม | ที่อยู่ใหม่ |
|---|---|
| ส่วนที่ 1 `internship-log-system-docs.md` | `reference/system-spec.md` ส่วนที่ 2 |
| ส่วนที่ 2 `internship-log-ui-design.md` | `reference/ui-design.md` ส่วนที่ 1 |
| ส่วนที่ 3 `internship-log-analysis-sql.md` | `reference/system-spec.md` ส่วนที่ 3 |
| ส่วนที่ 4 `internship-log-review-decisions.md` | `reference/design-decisions.md` ส่วนที่ 1 |
| ส่วนที่ 5 `internship-log-development-roadmap.md` | `reference/roadmap-and-work-order.md` ส่วนที่ 1 |
| ส่วนที่ 6 `internship-log-figma-checklist.md` | `reference/ui-design.md` ส่วนที่ 2 |
| ส่วนที่ 7 `internship-log-auth-gap-fix.md` | `reference/design-decisions.md` ส่วนที่ 2 |
| ส่วนที่ 8 `internship-log-modular-architecture.md` | `reference/design-decisions.md` ส่วนที่ 3 |
| ส่วนที่ 9 `internship-log-figma-page-specs.md` | `reference/ui-design.md` ส่วนที่ 3 |
| ส่วนที่ 10 `internship-log-qa-test-cases.md` | `reference/qa-test-cases.md` |
| ส่วนที่ 11 | ไม่มีในต้นฉบับ |
| ส่วนที่ 12 `internship-log-final-spec.md` | `reference/system-spec.md` ส่วนที่ 1 |
| ส่วนที่ 13 `internship-log-work-order.md.md` | `reference/roadmap-and-work-order.md` ส่วนที่ 2 |
| `module4_worklogs_api_guide.md`, `module5_tasks_frontend_guide.md` | `reference/module-guides.md` |
| `daily-reports/*`, progress reports, `software-engineering-review.md` | `reference/progress-log.md` (เรียงตามวันที่) |

## เอกสารอื่นในโปรเจกต์

- `Use Case + SRS_Diagram/` — ภาพ diagram (Use Case, Login JWT, Time clock, Submit record)
- `database/InternLogDB.sql` — สคริปต์สร้างฐานข้อมูลฉบับแรก (ปัจจุบันใช้ EF Core Migrations แทน)

## การเพิ่มเอกสารใหม่

- รายงานประจำวัน: เพิ่มเป็นส่วนใหม่ท้าย `reference/progress-log.md` โดยใช้หัวข้อ `## ส่วนที่ N: YYYY-MM-DD — หัวข้อ`
- เมื่อเพิ่มหรือแก้ endpoint: อัปเดต [api-reference.md](api-reference.md)
- เมื่อปิดงานหรือพบปัญหาใหม่: อัปเดต [project-status.md](project-status.md)
