namespace JobAssistantSystem.API.Errors
{
    public sealed class NotFoundException : DomainException
    {
        public NotFoundException(string resourceName, int id)
            : base($"No {resourceName} was found for id '{id}'.")
        {
        }

        public override int StatusCode => StatusCodes.Status404NotFound;
        public override string Title => "Not found";
        public override string Type => "https://jobmatch.local/errors/not-found";
    }
}
