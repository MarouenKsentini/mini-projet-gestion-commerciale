import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ClientService } from '../../core/services/client.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-client-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './client-form.component.html'
})
export class ClientFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private clientService = inject(ClientService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  clientId: number | null = null;

  form = this.fb.group({
    nom: ['', [Validators.required, Validators.maxLength(100)]],
    prenomOuRaisonSociale: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    telephone: [''],
    adresse: ['']
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.clientId = +idParam;
      this.clientService.getById(this.clientId).subscribe((client: import('../../core/models/client.model').Client) => {
        this.form.patchValue(client);
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue() as any;

    const request$ = this.clientId
      ? this.clientService.update(this.clientId, value)
      : this.clientService.create(value);

    request$.subscribe({
      next: () => {
        this.toast.success(this.clientId ? 'Client modifié.' : 'Client créé.');
        this.router.navigate(['/clients']);
      }
    });
  }
}
