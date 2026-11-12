# เริ่มต้นใช้งาน (Getting Started)

คู่มือรันระบบบนเครื่อง development (Windows)

## สิ่งที่ต้องติดตั้ง

| เครื่องมือ | เวอร์ชัน | ใช้ทำอะไร |
|---|---|---|
| .NET SDK | 10.x | รัน Backend (`InternLog.API`) |
| SQL Server | 2019+ หรือ Express/LocalDB | ฐานข้อมูล `InternLogDB` |
| Node.js | 18.13+ หรือ 20.x | รัน Frontend (`InternLog.Web`) |
| Angular CLI | 17.x (ติดตั้งมาพร้อม `npm install`) | `ng serve` / `ng build` |

## 1. ตั้งค่า Backend

### 1.1 Connection string

ค่าเริ่มต้นใน `InternLog.API/appsettings.json` เชื่อม SQL Server บน `localhost` ด้วย Windows Authentication:

```
Server=localhost;Database=InternLogDB;Trusted_Connection=True;TrustServerCertificate=True;
```

ถ้าเครื่องใช้ instance อื่น (เช่น `localhost\SQLEXPRESS`) ให้ override ด้วย user-secrets แทนการแก้ไฟล์:

```powershell
cd InternLog.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=InternLogDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 1.2 JWT signing key (จำเป็น)

`appsettings.json` **ไม่มี** `Jwt:Key` และถ้าไม่ตั้งค่า API จะไม่ start (`JWT Key ไม่ได้ตั้งค่าใน appsettings`)
ตั้งผ่าน user-secrets (ความยาวอย่างน้อย 32 ตัวอักษร):

```powershell
cd InternLog.API
dotnet user-secrets set "Jwt:Key" "<สุ่มสตริงยาวอย่างน้อย 32 ตัวอักษร>"
```

> ห้าม commit key ลง `appsettings.json` — ไฟล์ `appsettings.Development.json` ถูก ignore ไว้แล้วเช่นกัน

### 1.3 รัน API

```powershell
cd InternLog.API
dotnet run --launch-profile http
```

- API: `http://localhost:5005`
- Swagger UI: `http://localhost:5005/swagger` (เฉพาะ Development)

เมื่อรันใน Development ระบบจะทำงานอัตโนมัติ:
1. `Database.Migrate()` — สร้าง/อัปเดตฐานข้อมูลตาม `InternLog.API/Migrations/`
   ซึ่งใส่ข้อมูลตั้งต้น (`HasData` ใน `AppDbContext`) ไว้แล้ว: roles 4 แบบ, ประเภทการลา 3 แบบ, หมวดงาน 5 หมวด และ admin `admin@internlog.dev`
2. Seed ข้อมูลทดสอบใน `Program.cs`: ผู้ใช้ทดสอบ 3 คน, บริษัท 2 แห่ง, ภาคเรียน 1/2569 และ internship 1 รายการ (student ↔ advisor)

ถ้า migration ล้มเหลว จะเห็น `⚠️ Migration/Seed error: ...` ใน console แต่ API ยังรันต่อ

## 2. ตั้งค่า Frontend

```powershell
cd InternLog.Web
npm install
npm start
```

- เว็บ: `http://localhost:4200`
- URL ของ API กำหนดที่ `src/environments/environment.ts` (`apiUrl: 'http://localhost:5005/api'`)
- CORS ฝั่ง API อนุญาตเฉพาะ `http://localhost:4200` ถ้าเปลี่ยน port ต้องแก้ policy `"Angular"` ใน `InternLog.API/Program.cs` ด้วย

## 3. บัญชีทดสอบ

| Email | Password | Role |
|---|---|---|
| `student@test.com` | `Test@1234` | student |
| `advisor@test.com` | `Test@1234` | advisor (ดูแล student ข้างบน) |
| `admin@test.com` | `Test@1234` | admin |
| `admin@internlog.dev` | `Admin@1234!` | admin (มาจาก migration) |

## 4. การทดสอบระบบลืมรหัสผ่าน

ยังไม่มีระบบส่งอีเมล ลิงก์รีเซ็ตจะถูกพิมพ์ใน console ของ API:

```
[DEV] Reset link: http://localhost:4200/reset-password?token=...
```

ลิงก์มีอายุ 1 ชั่วโมง (ดูข้อจำกัดใน [project-status.md](project-status.md) — หน้า reset-password ฝั่งเว็บยังไม่มี)

## 5. คำสั่งที่ใช้บ่อย

| งาน | คำสั่ง (รันในโฟลเดอร์ที่ระบุ) |
|---|---|
| Build API | `InternLog.API> dotnet build` |
| สร้าง migration ใหม่ | `InternLog.API> dotnet ef migrations add <ชื่อ>` |
| Apply migration ด้วยมือ | `InternLog.API> dotnet ef database update` |
| Build เว็บ | `InternLog.Web> npm run build` |
| Unit test เว็บ | `InternLog.Web> npm test` |

## 6. ปัญหาที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| API start ไม่ได้: `JWT Key ไม่ได้ตั้งค่า` | ยังไม่ได้ทำข้อ 1.2 |
| `Migration/Seed error: A network-related ... error` | SQL Server ไม่ได้รัน หรือชื่อ instance ไม่ตรง → ข้อ 1.1 |
| หน้าเว็บขึ้น CORS error | API ไม่ได้รันที่ `5005` หรือเว็บไม่ได้รันที่ `4200` |
| Login แล้วเด้งกลับหน้า login | Access token หมดอายุและ refresh ไม่สำเร็จ ลองล้าง localStorage แล้ว login ใหม่ |
| `npm install` ช้า/ล้มเหลวบนโฟลเดอร์ OneDrive | ย้ายโปรเจกต์ออกจาก OneDrive (เคยเกิดปัญหานี้ ดู progress-log 26 ส.ค.) |
