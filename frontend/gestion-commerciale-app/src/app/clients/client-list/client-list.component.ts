import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ClientService } from '../../core/services/client.service';
import { ToastService } from '../../core/services/toast.service';
import { Client } from '../../core/models/client.model';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './client-list.component.html'
})
export class ClientListComponent implements OnInit {
  private clientService = inject(ClientService);
  private toast = inject(ToastService);

  clients: Client[] = [];
  loading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.clientService.getAll().subscribe({
      next: (data: Client[]) => { this.clients = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  remove(client: Client): void {
    if (!client.id) return;
    if (!confirm(`Supprimer le client ${client.nom} ${client.prenomOuRaisonSociale} ?`)) return;

    this.clientService.delete(client.id).subscribe({
      next: () => {
        this.toast.success('Client supprimé.');
        this.load();
      }
    });
  }
}
