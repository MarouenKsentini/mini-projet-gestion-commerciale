export interface Client {
  id?: number;
  nom: string;
  prenomOuRaisonSociale: string;
  email: string;
  telephone?: string;
  adresse?: string;
  dateCreation?: string;
}
