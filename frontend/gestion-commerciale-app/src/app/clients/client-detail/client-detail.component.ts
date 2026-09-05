import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ClientService } from '../../core/services/client.service';
import { Client } from '../../core/models/client.model';

@Component({
  selector: 'app-client-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './client-detail.component.html'
})
export class ClientDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private clientService = inject(ClientService);

  client: Client | null = null;

  ngOnInit(): void {
    const id = +this.route.snapshot.paramMap.get('id')!;
    this.clientService.getById(id).subscribe((c: Client) => (this.client = c));
  }
}
