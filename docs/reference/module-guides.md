# คู่มือรายโมดูล (Module Guides)

_คู่มือเชื่อมต่อ API และงาน Frontend ของโมดูลที่มีเอกสารเฉพาะ_

> ⚠️ **เอกสารอ้างอิงจากช่วงออกแบบ/พัฒนา** — รวมจากไฟล์เดิมหลายไฟล์ เนื้อหาคงไว้ตามต้นฉบับ
> บางส่วนอาจไม่ตรงกับโค้ดปัจจุบัน (เช่น ระบุ .NET 8 แต่ใช้ .NET 10 จริง, โมดูล Messages ถูกลบแล้ว,
> ระบบคำนวณมาสายถูกยกเลิก) — ข้อมูลที่ตรงกับโค้ดล่าสุดดูที่ [architecture.md](../architecture.md), [api-reference.md](../api-reference.md) และ [project-status.md](../project-status.md)

## สารบัญ

1. [โมดูล 4: Work Logs — คู่มือเชื่อมต่อ API](#ส่วนที่-1-โมดูล-4-work-logs--คู่มือเชื่อมต่อ-api)
2. [โมดูล 5: Tasks / Kanban — งานฝั่ง Frontend](#ส่วนที่-2-โมดูล-5-tasks--kanban--งานฝั่ง-frontend)

---

## ส่วนที่ 1: โมดูล 4: Work Logs — คู่มือเชื่อมต่อ API

> ต้นฉบับ: `docs/module4_worklogs_api_guide.md` · จัดทำ 18 ส.ค. 2569

เอกสารนี้รวบรวม **API Endpoints** และ **Data Models** ของระบบบันทึกการทำงานรายวัน (Work Logs) ที่พร้อมใช้งานแล้วในฝั่ง Backend โดยทุก Request ต้องแนบ **JWT Token** ใน Header (`Authorization: Bearer <token>`) เสมอ

---

### 📦 1. Data Models (TypeScript Interfaces)
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

### 🚀 2. API Endpoints

ทุก Endpoint จะใช้ Base URL คือ `/api/worklogs` และตอบกลับมาในรูปแบบ `ApiResponse<T>`:
```json
{
  "success": true,
  "message": "...",
  "data": { ... }
}
```

#### 📋 2.1 ดึงข้อมูลหมวดหมู่งาน (Dropdown)
- **Method:** `GET`
- **Endpoint:** `/api/worklogs/categories`
- **Role:** ทุกคน (Student, Advisor)
- **Response Data:** `WorkCategory[]`

#### 📝 2.2 ดึงรายการบันทึกงานของตัวเอง
- **Method:** `GET`
- **Endpoint:** `/api/worklogs?year={year}&month={month}`
- **Role:** `student` (นักศึกษา)
- **Query Params:** `year` และ `month` (ระบุ 0 เพื่อดึงทั้งหมด)
- **Response Data:** `WorkLog[]`

#### 🔍 2.3 ดูรายละเอียดบันทึกงาน (ดึง 1 รายการ)
- **Method:** `GET`
- **Endpoint:** `/api/worklogs/{id}`
- **Role:** ทุกคน
- **Response Data:** `WorkLog` (ข้อมูลจะรวม ไฟล์แนบ และ คอมเมนต์ ทั้งหมดแล้ว)

#### ➕ 2.4 สร้างบันทึกงานใหม่ (รองรับการแนบไฟล์)
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

#### ✏️ 2.5 แก้ไขบันทึกงาน (รองรับการเพิ่ม/ลบไฟล์)
- **Method:** `PUT`
- **Endpoint:** `/api/worklogs/{id}`
- **Role:** `student`
- **Request Type:** `multipart/form-data`
- **ข้อมูลที่ต้องส่ง (FormData):**
  - *(สามารถส่งฟิลด์เหมือนตอนสร้างได้เลยเพื่ออัปเดตค่า)*
  - `newAttachments` (File[]) - ไฟล์ใหม่ที่ต้องการแนบเพิ่ม
  - `removedAttachmentIds` (number[]) - ID ของไฟล์เก่าที่ถูกลบออก (ส่งมาแบบ Array)
- **Response Data:** `WorkLog`

#### 🗑️ 2.6 ลบบันทึกงาน
- **Method:** `DELETE`
- **Endpoint:** `/api/worklogs/{id}`
- **Role:** `student`
- **Response Data:** `boolean` (true หากสำเร็จ)

---

### 👨‍🏫 3. API สำหรับอาจารย์ (Advisor)

#### 👥 3.1 ดึงรายการงานที่รอตรวจ (Pending) ของนักศึกษาในที่ปรึกษา
- **Method:** `GET`
- **Endpoint:** `/api/worklogs/pending`
- **Role:** `advisor`
- **Response Data:** `WorkLog[]`

#### 🎓 3.2 ดึงรายการงานทั้งหมดของนักศึกษา (รายบุคคล)
- **Method:** `GET`
- **Endpoint:** `/api/worklogs/student/{studentId}?year={year}&month={month}`
- **Role:** `advisor`
- **Response Data:** `WorkLog[]`

#### ✔️ 3.3 เปลี่ยนสถานะงาน (อนุมัติ / ปฏิเสธ)
- **Method:** `PUT`
- **Endpoint:** `/api/worklogs/{id}/status`
- **Role:** `advisor`
- **Request Type:** `application/json`
- **Body:** `{ "status": "approved" | "rejected" }`
- **Response Data:** `boolean`

#### 💬 3.4 เพิ่มคอมเมนต์ (แนะนำ / ตอบกลับ)
- **Method:** `POST`
- **Endpoint:** `/api/worklogs/{id}/comments`
- **Role:** `advisor` (จริงๆ นักศึกษาก็พิมพ์คุยตอบกลับได้)
- **Request Type:** `application/json`
- **Body:** `{ "message": "ข้อความคอมเมนต์..." }`
- **Response Data:** `LogComment`

---

### 💡 แนวทางการเขียนใน Angular Service

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

---

## ส่วนที่ 2: โมดูล 5: Tasks / Kanban — งานฝั่ง Frontend

> ต้นฉบับ: `docs/module5_tasks_frontend_guide.md` · จัดทำ 18 ส.ค. 2569

เอกสารนี้ระบุ **"Checklist งานที่ต้องทำ (To-Do)"** สำหรับ Frontend Developer ในการนำ `TaskService` และ `TaskModel` ที่เตรียมไว้ไปผูกกับหน้าจอ UI (Component) ของระบบจัดการงาน (Kanban Board)

---

### 🎯 1. หน้าจอที่เกี่ยวข้อง (UI Components)
คุณจะต้องเข้าไปจัดการโค้ดใน Components ที่เกี่ยวข้องกับระบบ Tasks (Kanban) โดยหลักๆ จะมีดังนี้:
- **Task Board (Kanban):** หน้าจอแสดงคอลัมน์ Todo, In Progress, Done
- **Task Modal (Dialog):** หน้าต่าง Popup สำหรับสร้างงานใหม่ หรือแก้ไขงานเดิม

---

### 🛠️ 2. Checklist สิ่งที่ต้องทำ (Step-by-Step)

#### [ ] 1. Inject Service เข้า Component
ที่ไฟล์ `task-board.component.ts` (หรือชื่อไฟล์ที่ใกล้เคียง) ให้ทำการดึง Service เข้ามาใช้งาน:
```typescript
import { inject, OnInit } from '@angular/core';
import { TaskService } from '@features/tasks/services/task.service';
import { Task } from '@features/tasks/models/task.model';

export class TaskBoardComponent implements OnInit {
  private taskService = inject(TaskService);
  tasks: Task[] = []; // เก็บรายการงานทั้งหมด

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.taskService.getMyTasks().subscribe({
      next: (res) => {
        if (res.success) this.tasks = res.data;
      },
      error: (err) => console.error(err)
    });
  }
}
```

#### [ ] 2. กรองข้อมูลลงแต่ละคอลัมน์ (Kanban Logic)
ในหน้าจอ HTML หรือใน TypeScript ให้แยก `tasks` ออกเป็น 3 กลุ่มตามฟิลด์ `status`:
```typescript
get todoTasks() { return this.tasks.filter(t => t.status === 'todo'); }
get inProgressTasks() { return this.tasks.filter(t => t.status === 'in_progress'); }
get doneTasks() { return this.tasks.filter(t => t.status === 'done'); }
```
แล้วนำไปวนลูป `@for (task of todoTasks; track task.taskID)` ในหน้า HTML

#### [ ] 3. ผูกระบบลากวาง (Drag & Drop) หรือ ปรับสถานะ
หากมีการลากการ์ดงาน (Drag & Drop) เปลี่ยนคอลัมน์ หรือกดปุ่มเปลี่ยนสถานะ ให้เรียกฟังก์ชัน `changeStatus`:
```typescript
onDropTask(taskId: number, newStatus: string) {
  this.taskService.changeStatus(taskId, newStatus).subscribe({
    next: (res) => {
      // แจ้งเตือนสำเร็จ
      this.loadTasks(); // โหลดข้อมูลใหม่เพื่อรีเฟรชบอร์ด
    }
  });
}
```

#### [ ] 4. ผูกฟอร์มสร้างงานใหม่ (Create Task)
เมื่อผู้ใช้กดปุ่ม **"สร้างงานใหม่"** และกรอกข้อมูลเสร็จ ให้ส่งข้อมูลเป็น Object ตาม Model `CreateTaskRequest`:
```typescript
onSubmitTask() {
  const payload = {
    internshipID: this.currentInternshipId, // ดึง ID การฝึกงานของนศ.คนนั้นมา
    title: this.form.value.title,
    detail: this.form.value.detail,
    dueDate: this.form.value.dueDate,
    priority: this.form.value.priority || 'medium',
    status: 'todo' // ค่าตั้งต้น
  };

  this.taskService.createTask(payload).subscribe({
    next: (res) => {
      // ปิด Modal และโหลดบอร์ดใหม่
      this.loadTasks();
    }
  });
}
```

#### [ ] 5. ผูกปุ่มลบงาน (Delete)
เพิ่มปุ่มลบ (รูปถังขยะ) ในการ์ดแต่ละใบ เมื่อกดให้ยืนยันการลบก่อนเรียก API:
```typescript
deleteTask(taskId: number) {
  if(confirm('ต้องการลบงานนี้ใช่หรือไม่?')) {
    this.taskService.deleteTask(taskId).subscribe({
      next: () => this.loadTasks()
    });
  }
}
```

---

### 🎨 3. ข้อแนะนำด้าน UI / UX (สีสันและไอคอน)
ตอน Render การ์ดแต่ละใบ แนะนำให้ใช้ Badge สีตาม **Priority** เพื่อให้ง่ายต่อการมองเห็น:
- **Low:** สีเทา (`bg-secondary` หรือ `text-muted`)
- **Medium:** สีเหลือง/ส้ม (`bg-warning`)
- **High:** สีแดง (`bg-danger`)

**ตัวอย่างใน HTML:**
```html
<div class="badge" 
     [class.bg-danger]="task.priority === 'high'"
     [class.bg-warning]="task.priority === 'medium'"
     [class.bg-secondary]="task.priority === 'low'">
  {{ task.priority | uppercase }}
</div>
```

---
**📍 สรุปสิ่งที่ Frontend มีพร้อมใช้แล้วในตอนนี้:**
✅ `task.model.ts` - โครงสร้างตัวแปร
✅ `task.service.ts` - ฟังก์ชันยิง API ครบทุกรูปแบบ (CRUD)
✅ `API Endpoints` - ฝั่ง Backend เปิดรับ Request รออยู่แล้ว

**งานของ Frontend:** หยิบข้อ 1-5 ด้านบนไปแปะเข้ากับ Component หน้า Kanban Board ได้เลย!
