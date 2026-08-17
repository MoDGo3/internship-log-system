import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TasksService } from '../../../core/services/tasks.service';
import { TaskRecord } from '../../../models/task.model';
import { format } from 'date-fns';

@Component({
  selector: 'app-tasks-kanban',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tasks-kanban.component.html',
  styleUrls: ['./tasks-kanban.component.scss']
})
export class TasksKanbanComponent implements OnInit {
  tasks: any[] = [];
  draggedTask: any | null = null;

  constructor(private tasksService: TasksService) {}

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.tasksService.getTasks().subscribe({
      next: (res) => {
        const data = res.data;
        if (data) {
          this.tasks = data.map((t: TaskRecord) => ({
            id: t.taskID,
            title: t.title,
            status: t.status?.toLowerCase() || 'todo', // Map Backend status to UI status if needed
            priority: t.priority?.toLowerCase() || 'medium',
            dueDate: t.dueDate ? format(new Date(t.dueDate), 'dd MMM yy') : '-',
            assigneeName: t.assignedBy || 'ไม่ระบุ'
          }));
        }
      },
      error: (err) => console.error('Failed to load tasks', err)
    });
  }

  getTasksByStatus(status: 'todo' | 'doing' | 'done'): any[] {
    return this.tasks.filter(t => t.status === status);
  }

  getPriorityClass(priority: string): string {
    switch (priority) {
      case 'high': return 'priority-danger';
      case 'medium': return 'priority-warning';
      case 'low': return 'priority-success';
      default: return '';
    }
  }

  getPriorityText(priority: string): string {
    switch (priority) {
      case 'high': return 'High';
      case 'medium': return 'Medium';
      case 'low': return 'Low';
      default: return priority;
    }
  }

  // --- HTML5 Drag & Drop ---
  onDragStart(task: any) {
    this.draggedTask = task;
  }

  onDragOver(event: DragEvent) {
    event.preventDefault(); // allow drop
  }

  onDrop(event: DragEvent, newStatus: 'todo' | 'doing' | 'done') {
    event.preventDefault();
    if (this.draggedTask) {
      const oldStatus = this.draggedTask.status;
      if (oldStatus !== newStatus) {
        this.draggedTask.status = newStatus;
        // Update API
        this.tasksService.updateTaskStatus(this.draggedTask.id, { status: newStatus }).subscribe({
          next: () => console.log('Task status updated to', newStatus),
          error: (err) => {
            console.error('Failed to update task status', err);
            this.draggedTask.status = oldStatus; // Revert on failure
            alert('อัปเดตสถานะไม่สำเร็จ: ' + err.message);
          }
        });
      }
      this.draggedTask = null;
    }
  }

  assignTask() {
    alert('เปิดหน้าต่างมอบหมายงาน');
  }

  viewTask(task: any) {
    alert(`เปิดรายละเอียดงาน: ${task.title}`);
  }
}
