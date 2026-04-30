namespace JobAssistantSystem.API.Errors
{
    public sealed class ProfileNotFoundException : DomainException
    {
        public ProfileNotFoundException(int id)
            : base($"No profile was found for id '{id}'.")
        {
        }

        public override int StatusCode => StatusCodes.Status404NotFound;
        public override string Title => "Profile not found";
        public override string Type => "https://jobmatch.local/errors/profile-not-found";
    }
}
