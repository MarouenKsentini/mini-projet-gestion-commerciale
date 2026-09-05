# Guide complet — Mini Projet Gestion Commerciale (.NET 8 + Angular + SQL Server)

Ce guide te donne une méthode concrète, étape par étape, pour livrer ce test technique en 2 jours avec une architecture propre.

---

## 1. Architecture globale

```
gestion-commerciale/
├── backend/
│   └── GestionCommerciale.Api/
│       ├── GestionCommerciale.Api/          (Web API + Controllers)
│       ├── GestionCommerciale.Application/  (Services, DTOs, interfaces)
│       ├── GestionCommerciale.Domain/       (Entities, enums)
│       └── GestionCommerciale.Infrastructure/ (DbContext, EF Core, repositories)
└── frontend/
    └── gestion-commerciale-app/  (Angular)
```

Une architecture en couches simple (API / Application / Domain / Infrastructure) suffit largement pour ce test — pas besoin de CQRS/MediatR, ça serait too much pour 2 jours. L'important c'est que la logique métier ne soit **jamais** dans les contrôleurs.

---

## 2. Base de données (script SQL de référence)

```sql
CREATE TABLE Clients (
    Id INT IDENTITY PRIMARY KEY,
    Nom NVARCHAR(100) NOT NULL,
    PrenomOuRaisonSociale NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    Telephone NVARCHAR(30),
    Adresse NVARCHAR(300),
    DateCreation DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Products (
    Id INT IDENTITY PRIMARY KEY,
    Reference NVARCHAR(50) NOT NULL UNIQUE,
    Nom NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500),
    PrixUnitaireHT DECIMAL(18,2) NOT NULL,
    QuantiteStock INT NOT NULL,
    DateCreation DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Orders (
    Id INT IDENTITY PRIMARY KEY,
    NumeroCommande NVARCHAR(50) NOT NULL UNIQUE,
    ClientId INT NOT NULL FOREIGN KEY REFERENCES Clients(Id),
    DateCommande DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Statut NVARCHAR(20) NOT NULL DEFAULT 'Brouillon', -- Brouillon, Validee, Annulee
    TotalHT DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalTTC DECIMAL(18,2) NOT NULL DEFAULT 0
);

CREATE TABLE OrderLines (
    Id INT IDENTITY PRIMARY KEY,
    OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(Id) ON DELETE CASCADE,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id),
    Quantite INT NOT NULL,
    PrixUnitaire DECIMAL(18,2) NOT NULL,
    TotalLigne AS (Quantite * PrixUnitaire) PERSISTED
);
```

En pratique, préfère **EF Core Code First + Migrations** : tu écris les entities en C#, tu génères la migration, et le script SQL ci-dessus devient ta référence pour vérifier que le modèle généré correspond bien. Livrer les migrations est plus propre qu'un script à la main.

---

## 3. Backend .NET 8 — étape par étape

### Étape 3.1 — Créer la solution

