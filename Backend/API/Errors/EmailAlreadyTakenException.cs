namespace JobAssistantSystem.API.Errors
{
    public sealed class EmailAlreadyTakenException : DomainException
    {
        public EmailAlreadyTakenException(string email)
            : base($"The email '{email}' is already registered to another account.")
        {
        }

        public override int StatusCode => StatusCodes.Status409Conflict;
        public override string Title => "Email is already taken";
        public override string Type => "https://jobmatch.local/errors/email-taken";
    }
}
