# สถานะโปรเจกต์ (Project Status)

> อัปเดตล่าสุด: **12 พ.ย. 2569** · อ้างอิงจากโค้ดใน branch `agents/add-project-to-git-repo` (รวมงานที่ยังไม่ commit)
> ประวัติการพัฒนาทั้งหมดดูที่ [reference/progress-log.md](reference/progress-log.md)

## 1. สรุป

- Backend ครบ 12 โมดูล, `dotnet build` ผ่าน (0 error, 0 warning)
- Frontend ครบ 21 หน้า + หน้า reset-password และแดชบอร์ดอาจารย์/admin, `ng build` ผ่าน
- มี automated test แล้ว: `InternLog.API.Tests` 42 tests (unit + integration ผ่าน HTTP) — ผ่านทั้งหมด
- รอบแก้ไข 12 พ.ย. 2569 ปิดปัญหาด้านความปลอดภัยทั้งหมดที่พบ (ข้อ 3) งานที่เหลือเป็นฟีเจอร์ frontend ที่ยังไม่ครบ (ข้อ 4)

## 2. สถานะรายโมดูล

| โมดูล | Backend | Frontend | หมายเหตุ |
|---|:---:|:---:|---|
| Auth (login, register, refresh, profile, เปลี่ยนรหัส) | ✅ | ✅ | refresh token อยู่ใน httpOnly cookie |
| Forgot / Reset password | ✅ | ✅ | ยังไม่ส่งอีเมลจริง (ลิงก์อยู่ใน console ของ API) |
| Attendance | ✅ | ✅ | ยกเลิกการคำนวณ "มาสาย" ตามเวลาบริษัทแล้ว · advisor ดูประวัตินักศึกษาได้ผ่าน `?studentId=` |
| Leaves | ✅ | ⚠️ | ดูไฟล์แนบได้แล้ว แต่ฟอร์มยื่นลายังไม่มีช่องแนบไฟล์ · ยังไม่มีหน้า advisor อนุมัติใบลาแยก |
| Work Logs | ✅ | ✅ | เปิดไฟล์แนบได้จากหน้าแก้ไขบันทึก |
| Tasks (Kanban) | ✅ | ✅ | |
| Advisor Visits | ✅ | ⚠️ | advisor ต้องระบุ `internshipId` เอง |
| Reports + PDF | ✅ | ⚠️ | ปุ่มดาวน์โหลด PDF ใช้งานได้แล้ว · ฝั่ง advisor ต้องส่ง `internshipId` เอง · PDF ยังไม่ฝังฟอนต์ไทย · หน้า preview ยังแสดงข้อมูลตัวอย่าง |
| Evaluations | ✅ | ⚠️ | เหมือน Reports เรื่อง `internshipId` |
| Notifications | ✅ | ✅ | |
| Dashboard | ✅ | ✅ | student / advisor / admin มีหน้าแยกกัน · calendar API ยังคืนข้อมูลเฉพาะ student |
| Companies (advisor) | ✅ | ⚠️ | API มีแค่ GET — ปุ่มแก้ไขในหน้า `/companies` เรียก `PUT /companies/{id}` ซึ่งไม่มี |
| Admin (users, companies, internships) | ✅ | ✅ | |

## 3. ปัญหาที่แก้แล้ว (12 พ.ย. 2569)

