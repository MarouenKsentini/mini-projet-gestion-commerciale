import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (toast.message(); as msg) {
      <div class="toast" [class.toast--error]="msg.type === 'error'" [class.toast--success]="msg.type === 'success'">
        {{ msg.text }}
      </div>
    }
  `,
  styles: [`
    .toast {
      position: fixed;
      top: 16px;
      right: 16px;
      padding: 12px 20px;
      border-radius: 6px;
      color: white;
      font-size: 14px;
      z-index: 1000;
      box-shadow: 0 2px 8px rgba(0,0,0,0.2);
    }
    .toast--success { background-color: #2e7d32; }
    .toast--error { background-color: #c62828; }
  `]
})
export class ToastComponent {
  toast = inject(ToastService);
}
