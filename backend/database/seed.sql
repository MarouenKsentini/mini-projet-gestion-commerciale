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
IF NOT EXISTS (SELECT 1 FROM Clients WHERE Email='sami.trabelsi@test.tn')
INSERT INTO Clients (Nom, PrenomOuRaisonSociale, Email, Telephone, Adresse) VALUES ('Trabelsi','Sami','sami.trabelsi@test.tn','22001122','Sousse');
IF NOT EXISTS (SELECT 1 FROM Clients WHERE Email='contact@abc.tn')
INSERT INTO Clients (Nom, PrenomOuRaisonSociale, Email, Telephone, Adresse) VALUES ('ABC','Ste ABC','contact@abc.tn','70112233','Monastir');
IF NOT EXISTS (SELECT 1 FROM Clients WHERE Email='leila.mansour@test.tn')
INSERT INTO Clients (Nom, PrenomOuRaisonSociale, Email, Telephone, Adresse) VALUES ('Mansour','Leila','leila.mansour@test.tn','23112233','Hammamet');
IF NOT EXISTS (SELECT 1 FROM Clients WHERE Email='contact@def.tn')
INSERT INTO Clients (Nom, PrenomOuRaisonSociale, Email, Telephone, Adresse) VALUES ('DEF','Ste DEF','contact@def.tn','70223344','Gabes');
IF NOT EXISTS (SELECT 1 FROM Clients WHERE Email='mohamed.gharbi@test.tn')
INSERT INTO Clients (Nom, PrenomOuRaisonSociale, Email, Telephone, Adresse) VALUES ('Gharbi','Mohamed','mohamed.gharbi@test.tn','24001133','Bizerte');
IF NOT EXISTS (SELECT 1 FROM Clients WHERE Email='contact@ghi.tn')
INSERT INTO Clients (Nom, PrenomOuRaisonSociale, Email, Telephone, Adresse) VALUES ('GHI','Ste GHI','contact@ghi.tn','70334455','Kairouan');

-- 2. Products (4) - Reference unique
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-001')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-001','Clavier AZERTY','Clavier filaire',85.50,50);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-002')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-002','Souris optique','Souris USB',45.00,100);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-003')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-003','Ecran 24 pouces','Full HD',450.00,20);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-004')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-004','Cable HDMI 2m',NULL,15.00,200);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-005')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-005','Imprimante laser','Monochrome A4',620.00,12);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-006')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-006','Webcam HD','1080p USB',95.00,30);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-007')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-007','Disque SSD 512Go','NVMe M.2',180.00,40);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-008')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-008','Casque audio','Bluetooth',120.00,25);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-009')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-009','Tapis souris','XXL',25.00,60);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-010')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-010','Clavier mecanique','RGB',210.00,18);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-011')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-011','Hub USB-C','7 ports',75.00,35);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Reference='REF-012')
INSERT INTO Products (Reference, Nom, Description, PrixUnitaireHT, QuantiteStock) VALUES ('REF-012','Ecran 27 pouces','QHD',690.00,8);

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
