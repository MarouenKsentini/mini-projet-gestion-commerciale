namespace GestionCommerciale.Domain.Exceptions;

/// <summary>
/// Exception levée pour toute violation d'une règle de gestion métier
/// (ex: stock insuffisant, quantité invalide, client manquant...).
/// Interceptée globalement pour renvoyer un code 400 avec un message clair.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