| # | ปัญหาเดิม | การแก้ |
|---|---|---|
| S1 | ไฟล์ใน `wwwroot/uploads` โหลดได้โดยไม่ต้อง login | ย้ายไป `App_Data/uploads` (นอก wwwroot) + ถอด `UseStaticFiles()` + endpoint ดาวน์โหลดที่ตรวจสิทธิ์ (`Core/Storage/FileStorage.cs`) |
| S2 | ไฟล์แนบใบลาไม่ถูกตรวจ | ใช้ `IFileStorage.SaveUploadAsync` ร่วมกัน: ตรวจขนาด นามสกุล magic bytes และไม่ใช้ชื่อไฟล์ของผู้ใช้บน disk |
| S3 | `System.Security.Cryptography.Pkcs 9.0.11` มีช่องโหว่ | อ้างอิงเวอร์ชัน 11.0.0 โดยตรง |
| S4 | Login ข้าม BCrypt เมื่อไม่พบ email | verify กับ hash จำลองทุกครั้ง |
| S5 | refresh token อยู่ใน localStorage | ย้ายไป cookie `HttpOnly; SameSite=Strict; Path=/api/auth` (access token ยังอยู่ใน localStorage อายุ 2 ชม.) |
| B1 | refresh rotation ผูก token ผิดตัว / ไม่ atomic | revoke ด้วย `UPDATE ... WHERE RevokedAt IS NULL` แล้วผูกกับ token ที่เพิ่งออก · user ที่ถูกปิดใช้งาน refresh ไม่ได้ |
| B2 | `DateTime.Now` ปน `UtcNow` | `AppClock` (เวลาไทยเสมอ ไม่ขึ้นกับ timezone ของ server) สำหรับเวลาธุรกิจ · UTC สำหรับ token |
| B3 | ลิงก์ไฟล์แนบใบลาเปิดไม่ได้ | DTO ส่ง URL ของ endpoint ดาวน์โหลด · เว็บโหลดผ่าน `FileDownloadService` (แนบ token) |
| B4 | ไม่มีหน้า `/reset-password` | เพิ่ม `features/auth/reset-password` |
| B5 | seed ใช้ RoleID ตายตัว / ภาคเรียนหมดอายุ | ย้ายไป `DevDataSeeder` หา role จากชื่อ · ช่วงฝึกงานทดสอบครอบคลุมวันปัจจุบันเสมอ |
| B6 | hash ของ admin ใน `HasData` เปลี่ยนทุกครั้ง | ใช้ค่าคงที่ตรงกับ model snapshot |
| — | interceptor: กรอกรหัสผิดตอน login แล้วถูก logout / logout วนซ้ำ | ข้าม refresh สำหรับ endpoint auth ที่ไม่ต้อง login · refresh ครั้งเดียวร่วมกันทุก request และทุก guard |
| Q1–Q2 | service/route/component ซ้ำและไม่ได้ใช้ใน frontend | ลบไฟล์ที่ไม่มีใคร import 39 ไฟล์ · รวม admin service · หน้า companies ใช้ AuthService ตัวหลัก |
| Q3 | ไม่มี test | เพิ่ม `InternLog.API.Tests` (xUnit + SQLite in-memory + WebApplicationFactory) |
| Q4 | `CompaniesController` query DB ตรง | ย้ายไป `CompanyService` |
| Q5 | string role ตายตัว | ใช้ `Constants.Roles` ใน service ทั้งหมด |
| Q6 | ไฟล์ที่ไม่ได้ใช้ | ลบ `package.json` ที่ root, โฟลเดอร์ `Controllers/` ว่าง · `InternLog.API.http` มีตัวอย่าง request จริง |
| — | หน้า admin users แสดง role ว่าง | map `roleName` จาก API |

## 4. ปัญหาที่ยังค้าง

| # | ระดับ | ปัญหา | หมายเหตุ |
|---|:---:|---|---|
| R1 | 🟡 | Role `supervisor` ยังอยู่ใน `HasData` และ DB เดิมอาจมี `supervisor@test.com` | การลบ role ต้องลบ/ย้าย user ที่ใช้ role นี้ก่อน — ให้ admin ปิดใช้งานผู้ใช้นั้นแล้วค่อยสร้าง migration ลบ role |
| R2 | 🟡 | Access token ยังอยู่ใน `localStorage` | อายุ 2 ชม. ถ้าต้องการปิดความเสี่ยง XSS ทั้งหมด ให้เก็บใน memory และ refresh ตอนเปิดแอป (guard ทำอยู่แล้ว) |
| R3 | 🟡 | ข้อมูลเวลาเก่าใน DB ที่บันทึกตอน server ไม่ได้อยู่เวลาไทย | ถ้าเคยรันบน server UTC ข้อมูลช่วงนั้นจะคลาด 7 ชม. (เครื่อง dev เป็นเวลาไทยอยู่แล้ว จึงไม่กระทบ) |
| F1 | — | ฟีเจอร์ frontend ที่ยังไม่ครบตามตารางข้อ 2 (แนบไฟล์ใบลา, หน้า advisor อนุมัติใบลา, ส่ง `internshipId` อัตโนมัติ, แก้ไขบริษัทของ advisor, ฟอนต์ไทยใน PDF) | |

## 5. งานถัดไป (เรียงตามลำดับที่แนะนำ)

1. Commit งานรอบนี้ และงานเดิมที่ค้างใน working tree
2. ทดสอบด้วยมือตาม checklist ใน [getting-started.md](getting-started.md#8-ทดสอบหลังแก้ไข-12-พย-2569)
3. F1 — ฟีเจอร์ frontend ที่เหลือ
4. R1 — เก็บกวาด role `supervisor`
5. เพิ่ม test ให้ครอบคลุมโมดูลอื่นตาม [reference/qa-test-cases.md](reference/qa-test-cases.md)