```bash
dotnet new sln -n GestionCommerciale
dotnet new webapi -n GestionCommerciale.Api -controllers
dotnet new classlib -n GestionCommerciale.Domain
dotnet new classlib -n GestionCommerciale.Application
dotnet new classlib -n GestionCommerciale.Infrastructure

dotnet sln add **/*.csproj
dotnet add GestionCommerciale.Api reference GestionCommerciale.Application
dotnet add GestionCommerciale.Application reference GestionCommerciale.Domain
dotnet add GestionCommerciale.Infrastructure reference GestionCommerciale.Domain GestionCommerciale.Application

cd GestionCommerciale.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Étape 3.2 — Entities (Domain)

```csharp
// Domain/Client.cs
public class Client
{
    public int Id { get; set; }
    public string Nom { get; set; } = default!;
    public string PrenomOuRaisonSociale { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

// Domain/Product.cs
public class Product
{
    public int Id { get; set; }
    public string Reference { get; set; } = default!;
    public string Nom { get; set; } = default!;
    public string? Description { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public int QuantiteStock { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}

public enum OrderStatus { Brouillon, Validee, Annulee }

// Domain/Order.cs
public class Order
{
    public int Id { get; set; }
    public string NumeroCommande { get; set; } = default!;
    public int ClientId { get; set; }
    public Client Client { get; set; } = default!;
    public DateTime DateCommande { get; set; } = DateTime.UtcNow;
    public OrderStatus Statut { get; set; } = OrderStatus.Brouillon;
    public decimal TotalHT { get; set; }
    public decimal TotalTTC { get; set; }
    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}

// Domain/OrderLine.cs
public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal TotalLigne => Quantite * PrixUnitaire;
}
```

### Étape 3.3 — DbContext (Infrastructure)

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .Property(o => o.Statut)
            .HasConversion<string>();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Reference)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.NumeroCommande)
            .IsUnique();
    }
}
```

Puis dans `Program.cs` :
```csharp
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

Génère la migration :
```bash
dotnet ef migrations add InitialCreate -p GestionCommerciale.Infrastructure -s GestionCommerciale.Api
dotnet ef database update -p GestionCommerciale.Infrastructure -s GestionCommerciale.Api
```

### Étape 3.4 — DTOs (Application)

Ne renvoie **jamais** les entities directement (évite les cycles de sérialisation et les fuites de champs internes).

```csharp
public record ClientDto(int Id, string Nom, string PrenomOuRaisonSociale, string Email, string? Telephone, string? Adresse, DateTime DateCreation);
public record CreateClientDto(string Nom, string PrenomOuRaisonSociale, string Email, string? Telephone, string? Adresse);

public record OrderLineDto(int ProductId, string ProductNom, int Quantite, decimal PrixUnitaire, decimal TotalLigne);
public record CreateOrderLineDto(int ProductId, int Quantite);

public record OrderDto(int Id, string NumeroCommande, int ClientId, string ClientNom, DateTime DateCommande, string Statut, decimal TotalHT, decimal TotalTTC, List<OrderLineDto> Lines);
public record CreateOrderDto(int ClientId, List<CreateOrderLineDto> Lines);
```

### Étape 3.5 — Couche service : la logique métier qui compte vraiment

C'est ici que se joue la note du test — les règles de gestion doivent être centralisées, pas dans le contrôleur.

```csharp
public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private const decimal TVA = 0.19m;

    public OrderService(AppDbContext db) => _db = db;

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var client = await _db.Clients.FindAsync(dto.ClientId)
            ?? throw new BusinessException("Client introuvable : impossible de créer une commande sans client.");

        if (dto.Lines is null || dto.Lines.Count == 0)
            throw new BusinessException("Une commande doit contenir au moins une ligne.");

        var order = new Order
        {
            NumeroCommande = $"CMD-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ClientId = client.Id,
            Statut = OrderStatus.Brouillon
        };

        foreach (var lineDto in dto.Lines)
        {
            if (lineDto.Quantite <= 0)
                throw new BusinessException("La quantité doit être supérieure à zéro.");

            var product = await _db.Products.FindAsync(lineDto.ProductId)
                ?? throw new BusinessException($"Produit {lineDto.ProductId} introuvable.");

            if (lineDto.Quantite > product.QuantiteStock)
                throw new BusinessException($"Stock insuffisant pour {product.Nom} (disponible : {product.QuantiteStock}).");

            order.Lines.Add(new OrderLine
            {
                ProductId = product.Id,
                Quantite = lineDto.Quantite,
                PrixUnitaire = product.PrixUnitaireHT
            });
        }

        RecalculateTotals(order);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return await GetOrderDtoAsync(order.Id);
    }

    public async Task ValidateOrderAsync(int orderId)
    {
        var order = await _db.Orders.Include(o => o.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new BusinessException("Commande introuvable.");

        if (order.Statut != OrderStatus.Brouillon)
            throw new BusinessException("Seule une commande en brouillon peut être validée.");

        foreach (var line in order.Lines)
        {
            if (line.Quantite > line.Product.QuantiteStock)
                throw new BusinessException($"Stock insuffisant pour {line.Product.Nom} au moment de la validation.");
        }

        foreach (var line in order.Lines)
            line.Product.QuantiteStock -= line.Quantite;

        order.Statut = OrderStatus.Validee;
        await _db.SaveChangesAsync();
    }

    private static void RecalculateTotals(Order order)
    {
        order.TotalHT = order.Lines.Sum(l => l.Quantite * l.PrixUnitaire);
        order.TotalTTC = Math.Round(order.TotalHT * (1 + TVA), 2);
    }

    private async Task<OrderDto> GetOrderDtoAsync(int id) { /* map entity -> OrderDto */ throw new NotImplementedException(); }
}
```

`BusinessException` est une exception custom que tu attrapes dans un **middleware global** pour renvoyer un code 400 propre avec un message clair, plutôt que de mettre des try/catch partout :

```csharp
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is BusinessException be)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = be.Message });
        }
        else
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { message = "Erreur interne." });
        }
    });
});
```

### Étape 3.6 — Contrôleurs (fins, pas de logique métier)

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;
    public OrdersController(IOrderService service) => _service = service;

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost] public async Task<IActionResult> Create(CreateOrderDto dto)
        => Ok(await _service.CreateOrderAsync(dto));

    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, CreateOrderDto dto)
        => Ok(await _service.UpdateOrderAsync(id, dto));

    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
    { await _service.DeleteAsync(id); return NoContent(); }

    [HttpPost("{id}/validate")] public async Task<IActionResult> Validate(int id)
    { await _service.ValidateOrderAsync(id); return Ok(); }
}
```

