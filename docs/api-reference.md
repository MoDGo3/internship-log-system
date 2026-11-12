# API Reference

รายการ endpoint ทั้งหมด สร้างจาก controllers ใน `InternLog.API/src/Modules/` (12 พ.ย. 2569)
ดู request/response schema แบบละเอียดได้ที่ Swagger: `http://localhost:5005/swagger`

## ข้อตกลงทั่วไป

- **Base URL:** `http://localhost:5005/api`
- **Auth:** ส่ง `Authorization: Bearer <accessToken>` ทุก endpoint ยกเว้นที่ระบุว่า "สาธารณะ"
- **Response:** `{ "success": bool, "message": string, "data": T | null }`
- **Status code:** `400` ข้อมูลไม่ถูกต้อง · `401` ยังไม่ login/token หมดอายุ · `403` role หรือเจ้าของข้อมูลไม่ตรง · `404` ไม่พบ · `409` ข้อมูลซ้ำ
- **ตัวย่อ role:** S = student · A = advisor · Ad = admin · ทุกคน = login แล้วทุก role
- **ขอบเขตข้อมูล:** student เห็นเฉพาะของตัวเอง, advisor เห็นเฉพาะนักศึกษาที่ตนดูแล (ผ่าน `Internship.AdvisorID`), admin เห็นทั้งหมด — ตรวจใน service ทุกตัว

## Auth — `/api/auth`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| POST | `/auth/login` | สาธารณะ | เข้าสู่ระบบ → `accessToken`, `refreshToken`, `user` |
| POST | `/auth/register` | สาธารณะ | สมัครสมาชิก (`role` ต้องเป็น `student`) → token คู่แรก, ตอบ `201` |
| POST | `/auth/refresh` | สาธารณะ | body `{ refreshToken }` → token คู่ใหม่ (ตัวเก่าถูก revoke) |
| POST | `/auth/logout` | สาธารณะ | body `{ refreshToken }` → revoke (เรียกซ้ำได้ ไม่ error) |
| GET | `/auth/me` | ทุกคน | ข้อมูลผู้ใช้ปัจจุบัน |
| PUT | `/auth/me` | ทุกคน | แก้ชื่อ นามสกุล เบอร์โทร |
| PUT | `/auth/change-password` | ทุกคน | `{ oldPassword, newPassword, confirmPassword }` → revoke refresh token ทั้งหมด |
| POST | `/auth/forgot-password` | สาธารณะ | `{ email }` → ตอบข้อความเดียวกันเสมอ (dev: ลิงก์อยู่ใน console) |
| POST | `/auth/reset-password` | สาธารณะ | `{ token, newPassword }` (≥ 8 ตัวอักษร) → ตั้งรหัสใหม่ + revoke refresh token |

## Attendance — `/api/attendance`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| POST | `/attendance/check-in` | S | ลงเวลาเข้า body `{ note? }` |
| POST | `/attendance/check-out` | S | ลงเวลาออก body `{ note? }` |
| GET | `/attendance?month=&year=&studentId=` | S, A, Ad | ประวัติรายเดือน · `studentId` ใช้ได้เฉพาะ advisor (นักศึกษาที่ดูแล) และ admin |
| GET | `/attendance/summary` | S | สรุปของตัวเอง |
| PUT | `/attendance/{id}/request-edit` | S | ขอแก้เวลา |
| PUT | `/attendance/{id}/review` | A | อนุมัติ/ปฏิเสธคำขอแก้เวลา |

## Leaves — `/api/leaves`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| GET | `/leaves/quota` | S | โควตาวันลาแต่ละประเภท (ใช้ไป/คงเหลือ) |
| GET | `/leaves` | S | ใบลาของตัวเอง |
| GET | `/leaves/pending` | A | ใบลารออนุมัติของนักศึกษาที่ดูแล |
| POST | `/leaves` | S | ยื่นใบลา — `multipart/form-data` (`leaveTypeID`, `startDate`, `endDate`, `reason`, `attachment?`) · นับเฉพาะวันจันทร์–ศุกร์ และตรวจโควตา |
| PUT | `/leaves/{id}/review` | A, Ad | `{ isApproved, reviewNote? }` |

## Work Logs — `/api/worklogs`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| GET | `/worklogs/categories` | ทุกคน | หมวดงานสำหรับ dropdown |
| GET | `/worklogs` | S | บันทึกงานของตัวเอง |
| GET | `/worklogs/all` | A, Ad | บันทึกงานของนักศึกษาที่ดูแล (admin: ทั้งหมด) |
| GET | `/worklogs/{id}` | S, A, Ad | รายละเอียด + ไฟล์แนบ + คอมเมนต์ |
| POST | `/worklogs` | S | สร้าง (ชั่วโมงคำนวณจาก attendance ฝั่ง server) |
| PUT | `/worklogs/{id}` | S | แก้ไข (เฉพาะของตัวเอง) |
| DELETE | `/worklogs/{id}` | S | ลบ (เฉพาะของตัวเอง) |
| POST | `/worklogs/{id}/attachments` | S | อัปโหลดไฟล์ `multipart/form-data` field `file` · ≤ 5 MB · `.pdf .jpg .jpeg .png .doc .docx` · ตรวจ magic bytes |
| DELETE | `/attachments/{id}` | S | ลบไฟล์แนบ (สังเกต: path ไม่มี `/worklogs` นำหน้า) |
| POST | `/worklogs/{id}/comments` | A, Ad | คอมเมนต์บันทึกงาน |

