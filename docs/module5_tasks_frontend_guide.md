# 📋 คู่มือฝั่ง Frontend: สิ่งที่ต้องทำสำหรับโมดูล 5 (Tasks / Kanban)

เอกสารนี้ระบุ **"Checklist งานที่ต้องทำ (To-Do)"** สำหรับ Frontend Developer ในการนำ `TaskService` และ `TaskModel` ที่เตรียมไว้ไปผูกกับหน้าจอ UI (Component) ของระบบจัดการงาน (Kanban Board)

---

## 🎯 1. หน้าจอที่เกี่ยวข้อง (UI Components)
คุณจะต้องเข้าไปจัดการโค้ดใน Components ที่เกี่ยวข้องกับระบบ Tasks (Kanban) โดยหลักๆ จะมีดังนี้:
- **Task Board (Kanban):** หน้าจอแสดงคอลัมน์ Todo, In Progress, Done
- **Task Modal (Dialog):** หน้าต่าง Popup สำหรับสร้างงานใหม่ หรือแก้ไขงานเดิม

---

## 🛠️ 2. Checklist สิ่งที่ต้องทำ (Step-by-Step)

### [ ] 1. Inject Service เข้า Component
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

### [ ] 2. กรองข้อมูลลงแต่ละคอลัมน์ (Kanban Logic)
ในหน้าจอ HTML หรือใน TypeScript ให้แยก `tasks` ออกเป็น 3 กลุ่มตามฟิลด์ `status`:
```typescript
get todoTasks() { return this.tasks.filter(t => t.status === 'todo'); }
get inProgressTasks() { return this.tasks.filter(t => t.status === 'in_progress'); }
get doneTasks() { return this.tasks.filter(t => t.status === 'done'); }
```
แล้วนำไปวนลูป `@for (task of todoTasks; track task.taskID)` ในหน้า HTML

### [ ] 3. ผูกระบบลากวาง (Drag & Drop) หรือ ปรับสถานะ
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

### [ ] 4. ผูกฟอร์มสร้างงานใหม่ (Create Task)
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

### [ ] 5. ผูกปุ่มลบงาน (Delete)
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

## 🎨 3. ข้อแนะนำด้าน UI / UX (สีสันและไอคอน)
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
