namespace CleanApi.Application.Common.Exceptions;

/// <summary>
/// Thrown by the validation behavior. Translated to a 400 ValidationProblemDetails response by the API.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException()
        : this(new Dictionary<string, string[]>())
    {
    }

    public ValidationException(string message)
        : base(message) => Errors = new Dictionary<string, string[]>();

    public ValidationException(string message, Exception innerException)
        : base(message, innerException) => Errors = new Dictionary<string, string[]>();

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.") => Errors = errors;

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
