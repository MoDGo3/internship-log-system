import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

/**
 * ดาวน์โหลดไฟล์จาก endpoint ที่ต้อง login (ไฟล์แนบใบลา, ไฟล์แนบ work log, PDF รายงาน)
 *
 * ไม่ใช้ <a href> ตรง ๆ เพราะ browser จะไม่แนบ Bearer token ไปด้วย —
 * โหลดผ่าน HttpClient (ผ่าน authInterceptor) เป็น blob แล้วเปิด/บันทึกจาก object URL แทน
 */
@Injectable({ providedIn: 'root' })
export class FileDownloadService {
  /** origin ของ API เช่น http://localhost:5005 (apiUrl ตัด /api ท้ายออก) */
  private readonly apiOrigin = environment.apiUrl.replace(/\/api\/?$/, '');

  constructor(private http: HttpClient) {}

  /** เปิดไฟล์ในแท็บใหม่ (PDF/รูปภาพ) — ไฟล์ชนิดอื่น browser จะดาวน์โหลดให้เอง */
  open(url: string): Observable<void> {
    // เปิดแท็บก่อนเรียก HTTP เพื่อไม่ให้ popup blocker บล็อก (ต้องเกิดใน click handler)
    const tab = window.open('', '_blank');
    return this.fetch(url).pipe(
      tap({ error: () => tab?.close() }),
      map(({ blob }) => {
        const objectUrl = URL.createObjectURL(blob);
        if (tab) {
          tab.location.href = objectUrl;
        } else {
          window.location.assign(objectUrl);
        }
        setTimeout(() => URL.revokeObjectURL(objectUrl), 60_000);
      })
    );
  }

  /** บันทึกไฟล์ลงเครื่องด้วยชื่อที่กำหนด (หรือชื่อจาก Content-Disposition) */
  save(url: string, fileName?: string): Observable<void> {
    return this.fetch(url).pipe(
      map(({ blob, name }) => {
        const objectUrl = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = objectUrl;
        link.download = fileName ?? name ?? 'download';
        link.click();
        URL.revokeObjectURL(objectUrl);
      })
    );
  }

  private fetch(url: string): Observable<{ blob: Blob; name: string | null }> {
    return this.http
      .get(this.resolve(url), { responseType: 'blob', observe: 'response' })
      .pipe(map((res: HttpResponse<Blob>) => ({
        blob: res.body as Blob,
        name: this.fileNameFrom(res.headers.get('Content-Disposition'))
      })));
  }

  /** รับได้ทั้ง URL เต็ม และ path ที่ API ส่งมา เช่น /api/leaves/5/attachment */
  private resolve(url: string): string {
    return /^https?:\/\//.test(url) ? url : `${this.apiOrigin}${url.startsWith('/') ? '' : '/'}${url}`;
  }

  private fileNameFrom(disposition: string | null): string | null {
    if (!disposition) return null;
    const utf8 = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
    if (utf8) return decodeURIComponent(utf8[1]);
    const plain = /filename="?([^";]+)"?/i.exec(disposition);
    return plain ? plain[1] : null;
  }
}
