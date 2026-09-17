---
name: internship-log-system
description: "พัฒนา แก้ไข ตรวจสอบ หรือทดสอบระบบ Internship Log System ด้วย Angular และ ASP.NET Core โดยยึด Final Spec, Modular Architecture, role-based access control, API integration, database consistency และ QA/UAT"
argument-hint: "ระบุโมดูลหรือปัญหาที่ต้องการให้ตรวจสอบและแก้ไข"
user-invocable: true
disable-model-invocation: false
---

# Internship Log System

## วัตถุประสงค์

ใช้ Skill นี้เมื่อทำงานกับระบบบันทึกการฝึกงานใน repository นี้ ทั้ง Backend, Frontend, database, API, security, UI และ QA

## แหล่งอ้างอิงและลำดับความสำคัญ

อ่านเอกสารเฉพาะที่เกี่ยวข้องก่อนแก้ไข โดยใช้ลำดับต่อไปนี้เมื่อข้อมูลขัดแย้งกัน:

1. `docs/internship-log-final-spec.md` เป็นข้อกำหนดหลัก
2. `docs/daily-reports/2026-08-27-daily-summary.md` เป็นสถานะงานล่าสุด
3. `docs/internship-log-review-decisions.md` และ `docs/internship-log-work-order.md.md` เป็นมติและลำดับงาน
4. `docs/internship-log-modular-architecture.md` เป็นแนวทางโครงสร้างโค้ด
5. คู่มือเฉพาะโมดูลและเอกสาร QA ใช้ประกอบการทำงาน

หากพบความขัดแย้ง ให้รายงานอย่างชัดเจนและยึดเอกสารที่มีลำดับสูงกว่า ห้ามแอบเลือกแนวทางโดยไม่แจ้งผู้ใช้

## ขอบเขตระบบ

ระบบมี 3 roles เท่านั้น:

- `student`: ลงเวลา ลางาน บันทึกงาน รับงาน ดูรายงาน และประเมินบริษัท
- `advisor`: ดูแลนักศึกษา อนุมัติลาและคำขอแก้เวลา คอมเมนต์งาน บันทึกนิเทศ มอบหมายงาน และจัดการบริษัทของตนเอง
- `admin`: จัดการผู้ใช้ บริษัท ภาคเรียน การจับคู่การฝึก รายงานรวม และดู Dashboard นักศึกษาแบบ read-only

โมดูลหลักคือ Auth, Profile, Attendance, Leaves, WorkLogs, Tasks, AdvisorVisits, Reports, Notifications, Evaluations, Calendar, Dashboard, Companies และ Admin

## กฎที่ต้องรักษา

### Architecture

- Backend อยู่ใต้ `InternLog.API/src` และแบ่งเป็น `Core`, `Infrastructure`, `Modules`
- แยก Controller, Service, Interface, DTOs และ Models ตาม feature
- Controller รับผิดชอบ HTTP เท่านั้น; business logic อยู่ใน Service
- ใช้ EF Core, Fluent API, Dependency Injection และ interface ระหว่างโมดูล
- ห้ามเรียก Controller ของโมดูลอื่นหรือเข้าถึง Entity ภายในโมดูลอื่นโดยตรง
- ใช้ response กลางรูปแบบ `{ success, message, data }` เมื่อสอดคล้องกับ implementation เดิม
- Frontend ใช้ Angular Standalone Components, feature-based folders, lazy loading, guards, interceptor และ Signals ตาม pattern เดิม

### Business rules

- `WorkLogs.Hours` ต้องคำนวณจาก Attendance ฝั่ง server ห้ามรับค่าที่เชื่อถือได้จาก client
- WorkLog เป็นการบันทึกจริง แก้ไขได้ตลอดเวลา และไม่มี approval flow ตาม Final Spec
- Advisor ต้องเห็นเฉพาะนักศึกษาและบริษัทที่ตนเองดูแลในทุก query และ endpoint
- Student ต้องเข้าถึงข้อมูลของตนเองเท่านั้น
- Admin impersonation ต้องเป็น read-only, token อายุ 30 นาที และสร้าง Audit Log
- Companies ที่ advisor สร้างหรือแก้ไขได้ต้องเป็นบริษัทของ advisor คนนั้นเท่านั้น
- ตรวจสอบ quota วันลา, การตัดเวลาเข้างานอัตโนมัติ, refresh-token rotation และการ sync ชั่วโมงเมื่อแก้เวลาได้รับอนุมัติ
- ห้ามนำ `Supervisor`, `Chat`, `Messages` หรือ WorkLog approval กลับมาใช้ เว้นแต่ผู้ใช้แก้ Final Spec อย่างชัดเจน

## ขั้นตอนการทำงาน

