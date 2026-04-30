namespace JobAssistantSystem.API.Errors
{
    /// <summary>
    /// Used when a dependency we call (embedding service, NLP service, etc.) fails or
    /// returns an unusable response. Maps to 502 Bad Gateway because the failure is
    /// upstream of our API, not a client mistake.
    /// </summary>
    public sealed class UpstreamServiceException : DomainException
    {
        public UpstreamServiceException(string detail)
            : base(detail)
        {
        }

        public override int StatusCode => StatusCodes.Status502BadGateway;
        public override string Title => "An upstream service failed";
        public override string Type => "https://jobmatch.local/errors/upstream-failure";
    }
}
