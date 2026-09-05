import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'commandes', pathMatch: 'full' },

  {
    path: 'clients',
    loadComponent: () =>
      import('./clients/client-list/client-list.component').then((m) => m.ClientListComponent)
  },
  {
    path: 'clients/nouveau',
    loadComponent: () =>
      import('./clients/client-form/client-form.component').then((m) => m.ClientFormComponent)
  },
  {
    path: 'clients/:id',
    loadComponent: () =>
      import('./clients/client-detail/client-detail.component').then((m) => m.ClientDetailComponent)
  },
  {
    path: 'clients/:id/modifier',
    loadComponent: () =>
      import('./clients/client-form/client-form.component').then((m) => m.ClientFormComponent)
  },

  {
    path: 'produits',
    loadComponent: () =>
      import('./products/product-list/product-list.component').then((m) => m.ProductListComponent)
  },
  {
    path: 'produits/nouveau',
    loadComponent: () =>
      import('./products/product-form/product-form.component').then((m) => m.ProductFormComponent)
  },
  {
    path: 'produits/:id',
    loadComponent: () =>
      import('./products/product-detail/product-detail.component').then((m) => m.ProductDetailComponent)
  },
  {
    path: 'produits/:id/modifier',
    loadComponent: () =>
      import('./products/product-form/product-form.component').then((m) => m.ProductFormComponent)
  },

  {
    path: 'commandes',
    loadComponent: () =>
      import('./orders/order-list/order-list.component').then((m) => m.OrderListComponent)
  },
  {
    path: 'commandes/nouvelle',
    loadComponent: () =>
      import('./orders/order-form/order-form.component').then((m) => m.OrderFormComponent)
  },
  {
    path: 'commandes/:id',
    loadComponent: () =>
      import('./orders/order-detail/order-detail.component').then((m) => m.OrderDetailComponent)
  },
  {
    path: 'commandes/:id/modifier',
    loadComponent: () =>
      import('./orders/order-form/order-form.component').then((m) => m.OrderFormComponent)
  },

  { path: '**', redirectTo: 'commandes' }
];
