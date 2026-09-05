import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../core/services/product.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './product-form.component.html'
})
export class ProductFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  productId: number | null = null;

  form = this.fb.group({
    reference: ['', [Validators.required, Validators.maxLength(50)]],
    nom: ['', [Validators.required, Validators.maxLength(150)]],
    description: [''],
    prixUnitaireHT: [0, [Validators.required, Validators.min(0.01)]],
    quantiteStock: [0, [Validators.required, Validators.min(0)]]
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.productId = +idParam;
      this.productService.getById(this.productId).subscribe((product: import('../../core/models/product.model').Product) => {
        this.form.patchValue(product);
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue() as any;

    const request$ = this.productId
      ? this.productService.update(this.productId, value)
      : this.productService.create(value);

    request$.subscribe({
      next: () => {
        this.toast.success(this.productId ? 'Produit modifié.' : 'Produit créé.');
        this.router.navigate(['/produits']);
      }
    });
  }
}
