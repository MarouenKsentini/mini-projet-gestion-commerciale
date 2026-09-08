# Mini Projet — Gestion Commerciale

Application de gestion commerciale (clients, produits, commandes, lignes de commande) développée pour le test technique.

- **Back-end** : .NET 8, architecture en couches (Domain / Application / Infrastructure / Api), Entity Framework Core, SQL Server
- **Front-end** : Angular 18 (standalone components), formulaires réactifs
- **Base de données** : SQL Server (migrations EF Core ou script SQL fourni en fallback)

## Structure du dépôt

```
gestion-commerciale/
├── backend/
│   ├── GestionCommerciale.sln
│   ├── src/
│   │   ├── GestionCommerciale.Domain/          (entities, enums, exceptions)
│   │   ├── GestionCommerciale.Application/     (DTOs, interfaces, services / logique métier)
│   │   ├── GestionCommerciale.Infrastructure/  (DbContext EF Core, injection de dépendances)
│   │   └── GestionCommerciale.Api/             (controllers, Program.cs, middleware d'erreurs)
│   └── database/
│       └── schema.sql                          (script SQL de secours)
└── frontend/
    └── gestion-commerciale-app/                (application Angular)
```

## Prérequis

- .NET 8 SDK
- Node.js 18+ et npm
- SQL Server ou SQL Server LocalDB
- Angular CLI (`npm install -g @angular/cli`) — optionnel, `npx` fonctionne aussi

## Lancer le back-end

```bash
cd backend/src/GestionCommerciale.Api
dotnet restore
dotnet run
```

Aucune commande EF n'est nécessaire au premier lancement : au démarrage, l'API applique automatiquement la migration `InitialCreate` (déjà commise) et insère des données de démo (2 clients, 4 produits, 1 commande). Une migration n'est à créer qu'après modification du modèle :

```bash
dotnet ef migrations add <Nom> -p ../GestionCommerciale.Infrastructure -s .
```

- Chaîne de connexion : `appsettings.Development.json` (`Server=DESKTOP-7VIA6NI\MAROUEN`) s'applique en dev, `appsettings.json` (LocalDB) sinon — à adapter selon la machine.
- Sans SQL Server : utiliser une chaîne `Data Source=...` (sans `Server=`) pour basculer automatiquement sur SQLite, ou exécuter `backend/database/schema.sql` puis `seed.sql` manuellement sur SQL Server.
- En cas d'erreur 26 (instance introuvable) : démarrer l'instance nommée et le SQL Server Browser (voir `tools/README.md`).

Swagger disponible sur l'URL affichée dans le terminal, par exemple `https://localhost:5001/swagger`.

## Lancer le front-end

```bash
cd frontend/gestion-commerciale-app
npm install
npm start
```

Application disponible sur `http://localhost:4200`. L'URL de l'API est configurée dans `src/environments/environment.ts` (`https://localhost:5001/api` par défaut — à adapter si le back tourne sur un autre port).

Si le navigateur affiche une erreur CORS, vérifier que `Cors:AllowedOrigin` dans `appsettings.json` du back correspond bien à l'URL du front (`http://localhost:4200`).

## Scénario de test

1. Créer un client (page Clients)
2. Créer plusieurs produits avec stock et prix (page Produits)
3. Créer une commande pour ce client, ajouter plusieurs lignes de produits (page Commandes → Nouvelle commande)
4. Les totaux HT et TTC (TVA 19%) sont calculés automatiquement
5. Valider la commande depuis le détail ou la liste → le stock des produits est décrémenté (une commande en brouillon peut aussi être annulée → statut `Annulée`)
6. Consulter la liste et le détail des commandes

## Authentification

Non implémentée dans cette version (bonus non traité par manque de temps). L'architecture (middleware, injection de dépendances) permet de l'ajouter facilement via `Microsoft.AspNetCore.Authentication.JwtBearer` côté API et un `HttpInterceptor` + `AuthGuard` côté Angular.

## Points d'architecture

- Toute la logique métier (calcul des totaux, TVA, vérification et mise à jour du stock, règles de validation) est centralisée dans les services de la couche `Application` (`OrderService` notamment) — les contrôleurs restent fins.
- Les erreurs métier (`BusinessException`, `NotFoundException`) sont interceptées par un middleware global (`ExceptionHandlingMiddleware`) qui renvoie des réponses HTTP cohérentes (400 / 404) avec un message clair, affiché côté front via un intercepteur HTTP et un composant toast.
