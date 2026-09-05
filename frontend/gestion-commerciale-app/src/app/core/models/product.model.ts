export interface Product {
  id?: number;
  reference: string;
  nom: string;
  description?: string;
  prixUnitaireHT: number;
  quantiteStock: number;
  dateCreation?: string;
}