Fais pareil pour `ClientsController` et `ProductsController` — CRUD simple qui délègue à `IClientService` / `IProductService`. C'est répétitif mais volontairement basique : ce sont les modules les plus rapides à écrire, garde ton temps pour la logique commandes.

### Étape 3.7 — Validation

- **Validations d'entrée** (champs requis, formats) → `[Required]`, `[EmailAddress]`, `[Range]` sur les DTOs + `ModelState.IsValid` (activé par défaut avec `[ApiController]`).
- **Validations métier** (stock, quantité > 0, client obligatoire) → dans le service, comme au-dessus.

### Étape 3.8 — Swagger

Déjà inclus par défaut avec `dotnet new webapi`. Vérifie juste que `app.UseSwagger(); app.UseSwaggerUI();` sont bien actifs même hors "Development" si tu veux que l'évaluateur y accède facilement.

---

## 4. Frontend Angular — étape par étape

### Étape 4.1 — Créer le projet

```bash
ng new gestion-commerciale-app --routing --style=scss
cd gestion-commerciale-app
ng generate module clients --routing
ng generate module products --routing
ng generate module orders --routing
```

### Étape 4.2 — Structure

```
src/app/
├── core/
│   ├── models/          (interfaces Client, Product, Order, OrderLine)
│   └── services/        (client.service.ts, product.service.ts, order.service.ts)
├── clients/
│   ├── client-list/
│   └── client-form/
├── products/
│   ├── product-list/
│   └── product-form/
├── orders/
│   ├── order-list/
│   ├── order-form/
│   └── order-detail/
└── shared/               (composants réutilisables : loader, toast/alert)
```

### Étape 4.3 — Modèles + Service HTTP (exemple Order)

```typescript
// core/models/order.model.ts
export interface OrderLine {
  productId: number;
  productNom?: string;
  quantite: number;
  prixUnitaire: number;
  totalLigne?: number;
}

export interface Order {
  id?: number;
  numeroCommande?: string;
  clientId: number;
  clientNom?: string;
  dateCommande?: string;
  statut?: string;
  totalHT?: number;
  totalTTC?: number;
  lines: OrderLine[];
}
```

```typescript
// core/services/order.service.ts
@Injectable({ providedIn: 'root' })
export class OrderService {
  private baseUrl = 'https://localhost:5001/api/orders';
  constructor(private http: HttpClient) {}

  getAll(): Observable<Order[]> { return this.http.get<Order[]>(this.baseUrl); }
  getById(id: number): Observable<Order> { return this.http.get<Order>(`${this.baseUrl}/${id}`); }
  create(order: Order): Observable<Order> { return this.http.post<Order>(this.baseUrl, order); }
  update(id: number, order: Order): Observable<Order> { return this.http.put<Order>(`${this.baseUrl}/${id}`, order); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/${id}`); }
  validate(id: number): Observable<void> { return this.http.post<void>(`${this.baseUrl}/${id}/validate`, {}); }
}
```

### Étape 4.4 — Formulaire de commande (le composant le plus important)

Utilise un `FormArray` pour les lignes, avec calcul du total en temps réel côté front (à titre d'affichage — le vrai total de référence reste calculé côté back).

```typescript
// order-form.component.ts
export class OrderFormComponent implements OnInit {
  form = this.fb.group({
    clientId: [null, Validators.required],
    lines: this.fb.array([])
  });

  products: Product[] = [];

  constructor(private fb: FormBuilder, private orderService: OrderService, private productService: ProductService) {}

  get lines() { return this.form.get('lines') as FormArray; }

  ngOnInit() {
    this.productService.getAll().subscribe(p => this.products = p);
  }

  addLine() {
    this.lines.push(this.fb.group({
      productId: [null, Validators.required],
      quantite: [1, [Validators.required, Validators.min(1)]]
    }));
  }

  removeLine(i: number) { this.lines.removeAt(i); }

  lineTotal(i: number): number {
    const line = this.lines.at(i).value;
    const product = this.products.find(p => p.id === line.productId);
    return product ? (line.quantite || 0) * product.prixUnitaireHT : 0;
  }

