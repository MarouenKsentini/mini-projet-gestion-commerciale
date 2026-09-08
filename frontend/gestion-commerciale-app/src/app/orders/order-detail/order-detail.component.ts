import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { ToastService } from '../../core/services/toast.service';
import { Order } from '../../core/models/order.model';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-detail.component.html'
})
export class OrderDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);
  private toast = inject(ToastService);
  private router = inject(Router);

  order: Order | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    const id = +this.route.snapshot.paramMap.get('id')!;
    this.orderService.getById(id).subscribe((o: Order) => (this.order = o));
  }

  validate(): void {
    if (!this.order?.id) return;
    if (!confirm('Valider cette commande ? Le stock sera mis à jour.')) return;

    this.orderService.validate(this.order.id).subscribe({
      next: () => {
        this.toast.success('Commande validée, stock mis à jour.');
        this.load();
      }
    });
  }

  cancel(): void {
    if (!this.order?.id) return;
    if (!confirm('Annuler cette commande ?')) return;

    this.orderService.cancel(this.order.id).subscribe({
      next: () => {
        this.toast.success('Commande annulée.');
        this.load();
      }
    });
  }
}
