# 📘 คู่มือเชื่อมต่อ API โมดูลที่ 4: Work Logs (บันทึกรายวัน)
> เอกสารสำหรับ Frontend Developer เพื่อนำไปเชื่อมต่อกับหน้าจอ UI

เอกสารนี้รวบรวม **API Endpoints** และ **Data Models** ของระบบบันทึกการทำงานรายวัน (Work Logs) ที่พร้อมใช้งานแล้วในฝั่ง Backend โดยทุก Request ต้องแนบ **JWT Token** ใน Header (`Authorization: Bearer <token>`) เสมอ

---

## 📦 1. Data Models (TypeScript Interfaces)
ก่อนเริ่มเชื่อมต่อ API ให้นำ Models เหล่านี้ไปวางในไฟล์ `src/app/features/work-logs/models/work-log.model.ts`

```typescript
export interface WorkCategory {
  categoryID: number;
  categoryName: string;
}

export interface Attachment {
  attachmentID: number;
  fileName: string;
  filePath: string;
  fileType?: string;
  fileSize?: number;
  uploadedAt: string;
}

export interface LogComment {
  commentID: number;
  message: string;
  createdAt: string;
  userID: number;
  userName: string;
  userRole: string;
}

export interface WorkLog {
  logID: number;
  studentID: number;
  studentName: string;
  logDate: string;
  title: string;
  description: string;
  problem?: string;
  solution?: string;
  categoryID?: number;
  categoryName?: string;
  hours?: number;
  status: 'draft' | 'pending' | 'approved' | 'rejected';
  createdAt: string;
  updatedAt?: string;
  attachments: Attachment[];
  comments: LogComment[];
}
```

---

## 🚀 2. API Endpoints

ทุก Endpoint จะใช้ Base URL คือ `/api/worklogs` และตอบกลับมาในรูปแบบ `ApiResponse<T>`:
```json
{
  "success": true,
  "message": "...",
  "data": { ... }
}
```

### 📋 2.1 ดึงข้อมูลหมวดหมู่งาน (Dropdown)
- **Method:** `GET`
- **Endpoint:** `/api/worklogs/categories`
- **Role:** ทุกคน (Student, Advisor)
- **Response Data:** `WorkCategory[]`

### 📝 2.2 ดึงรายการบันทึกงานของตัวเอง
- **Method:** `GET`
- **Endpoint:** `/api/worklogs?year={year}&month={month}`
- **Role:** `student` (นักศึกษา)
- **Query Params:** `year` และ `month` (ระบุ 0 เพื่อดึงทั้งหมด)
- **Response Data:** `WorkLog[]`

### 🔍 2.3 ดูรายละเอียดบันทึกงาน (ดึง 1 รายการ)
- **Method:** `GET`
- **Endpoint:** `/api/worklogs/{id}`
- **Role:** ทุกคน
- **Response Data:** `WorkLog` (ข้อมูลจะรวม ไฟล์แนบ และ คอมเมนต์ ทั้งหมดแล้ว)

### ➕ 2.4 สร้างบันทึกงานใหม่ (รองรับการแนบไฟล์)
- **Method:** `POST`
- **Endpoint:** `/api/worklogs`
- **Role:** `student`
- **Request Type:** `multipart/form-data` (เนื่องจากมีการส่งไฟล์)
- **ข้อมูลที่ต้องส่ง (FormData):**
  - `logDate` (string, ISO Date) - *Required*
  - `title` (string) - *Required*
  - `description` (string) - *Required*
  - `problem` (string) - *Optional*
  - `solution` (string) - *Optional*
  - `categoryID` (number) - *Optional*
  - `hours` (number) - *Optional*
  - `status` (string) - "draft" หรือ "pending"
  - `attachments` (File[]) - *Optional* (แนบได้หลายไฟล์)
- **Response Data:** `WorkLog`

### ✏️ 2.5 แก้ไขบันทึกงาน (รองรับการเพิ่ม/ลบไฟล์)
- **Method:** `PUT`
- **Endpoint:** `/api/worklogs/{id}`
- **Role:** `student`
- **Request Type:** `multipart/form-data`
- **ข้อมูลที่ต้องส่ง (FormData):**
  - *(สามารถส่งฟิลด์เหมือนตอนสร้างได้เลยเพื่ออัปเดตค่า)*
  - `newAttachments` (File[]) - ไฟล์ใหม่ที่ต้องการแนบเพิ่ม
  - `removedAttachmentIds` (number[]) - ID ของไฟล์เก่าที่ถูกลบออก (ส่งมาแบบ Array)
- **Response Data:** `WorkLog`

### 🗑️ 2.6 ลบบันทึกงาน
- **Method:** `DELETE`
- **Endpoint:** `/api/worklogs/{id}`
- **Role:** `student`
- **Response Data:** `boolean` (true หากสำเร็จ)

---

## 👨‍🏫 3. API สำหรับอาจารย์ (Advisor)

### 👥 3.1 ดึงรายการงานที่รอตรวจ (Pending) ของนักศึกษาในที่ปรึกษา
- **Method:** `GET`
- **Endpoint:** `/api/worklogs/pending`
- **Role:** `advisor`
- **Response Data:** `WorkLog[]`

### 🎓 3.2 ดึงรายการงานทั้งหมดของนักศึกษา (รายบุคคล)
- **Method:** `GET`
- **Endpoint:** `/api/worklogs/student/{studentId}?year={year}&month={month}`
- **Role:** `advisor`
- **Response Data:** `WorkLog[]`

### ✔️ 3.3 เปลี่ยนสถานะงาน (อนุมัติ / ปฏิเสธ)
- **Method:** `PUT`
- **Endpoint:** `/api/worklogs/{id}/status`
- **Role:** `advisor`
- **Request Type:** `application/json`
- **Body:** `{ "status": "approved" | "rejected" }`
- **Response Data:** `boolean`

### 💬 3.4 เพิ่มคอมเมนต์ (แนะนำ / ตอบกลับ)
- **Method:** `POST`
- **Endpoint:** `/api/worklogs/{id}/comments`
- **Role:** `advisor` (จริงๆ นักศึกษาก็พิมพ์คุยตอบกลับได้)
- **Request Type:** `application/json`
- **Body:** `{ "message": "ข้อความคอมเมนต์..." }`
- **Response Data:** `LogComment`

---

## 💡 แนวทางการเขียนใน Angular Service

เมื่อต้องยิง API ข้อ 2.4 หรือ 2.5 **ต้องใช้ FormData** ในการส่งข้อมูลตัวอย่างเช่น:

```typescript
// ตัวอย่างการสร้าง WorkLog พร้อมอัปโหลดไฟล์
createLog(data: any, files: File[]): Observable<ApiResponse<WorkLog>> {
  const formData = new FormData();
  formData.append('logDate', data.logDate);
  formData.append('title', data.title);
  formData.append('description', data.description);
  formData.append('status', data.status);
  
  if (data.categoryID) formData.append('categoryID', data.categoryID);
  if (data.hours) formData.append('hours', data.hours);
  
  // แนบไฟล์
  files.forEach(file => {
    formData.append('attachments', file, file.name);
  });

  return this.http.post<ApiResponse<WorkLog>>('/api/worklogs', formData);
}
```