  get totalHT(): number {
    return this.lines.controls.reduce((sum, _, i) => sum + this.lineTotal(i), 0);
  }

  get totalTTC(): number { return +(this.totalHT * 1.19).toFixed(2); }

  submit() {
    if (this.form.invalid) return;
    const payload = { ...this.form.value };
    this.orderService.create(payload as any).subscribe({
      next: () => { /* toast succès + redirection */ },
      error: (err) => { /* afficher err.error.message */ }
    });
  }
}
```

```html
<!-- order-form.component.html (extrait) -->
<form [formGroup]="form" (ngSubmit)="submit()">
  <select formControlName="clientId">
    <option *ngFor="let c of clients" [value]="c.id">{{ c.nom }} {{ c.prenomOuRaisonSociale }}</option>
  </select>

  <div formArrayName="lines">
    <div *ngFor="let line of lines.controls; let i = index" [formGroupName]="i">
      <select formControlName="productId">
        <option *ngFor="let p of products" [value]="p.id">{{ p.nom }} — {{ p.prixUnitaireHT }} DT</option>
      </select>
      <input type="number" formControlName="quantite" min="1" />
      <span>{{ lineTotal(i) }} DT</span>
      <button type="button" (click)="removeLine(i)">Supprimer</button>
    </div>
  </div>

  <button type="button" (click)="addLine()">+ Ajouter un produit</button>

  <p>Total HT : {{ totalHT }} DT — Total TTC : {{ totalTTC }} DT</p>
  <button type="submit" [disabled]="form.invalid">Enregistrer</button>
</form>
```

### Étape 4.5 — Routing + gestion des messages

- `app-routing.module.ts` : routes vers `client-list`, `client-form/:id`, `product-list`, `product-form/:id`, `order-list`, `order-form/:id`, `order-detail/:id`.
- Un `interceptor` HTTP simple pour capter les erreurs API et les afficher via un service de toast/alert partagé, plutôt que de gérer les erreurs composant par composant.

---

## 5. Bonus JWT (si le temps le permet)

Backend :
- `POST /api/auth/login` → vérifie un utilisateur simple (même en dur dans une table `Users`), génère un JWT avec `Microsoft.AspNetCore.Authentication.JwtBearer`.
- `[Authorize]` sur les contrôleurs sensibles.

Frontend :
- Page de login, stockage du token (en mémoire ou `localStorage`), `HttpInterceptor` qui ajoute le header `Authorization: Bearer ...`, `AuthGuard` sur les routes protégées.

Ne fais ce module qu'en dernier — c'est un bonus, pas le cœur de l'évaluation.

---

## 6. Planning réaliste sur 2 jours

**Jour 1 (Backend + DB)**
1. Setup solution, projets, packages (30 min)
2. Entities + DbContext + migration initiale (1h)
3. Module Clients : DTO, service, controller CRUD (1h)
4. Module Products : DTO, service, controller CRUD (1h)
5. Module Orders + OrderLines : DTOs, `OrderService` avec toute la logique métier (calcul totaux, stock, validation) (2-3h)
6. Tests manuels via Swagger de tout le back (30-45 min)

**Jour 2 (Frontend + finitions)**
1. Setup Angular, modules, routing (30 min)
2. Modules Clients + Products : liste + formulaire (1h30)
3. Module Orders : liste, formulaire avec `FormArray`, page détail (2-3h)
4. Gestion erreurs/succès (toast), validations front (1h)
5. README.md + captures d'écran + vérification du scénario complet (1h)
6. (Si temps restant) Bonus JWT

---

## 7. Checklist avant de livrer

- [ ] Scénario complet fonctionne de bout en bout : créer client → créer produits → créer commande → ajouter lignes → totaux corrects → valider → stock mis à jour → consulter liste/détail
- [ ] Impossible de créer une commande sans client
- [ ] Impossible d'ajouter une ligne avec quantité ≤ 0
- [ ] Impossible de commander plus que le stock disponible
- [ ] TVA 19% appliquée correctement sur le TTC
- [ ] Swagger accessible et documenté
- [ ] Gestion d'erreurs cohérente (codes HTTP + messages clairs)
- [ ] README avec prérequis, étapes de lancement back/front, infos de connexion si JWT
- [ ] Pas de logique métier dans les contrôleurs, pas de duplication, noms de variables clairs

---

Bon courage pour le test — vise une version simple et solide plutôt qu'une version ambitieuse mais instable, comme le précise l'énoncé.
