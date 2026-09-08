import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { ToastService } from '../../core/services/toast.service';
import { Order } from '../../core/models/order.model';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-list.component.html'
})
export class OrderListComponent implements OnInit {
  private orderService = inject(OrderService);
  private toast = inject(ToastService);

  orders: Order[] = [];
  loading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.orderService.getAll().subscribe({
      next: (data: Order[]) => { this.orders = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  remove(order: Order): void {
    if (!order.id) return;
    if (!confirm(`Supprimer la commande ${order.numeroCommande} ?`)) return;

    this.orderService.delete(order.id).subscribe({
      next: () => {
        this.toast.success('Commande supprimée.');
        this.load();
      }
    });
  }

  validate(order: Order): void {
    if (!order.id) return;
    if (!confirm(`Valider la commande ${order.numeroCommande} ? Le stock sera mis à jour.`)) return;

    this.orderService.validate(order.id).subscribe({
      next: () => {
        this.toast.success('Commande validée, stock mis à jour.');
        this.load();
      }
    });
  }

  cancel(order: Order): void {
    if (!order.id) return;
    if (!confirm(`Annuler la commande ${order.numeroCommande} ?`)) return;

    this.orderService.cancel(order.id).subscribe({
      next: () => {
        this.toast.success('Commande annulée.');
        this.load();
      }
    });
  }
}
