-- Seed de test pour GestionCommercialeDb
-- Execute after schema/migration. Safe to re-run (checks existence).
-- Usage SSMS: open GestionCommercialeDb > New Query > Execute this file
-- Usage sqlcmd: sqlcmd -S DESKTOP-7VIA6NI\MAROUEN -d GestionCommercialeDb -i seed.sql

SET NOCOUNT ON;

-- 1. Clients (2)
IF NOT EXISTS (SELECT 1 FROM Clients WHERE Email='ahmed.benali@test.tn')
INSERT INTO Clients (Nom, PrenomOuRaisonSociale, Email, Telephone, Adresse) VALUES ('Ben Ali','Ahmed','ahmed.benali@test.tn','20001000','Tunis');
IF NOT EXISTS (SELECT 1 FROM Clients WHERE Email='contact@xyz.tn')
INSERT INTO Clients (Nom, PrenomOuRaisonSociale, Email, Telephone, Adresse) VALUES ('XYZ','Societe XYZ','contact@xyz.tn','71000000','Sfax');

-- 2. Products (4) - Reference unique
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-001')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-001','Clavier AZERTY','Clavier filaire',85.50,50);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-002')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-002','Souris optique','Souris USB',45.00,100);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-003')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-003','Ecran 24 pouces','Full HD',450.00,20);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-004')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-004','Cable HDMI 2m',NULL,15.00,200);

-- 3. One Brouillon order for Ahmed (Client 1) with 2 lines -> TotalHT 396, TotalTTC 471.24 (TVA 19%)
-- Only insert if no order exists yet, to keep TotalHT/TTC authoritative from OrderService logic
IF NOT EXISTS (SELECT 1 FROM Orders WHERE NumeroCommande LIKE 'CMD-SEED-001%')
BEGIN
    DECLARE @clientId INT = (SELECT TOP 1 Id FROM Clients WHERE Email='ahmed.benali@test.tn');
    DECLARE @orderId INT;
    INSERT INTO Orders (NumeroCommande, ClientId, Statut, TotalHT, TotalTTC)
    VALUES ('CMD-SEED-001', @clientId, 'Brouillon', 396.00, 471.24);
    SET @orderId = SCOPE_IDENTITY();
    INSERT INTO OrderLines (OrderId, ProductId, Quantite, PrixUnitaire)
    VALUES (@orderId, (SELECT Id FROM Products WHERE Reference='REF-001'), 2, 85.50),
           (@orderId, (SELECT Id FROM Products WHERE Reference='REF-002'), 5, 45.00);
END

SELECT 'Seed done' AS Status, (SELECT COUNT(*) FROM Clients) AS Clients, (SELECT COUNT(*) FROM Products) AS Products, (SELECT COUNT(*) FROM Orders) AS Orders, (SELECT COUNT(*) FROM OrderLines) AS OrderLines;
