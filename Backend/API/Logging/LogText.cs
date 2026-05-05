namespace API.Logging;

internal static class LogText
{
    internal static string Truncate500(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text.Length <= 500 ? text : text[..500] + "...";
    }
}
