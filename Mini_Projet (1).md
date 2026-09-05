Mini projet de gestion commerciale

1. Objectif du test
   
   Dans le cadre du recrutement d’un développeur, nous souhaitons évaluer les compétences
   techniques du candidat à travers la réalisation d’un mini projet de gestion commerciale.
   Le projet doit être développé sur une durée maximale de 2 jours.
   L’objectif est d’évaluer la capacité du candidat à concevoir une application simple, structurée,
   fonctionnelle et maintenable en utilisant :
   * Back-end : .NET 8
   
   *  Front-end : Angular
   
   * Base de données : SQL Server.
2. Contexte fonctionnel
   
   L’application demandée est une mini application de gestion commerciale permettant de gérer :
   * Les clients
   
   * Les produits
   
   * Les commandes
   
   * Les lignes de commande
   
   * Le calcul du total d’une commande
     L’application doit permettre à un utilisateur de consulter, créer, modifier et supprimer les principales données commerciales.
3. Fonctionnalites attendues
   1. Gestion des clients
      
      Le candidat doit développer un module permettant de :
      • Afficher la liste des clients
      • Ajouter un nouveau client
      • Modifier les informations d’un client
      • Supprimer un client
      • Consulter le détail d’un client
      Un client doit contenir au minimum les informations suivantes :
      • Identifiant
      • Nom
      
      •Prénom ou raison sociale
      • Email
      • Téléphone
      • Adresse[Tableau]
      • Date de création
   
   2. Gestion des produits
      
      Le candidat doit développer un module permettant de :
      • Afficher la liste des produits
      • Ajouter un nouveau produit
      • Modifier un produit
      • Supprimer un produit
      • Consulter le détail d’un produit
      Un produit doit contenir au minimum les informations suivantes :
      • Identifiant
      • Référence
      • Nom du produit
      • Description
      • Prix unitaire HT
      • Quantité en stock
      • Date de création
   
   3. Gestion des commandes
      
      Le candidat doit développer un module permettant de :
      • Afficher la liste des commandes
      • Créer une nouvelle commande
      • Modifier une commande existante
      • Supprimer une commande
      • Consulter le détail d’une commande
      Une commande doit contenir au minimum :
      
      • Identifiant
      • Numéro de commande
      • Client associé
      • Date de commande
      • Statut de la commande
      • Total HT
      • Total TTC
      Les statuts possibles sont par exemple :
      • Brouillon
      • Validée
      • Annulée
   
   4. Lignes de commande
      
      Lors de la création ou modification d’une commande, l’utilisateur doit pouvoir :
      • Ajouter un ou plusieurs produits
      • Saisir la quantité commandée
      • Afficher le prix unitaire du produit
      • Calculer automatiquement le total de chaque ligne
      • Calculer automatiquement le total global de la commande
      Une ligne de commande doit contenir :
      • Produit
      • Quantité
      • Prix unitaire
      • Total ligne
4. Regles de gestion 
   
   Le candidat doit respecter les règles suivantes :
   * Un client peut avoir plusieurs commandes.
   
   * Une commande appartient à un seul client.
   
   * Une commande peut contenir plusieurs produits.
   
   * Un produit peut être présent dans plusieurs commandes.
   
   * Le total d’une ligne est calculé comme suit : Quantité × Prix unitaire
   
   *  Le total HT de la commande correspond à la somme des lignes.
   
   * Le total TTC peut être calculé avec une TVA fixe de 19%.
   
   * Il ne doit pas être possible de créer une commande sans client.
   
   * Il ne doit pas être possible de créer une ligne de commande avec une quantité inférieure ou égale à zéro.
   
   * Il ne doit pas être possible de commander une quantité supérieure au stock disponible.
   
   * Le stock produit doit être mis à jour lorsqu’une commande est validée.
