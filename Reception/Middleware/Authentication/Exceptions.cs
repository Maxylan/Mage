using Reception.Database.Models;

namespace Reception.Middleware.Authentication;

/// <summary>
/// Collection of custom throwable exeptions (<seealso cref="Exception"/>)
/// </summary>
public class AuthenticationException : Exception, IAuthenticationException
{
    public int Code { get; init; }

    /// <summary>
    /// Instantly throws a new instance of a <see cref="AuthenticationException"/> with a specified error.
    /// </summary>
    /// <param name="code">
    /// A source for the error that can be used to determine how to handle this Authentication Failure.
    /// </summary>
    /// <param name="message">
    /// A message that describes the error.
    /// </summary>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or a null reference if no inner exception
    /// is specified.
    /// </summary>
    public static void Throw(int code, string? message = null, Exception? innerException = null) =>
        throw new AuthenticationException(code, message, innerException);

    /// <summary>
    /// Initializes a new instance of a <see cref="AuthenticationException"/>.
    /// </summary>
    /// <param name="code">
    /// A source for the error that can be used to determine how to handle this Authentication Failure.
    /// </summary>
    public AuthenticationException(int code) : base(Messages.ByCode(code))
    {
        this.Code = code;
    }
    /// <summary>
    /// Initializes a new instance of a <see cref="AuthenticationException"/> with a specified error.
    /// </summary>
    /// <param name="code">
    /// A source for the error that can be used to determine how to handle this Authentication Failure.
    /// </summary>
    /// <param name="message">
    /// The message that describes the error.
    /// </summary>
    public AuthenticationException(int code, string? message) : base(message ?? Messages.ByCode(code))
    {
        this.Code = code;
    }
    /// <summary>
    /// Initializes a new instance of a <see cref="AuthenticationException"/> with a specified error.
    /// </summary>
    /// <param name="code">
    /// A source for the error that can be used to determine how to handle this Authentication Failure.
    /// </summary>
    /// <param name="message">
    /// The message that describes the error.
    /// </summary>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or a null reference if no inner exception
    /// is specified.
    /// </summary>
    public AuthenticationException(int code, string? message, Exception? innerException) : base(message ?? Messages.ByCode(code), innerException)
    {
        this.Code = code;
    }
}

/// <summary>
/// TODO! Document me!
/// </summary>
public interface IAuthenticationException
{
    public int Code { get; init; }
}
