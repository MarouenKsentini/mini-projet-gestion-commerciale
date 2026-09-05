export interface OrderLine {
  id?: number;
  productId: number;
  productNom?: string;
  quantite: number;
  prixUnitaire?: number;
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
