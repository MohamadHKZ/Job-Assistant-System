namespace JobAssistantSystem.API.Errors
{
    /// <summary>
    /// Base type for any error caused by a known business-rule violation.
    /// The global exception handler converts these to RFC 7807 ProblemDetails
    /// using <see cref="StatusCode"/>, <see cref="Title"/>, <see cref="Type"/>
    /// and <see cref="Detail"/>.
    /// </summary>
    public abstract class DomainException : Exception
    {
        public abstract int StatusCode { get; }
        public abstract string Title { get; }
        public abstract string Type { get; }
        public virtual string? Detail { get; }

        protected DomainException(string? detail = null) : base(detail)
        {
            Detail = detail;
        }
    }
}
