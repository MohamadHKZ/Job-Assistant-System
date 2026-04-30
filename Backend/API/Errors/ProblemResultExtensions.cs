using Microsoft.AspNetCore.Mvc;

namespace JobAssistantSystem.API.Errors
{
    /// <summary>
    /// Thin sugar over <see cref="ControllerBase.Problem"/> so a controller can
    /// emit a fully-typed RFC 7807 response in one line, with an explicit
    /// problem <c>type</c> URI for stable client matching.
    /// </summary>
    public static class ProblemResultExtensions
    {
        private const string TypeBase = "https://jobmatch.local/errors/";

        public static ObjectResult ProblemFor(
            this ControllerBase controller,
            string typeSlug,
            string title,
            int statusCode,
            string? detail = null)
        {
            return controller.Problem(
                detail: detail,
                statusCode: statusCode,
                title: title,
                type: TypeBase + typeSlug);
        }
    }
}
