namespace JobAssistantSystem.API.Errors
{
    public class ApiException
    {
        public int StatusCode { get; }
        public string? Details { get; }
        public string Message { get; }

        public ApiException(int statusCode, string message, string? details)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }
    }
}