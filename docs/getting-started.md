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

เปิดลิงก์นั้นในเบราว์เซอร์จะเข้าหน้าตั้งรหัสผ่านใหม่ ลิงก์มีอายุ 1 ชั่วโมงและใช้ได้ครั้งเดียว

## 5. คำสั่งที่ใช้บ่อย

| งาน | คำสั่ง (รันในโฟลเดอร์ที่ระบุ) |
|---|---|
| Build API | `InternLog.API> dotnet build` |
| Test API (42 tests) | `> dotnet test` (ที่ root ของ repo ใช้ `InternLog.slnx`) |
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
| Login แล้วเด้งกลับหน้า login | refresh token cookie หมดอายุหรือถูก revoke → login ใหม่ · ถ้าเปิดเว็บกับ API คนละ host (เช่น `127.0.0.1` กับ `localhost`) cookie จะไม่ถูกส่ง ให้ใช้ `localhost` ทั้งคู่ |
| รัน API ไม่ได้: `An Application Control policy has blocked this file` | Windows Smart App Control บล็อก DLL ที่เพิ่ง build — รันผ่าน Visual Studio หรือปิด Smart App Control สำหรับเครื่อง dev |
| `npm install` ช้า/ล้มเหลวบนโฟลเดอร์ OneDrive | ย้ายโปรเจกต์ออกจาก OneDrive (เคยเกิดปัญหานี้ ดู progress-log 26 ส.ค.) |

## 7. ไฟล์อัปโหลด

ไฟล์แนบใบลา, ไฟล์แนบ Work Log และ PDF รายงาน เก็บที่ `InternLog.API/App_Data/uploads/` (อยู่ใน `.gitignore`)
เปลี่ยนตำแหน่งได้ด้วย `FileStorage:Root` ใน appsettings/user-secrets · ไฟล์ดาวน์โหลดได้เฉพาะผ่าน API ที่ตรวจสิทธิ์ ไม่มี URL สาธารณะ

## 8. ทดสอบหลังแก้ไข (12 พ.ย. 2569)

`dotnet test` ครอบคลุม logic และ HTTP pipeline แล้ว แต่ควรทดสอบผ่านเบราว์เซอร์อีกรอบ:

- [ ] Login ด้วย `student@test.com` → DevTools › Application › Cookies (`localhost:5005`) มี `refreshToken` แบบ HttpOnly และ localStorage **ไม่มี** `refreshToken`
- [ ] ลบ `accessToken` ใน localStorage แล้วรีเฟรชหน้า → ยังอยู่ในระบบ (guard ต่ออายุด้วย cookie)
- [ ] กรอกรหัสผิดที่หน้า login → ขึ้นข้อความผิดพลาด ไม่เด้ง/วน logout
- [ ] Logout → รีเฟรชหน้า → ถูกพาไปหน้า login
- [ ] สร้าง Work Log + แนบ PDF → เปิดหน้าแก้ไข คลิกชื่อไฟล์ → เปิดไฟล์ในแท็บใหม่ได้
- [ ] ยื่นใบลาพร้อมไฟล์ (ผ่าน Swagger/`.http` เพราะฟอร์มยังไม่มีช่องแนบ) → หน้าใบลามีปุ่ม 📎 เปิดไฟล์ได้
- [ ] หน้า `/reports/1/preview` → ปุ่มดาวน์โหลด PDF ได้ไฟล์ `report-1.pdf`
- [ ] Login เป็น `advisor@test.com` และ `admin@test.com` → แดชบอร์ดแสดงตัวเลขสรุปของ role นั้น
- [ ] ขอลิงก์รีเซ็ตรหัสผ่าน → เปิดลิงก์จาก console → ตั้งรหัสใหม่ → login ด้วยรหัสใหม่ได้
- [ ] หน้า admin users แสดง role ของผู้ใช้ครบ
- [ ] `dotnet ef migrations has-pending-model-changes` (ใน `InternLog.API`) ต้องตอบว่าไม่มีการเปลี่ยนแปลง

