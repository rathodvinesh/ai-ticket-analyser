namespace AI_ticket_analyzer.Service
{
    public sealed class InvalidAiResponseException : Exception
    {
        public InvalidAiResponseException(string message)
            : base(message)
        {
        }
    }
}
