import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { ClientService } from '../../core/services/client.service';
import { ProductService } from '../../core/services/product.service';
import { ToastService } from '../../core/services/toast.service';
import { Client } from '../../core/models/client.model';
import { Product } from '../../core/models/product.model';
import { Order, OrderLine } from '../../core/models/order.model';

const TAUX_TVA = 0.19;

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './order-form.component.html'
})
export class OrderFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private orderService = inject(OrderService);
  private clientService = inject(ClientService);
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  orderId: number | null = null;
  clients: Client[] = [];
  products: Product[] = [];

  form = this.fb.group({
    clientId: [null as number | null, Validators.required],
    lines: this.fb.array([] as any[])
  });

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  ngOnInit(): void {
    this.clientService.getAll().subscribe((c: Client[]) => (this.clients = c));
    this.productService.getAll().subscribe((p: Product[]) => (this.products = p));

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.orderId = +idParam;
      this.orderService.getById(this.orderId).subscribe((order: Order) => {
        this.form.patchValue({ clientId: order.clientId });
        order.lines.forEach((line: OrderLine) =>
          this.lines.push(
            this.fb.group({
              productId: [line.productId, Validators.required],
              quantite: [line.quantite, [Validators.required, Validators.min(1)]]
            })
          )
        );
      });
    } else {
      this.addLine();
    }
  }

  addLine(): void {
    this.lines.push(
      this.fb.group({
        productId: [null, Validators.required],
        quantite: [1, [Validators.required, Validators.min(1)]]
      })
    );
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  productOf(productId: number | null): Product | undefined {
    return this.products.find((p) => p.id === productId);
  }

  unitPrice(index: number): number {
    const productId = this.lines.at(index).get('productId')?.value;
    return this.productOf(productId)?.prixUnitaireHT ?? 0;
  }

  availableStock(index: number): number {
    const productId = this.lines.at(index).get('productId')?.value;
    return this.productOf(productId)?.quantiteStock ?? 0;
  }

  lineTotal(index: number): number {
    const quantite = this.lines.at(index).get('quantite')?.value ?? 0;
    return +(quantite * this.unitPrice(index)).toFixed(2);
  }

  get totalHT(): number {
    return +this.lines.controls.reduce((sum, _, i) => sum + this.lineTotal(i), 0).toFixed(2);
  }

  get totalTTC(): number {
    return +(this.totalHT * (1 + TAUX_TVA)).toFixed(2);
  }

  submit(): void {
    if (this.form.invalid || this.lines.length === 0) {
      this.form.markAllAsTouched();
      this.toast.error('Sélectionnez un client et au moins une ligne de produit.');
      return;
    }

    const payload = {
      clientId: this.form.value.clientId ?? undefined,
      lines: this.lines.value
    };

    const request$ = this.orderId
      ? this.orderService.update(this.orderId, payload)
      : this.orderService.create(payload);

    request$.subscribe({
      next: () => {
        this.toast.success(this.orderId ? 'Commande modifiée.' : 'Commande créée.');
        this.router.navigate(['/commandes']);
      }
    });
  }
}
