namespace JobAssistantSystem.API.Errors
{
    public sealed class InvalidCredentialsException : DomainException
    {
        public InvalidCredentialsException()
            : base("The provided email or password is incorrect.")
        {
        }

        public override int StatusCode => StatusCodes.Status401Unauthorized;
        public override string Title => "Invalid credentials";
        public override string Type => "https://jobmatch.local/errors/invalid-credentials";
    }
}