## Tasks — `/api/tasks`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| GET | `/tasks` | S, A, Ad | งานตามขอบเขตของ role |
| GET | `/tasks/{id}` | S, A, Ad | รายละเอียด |
| POST | `/tasks` | A, Ad | มอบหมายงาน (ผูกกับ `internshipId`) |
| PUT | `/tasks/{id}` | A, Ad | แก้ไขงาน |
| PUT | `/tasks/{id}/status` | S, A, Ad | เปลี่ยนสถานะ `{ status: "todo" \| "doing" \| "done" }` |
| DELETE | `/tasks/{id}` | A, Ad | ลบงาน |

## Advisor Visits — `/api/advisorvisits`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| GET | `/advisorvisits?internshipId=` | S, A | รายการการนิเทศของ internship |
| GET | `/advisorvisits/{id}` | S, A | รายละเอียด |
| POST | `/advisorvisits` | A | บันทึกการนิเทศ |
| PUT | `/advisorvisits/{id}` | A | แก้ไข |

## Reports — `/api/reports`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| GET | `/reports?internshipId=` | S, A | รายงานของ internship |
| POST | `/reports/generate` | S | สร้างรายงานตามช่วงวันที่ |
| PUT | `/reports/{id}/submit` | S | ส่งให้อาจารย์ |
| PUT | `/reports/{id}/review` | A | `{ status: "approved" \| "returned", reviewNote? }` |
| GET | `/reports/{id}/export-pdf` | S, A | สร้าง PDF แล้วคืน URL ของไฟล์ (`/uploads/reports/...`) |

## Evaluations — `/api/evaluations`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| GET | `/evaluations?internshipId=` | ทุกคน | ผลประเมินของ internship |
| GET | `/evaluations/{id}` | ทุกคน | รายละเอียด |
| POST | `/evaluations` | A, S | ส่งแบบประเมิน |

## Notifications — `/api/notifications`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| GET | `/notifications` | ทุกคน | แจ้งเตือนของตัวเอง |
| PUT | `/notifications/{id}/read` | ทุกคน | อ่านแล้ว 1 รายการ |
| PUT | `/notifications/read-all` | ทุกคน | อ่านแล้วทั้งหมด |

## Dashboard — `/api/dashboard`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| GET | `/dashboard` | ทุกคน | ตัวเลขสรุป — เนื้อหาต่างกันตาม role (student / advisor / admin) |
| GET | `/dashboard/calendar?year=&month=` | ทุกคน | ข้อมูลปฏิทินรายวัน (ปัจจุบันคืนข้อมูลเฉพาะ student; role อื่นได้รายการว่าง) |

## Companies — `/api/companies`

| Method | Path | Role | คำอธิบาย |
|---|---|---|---|
| GET | `/companies` | A, Ad | advisor: เฉพาะบริษัทที่มีนักศึกษาในความดูแล · admin: ทั้งหมด |
| GET | `/companies/{id}` | A, Ad | รายละเอียดบริษัท |

## Admin — `/api/admin` (admin เท่านั้น)

| Method | Path | คำอธิบาย |
|---|---|---|
| GET | `/admin/users?role=&search=` | รายชื่อผู้ใช้ (กรองตาม role / ค้นหา) |
| GET | `/admin/users/{id}` | รายละเอียดผู้ใช้ |
| POST | `/admin/users` | สร้างผู้ใช้ (กำหนด role ได้) |
| PUT | `/admin/users/{id}` | แก้ไขผู้ใช้ |
| DELETE | `/admin/users/{id}` | ปิดใช้งานผู้ใช้ (soft delete: `IsActive = false`) |
| GET | `/admin/companies` | รายชื่อบริษัท |
| POST | `/admin/companies` | เพิ่มบริษัท |
| PUT | `/admin/companies/{id}` | แก้ไขบริษัท |
| DELETE | `/admin/companies/{id}` | ลบบริษัท |
| GET | `/admin/internships` | รายการ internship ทั้งหมด |
| POST | `/admin/internships` | จับคู่นักศึกษา–อาจารย์–บริษัท–ภาคเรียน |

## Static files

| Path | ที่มา |
|---|---|
| `/uploads/leaves/*` | ไฟล์แนบใบลา |
| `/uploads/worklogs/*` | ไฟล์แนบ Work Log |
| `/uploads/reports/*` | PDF รายงาน |

> ⚠️ path เหล่านี้เปิดให้โหลดได้โดยไม่ต้อง login — ดู [project-status.md](project-status.md)