1. ระบุโมดูล, endpoint, component, service หรือ migration ที่เป็นเจ้าของ behavior ก่อนค้นหาไฟล์
2. อ่าน implementation ใกล้เคียงและเอกสารเฉพาะโมดูลเท่าที่จำเป็น
3. ตั้งสมมติฐานสั้น ๆ ว่าปัญหาเกิดจากจุดใด และเลือก check ที่ถูกที่สุดเพื่อพิสูจน์หรือหักล้าง
4. ตรวจสอบผลกระทบข้ามชั้น: database/entity, service, controller/API contract, frontend model/service/component และ tests
5. แก้ที่ root cause ด้วยการเปลี่ยนแปลงให้น้อยที่สุด รักษา public API และ style เดิม
6. เพิ่มหรือปรับ test เฉพาะ behavior ที่เปลี่ยน โดยครอบคลุม success, validation, unauthorized และ forbidden ที่เกี่ยวข้อง
7. หลังแก้ไขครั้งแรก ให้รัน focused validation ทันที เช่น test, build, typecheck หรือ lint ของส่วนที่แตะ
8. แก้ defect ที่พบใน slice เดิมและรัน validation เดิมซ้ำก่อนขยายขอบเขต
9. ตรวจสอบ diff, diagnostics และผลกระทบต่อเอกสารหรือ schema ก่อนสรุป
10. รายงานไฟล์ที่เปลี่ยน เหตุผล validation ที่รัน และข้อจำกัดหรือความขัดแย้งที่ยังเหลือ

## แนวทางแยกตามประเภทงาน

### Backend/API

- ตรวจ `[Authorize]`, role policy และ ownership scope ก่อนแก้ business logic
- รับ input ผ่าน DTO และ validate ที่ boundary
- ใช้ async EF Core อย่างถูกต้อง หลีกเลี่ยง query ที่โหลดข้อมูลเกินจำเป็น
- อย่า expose password, refresh token, reset token หรือข้อมูลภายในที่ไม่จำเป็น
- File upload ต้องตรวจ extension, MIME type, ขนาดไฟล์, ชื่อไฟล์ และ path traversal
- Error ต้องใช้ status code ที่เหมาะสมและไม่เปิดเผยรายละเอียดภายใน

### Frontend

- ตรวจ route guard, interceptor, loading, empty, error และ forbidden states
- ใช้ service/model ที่มีอยู่ก่อนสร้าง abstraction ใหม่
- รักษา role-based menu และ read-only state ให้ตรงกับ API
- ห้ามแสดงข้อมูลของผู้ใช้อื่นเพียงเพราะ UI ซ่อนปุ่ม; backend ต้อง enforce เสมอ
- ตรวจ responsive layout และไม่ทำให้ข้อความหรือ control ซ้อนกัน

### Database

- ตรวจ entity, Fluent configuration, migration และ SQL script ให้สอดคล้องกัน
- Schema Final มี roles 3 ค่า และไม่มี Messages ตาม Final Spec
- ตรวจ unique constraints, foreign keys, nullable fields และการคำนวณชั่วโมง
- ห้ามแก้ migration เดิมที่ถูกใช้งานแล้วโดยไม่จำเป็น ให้สร้าง migration ใหม่เมื่อเหมาะสม

### QA/UAT

ให้เน้นกรณีเสี่ยงสูงก่อน:

- login, register, forgot/reset password, refresh และ logout
- role isolation และ advisor company/student scope
- attendance check-in/out, edit request และ WorkLog hours sync
- leave quota และ approval
- WorkLog create/edit/delete, comment และ attachment
- task assignment/status
- PDF report, impersonation read-only และ Audit Log
- XSS, injection, JWT forgery, brute force และ upload security

## ข้อห้าม

- ห้ามแก้ไฟล์ที่ไม่เกี่ยวข้องเพื่อจัดรูปแบบหรือ refactor เล่น
- ห้ามข้ามการตรวจสิทธิ์เพราะ endpoint ถูกเรียกจากหน้า UI ที่จำกัด role แล้ว
- ห้ามใช้ค่าจาก client แทนค่าที่ backend คำนวณได้
- ห้ามอ้างว่าเสร็จหรือผ่าน QA จากเอกสาร progress เพียงอย่างเดียว ต้องรัน check ที่เหมาะสม
- ห้ามลบหรือย้อนการเปลี่ยนแปลงเดิมของผู้ใช้
- ห้าม commit หรือสร้าง branch เว้นแต่ผู้ใช้สั่งโดยตรง

## รูปแบบสรุปผล

สรุปให้กระชับโดยระบุ:

- สิ่งที่พบและ root cause
- ไฟล์หรือโมดูลที่แก้ไข
- behavior ที่เปลี่ยน
- validation command และผลลัพธ์
- ความเสี่ยง, test gap หรือ decision ที่ยังต้องให้ผู้ใช้ยืนยัน
