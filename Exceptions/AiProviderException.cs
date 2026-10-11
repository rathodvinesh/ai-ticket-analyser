namespace AI_ticket_analyzer.Exceptions
{
    public sealed class AiProviderException : Exception
    {
        public int? StatusCode { get; }

        public AiProviderException(string message, int? statusCode = null, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
