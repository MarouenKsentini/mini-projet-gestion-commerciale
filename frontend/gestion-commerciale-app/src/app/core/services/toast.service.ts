import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  type: 'success' | 'error';
  text: string;
}

/**
 * Petit service de notification partagé, alimenté par l'intercepteur HTTP
 * pour afficher les erreurs métier renvoyées par l'API (message clair, pas de stack trace).
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly message = signal<ToastMessage | null>(null);

  success(text: string) {
    this.show({ type: 'success', text });
  }

  error(text: string) {
    this.show({ type: 'error', text });
  }

  private show(msg: ToastMessage) {
    this.message.set(msg);
    setTimeout(() => this.message.set(null), 4000);
  }
}