5. Exigences techinques
   1. Back-end Le back-end doit être développé avec .NET 8.
      
      Il est demandé de mettre en place :
      
      * Une API REST
      
      * Une architecture claire et organisée
      
      * Des modèles/entities
      
      * Des DTOs si nécessaire
      
      * Une couche service ou business logic
      
      * Une couche d’accès aux données
      
      * Entity Framework Core ou tout autre ORM compatible
      
      * Des validations côté back-end
      
      * Une gestion correcte des erreurs
      
      * Une documentation minimale des endpoints, par exemple Swagger/OpenAPI
      
      Exemples d’endpoints attendus :
      
      * GET /api/clients
      
      * GET /api/clients/{id}
      
      * POST /api/clients
      
      * PUT /api/clients/{id}
      
      * DELETE /api/clients/{id}
      
      * GET /api/products
      
      * GET /api/products/{id}
      
      * POST /api/products
      
      * PUT /api/products/{id}
      
      * DELETE /api/products/{id}
      
      * GET /api/orders
      
      * GET /api/orders/{id}
      
      * POST /api/orders
      
      * PUT /api/orders/{id}
      
      * DELETE /api/orders/{id}
      
      * POST /api/orders/{id}/validate
   
   2. Front-end
      
      Il est attendu :
      • Une interface simple, claire et utilisable
      • Une navigation entre les pages principales
      • Des formulaires de création et modification
      • Des tableaux de consultation
      • Une page de détail pour les commandes
      • Une consommation correcte de l’API REST
      • Une gestion simple des messages d’erreur et de succès
      • Une validation minimale des formulaires côté front-end
      
      Pages attendues :
      • Liste des clients
      • Formulaire client
      • Liste des produits
      • Formulaire produit
      • Liste des commandes
      • Formulaire de création/modification d’une commande
      • Détail d’une commande
6. Base de donnees
   
   La structure minimale attendue doit contenir les tables suivantes :
   * Clients
   
   * Products
   
   * Orders
   
   * OrderLines
   
   Le candidat doit fournir soit :
   
   * Un script SQL de création de la base de données
   
   * Ou des migrations Entity Framework Core
   
   * Ou une base SQLite prête à l’emploi
7. Athentifiaction 
   
   L’authentification n’est pas obligatoire
   
   Toutefois, un bonus sera accordé si le candidat met en place :
   * Une authentification simple par JWT
   
   * Une page de connexion
   
   * Une protection des routes côté front-end
   
   * Une protection des endpoints côté back-end
8. qaulite attendue du code
   
   Le code doit etre : 
   * clair , lisible, stucture, commente lorque necessaire,facile a lance , repectueux des bonnes pratique de developpement
   
   Le condidat doit eviter : 
   
   * Le code dupliqué; Les méthodes trop longues;Les noms de variables non explicites;La logique métier directement dans les contrôleurs; Les données codées en dur sans justification
9. Livrables attendus
   
   Le projet peut être livré sous forme de : 
   * Lien GitHub
   
   * Archive ZIP contenant le projet complet
   
   À la fin du test, le candidat doit fournir :
   
   * Le code source complet du back-end
   
   * Le code source complet du front-end
   
   * Les scripts ou migrations de base de données
   
   * Un fichier README.md contenant :
     
     * Les prérequis d’installation
     
     * Les étapes pour lancer le back-end
     
     * Les étapes pour lancer le front-end
     
     * Les informations de connexion si une authentification est mise en place
     
     * Quelques captures d’écran si possible
10. criters d evaluation
    
    Le project sera evalure selon les criters suivants:
    
    Critère : Importance
    Respect du cahier des charges  : Élevée
    Qualité de l’architecture back-end  : Élevée
    Qualité de l’interface front-end  : Moyenne
    Bonne utilisation de .NET 8 : Élevée
    Bonne consommation de l’API REST : Élevée
    Gestion des erreurs et validations  : Élevée
    Qualité du code et organisation du projet : Élevée
    
    Simplicité d’installation et documentation : Moyenne
    Bonus authentification JWT : Bonus
    Tests unitaires ou tests API : Bonus
11. duree du test
    
    duree de test 2 jours: En cas de manque de temps, il est préférable de fournir une version simple, fonctionnelle et bien structurée plutôt qu’une version incomplète ou instable.
12. Resultat attendu
    
    A a fin du test lapplication doit permettre de reqlise le scenr
    1. Créer un client
    
    2. Créer plusieurs produits avec stock et prix
    
    3. Créer une commande pour ce client
    
    4. Ajouter plusieurs produits à la commande
    
    5. Calculer automatiquement les totaux
    
    6. Valider la commande
    
    7. Mettre à jour le stock des produits
    
    8. Consulter la liste et le détail des commandes
13. Remarque final
    
    Le candidat est libre de proposer des améliorations techniques ou fonctionnelles, à condition que le périmètre principal du projet soit respecté.
    L’objectif principal est de démontrer une bonne maîtrise de .NET 8, d’un framework JavaScript moderne, de la conception d’API REST et de la structuration d’une application web complète.
