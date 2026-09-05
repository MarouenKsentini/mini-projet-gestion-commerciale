import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../core/services/product.service';
import { ToastService } from '../../core/services/toast.service';
import { Product } from '../../core/models/product.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './product-list.component.html'
})
export class ProductListComponent implements OnInit {
  private productService = inject(ProductService);
  private toast = inject(ToastService);

  products: Product[] = [];
  loading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.productService.getAll().subscribe({
      next: (data: Product[]) => { this.products = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  remove(product: Product): void {
    if (!product.id) return;
    if (!confirm(`Supprimer le produit ${product.nom} ?`)) return;

    this.productService.delete(product.id).subscribe({
      next: () => {
        this.toast.success('Produit supprimé.');
        this.load();
      }
    });
  }
}
