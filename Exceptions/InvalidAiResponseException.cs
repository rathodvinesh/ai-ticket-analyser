namespace AI_ticket_analyzer.Exceptions
{
    public sealed class InvalidAiResponseException : Exception
    {
        public InvalidAiResponseException(string message)
            : base(message)
        {
        }
    }
}
