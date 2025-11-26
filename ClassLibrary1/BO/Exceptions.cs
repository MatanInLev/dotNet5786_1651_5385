
namespace BO;

/// <summary>
/// Base exception class for all Business Logic Layer (BL) exceptions.
/// </summary>
[Serializable]
public class BlBaseException : Exception
{
    public BlBaseException(string message) : base(message) { }
    public BlBaseException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a requested Business Object (BO) or Data Object (DO)
/// does not exist in the data layer (e.g., trying to read a non-existent order).
/// </summary>
[Serializable]
public class BlDoesNotExistException : BlBaseException
{
    public BlDoesNotExistException(string message) : base(message) { }
    public BlDoesNotExistException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown for general logic errors related to invalid data,
/// such as an attempt to set a negative value where only positive is allowed,
/// or incorrect date logic.
/// </summary>
[Serializable]
public class BlInvalidValueException : BlBaseException
{
    public BlInvalidValueException(string message) : base(message) { }
    public BlInvalidValueException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an object that must be unique already exists (e.g., creating an order with an existing ID).
/// </summary>
[Serializable]
public class BlAlreadyExistsException : BlBaseException
{
    public BlAlreadyExistsException(string message) : base(message) { }
    public BlAlreadyExistsException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown for temporary failures, typically related to external services
/// (e.g., a network error when calling the geocoding service).
/// </summary>
[Serializable]
public class BlTemporaryNotAvailableException : BlBaseException
{
    public BlTemporaryNotAvailableException(string message) : base(message) { }
    public BlTemporaryNotAvailableException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an attempted deletion cannot be completed because doing so would
/// violate business rules or data integrity constraints.
/// </summary>
[Serializable]
public class BlDeletionImpossibleException : BlBaseException
{
    public BlDeletionImpossibleException(string message) : base(message) { }
    public BlDeletionImpossibleException(string message, Exception innerException)
        : base(message, innerException) { }
}

