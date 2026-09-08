-- Script de création de la base de données GestionCommercialeDb
-- Alternative aux migrations EF Core, fournie comme référence / fallback.

CREATE TABLE Clients (
    Id INT IDENTITY PRIMARY KEY,
    Nom NVARCHAR(100) NOT NULL,
    PrenomOuRaisonSociale NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    Telephone NVARCHAR(30) NULL,
    Adresse NVARCHAR(300) NULL,
    DateCreation DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Products (
    Id INT IDENTITY PRIMARY KEY,
    Reference NVARCHAR(50) NOT NULL,
    Nom NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    PrixUnitaireHT DECIMAL(18,2) NOT NULL,
    QuantiteStock INT NOT NULL,
    DateCreation DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Products_Reference UNIQUE (Reference)
);

CREATE TABLE Orders (
    Id INT IDENTITY PRIMARY KEY,
    NumeroCommande NVARCHAR(50) NOT NULL,
    ClientId INT NOT NULL,
    DateCommande DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Statut NVARCHAR(20) NOT NULL DEFAULT 'Brouillon',
    TotalHT DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalTTC DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Orders_NumeroCommande UNIQUE (NumeroCommande),
    CONSTRAINT FK_Orders_Clients FOREIGN KEY (ClientId) REFERENCES Clients(Id)
);

CREATE TABLE OrderLines (
    Id INT IDENTITY PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantite INT NOT NULL,
    PrixUnitaire DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderLines_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderLines_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Index créés par la migration EF Core InitialCreate (jointures / FK)
CREATE INDEX IX_Orders_ClientId ON Orders (ClientId);
CREATE INDEX IX_OrderLines_OrderId ON OrderLines (OrderId);
CREATE INDEX IX_OrderLines_ProductId ON OrderLines (ProductId);
