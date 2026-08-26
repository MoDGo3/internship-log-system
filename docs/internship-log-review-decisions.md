# 📝 ระบบจดบันทึกการฝึกงาน — เอกสารส่วนที่ 4
> บันทึกการตัดสินใจแก้ไข (Review Decisions & Change Log v2)
> จัดทำเมื่อ 14 สิงหาคม 2569 | ชื่อไฟล์แนะนำ: `internship-log-review-decisions.md`
>
> **แผนที่เอกสารทั้งชุด**
> - ส่วนที่ 1 `internship-log-system-docs.md` → โครงสร้างระบบ + ERD + Backend
> - ส่วนที่ 2 `internship-log-ui-design.md` → UI Design 20 หน้า
> - ส่วนที่ 3 `internship-log-analysis-sql.md` → Use Cases/SRS + Sequence + SQL
> - ส่วนที่ 4 `internship-log-review-decisions.md` → เอกสารฉบับนี้ (มติแก้ไขรอบ Review)

---

## 1. ตารางสรุปมติการแก้ไข

| # | ประเด็นที่ตรวจพบ | มติ | ประเภทการแก้ |
|---|---|---|---|
| 2 | WorkLogs.Hours ระบุว่า "ดึงจาก Attendances" แต่ไม่มี FK | Backend computed — ฝั่ง server เติมค่าเอง, ตัดฟิลด์ Hours ออกจาก DTO ฝั่งรับ (กันปลอมแปลง) | Logic + เอกสาร |
| 3 | AdvisorVisits.PersonNo กำกวม | PersonNo = ลำดับนักศึกษาในกลุ่มนิเทศชุดเดียวกัน, VisitNo = ครั้งที่นิเทศของ internship นั้น + UNIQUE (InternshipID, VisitNo) | นิยาม + Constraint |
| 4 | ไม่มี RefreshToken — JWT 8 ชม. อาจหมดอายุกลางงาน | เพิ่มตาราง RefreshTokens (ตารางที่ 19) + rotation • ปรับอายุเป็น Access 2 ชม. / Refresh 7 วัน • เพิ่ม endpoint refresh/logout | Schema + API + NFR |
| 5 | Tasks ไม่มี FK → Internships (เสี่ยงมอบหมายผิดคน) | แทนที่ InternID ด้วย InternshipID — ผู้รับงานได้จาก Internships.StudentID | Schema |

> ผลรวม: ฐานข้อมูลเพิ่มเป็น **19 ตาราง**

---

## 2. รายละเอียดแต่ละมติ

### 2.1 [#2] WorkLogs.Hours = Backend Computed
- Frontend **ไม่ส่ง** Hours ใน request — backend เติมจาก Attendances ของ (StudentID + LogDate) ซึ่งเป็น UNIQUE อยู่แล้ว จึงไม่ต้องเพิ่ม FK
- เมื่อคำขอแก้เวลาได้รับอนุมัติ → ให้ sync ค่า Hours ของ WorkLogs วันนั้นใหม่ทั้งหมด

```csharp
// WorkLogsService — ตอนสร้าง/ส่งบันทึก
var att = await db.Attendances.FirstOrDefaultAsync(a => a.StudentID == sid && a.AttDate == log.LogDate);
log.Hours = att?.TotalHours;

// AttendanceService.ReviewEditAsync — หลังอนุมัติแก้เวลา
var logs = await db.WorkLogs.Where(w => w.StudentID == sid && w.LogDate == date).ToListAsync();
logs.ForEach(w => w.Hours = att.TotalHours);
await db.SaveChangesAsync();
```

### 2.2 [#3] นิยาม PersonNo / VisitNo
- **PersonNo** = ลำดับที่ของนักศึกษาในกลุ่มนิเทศชุดเดียวกัน (อาจารย์คนเดียวกัน + ภาคเรียนเดียวกัน)
- **VisitNo** = ครั้งที่ของการนิเทศสำหรับ internship นั้น
- บังคับ `UNIQUE (InternshipID, VisitNo)` กันบันทึกครั้งที่ซ้ำ

### 2.3 [#4] Refresh Token (rotation + เก็บ hash)
**Flow ใหม่:**
```
login  → { accessToken (2 ชม.), refreshToken (7 วัน), user }
401    → Angular interceptor เรียก POST /api/auth/refresh → ได้คู่ใหม่
          (token เก่าถูก revoke = rotation) → retry คำขอเดิมอัตโนมัติ
logout → POST /api/auth/logout → revoke token
```
- เก็บเฉพาะ **hash** ของ refreshToken ใน DB (ไม่เก็บตัวดิบ)
- เพิกถอนได้รายเครื่อง/รายครั้ง รองรับ logout ปลอดภัย

### 2.4 [#5] Tasks ผูก Internship โดยตรง
- ตัด `InternID` ออก → เพิ่ม `InternshipID` (FK → Internships)
- ผู้รับงาน = `Internships.StudentID` (ได้จากการ JOIN) → supervisor มอบหมายข้ามคนไม่ได้
- API ปรับ: POST /api/tasks รับ `internshipId` แทน `internId`

