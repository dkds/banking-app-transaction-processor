using System.Runtime.Serialization;

namespace TransactionProcessor.Exceptions
{
    [Serializable]
    internal class InvalidAccountStatusException : Exception
    {
        public InvalidAccountStatusException()
        {
        }

        public InvalidAccountStatusException(string? message) : base(message)
        {
        }

        public InvalidAccountStatusException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidAccountStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}