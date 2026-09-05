# Fiche step-by-step — Lancer le projet et le pousser sur GitHub

---

## Partie A — Prérequis à installer

| Outil | Vérifier avec | Lien |
|---|---|---|
| .NET 8 SDK | `dotnet --version` (doit afficher 8.x) | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Node.js (LTS) + npm | `node -v` / `npm -v` | https://nodejs.org |
| Angular CLI | `ng version` | `npm install -g @angular/cli` |
| SQL Server (ou LocalDB, plus simple) | — | LocalDB est installé avec Visual Studio, sinon SQL Server Express |
| Git | `git --version` | https://git-scm.com |
| Compte GitHub | — | https://github.com |

---

## Partie B — Lancer le Backend (.NET 8)

1. **Ouvrir un terminal dans le dossier du backend**
   ```bash
   cd backend/GestionCommerciale.Api
   ```

2. **Restaurer les packages**
   ```bash
   dotnet restore
   ```

3. **Configurer la chaîne de connexion** dans `appsettings.json` :
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=(localdb)\\mssqllocaldb;Database=GestionCommercialeDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```
   Adapte `Server=` selon ton installation (LocalDB, instance nommée, ou SQL Server avec user/mot de passe).

4. **Appliquer les migrations EF Core** (crée la base de données)
   ```bash
   dotnet ef database update -p ../GestionCommerciale.Infrastructure -s .
   ```
   Si `dotnet ef` n'est pas reconnu :
   ```bash
   dotnet tool install --global dotnet-ef
   ```

5. **Lancer l'API**
   ```bash
   dotnet run
   ```
   Note l'URL affichée dans le terminal (ex. `https://localhost:5001`).

6. **Vérifier que ça fonctionne** : ouvre `https://localhost:5001/swagger` dans un navigateur — tu dois voir tous les endpoints (clients, products, orders).

---

## Partie C — Lancer le Frontend (Angular)

1. **Ouvrir un terminal dans le dossier du frontend**
   ```bash
   cd frontend/gestion-commerciale-app
   ```

2. **Installer les dépendances**
   ```bash
   npm install
   ```

3. **Vérifier l'URL de l'API** dans le service (ex. `core/services/order.service.ts`, `client.service.ts`, `product.service.ts`) — elle doit pointer vers l'URL du backend lancé à l'étape B.5, par exemple `https://localhost:5001/api`. Le plus propre est de la centraliser dans `environment.ts` :
   ```typescript
   // src/environments/environment.ts
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:5001/api'
   };
   ```

4. **Si erreur CORS** : ajoute dans le `Program.cs` du backend, avant `app.UseAuthorization()` :
   ```csharp
   builder.Services.AddCors(options =>
       options.AddPolicy("AllowAngular", policy =>
           policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
   // ...
   app.UseCors("AllowAngular");
   ```

5. **Lancer l'application**
   ```bash
   ng serve
   ```

6. **Ouvrir** `http://localhost:4200` dans le navigateur. Teste le scénario complet : créer un client, créer des produits, créer une commande, ajouter des lignes, valider.

---

## Partie D — Pousser le projet sur GitHub

### D.1 — Créer un `.gitignore` propre

À la racine du projet (`gestion-commerciale/.gitignore`) :
```
# .NET
bin/
obj/
*.user

# Angular / Node
node_modules/
dist/
.angular/

# IDE
.vs/
.vscode/
*.suo

# Divers
appsettings.Development.json
```

Ne commit jamais `node_modules` ou `bin/obj` — c'est l'erreur la plus fréquente qui rend un dépôt énorme et inutilisable.

### D.2 — Initialiser Git en local

Dans le dossier racine du projet :
```bash
cd gestion-commerciale
git init
git add .
git commit -m "Initial commit - mini projet gestion commerciale"
```

### D.3 — Créer le dépôt sur GitHub

1. Va sur https://github.com/new
2. Nom du dépôt : par exemple `mini-projet-gestion-commerciale`
3. Laisse-le **vide** (pas de README, pas de .gitignore — tu les as déjà en local)
4. Choisis Public ou Private selon la consigne du recruteur
5. Clique sur "Create repository"

### D.4 — Lier le dépôt local au dépôt distant et pousser

GitHub t'affiche les commandes après création, en résumé :
```bash
git branch -M main
git remote add origin https://github.com/marouenksentini/mini-projet-gestion-commerciale.git
git push -u origin main
```

Si Git demande une authentification et refuse le mot de passe classique : GitHub n'accepte plus les mots de passe en HTTPS, il faut soit :
- un **Personal Access Token** (Settings → Developer settings → Personal access tokens) utilisé à la place du mot de passe, soit
- une **clé SSH** configurée (`git remote set-url origin git@github.com:marouenksentini/mini-projet-gestion-commerciale.git`).

### D.5 — Vérifier le rendu

Ouvre `https://github.com/marouenksentini/mini-projet-gestion-commerciale` et vérifie :
- Le backend et le frontend sont bien présents avec leur structure
- Le `README.md` (prérequis, étapes de lancement back/front, infos de connexion) est visible et bien formaté
- Aucun `node_modules/`, `bin/`, `obj/` n'a été poussé par erreur (`git status` doit être propre)

### D.6 — Mises à jour ultérieures

À chaque avancée :
```bash
git add .
git commit -m "Ajout du module commandes"
git push
```

---

## Checklist rapide avant envoi du lien GitHub

- [ ] Le dépôt clone et se lance sans étape cachée non documentée
- [ ] `.gitignore` correct, pas de fichiers binaires/dépendances commités
- [ ] README.md à jour avec instructions claires
- [ ] Migrations EF Core présentes (pas juste une base locale non partageable)
- [ ] Chaîne de connexion dans `appsettings.json` ne contient pas de mot de passe réel en dur (si SQL Auth, mets un exemple générique)