---

## 3. SQL ส่วนที่เปลี่ยน (ใช้แทน/เพิ่มในสคริปต์ส่วนที่ 3)

> ถ้าสร้าง DB ไปแล้วและ Tasks ยังไม่มีข้อมูล: `DROP TABLE Tasks;` ก่อนแล้วสร้างใหม่

```sql
-- ===== Tasks ฉบับแก้แล้ว (แทนฉบับเดิม) =====
CREATE TABLE Tasks (
  TaskID INT IDENTITY(1,1) PRIMARY KEY,
  InternshipID INT NOT NULL REFERENCES Internships(InternshipID),
  AssignedBy INT NOT NULL REFERENCES Users(UserID),
  Title NVARCHAR(200) NOT NULL,
  Detail NVARCHAR(MAX) NULL,
  DueDate DATE NULL,
  Priority NVARCHAR(10) NOT NULL DEFAULT 'medium',
  Status NVARCHAR(20) NOT NULL DEFAULT 'todo',
  CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- ===== ตารางที่ 19: RefreshTokens (เพิ่มใหม่) =====
CREATE TABLE RefreshTokens (
  RefreshTokenID INT IDENTITY(1,1) PRIMARY KEY,
  UserID INT NOT NULL REFERENCES Users(UserID),
  TokenHash NVARCHAR(255) NOT NULL,
  ExpiresAt DATETIME NOT NULL,
  RevokedAt DATETIME NULL,
  ReplacedByTokenID INT NULL,
  CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
CREATE INDEX IX_RefreshTokens_User ON RefreshTokens(UserID);

-- ===== Constraint เพิ่มใน AdvisorVisits =====
ALTER TABLE AdvisorVisits
  ADD CONSTRAINT UQ_Visit_No UNIQUE (InternshipID, VisitNo);
GO
```

**ความสัมพันธ์ที่เพิ่ม:**
```
Users ─< RefreshTokens
Internships ─< Tasks   (แทน Users ─< Tasks เดิมฝั่งผู้รับ)
```

---

## 4. การเปลี่ยนแปลง API

| เปลี่ยนแปลง | รายละเอียด |
|---|---|
| POST /api/auth/login | response เป็น `{ accessToken, refreshToken, user }` |
| POST /api/auth/refresh | เพิ่มใหม่ — รับ refreshToken → ออกคู่ใหม่ + revoke ตัวเก่า |
| POST /api/auth/logout | เพิ่มใหม่ — revoke refreshToken |
| WorkLogs DTO ฝั่งรับ | ตัดฟิลด์ `hours` ออก (backend เติมเอง) |
| POST /api/tasks | รับ `internshipId` แทน `internId` |

---

## 5. อัปเดต NFR (Security)

- เดิม: "JWT หมดอายุ 8 ชม."
- ใหม่: **AccessToken 2 ชม. + RefreshToken 7 วัน (rotation, เพิกถอนได้)**, เก็บ refresh เป็น hash, silent refresh ผ่าน interceptor

---

## 6. เช็กลิสต์ Patch เอกสารส่วนที่ 1–3

**ส่วนที่ 1 (system-docs)**
- [ ] WorkLogs: หมายเหตุ Hours = backend computed, ตัดออกจาก DTO รับ
- [ ] AdvisorVisits: นิยาม PersonNo/VisitNo + unique constraint
- [ ] เพิ่มตาราง RefreshTokens (ตารางที่ 19) + ความสัมพันธ์
- [ ] Tasks: InternshipID แทน InternID
- [ ] เพิ่ม endpoint /api/auth/refresh, /api/auth/logout + response login ใหม่
- [ ] NFR Security อายุ token ฉบับใหม่

**ส่วนที่ 2 (ui-design)**
- [ ] ไม่มีผลต่อโครงสร้าง UI — เพิ่มหมายเหตุ: auth service Angular เก็บ refresh token + interceptor จัดการ 401

**ส่วนที่ 3 (analysis-sql)**
- [ ] แทน CREATE TABLE Tasks ฉบับแก้
- [ ] เพิ่ม CREATE TABLE RefreshTokens + index
- [ ] เพิ่ม ALTER AdvisorVisits (UQ_Visit_No)
- [ ] อัปเดตลำดับ Sequence 3.1 (login คืน refresh token + flow refresh)

---

## 7. ผลต่อฝั่ง Angular (บันทึกไว้ตอนเขียนโค้ด)

- `AuthService.login()` เก็บ `accessToken` + `refreshToken`
- `HttpInterceptor`: แนบ Bearer ทุกคำขอ • เจอ 401 → เรียก refresh ครั้งเดียว (queue คำขอที่ค้าง) → retry
- `logout()` เรียก API revoke แล้วล้าง token ในเครื่อง