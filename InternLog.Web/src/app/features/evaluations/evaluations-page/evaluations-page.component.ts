import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EvaluationsService } from '../../../core/services/evaluations.service';
import { EvaluationResponseDto, EvaluationRequestDto } from '../../../models/evaluation.model';

interface EvaluationItem {
  id: number;
  question: string;
  score: number;
}

@Component({
  selector: 'app-evaluations-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './evaluations-page.component.html',
  styleUrls: ['./evaluations-page.component.scss']
})
export class EvaluationsPageComponent implements OnInit {
  studentEvalItems: EvaluationItem[] = [
    { id: 1, question: 'ความเหมาะสมของสถานที่ฝึกงาน', score: 0 },
    { id: 2, question: 'การดูแลและให้คำแนะนำของพี่เลี้ยง', score: 0 },
    { id: 3, question: 'ปริมาณงานและความเหมาะสมของงานที่ได้รับ', score: 0 },
    { id: 4, question: 'บรรยากาศและวัฒนธรรมองค์กร', score: 0 },
    { id: 5, question: 'ภาพรวมความพึงพอใจ', score: 0 }
  ];

  mentorEvalItems: EvaluationItem[] = [
    { id: 1, question: 'ความรับผิดชอบต่องานที่ได้รับมอบหมาย', score: 4 },
    { id: 2, question: 'ความรู้ความสามารถพื้นฐาน', score: 4 },
    { id: 3, question: 'การปรับตัวเข้ากับเพื่อนร่วมงาน', score: 5 },
    { id: 4, question: 'ความคิดริเริ่มสร้างสรรค์', score: 3 },
    { id: 5, question: 'ภาพรวมการปฏิบัติงาน', score: 4 }
  ];

  studentComment = '';
  stars = [1, 2, 3, 4, 5];
  evaluations: EvaluationResponseDto[] = [];
  internshipID = 1; // Temporary hardcoded or retrieved from state

  constructor(private evaluationsService: EvaluationsService) {}

  ngOnInit() {
    this.loadEvaluations();
  }

  loadEvaluations() {
    this.evaluationsService.getEvaluations(this.internshipID).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.evaluations = res.data;
          
          // Load my submitted evaluation (Student -> Company)
          const myEval = this.evaluations.find(e => e.evaluatorRole === 'student_eval');
          if (myEval && myEval.criteriaJson) {
            try {
              const savedScores = JSON.parse(myEval.criteriaJson);
              this.studentEvalItems.forEach(item => {
                const s = savedScores.find((x: any) => x.id === item.id);
                if (s) item.score = s.score;
              });
              this.studentComment = myEval.comment || '';
            } catch (e) {}
          }

          // Load Mentor's evaluation of Student
          const mentorEval = this.evaluations.find(e => e.evaluatorRole === 'supervisor_eval');
          if (mentorEval && mentorEval.criteriaJson) {
            try {
              const savedScores = JSON.parse(mentorEval.criteriaJson);
              this.mentorEvalItems.forEach(item => {
                const s = savedScores.find((x: any) => x.id === item.id);
                if (s) item.score = s.score;
              });
            } catch (e) {}
          } else {
             // Reset to 0 if not evaluated yet
             this.mentorEvalItems.forEach(item => item.score = 0);
          }
        }
      },
      error: (err) => console.error('Failed to load evaluations', err)
    });
  }

  setScore(item: EvaluationItem, score: number) {
    item.score = score;
  }

  getTotalScore(items: EvaluationItem[]): number {
    const total = items.reduce((sum, item) => sum + item.score, 0);
    return (total / (items.length * 5)) * 100;
  }

  getGrade(scorePercentage: number): string {
    if (scorePercentage === 0) return '-';
    if (scorePercentage >= 80) return 'A';
    if (scorePercentage >= 70) return 'B';
    if (scorePercentage >= 60) return 'C';
    if (scorePercentage >= 50) return 'D';
    return 'F';
  }

  getGradeClass(grade: string): string {
    switch (grade) {
      case 'A': return 'grade-a';
      case 'B': return 'grade-b';
      case 'C': return 'grade-c';
      case 'D':
      case 'F': return 'grade-f';
      default: return '';
    }
  }

  submitEvaluation() {
    const isComplete = this.studentEvalItems.every(i => i.score > 0);
    if (!isComplete) {
      alert('กรุณาประเมินให้ครบทุกข้อ');
      return;
    }
    
    if (confirm('คุณต้องการส่งผลการประเมินใช่หรือไม่? (เมื่อส่งแล้วจะไม่สามารถแก้ไขได้)')) {
      const score = this.getTotalScore(this.studentEvalItems);
      const req: EvaluationRequestDto = {
        internshipID: this.internshipID,
        evaluatorRole: 'student_eval',
        criteriaJson: JSON.stringify(this.studentEvalItems.map(i => ({ id: i.id, score: i.score }))),
        totalScore: score,
        grade: this.getGrade(score),
        comment: this.studentComment
      };

      this.evaluationsService.submitEvaluation(req).subscribe({
        next: (res) => {
          if (res.success) {
            alert('บันทึกผลการประเมินสำเร็จ');
            this.loadEvaluations();
          }
        },
        error: (err) => alert('บันทึกไม่สำเร็จ: ' + err.message)
      });
    }
  }
}
